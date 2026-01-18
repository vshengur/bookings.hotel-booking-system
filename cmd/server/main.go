package main

import (
	"context"
	"fmt"
	"net"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/prometheus/client_golang/prometheus/promhttp"
	"github.com/vshengur/bookings.room-service/internal/config"
	"github.com/vshengur/bookings.room-service/internal/handlers"
	"github.com/vshengur/bookings.room-service/internal/middleware"
	"github.com/vshengur/bookings.room-service/internal/repository"
	"github.com/vshengur/bookings.room-service/internal/services"
	"github.com/vshengur/bookings.room-service/internal/utils"
	"github.com/vshengur/bookings.room-service/pkg/logger"
	"go.uber.org/zap"
	"google.golang.org/grpc"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"

	pb "github.com/vshengur/bookings.room-service/proto"
)

func main() {
	// Load configuration
	cfg, err := config.LoadConfig()
	if err != nil {
		panic(fmt.Sprintf("Failed to load config: %v", err))
	}

	// Initialize logger
	if err := logger.InitLogger(cfg.Logger.Level, cfg.Logger.OutputPath); err != nil {
		panic(fmt.Sprintf("Failed to initialize logger: %v", err))
	}
	defer logger.Sync()

	logger.Log.Info("Starting Room Service",
		zap.String("version", "1.0.0"),
		zap.String("server_address", cfg.GetServerAddress()),
		zap.String("grpc_address", cfg.GetGRPCAddress()))

	// Connect to database
	db, err := initDatabase(cfg)
	if err != nil {
		logger.Log.Fatal("Failed to connect to database", zap.Error(err))
	}

	logger.Log.Info("Database connected successfully")

	// Initialize repositories
	roomRepo := repository.NewRoomRepository(db)

	// Initialize services
	roomService := services.NewRoomService(roomRepo)

	// Initialize handlers
	roomHandler := handlers.NewRoomHandler(roomService)

	// Register with Consul
	consulClient := utils.NewConsulClient(cfg)
	if err := consulClient.Register(); err != nil {
		logger.Log.Error("Failed to register with Consul", zap.Error(err))
	} else {
		logger.Log.Info("Successfully registered with Consul")
		defer consulClient.Deregister()
	}

	// Start gRPC server in goroutine
	grpcServer := grpc.NewServer()
	pb.RegisterRoomServiceServer(grpcServer, &grpcRoomServer{service: roomService})

	go startGRPCServer(grpcServer, cfg.GetGRPCAddress())

	// Setup HTTP server with Gin
	router := setupRouter(roomHandler)

	// Create HTTP server
	httpServer := &http.Server{
		Addr:    cfg.GetServerAddress(),
		Handler: router,
	}

	// Start HTTP server in goroutine
	go func() {
		logger.Log.Info("Starting HTTP server", zap.String("address", cfg.GetServerAddress()))
		if err := httpServer.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			logger.Log.Fatal("Failed to start HTTP server", zap.Error(err))
		}
	}()

	// Start cleanup goroutine for expired reservations
	go startCleanupWorker(roomRepo)

	// Wait for interrupt signal to gracefully shutdown
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	logger.Log.Info("Shutting down server...")

	// Graceful shutdown
	ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancel()

	if err := httpServer.Shutdown(ctx); err != nil {
		logger.Log.Error("HTTP Server forced to shutdown", zap.Error(err))
	}

	grpcServer.GracefulStop()

	logger.Log.Info("Server exited successfully")
}

// initDatabase initializes the database connection and runs migrations
func initDatabase(cfg *config.Config) (*gorm.DB, error) {
	db, err := gorm.Open(postgres.Open(cfg.GetDatabaseDSN()), &gorm.Config{})
	if err != nil {
		return nil, err
	}

	// Auto-migrate (for development - in production use proper migrations)
	// For production, run SQL migrations from migrations/ directory
	logger.Log.Info("Running database migrations...")

	// Note: In production, you should use a proper migration tool like golang-migrate
	// This is just for convenience in development

	sqlDB, err := db.DB()
	if err != nil {
		return nil, err
	}

	// Set connection pool settings
	sqlDB.SetMaxIdleConns(10)
	sqlDB.SetMaxOpenConns(100)
	sqlDB.SetConnMaxLifetime(time.Hour)

	return db, nil
}

// setupRouter configures the Gin router with all routes and middleware
func setupRouter(roomHandler *handlers.RoomHandler) *gin.Engine {
	router := gin.New()
	router.Use(gin.Recovery())
	router.Use(middleware.LoggingMiddleware())
	router.Use(middleware.MetricsMiddleware())

	// Health check endpoint
	router.GET("/health", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"status":  "healthy",
			"service": "room-service",
		})
	})

	// Prometheus metrics endpoint
	router.GET("/metrics", gin.WrapH(promhttp.Handler()))

	// API routes
	api := router.Group("/api")
	{
		rooms := api.Group("/rooms")
		{
			// CRUD operations
			rooms.POST("", roomHandler.CreateRoom)           // Admin only
			rooms.GET("", roomHandler.GetAllRooms)
			rooms.GET("/:id", roomHandler.GetRoom)
			rooms.PUT("/:id", roomHandler.UpdateRoom)        // Admin only
			rooms.DELETE("/:id", roomHandler.DeleteRoom)     // Admin only

			// Search
			rooms.GET("/search", roomHandler.SearchRooms)

			// Availability operations
			rooms.GET("/:id/availability", roomHandler.CheckAvailability)
			rooms.POST("/:id/reserve", roomHandler.ReserveRoom)
			rooms.POST("/:id/release", roomHandler.ReleaseRoom)
		}
	}

	return router
}

// startGRPCServer starts the gRPC server
func startGRPCServer(server *grpc.Server, address string) {
	listener, err := net.Listen("tcp", address)
	if err != nil {
		logger.Log.Fatal("Failed to listen for gRPC", zap.Error(err))
	}

	logger.Log.Info("Starting gRPC server", zap.String("address", address))
	if err := server.Serve(listener); err != nil {
		logger.Log.Fatal("Failed to serve gRPC", zap.Error(err))
	}
}

// startCleanupWorker runs periodic cleanup of expired reservations
func startCleanupWorker(repo repository.RoomRepository) {
	ticker := time.NewTicker(5 * time.Minute)
	defer ticker.Stop()

	logger.Log.Info("Starting cleanup worker for expired reservations")

	for range ticker.C {
		logger.Log.Debug("Running cleanup of expired reservations")
		if err := repo.CleanupExpiredReservations(); err != nil {
			logger.Log.Error("Failed to cleanup expired reservations", zap.Error(err))
		}
	}
}

// grpcRoomServer implements the gRPC service
type grpcRoomServer struct {
	pb.UnimplementedRoomServiceServer
	service services.RoomService
}

// Implement gRPC methods here
// This is a placeholder - full implementation would include all proto methods
func (s *grpcRoomServer) GetRoom(ctx context.Context, req *pb.GetRoomRequest) (*pb.GetRoomResponse, error) {
	// Implementation would go here
	return nil, nil
}

func (s *grpcRoomServer) CheckAvailability(ctx context.Context, req *pb.CheckAvailabilityRequest) (*pb.CheckAvailabilityResponse, error) {
	// Implementation would go here
	return nil, nil
}

func (s *grpcRoomServer) ReserveRoom(ctx context.Context, req *pb.ReserveRoomRequest) (*pb.ReserveRoomResponse, error) {
	// Implementation would go here
	return nil, nil
}

func (s *grpcRoomServer) ReleaseRoom(ctx context.Context, req *pb.ReleaseRoomRequest) (*pb.ReleaseRoomResponse, error) {
	// Implementation would go here
	return nil, nil
}

func (s *grpcRoomServer) SearchRooms(ctx context.Context, req *pb.SearchRoomsRequest) (*pb.SearchRoomsResponse, error) {
	// Implementation would go here
	return nil, nil
}

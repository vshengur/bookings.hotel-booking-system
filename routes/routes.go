package routes

import (
	"github.com/gin-gonic/gin"
	"github.com/vshengur/bookings.auth-service/config"
	"github.com/vshengur/bookings.auth-service/handlers"
	"github.com/vshengur/bookings.auth-service/repository"
	"github.com/vshengur/bookings.auth-service/services"

	"gorm.io/gorm"
)

func SetupRoutes(r *gin.Engine, db *gorm.DB) {
	userRepo := repository.NewUserRepository(db)

	authService := handlers.AuthHandler{
		AuthService: services.NewAuthService(config.AppConfig.GoogleClientID, config.AppConfig.GoogleSecret, config.AppConfig.GoogleRedirectURL),
		UserRepo:    userRepo,
	}

	userHandler := handlers.UserHandler{
		UserRepo: userRepo,
	}

	r.GET("/auth/callback", authService.GoogleCallback)

	// CRUD для пользователей
	r.GET("/users", userHandler.GetAllUsers)
	r.GET("/users/:id", userHandler.GetUserByID)
	r.POST("/users", userHandler.CreateUser)
	r.PUT("/users/:id", userHandler.UpdateUser)
	r.DELETE("/users/:id", userHandler.DeleteUser)
}

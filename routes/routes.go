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
	authService := handlers.AuthHandler{
		AuthService: services.NewAuthService(config.AppConfig.GoogleClientID, config.AppConfig.GoogleSecret),
		UserRepo:    repository.NewUserRepository(db),
	}

	r.GET("/auth/callback", authService.GoogleCallback)
}

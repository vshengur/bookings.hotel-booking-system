package handlers

import (
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/vshengur/bookings.auth-service/repository"
	"github.com/vshengur/bookings.auth-service/services"
	"github.com/vshengur/bookings.auth-service/utils"
)

type AuthHandler struct {
	AuthService *services.AuthService
	UserRepo    repository.UserRepository
}

func (h *AuthHandler) GoogleCallback(c *gin.Context) {
	code := c.Query("code")
	if code == "" {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid code"})
		return
	}

	user, err := h.AuthService.HandleGoogleCallback(code)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	dbUser := h.UserRepo.FindOrCreate(user)
	token, err := utils.GenerateJWT(dbUser)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to generate token"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"token": token})
}

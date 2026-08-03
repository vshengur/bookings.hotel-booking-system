package handlers

import (
	"net/http"

	"github.com/gin-gonic/gin"
	"gorm.io/gorm"
)

type HealthHandler struct {
	DB *gorm.DB
}

// HealthCheck проверяет состояние сервиса и базы данных
func (h *HealthHandler) HealthCheck(c *gin.Context) {
	// Проверка подключения к базе данных
	sqlDB, err := h.DB.DB()
	if err != nil || sqlDB.Ping() != nil {
		c.JSON(http.StatusInternalServerError, gin.H{
			"status":  "error",
			"message": "Database is not reachable",
		})
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"status":  "ok",
		"message": "Service is healthy",
	})
}

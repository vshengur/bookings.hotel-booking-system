package handlers

import (
	"net/http"
	"strconv"

	"github.com/gin-gonic/gin"
	"github.com/vshengur/bookings.room-service/internal/models"
	"github.com/vshengur/bookings.room-service/internal/services"
	"github.com/vshengur/bookings.room-service/pkg/logger"
	"go.uber.org/zap"
)

// RoomHandler handles HTTP requests for room operations
type RoomHandler struct {
	service services.RoomService
}

// NewRoomHandler creates a new room handler instance
func NewRoomHandler(service services.RoomService) *RoomHandler {
	return &RoomHandler{
		service: service,
	}
}

// CreateRoom handles POST /api/rooms
func (h *RoomHandler) CreateRoom(c *gin.Context) {
	var req models.CreateRoomRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		logger.Log.Error("Invalid request body", zap.Error(err))
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: err.Error(),
			Code:    http.StatusBadRequest,
		})
		return
	}

	room, err := h.service.CreateRoom(&req)
	if err != nil {
		logger.Log.Error("Failed to create room", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusCreated, models.SuccessResponse{
		Message: "Room created successfully",
		Data:    room,
	})
}

// GetRoom handles GET /api/rooms/:id
func (h *RoomHandler) GetRoom(c *gin.Context) {
	idStr := c.Param("id")
	id, err := strconv.ParseInt(idStr, 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: "Invalid room ID",
			Code:    http.StatusBadRequest,
		})
		return
	}

	room, err := h.service.GetRoomByID(id)
	if err != nil {
		c.JSON(http.StatusNotFound, models.ErrorResponse{
			Error:   "Not Found",
			Message: "Room not found",
			Code:    http.StatusNotFound,
		})
		return
	}

	c.JSON(http.StatusOK, room)
}

// GetAllRooms handles GET /api/rooms
func (h *RoomHandler) GetAllRooms(c *gin.Context) {
	page, _ := strconv.Atoi(c.DefaultQuery("page", "1"))
	pageSize, _ := strconv.Atoi(c.DefaultQuery("pageSize", "10"))

	if page < 1 {
		page = 1
	}
	if pageSize < 1 || pageSize > 100 {
		pageSize = 10
	}

	response, err := h.service.GetAllRooms(page, pageSize)
	if err != nil {
		logger.Log.Error("Failed to get rooms", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusOK, response)
}

// UpdateRoom handles PUT /api/rooms/:id
func (h *RoomHandler) UpdateRoom(c *gin.Context) {
	idStr := c.Param("id")
	id, err := strconv.ParseInt(idStr, 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: "Invalid room ID",
			Code:    http.StatusBadRequest,
		})
		return
	}

	var req models.UpdateRoomRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		logger.Log.Error("Invalid request body", zap.Error(err))
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: err.Error(),
			Code:    http.StatusBadRequest,
		})
		return
	}

	room, err := h.service.UpdateRoom(id, &req)
	if err != nil {
		logger.Log.Error("Failed to update room", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusOK, models.SuccessResponse{
		Message: "Room updated successfully",
		Data:    room,
	})
}

// DeleteRoom handles DELETE /api/rooms/:id
func (h *RoomHandler) DeleteRoom(c *gin.Context) {
	idStr := c.Param("id")
	id, err := strconv.ParseInt(idStr, 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: "Invalid room ID",
			Code:    http.StatusBadRequest,
		})
		return
	}

	if err := h.service.DeleteRoom(id); err != nil {
		logger.Log.Error("Failed to delete room", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusOK, models.SuccessResponse{
		Message: "Room deleted successfully",
	})
}

// SearchRooms handles GET /api/rooms/search
func (h *RoomHandler) SearchRooms(c *gin.Context) {
	var req models.SearchRoomsRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		logger.Log.Error("Invalid query parameters", zap.Error(err))
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: err.Error(),
			Code:    http.StatusBadRequest,
		})
		return
	}

	// Set defaults
	if req.Page < 1 {
		req.Page = 1
	}
	if req.PageSize < 1 {
		req.PageSize = 10
	}

	response, err := h.service.SearchRooms(&req)
	if err != nil {
		logger.Log.Error("Failed to search rooms", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusOK, response)
}

// CheckAvailability handles GET /api/rooms/:id/availability
func (h *RoomHandler) CheckAvailability(c *gin.Context) {
	idStr := c.Param("id")
	roomID, err := strconv.ParseInt(idStr, 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: "Invalid room ID",
			Code:    http.StatusBadRequest,
		})
		return
	}

	var req models.CheckAvailabilityRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		logger.Log.Error("Invalid query parameters", zap.Error(err))
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: err.Error(),
			Code:    http.StatusBadRequest,
		})
		return
	}

	req.RoomID = roomID

	response, err := h.service.CheckAvailability(&req)
	if err != nil {
		logger.Log.Error("Failed to check availability", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusOK, response)
}

// ReserveRoom handles POST /api/rooms/:id/reserve
func (h *RoomHandler) ReserveRoom(c *gin.Context) {
	idStr := c.Param("id")
	roomID, err := strconv.ParseInt(idStr, 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: "Invalid room ID",
			Code:    http.StatusBadRequest,
		})
		return
	}

	var req models.ReserveRoomRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		logger.Log.Error("Invalid request body", zap.Error(err))
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: err.Error(),
			Code:    http.StatusBadRequest,
		})
		return
	}

	req.RoomID = roomID

	response, err := h.service.ReserveRoom(&req)
	if err != nil {
		logger.Log.Error("Failed to reserve room", zap.Error(err))
		c.JSON(http.StatusConflict, models.ErrorResponse{
			Error:   "Conflict",
			Message: err.Error(),
			Code:    http.StatusConflict,
		})
		return
	}

	c.JSON(http.StatusCreated, response)
}

// ReleaseRoom handles POST /api/rooms/:id/release
func (h *RoomHandler) ReleaseRoom(c *gin.Context) {
	idStr := c.Param("id")
	roomID, err := strconv.ParseInt(idStr, 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: "Invalid room ID",
			Code:    http.StatusBadRequest,
		})
		return
	}

	var req models.ReleaseRoomRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		logger.Log.Error("Invalid request body", zap.Error(err))
		c.JSON(http.StatusBadRequest, models.ErrorResponse{
			Error:   "Bad Request",
			Message: err.Error(),
			Code:    http.StatusBadRequest,
		})
		return
	}

	req.RoomID = roomID

	if err := h.service.ReleaseRoom(&req); err != nil {
		logger.Log.Error("Failed to release room", zap.Error(err))
		c.JSON(http.StatusInternalServerError, models.ErrorResponse{
			Error:   "Internal Server Error",
			Message: err.Error(),
			Code:    http.StatusInternalServerError,
		})
		return
	}

	c.JSON(http.StatusOK, models.SuccessResponse{
		Message: "Room reservation released successfully",
	})
}

package services

import (
	"fmt"
	"time"

	"github.com/vshengur/bookings.room-service/internal/models"
	"github.com/vshengur/bookings.room-service/internal/repository"
	"github.com/vshengur/bookings.room-service/pkg/logger"
	"go.uber.org/zap"
)

// RoomService interface defines room service methods
type RoomService interface {
	CreateRoom(req *models.CreateRoomRequest) (*models.Room, error)
	GetRoomByID(id int64) (*models.Room, error)
	GetAllRooms(page, pageSize int) (*models.SearchRoomsResponse, error)
	UpdateRoom(id int64, req *models.UpdateRoomRequest) (*models.Room, error)
	DeleteRoom(id int64) error
	SearchRooms(req *models.SearchRoomsRequest) (*models.SearchRoomsResponse, error)

	// Availability methods
	CheckAvailability(req *models.CheckAvailabilityRequest) (*models.AvailabilityResponse, error)
	ReserveRoom(req *models.ReserveRoomRequest) (*models.ReserveRoomResponse, error)
	ReleaseRoom(req *models.ReleaseRoomRequest) error
}

type roomService struct {
	repo            repository.RoomRepository
	pricingStrategy PricingStrategy
}

// NewRoomService creates a new room service instance
func NewRoomService(repo repository.RoomRepository) RoomService {
	return &roomService{
		repo:            repo,
		pricingStrategy: NewBasePricingStrategy(), // Strategy pattern
	}
}

// CreateRoom creates a new room using Builder pattern
func (s *roomService) CreateRoom(req *models.CreateRoomRequest) (*models.Room, error) {
	logger.Log.Info("Creating new room", zap.String("room_number", req.RoomNumber))

	// Check if room number already exists
	existing, err := s.repo.GetByRoomNumber(req.RoomNumber)
	if err == nil && existing != nil {
		return nil, fmt.Errorf("room with number %s already exists", req.RoomNumber)
	}

	// Build room using Builder pattern
	builder := NewRoomBuilder()
	room := builder.
		WithRoomNumber(req.RoomNumber).
		WithRoomType(req.RoomType).
		WithFloor(req.Floor).
		WithCapacity(req.Capacity).
		WithBedType(req.BedType).
		WithSize(req.Size).
		WithDescription(req.Description).
		WithBasePrice(req.BasePrice).
		WithCurrency(req.Currency).
		Build()

	// Add amenities
	for _, amenityReq := range req.Amenities {
		amenity := models.RoomAmenity{
			AmenityType: amenityReq.AmenityType,
			Name:        amenityReq.Name,
			Description: amenityReq.Description,
			IconURL:     amenityReq.IconURL,
		}
		room.Amenities = append(room.Amenities, amenity)
	}

	// Add images
	for _, imageReq := range req.Images {
		image := models.RoomImage{
			URL:         imageReq.URL,
			Title:       imageReq.Title,
			Description: imageReq.Description,
			IsPrimary:   imageReq.IsPrimary,
			SortOrder:   imageReq.SortOrder,
		}
		room.Images = append(room.Images, image)
	}

	if err := s.repo.Create(room); err != nil {
		logger.Log.Error("Failed to create room", zap.Error(err))
		return nil, err
	}

	logger.Log.Info("Room created successfully", zap.Int64("room_id", room.ID))
	return room, nil
}

// GetRoomByID retrieves a room by ID
func (s *roomService) GetRoomByID(id int64) (*models.Room, error) {
	logger.Log.Info("Getting room by ID", zap.Int64("room_id", id))
	return s.repo.GetByID(id)
}

// GetAllRooms retrieves all rooms with pagination
func (s *roomService) GetAllRooms(page, pageSize int) (*models.SearchRoomsResponse, error) {
	logger.Log.Info("Getting all rooms", zap.Int("page", page), zap.Int("page_size", pageSize))

	rooms, total, err := s.repo.GetAll(page, pageSize)
	if err != nil {
		logger.Log.Error("Failed to get all rooms", zap.Error(err))
		return nil, err
	}

	totalPages := int(total) / pageSize
	if int(total)%pageSize > 0 {
		totalPages++
	}

	response := &models.SearchRoomsResponse{
		Rooms:      s.mapRoomsToResponse(rooms),
		TotalCount: total,
		Page:       page,
		PageSize:   pageSize,
		TotalPages: totalPages,
	}

	return response, nil
}

// UpdateRoom updates a room
func (s *roomService) UpdateRoom(id int64, req *models.UpdateRoomRequest) (*models.Room, error) {
	logger.Log.Info("Updating room", zap.Int64("room_id", id))

	room, err := s.repo.GetByID(id)
	if err != nil {
		logger.Log.Error("Room not found", zap.Int64("room_id", id), zap.Error(err))
		return nil, err
	}

	// Update fields if provided
	if req.RoomNumber != nil {
		room.RoomNumber = *req.RoomNumber
	}
	if req.RoomType != nil {
		room.RoomType = *req.RoomType
	}
	if req.Floor != nil {
		room.Floor = *req.Floor
	}
	if req.Capacity != nil {
		room.Capacity = *req.Capacity
	}
	if req.BedType != nil {
		room.BedType = *req.BedType
	}
	if req.Size != nil {
		room.Size = *req.Size
	}
	if req.Description != nil {
		room.Description = *req.Description
	}
	if req.BasePrice != nil {
		room.BasePrice = *req.BasePrice
	}
	if req.Currency != nil {
		room.Currency = *req.Currency
	}
	if req.IsActive != nil {
		room.IsActive = *req.IsActive
	}

	// Update amenities if provided
	if req.Amenities != nil {
		room.Amenities = []models.RoomAmenity{}
		for _, amenityReq := range req.Amenities {
			amenity := models.RoomAmenity{
				RoomID:      room.ID,
				AmenityType: amenityReq.AmenityType,
				Name:        amenityReq.Name,
				Description: amenityReq.Description,
				IconURL:     amenityReq.IconURL,
			}
			room.Amenities = append(room.Amenities, amenity)
		}
	}

	// Update images if provided
	if req.Images != nil {
		room.Images = []models.RoomImage{}
		for _, imageReq := range req.Images {
			image := models.RoomImage{
				RoomID:      room.ID,
				URL:         imageReq.URL,
				Title:       imageReq.Title,
				Description: imageReq.Description,
				IsPrimary:   imageReq.IsPrimary,
				SortOrder:   imageReq.SortOrder,
			}
			room.Images = append(room.Images, image)
		}
	}

	if err := s.repo.Update(room); err != nil {
		logger.Log.Error("Failed to update room", zap.Error(err))
		return nil, err
	}

	logger.Log.Info("Room updated successfully", zap.Int64("room_id", id))
	return room, nil
}

// DeleteRoom deletes a room
func (s *roomService) DeleteRoom(id int64) error {
	logger.Log.Info("Deleting room", zap.Int64("room_id", id))

	if err := s.repo.Delete(id); err != nil {
		logger.Log.Error("Failed to delete room", zap.Error(err))
		return err
	}

	logger.Log.Info("Room deleted successfully", zap.Int64("room_id", id))
	return nil
}

// SearchRooms searches for rooms based on criteria
func (s *roomService) SearchRooms(req *models.SearchRoomsRequest) (*models.SearchRoomsResponse, error) {
	logger.Log.Info("Searching rooms", zap.Any("request", req))

	// Validate dates if provided
	if req.CheckIn != nil && req.CheckOut != nil {
		if req.CheckOut.Before(*req.CheckIn) || req.CheckOut.Equal(*req.CheckIn) {
			return nil, fmt.Errorf("checkout date must be after checkin date")
		}
	}

	rooms, total, err := s.repo.Search(req)
	if err != nil {
		logger.Log.Error("Failed to search rooms", zap.Error(err))
		return nil, err
	}

	pageSize := req.PageSize
	if pageSize < 1 {
		pageSize = 10
	}

	totalPages := int(total) / pageSize
	if int(total)%pageSize > 0 {
		totalPages++
	}

	response := &models.SearchRoomsResponse{
		Rooms:      s.mapRoomsToResponse(rooms),
		TotalCount: total,
		Page:       req.Page,
		PageSize:   pageSize,
		TotalPages: totalPages,
	}

	logger.Log.Info("Search completed", zap.Int64("total_count", total))
	return response, nil
}

// CheckAvailability checks if a room is available and calculates pricing
func (s *roomService) CheckAvailability(req *models.CheckAvailabilityRequest) (*models.AvailabilityResponse, error) {
	logger.Log.Info("Checking availability",
		zap.Int64("room_id", req.RoomID),
		zap.Time("check_in", req.CheckIn),
		zap.Time("check_out", req.CheckOut))

	// Validate dates
	if req.CheckOut.Before(req.CheckIn) || req.CheckOut.Equal(req.CheckIn) {
		return nil, fmt.Errorf("checkout date must be after checkin date")
	}

	// Get room details
	room, err := s.repo.GetByID(req.RoomID)
	if err != nil {
		return nil, err
	}

	// Check availability
	available, err := s.repo.CheckAvailability(req.RoomID, req.CheckIn, req.CheckOut)
	if err != nil {
		return nil, err
	}

	// Calculate pricing using Strategy pattern
	nights := int(req.CheckOut.Sub(req.CheckIn).Hours() / 24)
	totalPrice := s.pricingStrategy.CalculatePrice(room.BasePrice, nights, req.CheckIn, req.CheckOut)

	response := &models.AvailabilityResponse{
		RoomID:       room.ID,
		IsAvailable:  available,
		CheckIn:      req.CheckIn,
		CheckOut:     req.CheckOut,
		PricePerNight: room.BasePrice,
		TotalPrice:   totalPrice,
		Currency:     room.Currency,
	}

	return response, nil
}

// ReserveRoom reserves a room for booking
func (s *roomService) ReserveRoom(req *models.ReserveRoomRequest) (*models.ReserveRoomResponse, error) {
	logger.Log.Info("Reserving room",
		zap.Int64("room_id", req.RoomID),
		zap.Int64("booking_id", req.BookingID))

	// Validate dates
	if req.CheckOut.Before(req.CheckIn) || req.CheckOut.Equal(req.CheckIn) {
		return nil, fmt.Errorf("checkout date must be after checkin date")
	}

	// Reserve for 15 minutes
	reservedUntil := time.Now().Add(15 * time.Minute)

	availability, err := s.repo.ReserveRoom(req.RoomID, req.CheckIn, req.CheckOut, req.BookingID, reservedUntil)
	if err != nil {
		logger.Log.Error("Failed to reserve room", zap.Error(err))
		return nil, err
	}

	response := &models.ReserveRoomResponse{
		ReservationID: availability.ID,
		RoomID:        availability.RoomID,
		BookingID:     req.BookingID,
		CheckIn:       availability.CheckInDate,
		CheckOut:      availability.CheckOutDate,
		ReservedUntil: reservedUntil,
		Status:        availability.Status,
	}

	logger.Log.Info("Room reserved successfully",
		zap.Int64("reservation_id", availability.ID),
		zap.Time("reserved_until", reservedUntil))

	return response, nil
}

// ReleaseRoom releases a room reservation
func (s *roomService) ReleaseRoom(req *models.ReleaseRoomRequest) error {
	logger.Log.Info("Releasing room reservation",
		zap.Int64("room_id", req.RoomID),
		zap.Int64("booking_id", req.BookingID))

	if err := s.repo.ReleaseRoom(req.RoomID, req.BookingID); err != nil {
		logger.Log.Error("Failed to release room", zap.Error(err))
		return err
	}

	logger.Log.Info("Room released successfully")
	return nil
}

// Helper function to map Room models to RoomResponse
func (s *roomService) mapRoomsToResponse(rooms []models.Room) []models.RoomResponse {
	responses := make([]models.RoomResponse, len(rooms))
	for i, room := range rooms {
		responses[i] = models.RoomResponse{
			ID:          room.ID,
			RoomNumber:  room.RoomNumber,
			RoomType:    room.RoomType,
			Floor:       room.Floor,
			Capacity:    room.Capacity,
			BedType:     room.BedType,
			Size:        room.Size,
			Description: room.Description,
			BasePrice:   room.BasePrice,
			Currency:    room.Currency,
			IsActive:    room.IsActive,
			Amenities:   room.Amenities,
			Images:      room.Images,
			CreatedAt:   room.CreatedAt,
			UpdatedAt:   room.UpdatedAt,
		}
	}
	return responses
}

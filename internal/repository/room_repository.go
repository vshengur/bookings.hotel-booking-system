package repository

import (
	"fmt"
	"time"

	"github.com/vshengur/bookings.room-service/internal/models"
	"gorm.io/gorm"
)

// RoomRepository interface defines room repository methods
type RoomRepository interface {
	Create(room *models.Room) error
	GetByID(id int64) (*models.Room, error)
	GetByRoomNumber(roomNumber string) (*models.Room, error)
	GetAll(page, pageSize int) ([]models.Room, int64, error)
	Update(room *models.Room) error
	Delete(id int64) error
	Search(req *models.SearchRoomsRequest) ([]models.Room, int64, error)

	// Availability methods
	CheckAvailability(roomID int64, checkIn, checkOut time.Time) (bool, error)
	ReserveRoom(roomID int64, checkIn, checkOut time.Time, bookingID int64, reservedUntil time.Time) (*models.RoomAvailability, error)
	ReleaseRoom(roomID int64, bookingID int64) error
	GetAvailabilities(roomID int64, checkIn, checkOut time.Time) ([]models.RoomAvailability, error)
	CleanupExpiredReservations() error
}

type roomRepository struct {
	db *gorm.DB
}

// NewRoomRepository creates a new room repository instance
func NewRoomRepository(db *gorm.DB) RoomRepository {
	return &roomRepository{db: db}
}

// Create creates a new room
func (r *roomRepository) Create(room *models.Room) error {
	return r.db.Create(room).Error
}

// GetByID retrieves a room by ID with its relationships
func (r *roomRepository) GetByID(id int64) (*models.Room, error) {
	var room models.Room
	err := r.db.Preload("Amenities").Preload("Images").First(&room, id).Error
	if err != nil {
		return nil, err
	}
	return &room, nil
}

// GetByRoomNumber retrieves a room by room number
func (r *roomRepository) GetByRoomNumber(roomNumber string) (*models.Room, error) {
	var room models.Room
	err := r.db.Where("room_number = ?", roomNumber).First(&room).Error
	if err != nil {
		return nil, err
	}
	return &room, nil
}

// GetAll retrieves all rooms with pagination
func (r *roomRepository) GetAll(page, pageSize int) ([]models.Room, int64, error) {
	var rooms []models.Room
	var total int64

	offset := (page - 1) * pageSize

	if err := r.db.Model(&models.Room{}).Where("is_active = ?", true).Count(&total).Error; err != nil {
		return nil, 0, err
	}

	err := r.db.Preload("Amenities").Preload("Images").
		Where("is_active = ?", true).
		Offset(offset).
		Limit(pageSize).
		Find(&rooms).Error

	if err != nil {
		return nil, 0, err
	}

	return rooms, total, nil
}

// Update updates a room
func (r *roomRepository) Update(room *models.Room) error {
	return r.db.Session(&gorm.Session{FullSaveAssociations: true}).Save(room).Error
}

// Delete soft deletes a room
func (r *roomRepository) Delete(id int64) error {
	return r.db.Delete(&models.Room{}, id).Error
}

// Search searches for rooms based on criteria
func (r *roomRepository) Search(req *models.SearchRoomsRequest) ([]models.Room, int64, error) {
	query := r.db.Model(&models.Room{}).Where("is_active = ?", true)

	// Filter by room type
	if req.RoomType != "" {
		query = query.Where("room_type = ?", req.RoomType)
	}

	// Filter by minimum capacity
	totalGuests := req.Adults + req.Children
	if totalGuests > 0 {
		query = query.Where("capacity >= ?", totalGuests)
	} else if req.MinCapacity > 0 {
		query = query.Where("capacity >= ?", req.MinCapacity)
	}

	// Filter by price range
	if req.MinPrice != nil {
		query = query.Where("base_price >= ?", *req.MinPrice)
	}
	if req.MaxPrice != nil {
		query = query.Where("base_price <= ?", *req.MaxPrice)
	}

	// Filter by availability if dates are provided
	if req.CheckIn != nil && req.CheckOut != nil {
		// Subquery to find rooms that have conflicting reservations
		subQuery := r.db.Model(&models.RoomAvailability{}).
			Select("room_id").
			Where("status IN ?", []string{models.AvailabilityStatusReserved, models.AvailabilityStatusOccupied}).
			Where("(check_in_date < ? AND check_out_date > ?)", req.CheckOut, req.CheckIn)

		query = query.Where("id NOT IN (?)", subQuery)
	}

	// Filter by amenities
	if len(req.Amenities) > 0 {
		// Subquery to find rooms that have all requested amenities
		subQuery := r.db.Model(&models.RoomAmenity{}).
			Select("room_id").
			Where("name IN ?", req.Amenities).
			Group("room_id").
			Having("COUNT(DISTINCT name) = ?", len(req.Amenities))

		query = query.Where("id IN (?)", subQuery)
	}

	// Count total matching rooms
	var total int64
	if err := query.Count(&total).Error; err != nil {
		return nil, 0, err
	}

	// Apply sorting
	switch req.SortBy {
	case "price_asc":
		query = query.Order("base_price ASC")
	case "price_desc":
		query = query.Order("base_price DESC")
	case "capacity_asc":
		query = query.Order("capacity ASC")
	case "capacity_desc":
		query = query.Order("capacity DESC")
	default:
		query = query.Order("room_number ASC")
	}

	// Apply pagination
	page := req.Page
	if page < 1 {
		page = 1
	}
	pageSize := req.PageSize
	if pageSize < 1 {
		pageSize = 10
	}
	offset := (page - 1) * pageSize

	var rooms []models.Room
	err := query.Preload("Amenities").Preload("Images").
		Offset(offset).
		Limit(pageSize).
		Find(&rooms).Error

	if err != nil {
		return nil, 0, err
	}

	return rooms, total, nil
}

// CheckAvailability checks if a room is available for the given dates
func (r *roomRepository) CheckAvailability(roomID int64, checkIn, checkOut time.Time) (bool, error) {
	var count int64
	err := r.db.Model(&models.RoomAvailability{}).
		Where("room_id = ?", roomID).
		Where("status IN ?", []string{models.AvailabilityStatusReserved, models.AvailabilityStatusOccupied}).
		Where("check_in_date < ? AND check_out_date > ?", checkOut, checkIn).
		Count(&count).Error

	if err != nil {
		return false, err
	}

	return count == 0, nil
}

// ReserveRoom reserves a room for the given dates
func (r *roomRepository) ReserveRoom(roomID int64, checkIn, checkOut time.Time, bookingID int64, reservedUntil time.Time) (*models.RoomAvailability, error) {
	// First check if the room is available
	available, err := r.CheckAvailability(roomID, checkIn, checkOut)
	if err != nil {
		return nil, err
	}
	if !available {
		return nil, fmt.Errorf("room is not available for the selected dates")
	}

	// Create reservation
	availability := &models.RoomAvailability{
		RoomID:        roomID,
		CheckInDate:   checkIn,
		CheckOutDate:  checkOut,
		Status:        models.AvailabilityStatusReserved,
		BookingID:     &bookingID,
		ReservedUntil: &reservedUntil,
	}

	if err := r.db.Create(availability).Error; err != nil {
		return nil, err
	}

	return availability, nil
}

// ReleaseRoom releases a room reservation
func (r *roomRepository) ReleaseRoom(roomID int64, bookingID int64) error {
	return r.db.Where("room_id = ? AND booking_id = ?", roomID, bookingID).
		Delete(&models.RoomAvailability{}).Error
}

// GetAvailabilities retrieves availabilities for a room in a date range
func (r *roomRepository) GetAvailabilities(roomID int64, checkIn, checkOut time.Time) ([]models.RoomAvailability, error) {
	var availabilities []models.RoomAvailability
	err := r.db.Where("room_id = ?", roomID).
		Where("check_in_date < ? AND check_out_date > ?", checkOut, checkIn).
		Find(&availabilities).Error

	if err != nil {
		return nil, err
	}

	return availabilities, nil
}

// CleanupExpiredReservations removes expired temporary reservations
func (r *roomRepository) CleanupExpiredReservations() error {
	now := time.Now()
	return r.db.Where("status = ? AND reserved_until < ?", models.AvailabilityStatusReserved, now).
		Delete(&models.RoomAvailability{}).Error
}

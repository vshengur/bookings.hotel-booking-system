package models

import "time"

// CreateRoomRequest represents the request to create a new room
type CreateRoomRequest struct {
	RoomNumber  string             `json:"room_number" binding:"required,min=1,max=50"`
	RoomType    string             `json:"room_type" binding:"required,max=100"`
	Floor       int                `json:"floor" binding:"required"`
	Capacity    int                `json:"capacity" binding:"required,min=1,max=20"`
	BedType     string             `json:"bed_type" binding:"max=100"`
	Size        float64            `json:"size" binding:"min=0"`
	Description string             `json:"description"`
	Amenities   []AmenityRequest   `json:"amenities"`
	Images      []RoomImageRequest `json:"images"`
}

// UpdateRoomRequest represents the request to update a room
type UpdateRoomRequest struct {
	RoomNumber  *string            `json:"room_number,omitempty" binding:"omitempty,min=1,max=50"`
	RoomType    *string            `json:"room_type,omitempty" binding:"omitempty,max=100"`
	Floor       *int               `json:"floor,omitempty"`
	Capacity    *int               `json:"capacity,omitempty" binding:"omitempty,min=1,max=20"`
	BedType     *string            `json:"bed_type,omitempty" binding:"omitempty,max=100"`
	Size        *float64           `json:"size,omitempty" binding:"omitempty,min=0"`
	Description *string            `json:"description,omitempty"`
	IsActive    *bool              `json:"is_active,omitempty"`
	Amenities   []AmenityRequest   `json:"amenities,omitempty"`
	Images      []RoomImageRequest `json:"images,omitempty"`
}

// AmenityRequest represents an amenity in create/update requests
type AmenityRequest struct {
	AmenityType string `json:"amenity_type" binding:"required,max=100"`
	Name        string `json:"name" binding:"required,max=200"`
	Description string `json:"description"`
	IconURL     string `json:"icon_url" binding:"max=500"`
}

// RoomImageRequest represents an image in create/update requests
type RoomImageRequest struct {
	URL         string `json:"url" binding:"required,max=1000"`
	Title       string `json:"title" binding:"max=200"`
	Description string `json:"description"`
	IsPrimary   bool   `json:"is_primary"`
	SortOrder   int    `json:"sort_order"`
}

// SearchRoomsRequest represents the request to search for rooms
type SearchRoomsRequest struct {
	CheckIn     *time.Time `form:"checkIn" binding:"omitempty"`
	CheckOut    *time.Time `form:"checkOut" binding:"omitempty"`
	Adults      int        `form:"adults" binding:"omitempty,min=1"`
	Children    int        `form:"children" binding:"omitempty,min=0"`
	MinPrice    *float64   `form:"minPrice" binding:"omitempty,min=0"`
	MaxPrice    *float64   `form:"maxPrice" binding:"omitempty,min=0"`
	RoomType    string     `form:"roomType"`
	MinCapacity int        `form:"minCapacity" binding:"omitempty,min=1"`
	Amenities   []string   `form:"amenities"`
	Page        int        `form:"page" binding:"omitempty,min=1"`
	PageSize    int        `form:"pageSize" binding:"omitempty,min=1,max=100"`
	SortBy      string     `form:"sortBy"` // price_asc, price_desc, capacity_asc, capacity_desc
}

// CheckAvailabilityRequest represents the request to check room availability
type CheckAvailabilityRequest struct {
	RoomID   int64     `json:"room_id" binding:"required"`
	CheckIn  time.Time `json:"check_in" binding:"required"`
	CheckOut time.Time `json:"check_out" binding:"required"`
}

// ReserveRoomRequest represents the request to reserve a room
type ReserveRoomRequest struct {
	RoomID           int64     `json:"room_id" binding:"required"`
	CheckIn          time.Time `json:"check_in" binding:"required"`
	CheckOut         time.Time `json:"check_out" binding:"required"`
	BookingReference string    `json:"booking_reference" binding:"required"`
}

// ReleaseRoomRequest represents the request to release a room reservation
type ReleaseRoomRequest struct {
	RoomID           int64  `json:"room_id" binding:"required"`
	BookingReference string `json:"booking_reference" binding:"required"`
}

// RoomResponse represents the response for a room
type RoomResponse struct {
	ID          int64         `json:"id"`
	RoomNumber  string        `json:"room_number"`
	RoomType    string        `json:"room_type"`
	Floor       int           `json:"floor"`
	Capacity    int           `json:"capacity"`
	BedType     string        `json:"bed_type"`
	Size        float64       `json:"size"`
	Description string        `json:"description"`
	IsActive    bool          `json:"is_active"`
	Amenities   []RoomAmenity `json:"amenities"`
	Images      []RoomImage   `json:"images"`
	CreatedAt   time.Time     `json:"created_at"`
	UpdatedAt   time.Time     `json:"updated_at"`
}

// SearchRoomsResponse represents the response for room search
type SearchRoomsResponse struct {
	Rooms      []RoomResponse `json:"rooms"`
	TotalCount int64          `json:"total_count"`
	Page       int            `json:"page"`
	PageSize   int            `json:"page_size"`
	TotalPages int            `json:"total_pages"`
}

// AvailabilityResponse represents the response for availability check
// Note: Price information should be retrieved from Pricing Service
type AvailabilityResponse struct {
	RoomID      int64     `json:"room_id"`
	IsAvailable bool      `json:"is_available"`
	CheckIn     time.Time `json:"check_in"`
	CheckOut    time.Time `json:"check_out"`
}

// ReserveRoomResponse represents the response for room reservation
type ReserveRoomResponse struct {
	ReservationID    int64     `json:"reservation_id"`
	RoomID           int64     `json:"room_id"`
	BookingReference string    `json:"booking_reference"`
	CheckIn          time.Time `json:"check_in"`
	CheckOut         time.Time `json:"check_out"`
	ReservedUntil    time.Time `json:"reserved_until"`
	Status           string    `json:"status"`
}

// ErrorResponse represents an error response
type ErrorResponse struct {
	Error   string `json:"error"`
	Message string `json:"message"`
	Code    int    `json:"code"`
}

// SuccessResponse represents a generic success response
type SuccessResponse struct {
	Message string      `json:"message"`
	Data    interface{} `json:"data,omitempty"`
}

package models

import (
	"time"
)

// Room represents a hotel room entity
type Room struct {
	ID          int64      `json:"id" gorm:"primaryKey;autoIncrement"`
	RoomNumber  string     `json:"room_number" gorm:"uniqueIndex;not null;size:50"`
	RoomType    string     `json:"room_type" gorm:"not null;size:100"`
	Floor       int        `json:"floor" gorm:"not null"`
	Capacity    int        `json:"capacity" gorm:"not null"`
	BedType     string     `json:"bed_type" gorm:"size:100"`
	Size        float64    `json:"size" gorm:"type:decimal(10,2)"`
	Description string     `json:"description" gorm:"type:text"`
	BasePrice   float64    `json:"base_price" gorm:"type:decimal(10,2);not null"`
	Currency    string     `json:"currency" gorm:"size:3;default:'EUR'"`
	IsActive    bool       `json:"is_active" gorm:"default:true"`
	CreatedAt   time.Time  `json:"created_at" gorm:"autoCreateTime"`
	UpdatedAt   time.Time  `json:"updated_at" gorm:"autoUpdateTime"`
	DeletedAt   *time.Time `json:"deleted_at,omitempty" gorm:"index"`

	// Relationships
	Amenities      []RoomAmenity      `json:"amenities,omitempty" gorm:"foreignKey:RoomID;constraint:OnDelete:CASCADE"`
	Images         []RoomImage        `json:"images,omitempty" gorm:"foreignKey:RoomID;constraint:OnDelete:CASCADE"`
	Availabilities []RoomAvailability `json:"availabilities,omitempty" gorm:"foreignKey:RoomID;constraint:OnDelete:CASCADE"`
}

// TableName specifies the table name for Room model
func (Room) TableName() string {
	return "rooms"
}

// RoomAmenity represents amenities available in a room
type RoomAmenity struct {
	ID          int64     `json:"id" gorm:"primaryKey;autoIncrement"`
	RoomID      int64     `json:"room_id" gorm:"not null;index"`
	AmenityType string    `json:"amenity_type" gorm:"not null;size:100"`
	Name        string    `json:"name" gorm:"not null;size:200"`
	Description string    `json:"description" gorm:"type:text"`
	IconURL     string    `json:"icon_url" gorm:"size:500"`
	CreatedAt   time.Time `json:"created_at" gorm:"autoCreateTime"`
}

// TableName specifies the table name for RoomAmenity model
func (RoomAmenity) TableName() string {
	return "room_amenities"
}

// RoomImage represents images of a room
type RoomImage struct {
	ID          int64     `json:"id" gorm:"primaryKey;autoIncrement"`
	RoomID      int64     `json:"room_id" gorm:"not null;index"`
	URL         string    `json:"url" gorm:"not null;size:1000"`
	Title       string    `json:"title" gorm:"size:200"`
	Description string    `json:"description" gorm:"type:text"`
	IsPrimary   bool      `json:"is_primary" gorm:"default:false"`
	SortOrder   int       `json:"sort_order" gorm:"default:0"`
	CreatedAt   time.Time `json:"created_at" gorm:"autoCreateTime"`
}

// TableName specifies the table name for RoomImage model
func (RoomImage) TableName() string {
	return "room_images"
}

// RoomAvailability represents room availability and reservations
type RoomAvailability struct {
	ID            int64      `json:"id" gorm:"primaryKey;autoIncrement"`
	RoomID        int64      `json:"room_id" gorm:"not null;index:idx_room_dates"`
	CheckInDate   time.Time  `json:"check_in_date" gorm:"not null;index:idx_room_dates"`
	CheckOutDate  time.Time  `json:"check_out_date" gorm:"not null;index:idx_room_dates"`
	Status        string     `json:"status" gorm:"not null;size:50;default:'available'"` // available, reserved, blocked, occupied
	BookingID     *int64     `json:"booking_id,omitempty" gorm:"index"`
	ReservedUntil *time.Time `json:"reserved_until,omitempty"` // For temporary holds (15 min)
	CreatedAt     time.Time  `json:"created_at" gorm:"autoCreateTime"`
	UpdatedAt     time.Time  `json:"updated_at" gorm:"autoUpdateTime"`
}

// TableName specifies the table name for RoomAvailability model
func (RoomAvailability) TableName() string {
	return "room_availabilities"
}

// IsExpired checks if the reservation hold has expired
func (ra *RoomAvailability) IsExpired() bool {
	if ra.ReservedUntil == nil {
		return false
	}
	return time.Now().After(*ra.ReservedUntil)
}

// Availability status constants
const (
	AvailabilityStatusAvailable = "available"
	AvailabilityStatusReserved  = "reserved"
	AvailabilityStatusBlocked   = "blocked"
	AvailabilityStatusOccupied  = "occupied"
)

// Room type constants
const (
	RoomTypeSingle    = "single"
	RoomTypeDouble    = "double"
	RoomTypeTwin      = "twin"
	RoomTypeSuite     = "suite"
	RoomTypeDeluxe    = "deluxe"
	RoomTypePenthouse = "penthouse"
)

// Amenity type constants
const (
	AmenityTypeBasic       = "basic"
	AmenityTypeEntertainment = "entertainment"
	AmenityTypeBathroom    = "bathroom"
	AmenityTypeKitchen     = "kitchen"
	AmenityTypeView        = "view"
	AmenityTypeAccessibility = "accessibility"
)

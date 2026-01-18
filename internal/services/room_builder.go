package services

import (
	"github.com/vshengur/bookings.room-service/internal/models"
)

// RoomBuilder implements the Builder pattern for constructing Room objects
type RoomBuilder struct {
	room *models.Room
}

// NewRoomBuilder creates a new RoomBuilder instance
func NewRoomBuilder() *RoomBuilder {
	return &RoomBuilder{
		room: &models.Room{
			IsActive: true,
			Currency: "EUR",
		},
	}
}

// WithRoomNumber sets the room number
func (b *RoomBuilder) WithRoomNumber(roomNumber string) *RoomBuilder {
	b.room.RoomNumber = roomNumber
	return b
}

// WithRoomType sets the room type
func (b *RoomBuilder) WithRoomType(roomType string) *RoomBuilder {
	b.room.RoomType = roomType
	return b
}

// WithFloor sets the floor
func (b *RoomBuilder) WithFloor(floor int) *RoomBuilder {
	b.room.Floor = floor
	return b
}

// WithCapacity sets the capacity
func (b *RoomBuilder) WithCapacity(capacity int) *RoomBuilder {
	b.room.Capacity = capacity
	return b
}

// WithBedType sets the bed type
func (b *RoomBuilder) WithBedType(bedType string) *RoomBuilder {
	b.room.BedType = bedType
	return b
}

// WithSize sets the size
func (b *RoomBuilder) WithSize(size float64) *RoomBuilder {
	b.room.Size = size
	return b
}

// WithDescription sets the description
func (b *RoomBuilder) WithDescription(description string) *RoomBuilder {
	b.room.Description = description
	return b
}

// WithBasePrice sets the base price
func (b *RoomBuilder) WithBasePrice(basePrice float64) *RoomBuilder {
	b.room.BasePrice = basePrice
	return b
}

// WithCurrency sets the currency
func (b *RoomBuilder) WithCurrency(currency string) *RoomBuilder {
	if currency != "" {
		b.room.Currency = currency
	}
	return b
}

// WithIsActive sets the active status
func (b *RoomBuilder) WithIsActive(isActive bool) *RoomBuilder {
	b.room.IsActive = isActive
	return b
}

// AddAmenity adds an amenity to the room
func (b *RoomBuilder) AddAmenity(amenity models.RoomAmenity) *RoomBuilder {
	b.room.Amenities = append(b.room.Amenities, amenity)
	return b
}

// AddImage adds an image to the room
func (b *RoomBuilder) AddImage(image models.RoomImage) *RoomBuilder {
	b.room.Images = append(b.room.Images, image)
	return b
}

// Build constructs and returns the final Room object
func (b *RoomBuilder) Build() *models.Room {
	return b.room
}

// Reset resets the builder to start building a new room
func (b *RoomBuilder) Reset() *RoomBuilder {
	b.room = &models.Room{
		IsActive: true,
		Currency: "EUR",
	}
	return b
}

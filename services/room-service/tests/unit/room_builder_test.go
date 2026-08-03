package unit

import (
	"testing"

	"github.com/vshengur/bookings.room-service/internal/services"
)

func TestRoomBuilder(t *testing.T) {
	t.Run("Build room with all fields", func(t *testing.T) {
		builder := services.NewRoomBuilder()
		room := builder.
			WithRoomNumber("101").
			WithRoomType("single").
			WithFloor(1).
			WithCapacity(2).
			WithBedType("Queen Bed").
			WithSize(25.5).
			WithDescription("Test room").
			WithBasePrice(99.99).
			WithCurrency("EUR").
			WithIsActive(true).
			Build()

		if room.RoomNumber != "101" {
			t.Errorf("Expected room number 101, got %s", room.RoomNumber)
		}
		if room.RoomType != "single" {
			t.Errorf("Expected room type single, got %s", room.RoomType)
		}
		if room.Floor != 1 {
			t.Errorf("Expected floor 1, got %d", room.Floor)
		}
		if room.Capacity != 2 {
			t.Errorf("Expected capacity 2, got %d", room.Capacity)
		}
		if room.BasePrice != 99.99 {
			t.Errorf("Expected base price 99.99, got %f", room.BasePrice)
		}
		if room.Currency != "EUR" {
			t.Errorf("Expected currency EUR, got %s", room.Currency)
		}
		if !room.IsActive {
			t.Error("Expected room to be active")
		}
	})

	t.Run("Build room with defaults", func(t *testing.T) {
		builder := services.NewRoomBuilder()
		room := builder.
			WithRoomNumber("102").
			WithRoomType("double").
			WithFloor(2).
			WithCapacity(2).
			WithBasePrice(129.99).
			Build()

		if room.Currency != "EUR" {
			t.Errorf("Expected default currency EUR, got %s", room.Currency)
		}
		if !room.IsActive {
			t.Error("Expected room to be active by default")
		}
	})

	t.Run("Reset builder", func(t *testing.T) {
		builder := services.NewRoomBuilder()

		// Build first room
		room1 := builder.
			WithRoomNumber("103").
			WithRoomType("suite").
			WithFloor(3).
			WithCapacity(3).
			WithBasePrice(249.99).
			Build()

		// Reset and build second room
		room2 := builder.Reset().
			WithRoomNumber("104").
			WithRoomType("single").
			WithFloor(1).
			WithCapacity(1).
			WithBasePrice(89.99).
			Build()

		if room1.RoomNumber == room2.RoomNumber {
			t.Error("Expected different room numbers after reset")
		}
		if room2.RoomNumber != "104" {
			t.Errorf("Expected room number 104, got %s", room2.RoomNumber)
		}
	})
}

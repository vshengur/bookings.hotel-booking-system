package unit

import (
	"math"
	"testing"
	"time"

	"github.com/vshengur/bookings.room-service/internal/services"
)

func TestBasePricingStrategy(t *testing.T) {
	strategy := services.NewBasePricingStrategy()
	basePrice := 100.0
	nights := 3

	checkIn := time.Date(2026, 2, 1, 14, 0, 0, 0, time.UTC)
	checkOut := checkIn.Add(time.Duration(nights) * 24 * time.Hour)

	totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
	expected := 300.0

	if totalPrice != expected {
		t.Errorf("Expected total price %f, got %f", expected, totalPrice)
	}
}

func TestSeasonalPricingStrategy(t *testing.T) {
	weekendMultiplier := 1.2
	highSeasonMultiplier := 1.5
	strategy := services.NewSeasonalPricingStrategy(weekendMultiplier, highSeasonMultiplier)

	t.Run("Weekday in regular season", func(t *testing.T) {
		basePrice := 100.0
		// Monday to Tuesday (1 night)
		checkIn := time.Date(2026, 3, 2, 14, 0, 0, 0, time.UTC)  // Monday
		checkOut := time.Date(2026, 3, 3, 10, 0, 0, 0, time.UTC) // Tuesday
		nights := 1

		totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
		expected := 100.0 // No multipliers

		if totalPrice != expected {
			t.Errorf("Expected total price %f, got %f", expected, totalPrice)
		}
	})

	t.Run("Weekend pricing", func(t *testing.T) {
		basePrice := 100.0
		// Friday to Saturday (1 night)
		checkIn := time.Date(2026, 3, 6, 14, 0, 0, 0, time.UTC)  // Friday
		checkOut := time.Date(2026, 3, 7, 10, 0, 0, 0, time.UTC) // Saturday
		nights := 1

		totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
		expected := 120.0 // 100 * 1.2 (weekend)

		if totalPrice != expected {
			t.Errorf("Expected total price %f, got %f", expected, totalPrice)
		}
	})

	t.Run("High season pricing (Summer)", func(t *testing.T) {
		basePrice := 100.0
		// July 1-2 (1 night, summer)
		checkIn := time.Date(2026, 7, 1, 14, 0, 0, 0, time.UTC)
		checkOut := time.Date(2026, 7, 2, 10, 0, 0, 0, time.UTC)
		nights := 1

		totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
		expected := 150.0 // 100 * 1.5 (high season)

		if totalPrice != expected {
			t.Errorf("Expected total price %f, got %f", expected, totalPrice)
		}
	})

	t.Run("Weekend in high season", func(t *testing.T) {
		basePrice := 100.0
		// Friday in July (weekend + high season)
		checkIn := time.Date(2026, 7, 3, 14, 0, 0, 0, time.UTC)  // Friday
		checkOut := time.Date(2026, 7, 4, 10, 0, 0, 0, time.UTC) // Saturday
		nights := 1

		totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
		expected := 180.0 // 100 * 1.2 (weekend) * 1.5 (high season)

		if totalPrice != expected {
			t.Errorf("Expected total price %f, got %f", expected, totalPrice)
		}
	})
}

func TestPromotionalPricingStrategy(t *testing.T) {
	baseStrategy := services.NewBasePricingStrategy()
	discountPercent := 10.0
	minNights := 5

	strategy := services.NewPromotionalPricingStrategy(baseStrategy, discountPercent, minNights)

	t.Run("No discount for short stays", func(t *testing.T) {
		basePrice := 100.0
		nights := 3
		checkIn := time.Date(2026, 2, 1, 14, 0, 0, 0, time.UTC)
		checkOut := checkIn.Add(time.Duration(nights) * 24 * time.Hour)

		totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
		expected := 300.0 // No discount

		if totalPrice != expected {
			t.Errorf("Expected total price %f, got %f", expected, totalPrice)
		}
	})

	t.Run("Discount for extended stays", func(t *testing.T) {
		basePrice := 100.0
		nights := 7
		checkIn := time.Date(2026, 2, 1, 14, 0, 0, 0, time.UTC)
		checkOut := checkIn.Add(time.Duration(nights) * 24 * time.Hour)

		totalPrice := strategy.CalculatePrice(basePrice, nights, checkIn, checkOut)
		expected := 630.0 // 700 - 10% = 630

		if math.Abs(totalPrice-expected) > 0.01 {
			t.Errorf("Expected total price %f, got %f", expected, totalPrice)
		}
	})
}

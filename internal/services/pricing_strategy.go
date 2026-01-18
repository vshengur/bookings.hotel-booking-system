package services

import (
	"time"
)

// PricingStrategy defines the Strategy pattern interface for pricing calculation
type PricingStrategy interface {
	CalculatePrice(basePrice float64, nights int, checkIn, checkOut time.Time) float64
}

// BasePricingStrategy implements simple base price calculation
type BasePricingStrategy struct{}

// NewBasePricingStrategy creates a new base pricing strategy
func NewBasePricingStrategy() PricingStrategy {
	return &BasePricingStrategy{}
}

// CalculatePrice calculates the total price based on base price and nights
func (s *BasePricingStrategy) CalculatePrice(basePrice float64, nights int, checkIn, checkOut time.Time) float64 {
	return basePrice * float64(nights)
}

// SeasonalPricingStrategy implements seasonal pricing with weekend and high season adjustments
type SeasonalPricingStrategy struct {
	weekendMultiplier   float64
	highSeasonMultiplier float64
}

// NewSeasonalPricingStrategy creates a new seasonal pricing strategy
func NewSeasonalPricingStrategy(weekendMultiplier, highSeasonMultiplier float64) PricingStrategy {
	return &SeasonalPricingStrategy{
		weekendMultiplier:   weekendMultiplier,
		highSeasonMultiplier: highSeasonMultiplier,
	}
}

// CalculatePrice calculates price with seasonal adjustments
func (s *SeasonalPricingStrategy) CalculatePrice(basePrice float64, nights int, checkIn, checkOut time.Time) float64 {
	totalPrice := 0.0

	current := checkIn
	for i := 0; i < nights; i++ {
		dayPrice := basePrice

		// Apply weekend pricing (Friday, Saturday)
		if current.Weekday() == time.Friday || current.Weekday() == time.Saturday {
			dayPrice *= s.weekendMultiplier
		}

		// Apply high season pricing (June-August, December)
		month := current.Month()
		if month >= time.June && month <= time.August || month == time.December {
			dayPrice *= s.highSeasonMultiplier
		}

		totalPrice += dayPrice
		current = current.Add(24 * time.Hour)
	}

	return totalPrice
}

// DynamicPricingStrategy implements dynamic pricing based on occupancy
type DynamicPricingStrategy struct {
	occupancyThresholds map[int]float64 // occupancy percentage -> multiplier
}

// NewDynamicPricingStrategy creates a new dynamic pricing strategy
func NewDynamicPricingStrategy() PricingStrategy {
	return &DynamicPricingStrategy{
		occupancyThresholds: map[int]float64{
			90: 1.5,  // 90%+ occupancy: 1.5x price
			75: 1.3,  // 75%+ occupancy: 1.3x price
			50: 1.1,  // 50%+ occupancy: 1.1x price
			0:  1.0,  // < 50% occupancy: base price
		},
	}
}

// CalculatePrice calculates price based on dynamic factors
// Note: In a real implementation, this would query current occupancy from the database
func (s *DynamicPricingStrategy) CalculatePrice(basePrice float64, nights int, checkIn, checkOut time.Time) float64 {
	// Simplified implementation - would need actual occupancy data
	multiplier := 1.0

	// This is a placeholder - in production, you would:
	// 1. Query current occupancy for the date range
	// 2. Apply appropriate multiplier based on occupancy
	// 3. Consider historical booking patterns

	return basePrice * float64(nights) * multiplier
}

// PromotionalPricingStrategy implements promotional discounts
type PromotionalPricingStrategy struct {
	baseStrategy     PricingStrategy
	discountPercent  float64
	minNightsForDiscount int
}

// NewPromotionalPricingStrategy creates a new promotional pricing strategy (Decorator pattern)
func NewPromotionalPricingStrategy(baseStrategy PricingStrategy, discountPercent float64, minNights int) PricingStrategy {
	return &PromotionalPricingStrategy{
		baseStrategy:     baseStrategy,
		discountPercent:  discountPercent,
		minNightsForDiscount: minNights,
	}
}

// CalculatePrice calculates price with promotional discounts
func (s *PromotionalPricingStrategy) CalculatePrice(basePrice float64, nights int, checkIn, checkOut time.Time) float64 {
	totalPrice := s.baseStrategy.CalculatePrice(basePrice, nights, checkIn, checkOut)

	// Apply discount for extended stays
	if nights >= s.minNightsForDiscount {
		discount := totalPrice * (s.discountPercent / 100.0)
		totalPrice -= discount
	}

	return totalPrice
}

package domain

import (
	"time"
)

// Strategy
type TariffStrategy interface {
	DailyPrice(roomID string, date time.Time, adults, children int, occupancy float64) Money
	Code() string
}

// Fixed strategy: constant base price per room per day
type FixedStrategy struct {
	Base Money
}

func (s FixedStrategy) DailyPrice(_ string, _ time.Time, _ int, _ int, _ float64) Money {
	return s.Base
}
func (s FixedStrategy) Code() string { return "fixed" }

// Occupancy-based: price scales by occupancy (0..1), e.g., base + occupancy*markup
type OccupancyBasedStrategy struct {
	Base   Money
	Markup int64 // minor units added at 100% occupancy (linear)
}

func (s OccupancyBasedStrategy) DailyPrice(_ string, _ time.Time, _ int, _ int, occupancy float64) Money {
	if occupancy < 0 { occupancy = 0 }
	if occupancy > 1 { occupancy = 1 }
	add := int64(float64(s.Markup) * occupancy)
	return Money{Currency: s.Base.Currency, AmountMinor: s.Base.AmountMinor + add}
}
func (s OccupancyBasedStrategy) Code() string { return "occupancy" }

package domain

import (
	"testing"
	"time"
)

func TestOccupancyBased(t *testing.T) {
	s := OccupancyBasedStrategy{ Base: Money{Currency:"EUR", AmountMinor:10000}, Markup: 5000 }
	p := s.DailyPrice("R1", time.Now(), 2, 0, 0.6)
	if p.AmountMinor <= 10000 { t.Fatalf("expected markup applied") }
}

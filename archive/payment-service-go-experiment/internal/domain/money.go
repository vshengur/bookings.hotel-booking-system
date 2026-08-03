package domain

import "fmt"

type Money struct {
	Currency string
	AmountMinor int64 // minor units (e.g., cents)
}

func (m Money) Add(n Money) Money {
	if m.Currency != n.Currency { panic("currency mismatch") }
	return Money{Currency: m.Currency, AmountMinor: m.AmountMinor + n.AmountMinor}
}

func (m Money) String() string {
	return fmt.Sprintf("%s %.2f", m.Currency, float64(m.AmountMinor)/100.0)
}

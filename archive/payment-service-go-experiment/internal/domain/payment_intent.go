package domain

import "time"

type PaymentStatus string
const (
	StatusPending  PaymentStatus = "Pending"
	StatusSucceeded PaymentStatus = "Succeeded"
	StatusFailed    PaymentStatus = "Failed"
	StatusRefunded  PaymentStatus = "Refunded"
)

type PaymentIntent struct {
	ID          string
	BookingID   string
	RoomID      string
	Amount      Money
	Currency    string
	CustomerEmail string
	PSPRef      string
	Status      PaymentStatus
	CreatedAt   time.Time
	UpdatedAt   time.Time
}

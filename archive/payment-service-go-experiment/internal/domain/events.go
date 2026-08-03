package domain

import "time"

// Events published to RabbitMQ
type PaymentIntentCreated struct {
	IntentID   string     `json:"intentId"`
	BookingID  string     `json:"bookingId"`
	RoomID     string     `json:"roomId"`
	Amount     Money      `json:"amount"`
	OccurredAt time.Time  `json:"occurredAt"`
}

type PaymentSucceeded struct {
	IntentID   string     `json:"intentId"`
	BookingID  string     `json:"bookingId"`
	PSPRef     string     `json:"pspRef"`
	OccurredAt time.Time  `json:"occurredAt"`
}

type PaymentFailed struct {
	IntentID   string     `json:"intentId"`
	BookingID  string     `json:"bookingId"`
	Reason     string     `json:"reason"`
	OccurredAt time.Time  `json:"occurredAt"`
}

type PriceListUpdated struct {
	RoomID     string     `json:"roomId"`
	FromDate   string     `json:"fromDate"`
	ToDate     string     `json:"toDate"`
	OccurredAt time.Time  `json:"occurredAt"`
}

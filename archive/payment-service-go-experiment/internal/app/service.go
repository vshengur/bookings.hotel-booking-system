package app

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"time"

	"payment-service/internal/logging"
	"payment-service/internal/domain"
)

// Ports
type PriceRepo interface {
	GetDailyPrice(ctx context.Context, roomID string, date time.Time) (domain.Money, bool, error)
	SetDailyPrice(ctx context.Context, roomID string, date time.Time, m domain.Money) error
	ListRooms(ctx context.Context) ([]string, error)
}

type IntentRepo interface {
	CreateIntent(ctx context.Context, pi *domain.PaymentIntent) error
	UpdateIntentStatus(ctx context.Context, intentID string, status domain.PaymentStatus, pspRef string) error
	GetIntent(ctx context.Context, intentID string) (*domain.PaymentIntent, error)
}

type Cache interface {
	GetQuote(ctx context.Context, key string) (domain.Money, bool)
	SetQuote(ctx context.Context, key string, val domain.Money, ttl time.Duration)
	Close()
}

type Events interface {
	Publish(exchange string, routingKey string, payload any) error
	Close()
}

type PSP interface {
	CreatePayment(ctx context.Context, pi domain.PaymentIntent) (redirectURL string, pspRef string, err error)
	Refund(ctx context.Context, pi domain.PaymentIntent, amountMinor int64) (refundID string, err error)
	VerifySignature(rawBody []byte, headerSig string) bool
}

type Service struct {
	log    *logging.Logger
	prices PriceRepo
	intents IntentRepo
	cache  Cache
	events Events
	psp    PSP
	strategy domain.TariffStrategy
}

func NewService(log *logging.Logger, repo PriceRepo, cache Cache, events Events, psp PSP) *Service {
	// default strategy could be occupancy-based with simple parameters
	strategy := domain.OccupancyBasedStrategy{ Base: domain.Money{Currency: "EUR", AmountMinor: 10000}, Markup: 5000 }
	return &Service{log: log, prices: repo, cache: cache, events: events, psp: psp, strategy: strategy}
}

// 3.1 Quote
func (s *Service) Quote(ctx context.Context, roomID string, from, to time.Time, adults, children int) (domain.Money, []struct{Date time.Time; Price domain.Money}, error) {
	if !to.After(from) { return domain.Money{}, nil, errors.New("invalid date range") }
	key := fmt.Sprintf("%s:%s:%s:A%d:C%d", roomID, from.Format("2006-01-02"), to.Format("2006-01-02"), adults, children)
	if v, ok := s.cache.GetQuote(ctx, key); ok {
		return v, nil, nil
	}
	total := domain.Money{Currency: "EUR", AmountMinor: 0}
	var daily []struct{Date time.Time; Price domain.Money}
	for d := from; d.Before(to); d = d.Add(24*time.Hour) {
		if price, ok, err := s.prices.GetDailyPrice(ctx, roomID, d); err == nil && ok {
			total = total.Add(price)
			daily = append(daily, struct{Date time.Time; Price domain.Money}{Date: d, Price: price})
			continue
		}
		// naive occupancy = 0.5 (stub), in production query availability-service
		p := s.strategy.DailyPrice(roomID, d, adults, children, 0.5)
		_ = s.prices.SetDailyPrice(ctx, roomID, d, p)
		total = total.Add(p)
		daily = append(daily, struct{Date time.Time; Price domain.Money}{Date: d, Price: p})
	}
	s.cache.SetQuote(ctx, key, total, 10*time.Minute)
	return total, daily, nil
}

// 3.2 Create Intent
func (s *Service) CreateIntent(ctx context.Context, bookingID, roomID, email string, amount domain.Money) (intentID string, redirectURL string, err error) {
	pi := &domain.PaymentIntent{
		ID: generateID(), BookingID: bookingID, RoomID: roomID, Amount: amount, Currency: amount.Currency,
		CustomerEmail: email, Status: domain.StatusPending, CreatedAt: time.Now().UTC(), UpdatedAt: time.Now().UTC(),
	}
	if err = s.intents.CreateIntent(ctx, pi); err != nil { return "", "", err }
	url, pspRef, err := s.psp.CreatePayment(ctx, *pi)
	if err != nil { return "", "", err }
	_ = s.intents.UpdateIntentStatus(ctx, pi.ID, domain.StatusPending, pspRef)
	_ = s.events.Publish("Bookings.Contracts:PaymentIntentCreated", "", domain.PaymentIntentCreated{
		IntentID: pi.ID, BookingID: bookingID, RoomID: roomID, Amount: amount, OccurredAt: time.Now().UTC(),
	})
	return pi.ID, url, nil
}

// 3.3 Webhook processing (capture/verify)
func (s *Service) HandlePSPWebhook(ctx context.Context, rawBody []byte, sig string) error {
	if !s.psp.VerifySignature(rawBody, sig) {
		return errors.New("invalid signature")
	}
	type payload struct {
		Status string `json:"status"`
		IntentID string `json:"intentId"`
		PSPRef string `json:"pspRef"`
		Reason string `json:"reason"`
	}
	var p payload
	if err := json.Unmarshal(rawBody, &p); err != nil { return err }
	switch p.Status {
	case "succeeded":
		_ = s.intents.UpdateIntentStatus(ctx, p.IntentID, domain.StatusSucceeded, p.PSPRef)
		_ = s.events.Publish("Bookings.Contracts:PaymentSucceeded", "", domain.PaymentSucceeded{
			IntentID: p.IntentID, BookingID: "", PSPRef: p.PSPRef, OccurredAt: time.Now().UTC(),
		})
	case "failed":
		_ = s.intents.UpdateIntentStatus(ctx, p.IntentID, domain.StatusFailed, p.PSPRef)
		_ = s.events.Publish("Bookings.Contracts:PaymentFailed", "", domain.PaymentFailed{
			IntentID: p.IntentID, BookingID: "", Reason: p.Reason, OccurredAt: time.Now().UTC(),
		})
	default:
		return errors.New("unknown status")
	}
	return nil
}

// 3.4 Refund (skeleton)
func (s *Service) Refund(ctx context.Context, bookingID string, amountMinor int64) (string, error) {
	return "", errors.New("not implemented: link booking->intent lookup")
}

// 3.5 Recompute price list (batch 30 days ahead)
func (s *Service) RecomputePriceList(ctx context.Context, horizonDays int) {
	rooms, err := s.prices.ListRooms(ctx)
	if err != nil { s.log.Error("list rooms", logging.F("err", err)); return }
	from := time.Now().UTC().Truncate(24*time.Hour)
	to := from.Add(time.Duration(horizonDays)*24*time.Hour)
	for _, r := range rooms {
		for d := from; d.Before(to); d = d.Add(24*time.Hour) {
			p := s.strategy.DailyPrice(r, d, 2, 0, 0.5) // naive occupancy
			if err := s.prices.SetDailyPrice(ctx, r, d, p); err != nil {
				s.log.Error("set price", logging.F("room", r), logging.F("err", err))
			}
		}
		_ = s.events.Publish("Prices.Contracts:PriceListUpdated", "", domain.PriceListUpdated{
			RoomID: r, FromDate: from.Format("2006-01-02"), ToDate: to.Format("2006-01-02"), OccurredAt: time.Now().UTC(),
		})
	}
}

// helpers

func generateID() string {
	return fmt.Sprintf("pi_%d", time.Now().UnixNano())
}

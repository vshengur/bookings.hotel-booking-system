package postgres

import (
	"context"
	"errors"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
	"payment-service/internal/domain"
)

type Postgres struct {
	pool *pgxpool.Pool
}

func NewPostgres(ctx context.Context, url string) (*Postgres, error) {
	cfg, err := pgxpool.ParseConfig(url)
	if err != nil { return nil, err }
	pool, err := pgxpool.NewWithConfig(ctx, cfg)
	if err != nil { return nil, err }
	return &Postgres{pool: pool}, nil
}

func (p *Postgres) Close(ctx context.Context) { p.pool.Close() }

// PriceRepo
func (p *Postgres) GetDailyPrice(ctx context.Context, roomID string, date time.Time) (domain.Money, bool, error) {
	var amount int64
	var currency string
	err := p.pool.QueryRow(ctx, `SELECT amount_minor, currency FROM price_list WHERE room_id=$1 AND day=$2`, roomID, date).Scan(&amount, &currency)
	if err != nil {
		if errors.Is(err, context.Canceled) { return domain.Money{}, false, err }
		return domain.Money{}, false, nil
	}
	return domain.Money{Currency: currency, AmountMinor: amount}, true, nil
}

func (p *Postgres) SetDailyPrice(ctx context.Context, roomID string, date time.Time, m domain.Money) error {
	_, err := p.pool.Exec(ctx, `INSERT INTO price_list(room_id, day, amount_minor, currency)
		VALUES($1,$2,$3,$4)
		ON CONFLICT(room_id, day) DO UPDATE SET amount_minor=EXCLUDED.amount_minor, currency=EXCLUDED.currency`, roomID, date, m.AmountMinor, m.Currency)
	return err
}

func (p *Postgres) ListRooms(ctx context.Context) ([]string, error) {
	rows, err := p.pool.Query(ctx, `SELECT DISTINCT room_id FROM price_list LIMIT 1000`)
	if err != nil { return nil, err }
	defer rows.Close()
	var out []string
	for rows.Next() {
		var r string
		_ = rows.Scan(&r)
		out = append(out, r)
	}
	if len(out) == 0 {
		out = []string{"R1","R2"}
	}
	return out, nil
}

// IntentRepo
func (p *Postgres) CreateIntent(ctx context.Context, pi *domain.PaymentIntent) error {
	_, err := p.pool.Exec(ctx, `INSERT INTO payment_intent(id, booking_id, room_id, amount_minor, currency, customer_email, psp_ref, status, created_at, updated_at)
		VALUES($1,$2,$3,$4,$5,$6,$7,$8,$9,$10)`,
		pi.ID, pi.BookingID, pi.RoomID, pi.Amount.AmountMinor, pi.Currency, pi.CustomerEmail, pi.PSPRef, string(pi.Status), pi.CreatedAt, pi.UpdatedAt)
	return err
}

func (p *Postgres) UpdateIntentStatus(ctx context.Context, intentID string, status domain.PaymentStatus, pspRef string) error {
	_, err := p.pool.Exec(ctx, `UPDATE payment_intent SET status=$2, psp_ref=$3, updated_at=NOW() WHERE id=$1`, intentID, string(status), pspRef)
	return err
}

func (p *Postgres) GetIntent(ctx context.Context, intentID string) (*domain.PaymentIntent, error) {
	pi := &domain.PaymentIntent{}
	err := p.pool.QueryRow(ctx, `SELECT id, booking_id, room_id, amount_minor, currency, customer_email, psp_ref, status, created_at, updated_at
		FROM payment_intent WHERE id=$1`, intentID).
		Scan(&pi.ID, &pi.BookingID, &pi.RoomID, &pi.Amount.AmountMinor, &pi.Currency, &pi.CustomerEmail, &pi.PSPRef, &pi.Status, &pi.CreatedAt, &pi.UpdatedAt)
	if err != nil { return nil, err }
	pi.Amount.Currency = pi.Currency
	return pi, nil
}

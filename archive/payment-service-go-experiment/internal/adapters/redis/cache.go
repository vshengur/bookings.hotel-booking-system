package redisad

import (
	"context"
	"fmt"
	"time"

	"github.com/redis/go-redis/v9"
	"payment-service/internal/domain"
)

type Cache struct { client *redis.Client }

func NewRedis(url string) *Cache {
	opt, _ := redis.ParseURL(url)
	return &Cache{ client: redis.NewClient(opt) }
}

func (c *Cache) Close() { _ = c.client.Close() }

func (c *Cache) GetQuote(ctx context.Context, key string) (domain.Money, bool) {
	var m domain.Money
	v, err := c.client.HGetAll(ctx, "quote:"+key).Result()
	if err != nil || len(v) == 0 { return domain.Money{}, false }
	m.Currency = v["currency"]
	if v["amount"] == "" { return domain.Money{}, false }
	var amt int64; _, _ = fmt.Sscanf(v["amount"], "%d", &amt)
	m.AmountMinor = amt
	return m, true
}

func (c *Cache) SetQuote(ctx context.Context, key string, val domain.Money, ttl time.Duration) {
	_ = c.client.HSet(ctx, "quote:"+key, map[string]string{
		"currency": val.Currency,
		"amount": fmt.Sprintf("%d", val.AmountMinor),
	}).Err()
	_ = c.client.Expire(ctx, "quote:"+key, ttl).Err()
}

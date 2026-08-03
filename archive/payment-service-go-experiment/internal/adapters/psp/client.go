package psp

import (
	"context"
	"crypto/hmac"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"bytes"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/hashicorp/go-retryablehttp"
	"github.com/sony/gobreaker"
	"payment-service/internal/domain"
	"payment-service/internal/logging"
)

type Client struct {
	base string
	secret string
	log *logging.Logger
	h *http.Client
	cb *gobreaker.CircuitBreaker
}

func NewClient(baseURL, secret string, log *logging.Logger) *Client {
	rh := retryablehttp.NewClient()
	rh.RetryMax = 3
	rh.RetryWaitMin = 200 * time.Millisecond
	rh.RetryWaitMax = 1500 * time.Millisecond
	cb := gobreaker.NewCircuitBreaker(gobreaker.Settings{Name: "psp", Timeout: 5*time.Second})
	return &Client{
		base: baseURL, secret: secret, log: log,
		h: rh.StandardClient(), cb: cb,
	}
}

func (c *Client) CreatePayment(ctx context.Context, pi domain.PaymentIntent) (string, string, error) {
	// Builder pattern for PSP payload
	payload := NewPayloadBuilder().
		WithAmount(pi.Amount.AmountMinor, pi.Currency).
		WithCustomer(pi.CustomerEmail).
		WithReference(pi.ID).
		Build()

	b, _ := json.Marshal(payload)
	url := c.base + "/payments"
	resI, err := c.cb.Execute(func() (any, error) {
		req, _ := http.NewRequestWithContext(ctx, http.MethodPost, url, bytes.NewReader(b))
		req.Header.Set("Content-Type", "application/json")
		resp, err := c.h.Do(req)
		if err != nil { return nil, err }
		defer resp.Body.Close()
		if resp.StatusCode >= 500 { return nil, fmt.Errorf("psp %d", resp.StatusCode) }
		bb, _ := io.ReadAll(resp.Body)
		var out struct{ RedirectURL, PSPRef string }
		_ = json.Unmarshal(bb, &out)
		if out.RedirectURL == "" { out.RedirectURL = "https://psp.example/redirect/"+pi.ID }
		if out.PSPRef == "" { out.PSPRef = "psp_"+pi.ID }
		return out, nil
	})
	if err != nil { return "", "", err }
	out := resI.(struct{ RedirectURL, PSPRef string })
	return out.RedirectURL, out.PSPRef, nil
}

func (c *Client) Refund(ctx context.Context, pi domain.PaymentIntent, amountMinor int64) (string, error) {
	// Simplified
	return "refund_"+pi.ID, nil
}

func (c *Client) VerifySignature(raw []byte, sig string) bool {
	h := hmac.New(sha256.New, []byte(c.secret))
	h.Write(raw)
	expected := hex.EncodeToString(h.Sum(nil))
	return hmac.Equal([]byte(expected), []byte(sig))
}

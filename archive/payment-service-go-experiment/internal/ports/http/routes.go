package httpports

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/go-chi/chi/v5"
	"payment-service/internal/app"
	"payment-service/internal/config"
	"payment-service/internal/logging"
	"payment-service/internal/metrics"
)

func RegisterRoutes(r *chi.Mux, svc *app.Service, log *logging.Logger, cfg config.Config) {
	r.Get("/healthz", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		_, _ = w.Write([]byte("ok"))
	})

	r.Handle("/metrics", metrics.Handler())

	r.Get("/payment/quote", func(w http.ResponseWriter, r *http.Request) {
		roomID := r.URL.Query().Get("roomId")
		start := r.URL.Query().Get("start")
		end := r.URL.Query().Get("end")
		adults := atoiDefault(r.URL.Query().Get("adults"), 2)
		children := atoiDefault(r.URL.Query().Get("children"), 0)
		from, err1 := time.Parse("2006-01-02", start)
		to, err2 := time.Parse("2006-01-02", end)
		if roomID == "" || err1 != nil || err2 != nil {
			respondJSON(w, http.StatusBadRequest, map[string]any{"error":"invalid params"}); return
		}
		total, _, err := svc.Quote(r.Context(), roomID, from, to, adults, children)
		if err != nil { respondJSON(w, http.StatusBadRequest, map[string]any{"error": err.Error()}); return }
		respondJSON(w, http.StatusOK, map[string]any{"roomId": roomID, "total": total})
	})

	r.Post("/payment/webhook", func(w http.ResponseWriter, r *http.Request) {
		b, _ := io.ReadAll(r.Body)
		sig := r.Header.Get("X-PSP-Signature")
		if err := svc.HandlePSPWebhook(r.Context(), b, sig); err != nil {
			log.Error("webhook", logging.F("err", err))
			respondJSON(w, http.StatusBadRequest, map[string]any{"error": err.Error()}); return
		}
		respondJSON(w, http.StatusOK, map[string]any{"ok": true})
	})
}

func atoiDefault(s string, def int) int {
	var v int
	_, err := fmt.Sscanf(s, "%d", &v)
	if err != nil { return def }
	return v
}

func respondJSON(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(v)
}

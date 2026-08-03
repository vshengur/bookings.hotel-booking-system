package main

import (
	"context"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"payment-service/internal/config"
	"payment-service/internal/logging"
	pgrepo "payment-service/internal/adapters/postgres"
	redisad "payment-service/internal/adapters/redis"
	rabbit "payment-service/internal/adapters/rabbitmq"
	"payment-service/internal/adapters/psp"
	"payment-service/internal/app"
	"payment-service/internal/ports/http"
	"payment-service/internal/scheduler"
	"payment-service/internal/metrics"

	"github.com/go-chi/chi/v5"
)

func main() {
	cfg := config.Load()
	logger := logging.NewLogger(cfg.Env)

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Infra adapters
	pg, err := pgrepo.NewPostgres(ctx, cfg.PostgresURL)
	if err != nil { logger.Fatal("postgres connect", logging.F("err", err)) }
	defer pg.Close(ctx)

	cache := redisad.NewRedis(cfg.RedisURL)
	defer cache.Close()

	events, err := rabbit.NewPublisher(cfg.RabbitURL, logger)
	if err != nil { logger.Fatal("rabbitmq", logging.F("err", err)) }
	defer events.Close()

	pspClient := psp.NewClient(cfg.PSPBaseURL, cfg.PSPSecret, logger)

	// App service
	appSvc := app.NewService(logger, pg, cache, events, pspClient)

	// Metrics
	metrics.Register()

	// HTTP router
	r := chi.NewRouter()
	httpports.RegisterRoutes(r, appSvc, logger, cfg) // /payment/quote, /payment/webhook, /healthz, /metrics

	// Cron (nightly price recomputation)
	cron := scheduler.NewCron(logger, func() { appSvc.RecomputePriceList(context.Background(), 30) })
	if cfg.EnableCron {
		cron.Start("30 2 * * *") // 02:30 nightly
		defer cron.Stop()
	}

	srv := &http.Server{
		Addr: ":" + cfg.HTTPPort,
		Handler: r,
		ReadTimeout: 10*time.Second,
		WriteTimeout: 30*time.Second,
	}

	go func() {
		logger.Info("http start", logging.F("port", cfg.HTTPPort))
		if err := srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			logger.Fatal("http", logging.F("err", err))
		}
	}()

	sig := make(chan os.Signal, 1)
	signal.Notify(sig, syscall.SIGINT, syscall.SIGTERM)
	<-sig
	logger.Info("shutting down...")

	shutdownCtx, cancel2 := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel2()
	_ = srv.Shutdown(shutdownCtx)
}

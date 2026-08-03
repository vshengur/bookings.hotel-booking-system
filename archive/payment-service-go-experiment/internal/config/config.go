package config

import (
	"os"
)

type Config struct {
	Env          string
	HTTPPort     string
	PostgresURL  string
	RedisURL     string
	RabbitURL    string
	PSPBaseURL   string
	PSPSecret    string
	EnableCron   bool
}

func getenv(k, def string) string {
	v := os.Getenv(k)
	if v == "" { return def }
	return v
}

func Load() Config {
	return Config{
		Env:         getenv("ENV", "dev"),
		HTTPPort:    getenv("HTTP_PORT", "8080"),
		PostgresURL: getenv("POSTGRES_URL", "postgres://payment_user:payment_pass@localhost:5433/payment?sslmode=disable"),
		RedisURL:    getenv("REDIS_URL", "redis://localhost:6379/0"),
		RabbitURL:   getenv("RABBIT_URL", "amqp://guest:guest@localhost:5672/"),
		PSPBaseURL:  getenv("PSP_BASE_URL", "http://mock-psp:8081"),
		PSPSecret:   getenv("PSP_SECRET", "changeme"),
		EnableCron:  getenv("ENABLE_CRON", "true") == "true",
	}
}

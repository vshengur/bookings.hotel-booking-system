package gateway

import (
	"api-gateway/internal/middleware"
	"api-gateway/internal/routes"
	"log"

	"github.com/gorilla/mux"
	"github.com/hashicorp/consul/api"
	"golang.org/x/time/rate"
)

const consulAddress = "http://localhost:8500"

type Gateway struct {
	Limiter *rate.Limiter
	Router  *mux.Router
	Consul  *api.Client
}

func NewGateway() *Gateway {
	limiter := rate.NewLimiter(1, 5) // 1 request per second with a burst of 5
	consul, err := api.NewClient(api.DefaultConfig())
	if err != nil {
		log.Fatalf("Failed to connect to Consul: %v", err)
	}

	gateway := &Gateway{
		Limiter: limiter,
		Router:  mux.NewRouter(),
		Consul:  consul,
	}

	gateway.Router.Use(middleware.RateLimiter(gateway.Limiter))
	gateway.Router.Use(middleware.Logging())

	routes.SetupRoutes(gateway.Router, gateway.Consul)

	return gateway
}

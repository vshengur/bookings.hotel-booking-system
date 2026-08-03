package handlers

import (
	"api-gateway/internal/services"
	"io"
	"log"
	"net/http"

	"github.com/hashicorp/consul/api"
)

func AuthHandler(consul *api.Client) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		serviceAddress, err := services.DiscoverService(consul, "auth-service", "REST")
		if err != nil {
			http.Error(w, "Authentication service unavailable", http.StatusInternalServerError)
			return
		}

		log.Printf("Redirecting auth request to %s", serviceAddress)
		resp, err := http.Post(serviceAddress+"/verify", "application/json", r.Body)
		if err != nil {
			http.Error(w, "Authentication failed", http.StatusUnauthorized)
			return
		}
		defer resp.Body.Close()

		io.Copy(w, resp.Body)
	}
}

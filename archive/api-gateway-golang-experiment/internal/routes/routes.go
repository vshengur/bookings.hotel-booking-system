package routes

import (
	"api-gateway/internal/handlers"

	"github.com/gorilla/mux"
	"github.com/hashicorp/consul/api"
)

func SetupRoutes(router *mux.Router, consul *api.Client) {
	router.HandleFunc("/auth", handlers.AuthHandler(consul)).Methods("POST")
	router.PathPrefix("/rest").HandlerFunc(handlers.RestHandler(consul)).Methods("GET", "POST")
	router.HandleFunc("/grpc/{service}/{method}", handlers.GrpcHandler(consul)).Methods("POST")
}

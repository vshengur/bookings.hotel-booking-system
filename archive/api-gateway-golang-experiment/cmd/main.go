package main

import (
	"log"
	"net/http"

	"api-gateway/internal/gateway"
)

func main() {
	gateway := gateway.NewGateway()
	log.Println("Starting API Gateway service on port 8080")
	http.ListenAndServe(":8080", gateway.Router)
}

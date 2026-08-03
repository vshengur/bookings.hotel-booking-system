package handlers

import (
	"io"
	"log"
	"net/http"

	"api-gateway/internal/services"

	"github.com/hashicorp/consul/api"
)

func RestHandler(consul *api.Client) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		// Определяем путь сервиса через Consul
		serviceAddress, err := services.DiscoverService(consul, "rest-service", "REST")
		if err != nil {
			http.Error(w, "REST service unavailable", http.StatusInternalServerError)
			log.Printf("Service discovery failed: %v", err)
			return
		}

		// Формируем URL для проксирования
		proxyURL := serviceAddress + r.URL.Path

		// Создаем новый запрос для REST сервиса
		req, err := http.NewRequest(r.Method, proxyURL, r.Body)
		if err != nil {
			http.Error(w, "Failed to create request", http.StatusInternalServerError)
			log.Printf("Request creation failed: %v", err)
			return
		}

		// Копируем заголовки из исходного запроса
		req.Header = r.Header

		// Выполняем запрос к REST-сервису
		client := &http.Client{}
		resp, err := client.Do(req)
		if err != nil {
			http.Error(w, "Failed to reach REST service", http.StatusBadGateway)
			log.Printf("Request to REST service failed: %v", err)
			return
		}
		defer resp.Body.Close()

		// Копируем статус ответа
		w.WriteHeader(resp.StatusCode)

		// Копируем тело ответа
		io.Copy(w, resp.Body)
	}
}

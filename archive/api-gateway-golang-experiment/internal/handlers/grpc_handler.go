package handlers

import (
	"context"
	"io"
	"log"
	"net/http"
	"time"

	"api-gateway/internal/services"
	pb "path/to/your/generated/package" // Замените на путь вашего сгенерированного gRPC пакета

	"github.com/gorilla/mux"
	"github.com/hashicorp/consul/api"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/protobuf/proto"
)

func GrpcHandler(consul *api.Client) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		vars := mux.Vars(r)
		service := vars["service"]
		method := vars["method"]

		// Обнаружение адреса gRPC-сервиса через Consul
		serviceAddress, err := services.DiscoverService(consul, service, "gRPC")
		if err != nil {
			http.Error(w, "Service unavailable", http.StatusInternalServerError)
			log.Printf("Service discovery failed: %v", err)
			return
		}

		// Создание контекста с тайм-аутом
		ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancel()

		// Создание подключения через grpc.Dial
		conn, err := grpc.DialContext(ctx, serviceAddress, grpc.WithTransportCredentials(insecure.NewCredentials()))
		if err != nil {
			http.Error(w, "Failed to connect to gRPC service", http.StatusInternalServerError)
			log.Printf("Failed to connect to gRPC service: %v", err)
			return
		}
		defer conn.Close()

		// Создание строго типизированного клиента
		client := pb.NewYourServiceClient(conn)

		// Чтение тела запроса
		incomingBody, err := io.ReadAll(r.Body)
		if err != nil {
			http.Error(w, "Failed to read request body", http.StatusBadRequest)
			log.Printf("Failed to read request body: %v", err)
			return
		}

		// Десериализация запроса
		req := &pb.YourRequestMessage{} // Замените на реальный тип сообщения
		err = proto.Unmarshal(incomingBody, req)
		if err != nil {
			http.Error(w, "Invalid request body", http.StatusBadRequest)
			log.Printf("Failed to unmarshal request body: %v", err)
			return
		}

		// Вызов gRPC метода
		var resp *pb.YourResponseMessage
		switch method {
		case "SomeMethod": // Подставьте реальный метод
			resp, err = client.SomeMethod(ctx, req)
		default:
			http.Error(w, "Unknown method", http.StatusBadRequest)
			log.Printf("Unknown method: %s", method)
			return
		}

		if err != nil {
			http.Error(w, "gRPC call failed", http.StatusInternalServerError)
			log.Printf("gRPC method invocation failed: %v", err)
			return
		}

		// Сериализация ответа
		responseData, err := proto.Marshal(resp)
		if err != nil {
			http.Error(w, "Failed to marshal response", http.StatusInternalServerError)
			log.Printf("Failed to marshal gRPC response: %v", err)
			return
		}

		w.Header().Set("Content-Type", "application/grpc")
		w.Write(responseData)
	}
}

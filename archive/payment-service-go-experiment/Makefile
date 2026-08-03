SHELL := /bin/bash

build:
	go build -o ./bin/payment-service ./cmd/payment-service

run:
	ENV=dev HTTP_PORT=8080 POSTGRES_URL=postgres://payment_user:payment_pass@localhost:5433/payment?sslmode=disable \
	REDIS_URL=redis://localhost:6379/0 RABBIT_URL=amqp://guest:guest@localhost:5672/ \
	PSP_BASE_URL=http://localhost:8081 PSP_SECRET=changeme ENABLE_CRON=true \
	go run ./cmd/payment-service

test:
	go test ./... -count=1

gen:
	@echo "# Требуется protoc + protoc-gen-go + protoc-gen-go-grpc"
	protoc --go_out=api/gen/go --go-grpc_out=api/gen/go --proto_path ../../proto/payment.proto

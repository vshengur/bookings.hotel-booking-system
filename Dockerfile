# Указание базового образа с поддержкой Go
FROM golang:1.23 AS builder

# Установка рабочей директории
WORKDIR /app

# Копирование файлов проекта
COPY go.mod go.sum ./
RUN go mod download

COPY . .

# Сборка бинарного файла для Linux
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o bookings-auth-service ./cmd/main.go

# Второй этап: минимизация образа
FROM alpine:latest
# FROM debian:bullseye-slim

# Установка минимально необходимых зависимостей
RUN apk --no-cache add ca-certificates

# Установка рабочей директории
WORKDIR /root/

# Копирование бинарного файла из предыдущего этапа
COPY --from=builder /app/bookings-auth-service .

# Обеспечение разрешения на выполнение
RUN chmod +x ./bookings-auth-service

# Открытие порта
EXPOSE 8080

# Запуск приложения
CMD ["./bookings-auth-service"]
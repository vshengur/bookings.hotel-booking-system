# Use official Golang image as a base image
FROM golang:1.23-alpine as builder

# Set the Current Working Directory inside the container
WORKDIR /app

# Copy go mod and sum files
COPY go.mod go.sum ./

# Download all dependencies. Dependencies are cached if the go.mod and go.sum files are not changed
RUN go mod download

# Copy the source code into the container
COPY . .

# Build the Go app
RUN go build -o api-gateway ./cmd/main.go

# Use a minimal image as a base image
FROM debian:bullseye-slim

# Set the Current Working Directory inside the container
WORKDIR /root/

# Copy the Prebuilt binary file from the previous stage
COPY --from=builder /app/api-gateway .

# Command to run the executable
CMD ["./api-gateway"]
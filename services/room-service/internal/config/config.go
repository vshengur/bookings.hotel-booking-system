package config

import (
	"fmt"
	"os"
	"strconv"
)

// Config holds all configuration for the room service
type Config struct {
	Server   ServerConfig
	Database DatabaseConfig
	Consul   ConsulConfig
	Logger   LoggerConfig
}

// ServerConfig holds server configuration
type ServerConfig struct {
	Host     string
	Port     int
	GRPCPort int
}

// DatabaseConfig holds database configuration
type DatabaseConfig struct {
	Host     string
	Port     int
	User     string
	Password string
	DBName   string
	SSLMode  string
}

// ConsulConfig holds Consul configuration
type ConsulConfig struct {
	Host        string
	Port        int
	ServiceName string
	ServiceID   string
}

// LoggerConfig holds logger configuration
type LoggerConfig struct {
	Level      string
	OutputPath string
}

// LoadConfig loads configuration from environment variables
func LoadConfig() (*Config, error) {
	cfg := &Config{
		Server: ServerConfig{
			Host:     getEnv("SERVER_HOST", "0.0.0.0"),
			Port:     getEnvAsInt("SERVER_PORT", 8083),
			GRPCPort: getEnvAsInt("GRPC_PORT", 50053),
		},
		Database: DatabaseConfig{
			Host:     getEnv("DB_HOST", "localhost"),
			Port:     getEnvAsInt("DB_PORT", 5432),
			User:     getEnv("DB_USER", "postgres"),
			Password: getEnv("DB_PASSWORD", "postgres"),
			DBName:   getEnv("DB_NAME", "room_service_db"),
			SSLMode:  getEnv("DB_SSL_MODE", "disable"),
		},
		Consul: ConsulConfig{
			Host:        getEnv("CONSUL_HOST", "localhost"),
			Port:        getEnvAsInt("CONSUL_PORT", 8500),
			ServiceName: getEnv("CONSUL_SERVICE_NAME", "room-service"),
			ServiceID:   getEnv("CONSUL_SERVICE_ID", "room-service-1"),
		},
		Logger: LoggerConfig{
			Level:      getEnv("LOG_LEVEL", "info"),
			OutputPath: getEnv("LOG_OUTPUT_PATH", "stdout"),
		},
	}

	return cfg, nil
}

// GetDatabaseDSN returns the database connection string
func (c *Config) GetDatabaseDSN() string {
	return fmt.Sprintf(
		"host=%s port=%d user=%s password=%s dbname=%s sslmode=%s",
		c.Database.Host,
		c.Database.Port,
		c.Database.User,
		c.Database.Password,
		c.Database.DBName,
		c.Database.SSLMode,
	)
}

// GetServerAddress returns the server address
func (c *Config) GetServerAddress() string {
	return fmt.Sprintf("%s:%d", c.Server.Host, c.Server.Port)
}

// GetGRPCAddress returns the gRPC server address
func (c *Config) GetGRPCAddress() string {
	return fmt.Sprintf("%s:%d", c.Server.Host, c.Server.GRPCPort)
}

// GetConsulAddress returns the Consul address
func (c *Config) GetConsulAddress() string {
	return fmt.Sprintf("%s:%d", c.Consul.Host, c.Consul.Port)
}

// Helper functions

func getEnv(key, defaultValue string) string {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}
	return value
}

func getEnvAsInt(key string, defaultValue int) int {
	valueStr := os.Getenv(key)
	if valueStr == "" {
		return defaultValue
	}
	value, err := strconv.Atoi(valueStr)
	if err != nil {
		return defaultValue
	}
	return value
}

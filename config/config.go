package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	DbUser            string
	DbPassword        string
	DbHost            string
	DbPort            string
	DbName            string
	GoogleRedirectURL string
	GoogleClientID    string
	GoogleSecret      string
	JWTSecret         string
}

var AppConfig *Config

func LoadConfig() {
	if err := godotenv.Load(); err != nil {
		log.Println("No .env file found, using default environment variables")
	}

	AppConfig = &Config{
		DbUser:            os.Getenv("DB_USER"),
		DbPassword:        os.Getenv("DB_PASSWORD"),
		DbHost:            os.Getenv("DB_HOST"),
		DbPort:            os.Getenv("DB_PORT"),
		DbName:            os.Getenv("DB_NAME"),
		GoogleRedirectURL: os.Getenv("GOOGLE_REDIRECT_URL"),
		GoogleClientID:    os.Getenv("GOOGLE_CLIENT_ID"),
		GoogleSecret:      os.Getenv("GOOGLE_SECRET"),
		JWTSecret:         os.Getenv("JWT_SECRET"),
	}
}

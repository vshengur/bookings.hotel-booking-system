package services

import (
	"log"

	"github.com/hashicorp/consul/api"
	"github.com/spf13/viper"
)

// GetConsulSecret получает секрет из Consul
func GetConsulSecret(key string) (string, error) {
	consulAddress := viper.GetString("CONSUL_ADDRESS")
	consulNamespace := viper.GetString("CONSUL_NAMESPACE")

	// Подключение к Consul
	client, err := api.NewClient(&api.Config{
		Address: consulAddress,
	})
	if err != nil {
		return "", err
	}

	kv, _, err := client.KV().Get(consulNamespace+"/"+key, nil)
	if err != nil {
		return "", err
	}
	if kv == nil {
		return "", nil // Если ключ не найден
	}
	return string(kv.Value), nil
}

func MustGetConsulSecret(key string) string {

	value, err := GetConsulSecret(key)
	if err != nil || value == "" {
		log.Fatalf("Failed to get secret from Consul: %v", err)
	}
	return value
}

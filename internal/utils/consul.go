package utils

import (
	"fmt"

	consulapi "github.com/hashicorp/consul/api"
	"github.com/vshengur/bookings.room-service/internal/config"
	"github.com/vshengur/bookings.room-service/pkg/logger"
	"go.uber.org/zap"
)

// ConsulClient wraps Consul API client
type ConsulClient struct {
	client *consulapi.Client
	config *config.Config
}

// NewConsulClient creates a new Consul client
func NewConsulClient(cfg *config.Config) *ConsulClient {
	consulConfig := consulapi.DefaultConfig()
	consulConfig.Address = cfg.GetConsulAddress()

	client, err := consulapi.NewClient(consulConfig)
	if err != nil {
		logger.Log.Error("Failed to create Consul client", zap.Error(err))
		return nil
	}

	return &ConsulClient{
		client: client,
		config: cfg,
	}
}

// Register registers the service with Consul
func (c *ConsulClient) Register() error {
	if c.client == nil {
		return fmt.Errorf("consul client is nil")
	}

	registration := &consulapi.AgentServiceRegistration{
		ID:   c.config.Consul.ServiceID,
		Name: c.config.Consul.ServiceName,
		Port: c.config.Server.Port,
		Address: c.config.Server.Host,
		Tags: []string{
			"room-service",
			"hotel-booking",
			"v1",
		},
		Check: &consulapi.AgentServiceCheck{
			HTTP:                           fmt.Sprintf("http://%s:%d/health", c.config.Server.Host, c.config.Server.Port),
			Interval:                       "10s",
			Timeout:                        "5s",
			DeregisterCriticalServiceAfter: "30s",
		},
		Meta: map[string]string{
			"version": "1.0.0",
			"protocol": "http",
			"grpc_port": fmt.Sprintf("%d", c.config.Server.GRPCPort),
		},
	}

	err := c.client.Agent().ServiceRegister(registration)
	if err != nil {
		logger.Log.Error("Failed to register service with Consul", zap.Error(err))
		return err
	}

	logger.Log.Info("Service registered with Consul",
		zap.String("service_id", c.config.Consul.ServiceID),
		zap.String("service_name", c.config.Consul.ServiceName))

	return nil
}

// Deregister removes the service from Consul
func (c *ConsulClient) Deregister() error {
	if c.client == nil {
		return fmt.Errorf("consul client is nil")
	}

	err := c.client.Agent().ServiceDeregister(c.config.Consul.ServiceID)
	if err != nil {
		logger.Log.Error("Failed to deregister service from Consul", zap.Error(err))
		return err
	}

	logger.Log.Info("Service deregistered from Consul",
		zap.String("service_id", c.config.Consul.ServiceID))

	return nil
}

// GetService discovers a service from Consul
func (c *ConsulClient) GetService(serviceName string) ([]*consulapi.ServiceEntry, error) {
	if c.client == nil {
		return nil, fmt.Errorf("consul client is nil")
	}

	services, _, err := c.client.Health().Service(serviceName, "", true, nil)
	if err != nil {
		logger.Log.Error("Failed to discover service from Consul",
			zap.String("service_name", serviceName),
			zap.Error(err))
		return nil, err
	}

	return services, nil
}

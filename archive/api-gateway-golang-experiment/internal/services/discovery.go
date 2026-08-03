package services

import (
	"fmt"

	"github.com/hashicorp/consul/api"
)

func DiscoverService(consul *api.Client, serviceName, protocol string) (string, error) {
	services, _, err := consul.Catalog().Service(serviceName, "", nil)
	if err != nil {
		return "", err
	}

	for _, service := range services {
		if service.ServiceMeta["protocol"] == protocol {
			return service.ServiceAddress, nil
		}
	}

	return "", fmt.Errorf("no service found")
}

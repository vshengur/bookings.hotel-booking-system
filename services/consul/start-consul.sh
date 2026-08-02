#!/bin/bash

# Цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  Consul Setup for Hotel Booking System${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Проверка наличия secrets.env
if [ ! -f "secrets.env" ]; then
    echo -e "${YELLOW}secrets.env not found. Creating from example...${NC}"
    if [ -f "secrets.env.example" ]; then
        cp secrets.env.example secrets.env
        echo -e "${GREEN}✓ Created secrets.env from example${NC}"
        echo -e "${YELLOW}⚠ Please review and update secrets.env with your actual credentials${NC}"
        echo ""
    else
        echo -e "${RED}✗ secrets.env.example not found!${NC}"
        exit 1
    fi
else
    echo -e "${GREEN}✓ secrets.env found${NC}"
fi

# Проверка Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker not found. Please install Docker first.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker is installed${NC}"

# Проверка Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}✗ Docker Compose not found. Please install Docker Compose first.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker Compose is installed${NC}"
echo ""

# Создание сети, если не существует
echo -e "${BLUE}Checking Docker network...${NC}"
if ! docker network ls | grep -q "booking-system-network"; then
    echo -e "${YELLOW}Creating booking-system-network...${NC}"
    docker network create booking-system-network
    echo -e "${GREEN}✓ Network created${NC}"
else
    echo -e "${GREEN}✓ Network already exists${NC}"
fi
echo ""

# Запуск Consul
echo -e "${BLUE}Starting Consul...${NC}"
docker-compose -f docker-compose.consul.yml up -d

echo ""
echo -e "${YELLOW}Waiting for Consul to initialize...${NC}"
sleep 5

# Проверка статуса
echo ""
echo -e "${BLUE}Checking Consul status...${NC}"
if curl -s http://localhost:8500/v1/status/leader | grep -q .; then
    echo -e "${GREEN}✓ Consul is running${NC}"
else
    echo -e "${RED}✗ Consul is not responding${NC}"
    exit 1
fi

# Проверка consul-init
echo ""
echo -e "${BLUE}Checking secrets initialization...${NC}"
INIT_STATUS=$(docker inspect consul-init --format='{{.State.Status}}' 2>/dev/null || echo "not found")
if [ "$INIT_STATUS" = "exited" ]; then
    EXIT_CODE=$(docker inspect consul-init --format='{{.State.ExitCode}}' 2>/dev/null || echo "1")
    if [ "$EXIT_CODE" = "0" ]; then
        echo -e "${GREEN}✓ Secrets initialized successfully${NC}"
    else
        echo -e "${RED}✗ Secrets initialization failed (exit code: $EXIT_CODE)${NC}"
        echo -e "${YELLOW}Check logs: docker logs consul-init${NC}"
    fi
else
    echo -e "${YELLOW}⚠ Secrets initialization status: $INIT_STATUS${NC}"
fi

# Показать загруженные ключи
echo ""
echo -e "${BLUE}Loaded secrets in Consul:${NC}"
curl -s http://localhost:8500/v1/kv/config/?keys | jq -r '.[]' | while read key; do
    echo -e "${GREEN}  ✓ $(basename $key)${NC}"
done

echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}✓ Consul setup complete!${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Access Consul UI: ${BLUE}http://localhost:8500/ui${NC}"
echo -e "  2. Check KV store: ${BLUE}http://localhost:8500/ui/dc1/kv/config/${NC}"
echo -e "  3. View secrets initialization logs: ${BLUE}docker logs consul-init${NC}"
echo -e "  4. Start your services (e.g., auth-service)"
echo ""
echo -e "${YELLOW}Useful commands:${NC}"
echo -e "  • View Consul logs: ${BLUE}docker logs consul${NC}"
echo -e "  • Stop Consul: ${BLUE}docker-compose -f docker-compose.consul.yml down${NC}"
echo -e "  • Restart secrets init: ${BLUE}docker-compose -f docker-compose.consul.yml restart consul-init${NC}"
echo ""

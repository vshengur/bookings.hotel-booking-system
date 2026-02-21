#!/bin/sh
set -e

# Цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo "${GREEN}Starting Consul secrets initialization...${NC}"

# Ожидание запуска Consul
echo "${YELLOW}Waiting for Consul to be ready...${NC}"
until curl -s http://consul:8500/v1/status/leader | grep -q .; do
  echo "Waiting for Consul..."
  sleep 2
done
echo "${GREEN}Consul is ready!${NC}"

# Функция для установки секрета в Consul
set_secret() {
  KEY=$1
  VALUE=$2
  FOLDER=${CONSUL_FOLDER:-config}

  # Проверяем, существует ли уже секрет
  EXISTING=$(curl -s "http://consul:8500/v1/kv/${FOLDER}/${KEY}" | jq -r '.[0].Value // empty' 2>/dev/null || echo "")

  if [ -n "$EXISTING" ]; then
    echo "${YELLOW}Secret ${KEY} already exists, skipping...${NC}"
  else
    echo "${GREEN}Setting secret: ${KEY}${NC}"
    curl -X PUT -d "$VALUE" "http://consul:8500/v1/kv/${FOLDER}/${KEY}"
    echo ""
  fi
}

# Функция для генерации случайного JWT secret
generate_jwt_secret() {
  # Генерируем 64-байтовый случайный секрет в base64
  openssl rand -base64 64 | tr -d '\n'
}

# Получаем окружение (по умолчанию dev)
ENVIRONMENT=${RUN_MODE:-dev}
echo "${GREEN}Environment: ${ENVIRONMENT}${NC}"

# JWT Secret - генерируем автоматически, если не указан
if [ -z "$JWT_SECRET" ]; then
  echo "${YELLOW}JWT_SECRET not provided, generating new one...${NC}"
  JWT_SECRET=$(generate_jwt_secret)
  echo "${GREEN}Generated JWT_SECRET${NC}"
fi

# Устанавливаем секреты в Consul
set_secret "JWT_SECRET" "$JWT_SECRET"

# Для разработки добавляем Google OAuth credentials, если они указаны
if [ "$ENVIRONMENT" = "dev" ] || [ "$ENVIRONMENT" = "local" ]; then
  if [ -n "$GOOGLE_CLIENT_ID" ]; then
    set_secret "GOOGLE_CLIENT_ID" "$GOOGLE_CLIENT_ID"
  fi

  if [ -n "$GOOGLE_CLIENT_SECRET" ]; then
    set_secret "GOOGLE_CLIENT_SECRET" "$GOOGLE_CLIENT_SECRET"
  fi

  if [ -n "$GOOGLE_REDIRECT_URL" ]; then
    set_secret "GOOGLE_REDIRECT_URL" "$GOOGLE_REDIRECT_URL"
  fi
fi

# Устанавливаем конфигурацию базы данных, если указана
if [ -n "$DB_HOST" ]; then
  set_secret "DB_HOST" "$DB_HOST"
fi

if [ -n "$DB_PORT" ]; then
  set_secret "DB_PORT" "$DB_PORT"
fi

if [ -n "$DB_USER" ]; then
  set_secret "DB_USER" "$DB_USER"
fi

if [ -n "$DB_PASSWORD" ]; then
  set_secret "DB_PASSWORD" "$DB_PASSWORD"
fi

if [ -n "$DB_NAME" ]; then
  set_secret "DB_NAME" "$DB_NAME"
fi

if [ -n "$RUN_MODE" ]; then
  set_secret "RUN_MODE" "$RUN_MODE"
fi

echo "${GREEN}Secrets initialization completed!${NC}"
echo "${YELLOW}Note: Existing secrets were not overwritten${NC}"

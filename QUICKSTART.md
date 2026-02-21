# Quick Start Guide

## Быстрый запуск для разработки

```bash
# 1. Перейти в директорию consul
cd services/consul

# 2. Запустить автоматическую настройку
./start-consul.sh

# 3. Готово! Consul работает с автоматически сгенерированными секретами
```

## Что происходит при запуске?

1. ✅ Проверяется наличие `secrets.env` (создается из example, если нет)
2. ✅ Создается Docker сеть `booking-system-network`
3. ✅ Запускается Consul
4. ✅ Автоматически генерируется JWT_SECRET (если не указан)
5. ✅ Все секреты загружаются в Consul KV Store

## Проверка работы

### Consul UI
Откройте [http://localhost:8500/ui](http://localhost:8500/ui)

### Просмотр секретов
```bash
# Список ключей
curl http://localhost:8500/v1/kv/config/?keys | jq

# Получить JWT_SECRET
curl -s http://localhost:8500/v1/kv/config/JWT_SECRET | jq -r '.[0].Value' | base64 -d
```

### Логи
```bash
# Логи Consul
docker logs consul

# Логи инициализации секретов
docker logs consul-init
```

## Запуск сервисов

После запуска Consul, запустите ваши сервисы:

```bash
# Запуск auth-service
cd ../auth-service
docker-compose -f docker-compose.auth.yaml up -d
```

Сервисы автоматически подключатся к Consul и получат все необходимые секреты.

## Остановка

```bash
cd services/consul
docker-compose -f docker-compose.consul.yml down
```

## Дополнительная информация

Подробная документация: [SECRETS_MANAGEMENT.md](SECRETS_MANAGEMENT.md)

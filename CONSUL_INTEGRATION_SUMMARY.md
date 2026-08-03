# ✅ Решено: Автоматическое управление JWT Secret через Consul

## Проблема
```
2026/01/18 20:42:41 JWT Secret is required but not found!
```

## Решение
Реализована полная автоматизация управления секретами через Consul с автогенерацией JWT secret.

## Быстрый старт

```bash
# 1. Запустить инфраструктуру (Consul + автоинициализация)
cd infra
docker-compose -f docker-compose.infra.yml up -d

# 2. Запустить сервисы
cd ../services/auth-service
docker-compose -f docker-compose.auth.yaml up -d

# Готово! JWT_SECRET автоматически сгенерирован и загружен в Consul
```

## Что было реализовано

### 1. Автоматическая инициализация секретов
- [services/consul/consul-config/init-secrets.sh](services/consul/consul-config/init-secrets.sh) - Скрипт автогенерации JWT и загрузки секретов
- [infra/docker-compose.infra.yml](infra/docker-compose.infra.yml) - Добавлен `consul-init` контейнер для автоматической настройки
- [infra/.env](infra/.env) - Централизованная конфигурация для всех сред

### 2. Интеграция сервисов с Consul
- [services/auth-service/config/config.go](services/auth-service/config/config.go) - Обновлена логика чтения конфигурации
- [services/auth-service/services/consul.go](services/auth-service/services/consul.go) - Добавлено логирование подключения
- [services/auth-service/docker-compose.auth.yaml](services/auth-service/docker-compose.auth.yaml) - Настроено подключение к Consul

### 3. Документация
- [services/consul/QUICKSTART.md](services/consul/QUICKSTART.md) - Быстрый старт
- [services/consul/SECRETS_MANAGEMENT.md](services/consul/SECRETS_MANAGEMENT.md) - Полное руководство по управлению секретами
- [services/consul/README.md](services/consul/README.md) - Обзор Consul
- [README_CONSUL_SETUP.md](README_CONSUL_SETUP.md) - Это решение

### 4. Скрипты и утилиты
- [services/consul/start-consul.sh](services/consul/start-consul.sh) - Скрипт для быстрого запуска
- [infra/.env.example](infra/.env.example) - Шаблон конфигурации

## Архитектура решения

```
┌─────────────────┐
│   infra/.env    │  Конфигурация и секреты
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  consul-init    │  Автоматическая инициализация:
│   (контейнер)   │  • Генерирует JWT_SECRET
└────────┬────────┘  • Загружает секреты в Consul
         │
         ▼
┌─────────────────┐
│   Consul KV     │  Централизованное хранилище
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  auth-service   │  Читает секреты из Consul
│  (и др. сервисы)│
└─────────────────┘
```

## Результат

✅ **JWT Secret генерируется автоматически** при первом запуске
✅ **Не нужно вручную создавать секреты** на каждой среде
✅ **Централизованное управление** всеми секретами
✅ **Поддержка разных окружений** (dev, uat, prod)
✅ **Безопасное хранение** секретов вне репозитория

## Проверка работы

```bash
# 1. Проверить, что секреты загружены
curl http://localhost:8500/v1/kv/config/?keys

# Вывод:
# [
#     "config/DB_HOST",
#     "config/JWT_SECRET",  ← Автоматически сгенерирован!
#     ...
# ]

# 2. Проверить логи auth-service
docker logs auth-service-auth-service-1 | grep JWT

# Вывод:
# 2026/01/19 19:47:27 Loaded configuration: {...JWTSecret:rIznXrU/RM4/...}
```

## Управление секретами

```bash
# Просмотр через UI
http://localhost:8500/ui/dc1/kv/config/

# Просмотр через CLI
curl http://localhost:8500/v1/kv/config/?keys

# Обновление секрета
curl -X PUT -d "новое-значение" http://localhost:8500/v1/kv/config/JWT_SECRET
```

## Для других окружений

### UAT
```bash
# Отредактировать infra/.env, установить сильный JWT_SECRET
JWT_SECRET=$(openssl rand -base64 64)

# Запустить
cd infra
docker-compose -f docker-compose.infra.yml up -d
```

### Production
```bash
# Обязательно установить сильный JWT_SECRET в infra/.env
# НЕ оставляйте пустым в production!

# Пример генерации:
openssl rand -base64 64 >> infra/.env
```

## Дополнительная информация

📚 Полная документация: [services/consul/SECRETS_MANAGEMENT.md](services/consul/SECRETS_MANAGEMENT.md)
🚀 Быстрый старт: [services/consul/QUICKSTART.md](services/consul/QUICKSTART.md)
📖 Детальное решение: [README_CONSUL_SETUP.md](README_CONSUL_SETUP.md)

## Следующие шаги

- [ ] Интегрировать другие сервисы с Consul
- [ ] Настроить Consul ACL для production
- [ ] Добавить мониторинг Consul
- [ ] Настроить backup для Consul данных

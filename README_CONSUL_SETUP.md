# Автоматизированное управление секретами через Consul

## Решение проблемы

### Что было
```
2026/01/18 20:42:41 JWT Secret is required but not found!
```

### Что стало
```
2026/01/19 19:47:27 Consul configuration: Address=http://consul:8500, Folder=config
2026/01/19 19:47:27 Loaded configuration: {...JWTSecret:rIznXrU/RM4/zs64Gwamawk8nWwId9Pee2WyGFOZ32WFeuBiBQhbLomKdrOENgr9vzZZJbt6AOmzHh1ZuomVlQ==}
2026/01/19 19:47:27 Database connected successfully!
```

## Как это работает

### 1. Автоматическая инициализация секретов

При запуске инфраструктуры:
- Consul стартует и проходит health check
- Контейнер `consul-init` автоматически:
  - Генерирует JWT_SECRET (если не указан вручную)
  - Загружает все секреты из `infra/.env` в Consul KV Store
  - Не перезаписывает существующие секреты

### 2. Сервисы читают из Consul

Каждый сервис (например, auth-service):
1. Пытается прочитать конфигурацию из переменных окружения
2. Если не найдено - читает из Consul
3. Если не найдено нигде - использует значение по умолчанию

## Быстрый старт

### Для разработки

```bash
# 1. Запустить инфраструктуру (включая Consul)
cd infra
docker-compose -f docker-compose.infra.yml up -d

# 2. Проверить, что секреты загружены
curl http://localhost:8500/v1/kv/config/?keys

# 3. Запустить auth-service
cd ../services/auth-service
docker-compose -f docker-compose.auth.yaml up -d

# 4. Проверить логи - должен быть JWT_SECRET
docker logs auth-service-auth-service-1 | grep "JWT"
```

### Для других окружений (UAT/Production)

```bash
# 1. Отредактировать infra/.env
nano infra/.env

# 2. Установить сильный JWT_SECRET (или оставить пустым для автогенерации)
# Сгенерировать: openssl rand -base64 64
JWT_SECRET=ваш-сильный-секрет-для-production

# 3. Запустить инфраструктуру
cd infra
docker-compose -f docker-compose.infra.yml up -d
```

## Конфигурационные файлы

### Основная конфигурация

- **[infra/.env](infra/.env)** - Секреты и конфигурация для всех сред
- **[infra/.env.example](infra/.env.example)** - Шаблон с примерами
- **[infra/docker-compose.infra.yml](infra/docker-compose.infra.yml)** - Инфраструктура с Consul

### Документация

- **[services/consul/QUICKSTART.md](services/consul/QUICKSTART.md)** - Быстрый старт
- **[services/consul/SECRETS_MANAGEMENT.md](services/consul/SECRETS_MANAGEMENT.md)** - Полная документация
- **[services/consul/README.md](services/consul/README.md)** - Обзор

## Структура решения

```
hotel-booking-system/
├── infra/
│   ├── docker-compose.infra.yml  # Consul + consul-init
│   ├── .env                       # Секреты (gitignored)
│   └── .env.example              # Шаблон
│
├── services/
│   ├── consul/
│   │   ├── consul-config/
│   │   │   └── init-secrets.sh   # Скрипт инициализации
│   │   ├── QUICKSTART.md
│   │   ├── SECRETS_MANAGEMENT.md
│   │   └── README.md
│   │
│   └── auth-service/
│       ├── config/config.go      # Читает из ENV → Consul
│       ├── services/consul.go    # Клиент Consul
│       └── docker-compose.auth.yaml
│
└── README_CONSUL_SETUP.md        # Этот файл
```

## Преимущества решения

✅ **Автоматизация** - Не нужно вручную создавать JWT secret на каждой среде
✅ **Безопасность** - Секреты хранятся в Consul, не в репозитории
✅ **Гибкость** - Поддержка разных окружений (dev, uat, prod)
✅ **Централизация** - Все секреты в одном месте
✅ **Удобство** - Один `.env` файл для всей инфраструктуры

## Управление секретами

### Просмотр секретов

```bash
# UI
open http://localhost:8500/ui/dc1/kv/config/

# CLI
curl http://localhost:8500/v1/kv/config/?keys
```

### Обновление секрета

```bash
# Через API
curl -X PUT -d "new-value" http://localhost:8500/v1/kv/config/JWT_SECRET

# Через UI
# Откройте http://localhost:8500/ui и отредактируйте значение
```

### Ротация JWT Secret

```bash
# 1. Сгенерировать новый секрет
NEW_SECRET=$(openssl rand -base64 64)

# 2. Обновить в Consul
curl -X PUT -d "$NEW_SECRET" http://localhost:8500/v1/kv/config/JWT_SECRET

# 3. Перезапустить сервисы
cd services/auth-service
docker-compose -f docker-compose.auth.yaml restart
```

## Troubleshooting

### Проблема: JWT Secret не найден

**Проверьте:**
1. Consul запущен: `docker ps | grep consul`
2. Секреты загружены: `curl http://localhost:8500/v1/kv/config/?keys`
3. Сервис подключается к правильному Consul: проверьте `CONSUL_ADDRESS` в логах

### Проблема: Секреты не обновляются

```bash
# Пересоздать consul-init
cd infra
docker-compose -f docker-compose.infra.yml rm -f consul-init
docker-compose -f docker-compose.infra.yml up -d consul-init
```

### Проблема: Разные секреты на разных средах

Создайте отдельные `.env` файлы:
- `infra/.env.dev`
- `infra/.env.uat`
- `infra/.env.prod`

Запускайте с нужным файлом:
```bash
docker-compose -f docker-compose.infra.yml --env-file .env.prod up -d
```

## Безопасность

⚠️ **ВАЖНО:**
- Никогда не коммитьте файлы с секретами (`.env` уже в `.gitignore`)
- Используйте сильные секреты в production (минимум 64 символа)
- Регулярно ротируйте секреты
- Ограничьте доступ к Consul UI в production (используйте ACL)

## Следующие шаги

1. ✅ Consul работает с автоматической инициализацией секретов
2. ✅ Auth-service успешно читает JWT_SECRET из Consul
3. 📝 Настроить другие сервисы для чтения из Consul
4. 📝 Настроить Consul ACL для production
5. 📝 Добавить мониторинг Consul

## Полезные команды

```bash
# Просмотр всех секретов
curl -s http://localhost:8500/v1/kv/config/?keys | jq

# Получить конкретный секрет
curl -s http://localhost:8500/v1/kv/config/JWT_SECRET | \
  jq -r '.[0].Value' | base64 -d

# Логи инициализации
docker logs consul-init

# Перезапуск инфраструктуры
cd infra
docker-compose -f docker-compose.infra.yml restart

# Полная очистка
docker-compose -f docker-compose.infra.yml down -v
```

## Контакты и поддержка

- Документация: [services/consul/SECRETS_MANAGEMENT.md](services/consul/SECRETS_MANAGEMENT.md)
- Issues: [GitHub Issues](https://github.com/vshengur/hotel-booking-system/issues)

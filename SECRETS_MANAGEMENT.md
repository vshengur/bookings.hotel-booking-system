# Consul Secrets Management

Этот документ описывает автоматизированное управление секретами через Consul для разных окружений.

## Как это работает

### Архитектура

```
┌─────────────────┐      ┌──────────────┐      ┌─────────────────┐
│  secrets.env    │─────▶│ consul-init  │─────▶│  Consul KV      │
│  (окружение)    │      │  (скрипт)    │      │  (хранилище)    │
└─────────────────┘      └──────────────┘      └─────────────────┘
                                                         │
                                                         ▼
                                                ┌─────────────────┐
                                                │  auth-service   │
                                                │  (приложение)   │
                                                └─────────────────┘
```

1. **secrets.env** - файл с переменными окружения для конкретного окружения
2. **init-secrets.sh** - скрипт автоматической инициализации секретов в Consul
3. **Consul KV** - централизованное хранилище конфигурации
4. **Services** - сервисы читают секреты из Consul при старте

### Автоматическое управление JWT Secret

JWT Secret генерируется автоматически при первом запуске, если не указан явно:

- **Dev окружение**: Автогенерация при каждом запуске (если не задан)
- **UAT/Prod окружение**: ОБЯЗАТЕЛЬНО задать вручную

## Настройка для разных окружений

### Development (локальная разработка)

```bash
cd services/consul

# 1. Скопировать example файл
cp secrets.env.example secrets.env

# 2. Отредактировать secrets.env (опционально, можно оставить JWT_SECRET пустым)
# JWT_SECRET будет сгенерирован автоматически

# 3. Запустить Consul
docker-compose -f docker-compose.consul.yml up -d

# 4. Проверить, что секреты загружены
curl http://localhost:8500/v1/kv/config/?keys
```

### UAT Environment

```bash
cd services/consul

# 1. Использовать UAT конфигурацию
cp secrets.uat.env secrets.env

# 2. Отредактировать secrets.env
# ВАЖНО: Установить сильный JWT_SECRET!
# Сгенерировать: openssl rand -base64 64

# 3. Запустить с UAT профилем
docker-compose -f docker-compose.consul.yml --env-file secrets.env up -d
```

### Production Environment

```bash
cd services/consul

# 1. Использовать Production конфигурацию
cp secrets.prod.env secrets.env

# 2. Отредактировать secrets.env
# КРИТИЧНО: JWT_SECRET должен быть установлен!
# Сгенерировать: openssl rand -base64 64

# 3. Проверить все значения перед запуском
cat secrets.env

# 4. Запустить
docker-compose -f docker-compose.consul.yml --env-file secrets.env up -d
```

## Генерация JWT Secret

### Автоматическая генерация

Если `JWT_SECRET` не задан в `secrets.env`, он будет автоматически сгенерирован при старте:

```bash
# В secrets.env
JWT_SECRET=
```

### Ручная генерация

Для production рекомендуется сгенерировать и сохранить секрет:

```bash
# Генерация 64-байтового случайного секрета
openssl rand -base64 64

# Скопировать вывод в secrets.env
JWT_SECRET=<вставить сгенерированное значение>
```

## Управление секретами

### Просмотр секретов в Consul

```bash
# Список всех ключей
curl http://localhost:8500/v1/kv/config/?keys | jq

# Получить конкретный секрет (base64 encoded)
curl http://localhost:8500/v1/kv/config/JWT_SECRET | jq -r '.[0].Value' | base64 -d

# Просмотр через UI
http://localhost:8500/ui/dc1/kv/config/
```

### Обновление секрета вручную

```bash
# Через API
curl -X PUT -d "new-secret-value" http://localhost:8500/v1/kv/config/JWT_SECRET

# Через UI
# Откройте http://localhost:8500/ui/dc1/kv/config/ и отредактируйте значение
```

### Удаление секрета

```bash
# Удалить конкретный ключ
curl -X DELETE http://localhost:8500/v1/kv/config/JWT_SECRET

# Удалить все секреты (осторожно!)
curl -X DELETE http://localhost:8500/v1/kv/config/?recurse
```

## Безопасность

### ⚠️ Важные правила

1. **Никогда не коммитьте `secrets.env`** - он уже добавлен в `.gitignore`
2. **В production используйте только сильные секреты** - минимум 64 символа для JWT_SECRET
3. **Регулярно ротируйте секреты** - особенно после потенциальной утечки
4. **Используйте разные секреты для разных окружений**
5. **Ограничьте доступ к Consul UI** в production (используйте ACL)

### Рекомендации по безопасности

```bash
# 1. Используйте отдельные базы данных для каждого окружения
# 2. Настройте Consul ACL для production
# 3. Используйте TLS для Consul в production
# 4. Регулярно проводите аудит секретов
# 5. Используйте vault или внешний secrets manager для критичных данных
```

## Интеграция с сервисами

Сервисы автоматически читают секреты из Consul при старте:

```go
// В config/config.go
func getConfigValue(key string) string {
    // 1. Проверяем ENV переменные
    value := viper.GetString(key)
    if value != "" {
        return value
    }

    // 2. Читаем из Consul
    value, err := services.GetConsulSecret(key)
    if err != nil {
        log.Printf("Error fetching key %s from Consul: %v", key, err)
    }
    return value
}
```

### Приоритет загрузки конфигурации

1. **Environment Variables** (переменные окружения контейнера)
2. **Consul KV Store** (если не найдено в ENV)
3. **Default Values** (если не найдено нигде)

## Troubleshooting

### Проблема: "JWT Secret is required but not found!"

**Решение:**
1. Убедитесь, что Consul запущен и доступен
2. Проверьте, что consul-init контейнер успешно выполнился
3. Проверьте секреты в Consul: `curl http://localhost:8500/v1/kv/config/JWT_SECRET`

### Проблема: Секреты не обновляются после изменения

**Решение:**
```bash
# 1. Остановить сервисы
docker-compose down

# 2. Пересоздать consul-init контейнер
docker-compose -f docker-compose.consul.yml rm -f consul-init

# 3. Запустить заново
docker-compose -f docker-compose.consul.yml up -d
```

### Проблема: Нужно обновить существующий секрет

Скрипт `init-secrets.sh` НЕ перезаписывает существующие секреты. Для обновления:

```bash
# Вариант 1: Удалить и пересоздать
curl -X DELETE http://localhost:8500/v1/kv/config/JWT_SECRET
docker-compose -f docker-compose.consul.yml restart consul-init

# Вариант 2: Обновить напрямую
curl -X PUT -d "new-value" http://localhost:8500/v1/kv/config/JWT_SECRET
```

## Миграция с .env на Consul

Если у вас уже есть `.env` файлы в сервисах:

1. **Не удаляйте .env файлы сразу** - они используются как fallback
2. **Постепенно переносите секреты в Consul**
3. **Тестируйте каждый сервис после миграции**
4. **Только после успешного тестирования удаляйте .env**

```bash
# Пример миграции
# 1. Запустите Consul с секретами
cd services/consul
docker-compose -f docker-compose.consul.yml up -d

# 2. Проверьте, что сервис работает с Consul
cd ../auth-service
docker-compose -f docker-compose.auth.yaml up -d

# 3. Если все работает, можете удалить local.env (опционально)
```

## Дополнительные ресурсы

- [Consul KV Store Documentation](https://www.consul.io/docs/dynamic-app-config/kv)
- [Consul ACL System](https://www.consul.io/docs/security/acl)
- [Best Practices for Secret Management](https://www.consul.io/docs/security/security-models)

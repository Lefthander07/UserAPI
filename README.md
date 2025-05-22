# UserAPI

RESTful API для управления пользователями с аутентификацией и авторизацией.  
Проект разворачивается в Docker-контейнерах с PostgreSQL и Swagger-документацией.

---

## 🚀 Быстрый старт

### Предварительные требования
- Установленный [Docker](https://www.docker.com/)

### Запуск проекта
```bash
# Клонировать репозиторий
git clone https://github.com/Lefthander07/UserAPI.git
cd UserAPI

# Запустить сервисы через Docker Compose
cd docker
docker compose -p atontest -f docker-compose.yml up --build -d
```

После запуска откройте браузер и перейдите по адресу:

👉 http://localhost:10000

Вы увидите интерфейс Swagger для взаимодействия с API.
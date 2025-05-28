# MatchMaking Microservice

This is a .NET microservice that implements matchmaking logic using Redis, Kafka, and REST API.

## ⚙️ Running the Project

### 1. Clone the Repository

```bash
git clone https://github.com/yourname/MatchMaking.git
cd MatchMaking
```

### 2. Run with Docker Compose

```bash
docker compose up --build
```

This will start the following services:
- `matchmaking-service` — API service in .NET
- `matchmaking-worker` — Kafka worker
- `redis` — Redis
- `kafka` and `zookeeper` — for message exchange

> Ensure that ports `5001`, `9092`, `6379` are available.

## 🔗 Swagger

After starting, the API will be available at:

```
http://localhost:5001
```

And Swagger UI:

```
http://localhost:5001/index.html
```

## 📮 Testing Endpoints with CURL

### 🔎 Start Matchmaking

```bash
curl -X POST "http://localhost:5001/matchmaking/search?userId=user1"
```

Expected response: `204 No Content`

### 📊 Check Match Status

```bash
curl -X GET "http://localhost:5001/matchmaking/status?userId=user1"
```

Possible responses:
- `404 Not Found` — if the match is not yet found
- `200 OK` with JSON if found

## 📦 Kafka: Manual Testing

Use the official Kafka CLI (or from the container):

### Send a Message Manually

```bash
docker exec -it kafka kafka-console-producer \
  --bootstrap-server kafka:9092 \
  --topic matchmaking.complete
```

Now enter a JSON message:

```json
{"MatchId":"match123","UserIds":["user1", "user2", "user3"]}
```

After this, you can make a request:
```bash
curl -X GET "http://localhost:5001/matchmaking/status?userId=user1"
```

## 🧪 Checking Redis Manually

If you want to inspect Redis content:

```bash
docker exec -it redis redis-cli
```

Inside:

```redis
KEYS *
GET user:user1:match
```

## 🛑 Stop the Service

```bash
docker compose down
```

## 📁 Project Structure

```
MatchMaking/
├── MatchMaking.Service/         # API service
├── MatchMaking.Worker/          # Kafka worker
├── Shared.Contracts/            # Shared messages (DTO)
├── docker-compose.yml
├── Dockerfile
├── README.md                    # This file
```

## 🧠 Tips

- To update Swagger, open `http://localhost:5001/index.html`
- For debugging Kafka, check logs:  
```bash
docker compose logs -f matchmaking.service
```

## 🧩 Requirements

- Docker + Docker Compose
- .NET 9 SDK (if you want to run locally without Docker)

## 🔐 Authorization

Currently, the endpoints are public. For production, authentication should be added.

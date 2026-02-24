# Pact Contract Testing with .NET

A demonstration of **consumer-driven contract testing** using [PactNet](https://github.com/pact-foundation/pact-net)
across two .NET 10 microservices built with **Clean Architecture** and **Domain-Driven Design**.

> **Note:** This commit contains the ProductService and OrderService implementations. The Pact contract tests will
> be added in a subsequent commit.

---

## What This Repository Contains

The solution demonstrates how two microservices — **ProductService** and **OrderService** — communicate with each other
via two channels:

- **HTTP REST** — OrderService calls ProductService's API to look up product details when placing an order
- **AWS SQS** — OrderService publishes an `OrderPlaced` event to a queue which ProductService consumes to reserve stock

Each service follows **Clean Architecture** with strict dependency rules across four layers — Domain, Application,
Infrastructure, and Api — and **Domain-Driven Design** principles including aggregates, domain events, and integration
events.

---

## Project Structure

```
microservice-pact-contract-testing/
├── src/
│   ├── ProductService/
│   │   ├── ProductService.Domain/          Pure domain — entities, value objects, domain events
│   │   ├── ProductService.Application/     Use cases, DTOs, ports, integration events, domain event handlers
│   │   ├── ProductService.Infrastructure/  SQS consumer/publisher, in-memory repository, DI registration
│   │   └── ProductService.Api/             Minimal API endpoints, OpenAPI/Scalar, DI wiring
│   │
│   └── OrderService/
│       ├── OrderService.Domain/            Pure domain — Order aggregate, value objects, domain events
│       ├── OrderService.Application/       Use cases, DTOs, ports, integration events, domain event handlers
│       ├── OrderService.Infrastructure/    SQS publisher, HTTP client adapter, DI registration
│       └── OrderService.Api/               Minimal API endpoints, OpenAPI/Scalar, DI wiring
│
├── infra/
│   └── localstack-init/
│       └── 01-create-queues.sh             Creates SQS queues in LocalStack on startup
│
├── docker-compose.yml                      Runs LocalStack locally for SQS emulation
├── .editorconfig                           Code style and formatting rules
├── .gitattributes                          Line ending normalisation
└── PactContractTesting.Demo.slnx
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [AWS CLI](https://aws.amazon.com/cli/)

---

## Getting Started

### 1. Clone and restore

```bash
git clone <repo-url>
cd microservice-pact-contract-testing
dotnet restore PactContractTesting.Demo.slnx
```

### 2. Start LocalStack

LocalStack emulates AWS SQS locally, so you don't need a real AWS account for development.

```bash
docker compose up -d
```

This starts LocalStack on `http://localhost:4566` and automatically runs the init script to create the required SQS
queues.

Verify LocalStack is healthy:

```bash
curl http://localhost:4566/_localstack/health
```

### 3. Configure AWS credentials for LocalStack

The AWS CLI and SDK require credentials to be present even though LocalStack doesn't validate them. Create a dedicated
profile:

```bash
aws configure --profile localstack
```

Enter the following when prompted:

```
AWS Access Key ID:     test
AWS Secret Access Key: test
Default region:        us-east-1
Default output format: json
```

### 4. Verify the SQS queues were created

```bash
aws sqs list-queues \
  --endpoint-url http://localhost:4566 \
  --region us-east-1 \
  --profile localstack
```

You should see both queues returned:

```json
{
    "QueueUrls": [
        "http://localhost:4566/000000000000/order-placed",
        "http://localhost:4566/000000000000/stock-reserved"
    ]
}
```

If the queues are missing, the init script may not have run — see the [Troubleshooting](#troubleshooting) section below.

### 5. Run both services

Open the solution in **Rider** or **Visual Studio** and use the **Compound Run Configuration** to start both services
simultaneously, or run each from the CLI in separate terminals:

```bash
# Terminal 1
cd src/ProductService/ProductService.Api
dotnet run --launch-profile https

# Terminal 2
cd src/OrderService/OrderService.Api
dotnet run --launch-profile https
```

Once running, the API UIs are available at:

- ProductService — `https://localhost:7167/scalar/v1`
- OrderService — `https://localhost:7010/scalar/v1`

> If Rider assigns different ports than expected, check the console output for the `Now listening on` line and update
> the `@baseUrl` in the relevant `.http` file accordingly.

---

## Configuration

Both services use `appsettings.Development.json` when running locally to point the AWS SDK at LocalStack instead of
real AWS.

### ProductService — appsettings.Development.json

```json
{
  "AWS": {
    "Region": "us-east-1",
    "ServiceURL": "http://localhost:4566",
    "AuthenticationRegion": "us-east-1",
    "Profile": "localstack"
  },
  "Sqs": {
    "OrderPlacedQueueUrl":   "http://localhost:4566/000000000000/order-placed",
    "StockReservedQueueUrl": "http://localhost:4566/000000000000/stock-reserved"
  }
}
```

### OrderService — appsettings.Development.json

```json
{
  "AWS": {
    "Region": "us-east-1",
    "ServiceURL": "http://localhost:4566",
    "AuthenticationRegion": "us-east-1",
    "Profile": "localstack"
  },
  "Sqs": {
    "OrderPlacedQueueUrl": "http://localhost:4566/000000000000/order-placed"
  }
}
```

---

## Testing the Endpoints

HTTP files are provided for testing both APIs directly from Rider or VS Code:

```
src/ProductService/ProductService.Api/ProductService.Api.http
src/OrderService/OrderService.Api/OrderService.Api.http
```

Open either file in Rider and click the green play button next to any request to run it individually.

### Seeded Products

The ProductService in-memory repository is pre-loaded with three products for local development:

| ID                                     | Name                | Price   | Stock            |
|----------------------------------------|---------------------|---------|------------------|
| `3fa85f64-5717-4562-b3fc-2c963f66afa1` | Wireless Headphones | £149.99 | 50               |
| `4fb96f75-6828-5673-b4fc-3d074f77afa2` | Mechanical Keyboard | £89.99  | 30               |
| `5fc07a86-7939-6784-c5ad-4e185a88bac3` | USB-C Hub           | £49.99  | 0 (out of stock) |

---

## Testing the Full End-to-End Flow

With both services running, place an order via the OrderService API and watch the full flow execute across both
services.

### Step 1 — Place an order

Use `OrderService.Api.http` to send a valid order request, or call the endpoint directly:

```http
POST https://localhost:7010/api/v1/orders
Content-Type: application/json

{
  "customerId": "aabbccdd-0000-0000-0000-000000000001",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa1",
      "quantity": 2
    }
  ]
}
```

Expect a `201 Created` response with the order ID, status, and timestamp.

### Step 2 — Verify the message was published to SQS

```bash
aws sqs receive-message \
  --endpoint-url http://localhost:4566 \
  --region us-east-1 \
  --profile localstack \
  --queue-url http://localhost:4566/000000000000/order-placed \
  --visibility-timeout 0
```

The `--visibility-timeout 0` flag lets you peek at the message without removing it from the queue.

### Step 3 — Verify ProductService consumed the message

Check the ProductService console output in Rider for log lines confirming it picked up and processed the message. Then
confirm the stock was reduced:

```http
GET https://localhost:7167/api/v1/products/3fa85f64-5717-4562-b3fc-2c963f66afa1
```

The `stockQuantity` should have dropped from 50 to 48.

---

## Testing the SQS Consumer Directly

You can test ProductService's message consumer in isolation without OrderService by publishing a message directly to the
queue using the AWS CLI:

```bash
aws sqs send-message \
  --endpoint-url http://localhost:4566 \
  --region us-east-1 \
  --profile localstack \
  --queue-url http://localhost:4566/000000000000/order-placed \
  --message-body '{
    "orderId": "aabbccdd-0000-0000-0000-000000000001",
    "customerId": "aabbccdd-0000-0000-0000-000000000002",
    "placedAt": "2024-06-01T10:00:00+00:00",
    "lines": [
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa1",
        "quantity": 2,
        "unitPrice": 149.99
      }
    ]
  }'
```

---

## Event Flows

### OrderService — placing an order

When `POST /api/v1/orders` is called, OrderService:

1. Calls ProductService via HTTP to validate each product is in stock and retrieve its price
2. The `Order` aggregate is created and raises an `OrderPlacedEvent` (domain event — internal only)
3. The `DomainEventDispatcher` dispatches the domain event to `OrderPlacedDomainEventHandler` for internal concerns
4. The use case publishes an `OrderPlacedIntegrationEvent` to the `order-placed` SQS queue

### ProductService — consuming an order

When an `OrderPlaced` message arrives on the SQS queue, ProductService:

1. Deserialises the message and calls the `ReserveStock` use case for each order line
2. The `Product` aggregate mutates its stock and raises a `StockReservedEvent` (domain event — internal only)
3. The `DomainEventDispatcher` dispatches the domain event to `StockReservedDomainEventHandler` for internal concerns
4. The use case publishes a `StockReservedIntegrationEvent` to the `stock-reserved` SQS queue for downstream services

---

## Troubleshooting

### Services cannot reach each other

If OrderService cannot reach ProductService, check that:

- ProductService is running and the console shows `Now listening on`
- `ProductService.BaseUrl` in OrderService's `appsettings.json` matches the port ProductService is
  listening on
- If running via a Compound Run Configuration in Rider and one service becomes unreachable, try restarting both
  services together

### SQS queues not created on startup

The init script must be executable. If you're on Mac/Linux:

```bash
chmod +x infra/localstack-init/01-create-queues.sh
docker compose restart localstack
```

If you're on Windows, the script may have CRLF line endings which the Linux container can't execute:

```bash
sed -i 's/\r//' infra/localstack-init/01-create-queues.sh
docker compose restart localstack
```

You can also create the queues manually without restarting:

```bash
docker compose exec localstack awslocal sqs create-queue --queue-name order-placed
docker compose exec localstack awslocal sqs create-queue --queue-name stock-reserved
```

### Unable to locate AWS credentials

Make sure you have created the `localstack` AWS profile as described in step 3, or add the credentials directly to
`launchSettings.json`:

```json
"environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "AWS_ACCESS_KEY_ID": "test",
    "AWS_SECRET_ACCESS_KEY": "test",
    "AWS_DEFAULT_REGION": "us-east-1"
}
```

### HTTPS certificate not trusted

Run the following to generate and trust the .NET developer certificate:

```bash
dotnet dev-certs https --trust
```

If you get an error about an existing certificate, clean it first:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

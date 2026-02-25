# Pact Contract Testing with .NET

A demonstration of **consumer-driven contract testing** using [PactNet](https://github.com/pact-foundation/pact-net)
across two .NET 10 microservices built with **Clean Architecture** and **Domain-Driven Design**.

---

## What This Repository Contains

The solution demonstrates how two microservices — **ProductService** and **OrderService** — communicate with each other
via two channels:

- **HTTP REST** — OrderService calls ProductService's API to look up product details when placing an order
- **AWS SQS** — OrderService publishes an `OrderPlaced` event to a queue which ProductService consumes to reserve stock

Each service follows **Clean Architecture** with strict dependency rules across four layers — Domain, Application,
Infrastructure, and Api — and **Domain-Driven Design** principles including aggregates, domain events, and integration
events.

Contract tests are implemented using **PactNet v5** and cover both communication channels:

- **HTTP contracts** — define and verify the shape of REST API requests and responses
- **Message contracts** — define and verify the shape of SQS messages

---

## Project Structure

```
microservice-pact-contract-testing/
├── src/
│   ├── ProductService/
│   │   ├── ProductService.Domain/          Pure domain — entities, value objects, domain events
│   │   ├── ProductService.Application/     Use cases, DTOs, ports, integration events, domain event handlers
│   │   ├── ProductService.Infrastructure/  SQS consumer/publisher, in-memory repository, DI registration
│   │   ├── ProductService.Api/             Minimal API endpoints, OpenAPI/Scalar, DI wiring
│   │   └── tests/
│   │       └── ProductService.ContractTests/
│   │           ├── Http/                   HTTP provider verification against OrderService-ProductService.json
│   │           ├── Messaging/              Message consumer tests — generates ProductService-OrderService.json
│   │           └── Infrastructure/         ProviderStateMiddleware, WebApplicationFactory
│   │
│   └── OrderService/
│       ├── OrderService.Domain/            Pure domain — Order aggregate, value objects, domain events
│       ├── OrderService.Application/       Use cases, DTOs, ports, integration events, domain event handlers
│       ├── OrderService.Infrastructure/    SQS publisher, HTTP client adapter, DI registration
│       ├── OrderService.Api/               Minimal API endpoints, OpenAPI/Scalar, DI wiring
│       └── tests/
│           └── OrderService.ContractTests/
│               ├── Http/                   HTTP consumer tests — generates OrderService-ProductService.json
│               └── Messaging/              Message provider verification against ProductService-OrderService.json
│
├── pacts/                                  Generated pact files shared between test projects
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

### OrderService — appsettings.json

The base URL for ProductService is configured here rather than in the Development override, so it applies across all
environments:

```json
{
  "ProductService": {
    "BaseUrl": "https://localhost:7167"
  }
}
```

> Update `ProductService.BaseUrl` if ProductService is running on a different port — check the `Now listening on` line
> in ProductService's console output to confirm the actual port.

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

## Contract Tests

Contract tests verify the communication contracts between ProductService and OrderService without requiring both
services to be running simultaneously. They use [PactNet v5](https://github.com/pact-foundation/pact-net) to generate
and verify pact files.

### How the contracts are structured

Each service is both a consumer and a provider depending on the communication channel:

| Service        | Role              | Channel  | Pact file generated                  |
|----------------|-------------------|----------|--------------------------------------|
| OrderService   | HTTP Consumer     | REST API | `pacts/OrderService-ProductService.json` |
| ProductService | HTTP Provider     | REST API | Reads `OrderService-ProductService.json` |
| ProductService | Message Consumer  | SQS      | `pacts/ProductService-OrderService.json` |
| OrderService   | Message Provider  | SQS      | Reads `ProductService-OrderService.json` |

The pact files in the `pacts/` directory act as the shared contract — consumer tests generate them and provider tests
verify against them. The two test projects therefore have a dependency on each other's output:

```
Run order:
  1. OrderService.ContractTests   →  generates  pacts/OrderService-ProductService.json
  2. ProductService.ContractTests →  generates  pacts/ProductService-OrderService.json
                                  →  verifies   pacts/OrderService-ProductService.json

  (Both can be run independently but provider tests require the consumer's pact file to exist first)
```

### Running the contract tests

From the solution root:

```bash
# Step 1 — run only OrderService consumer tests
# Generates: pacts/OrderService-ProductService.json
dotnet test src/OrderService/tests/OrderService.ContractTests \
  --filter "FullyQualifiedName~Http"

# Step 2 — run all ProductService tests
# Generates: pacts/ProductService-OrderService.json
# Verifies:  pacts/OrderService-ProductService.json
dotnet test src/ProductService/tests/ProductService.ContractTests

# Step 3 — run all OrderService tests
# Verifies:  pacts/ProductService-OrderService.json
dotnet test src/OrderService/tests/OrderService.ContractTests
```

Or run selected tests from within the IDE.

> **Note:** The contract tests do not require LocalStack or either service to be running. They are fully self-contained
> — the HTTP provider test boots ProductService using `WebApplicationFactory`.

### What each test project contains

**ProductService.ContractTests**

- `Messaging/OrderPlacedMessageConsumerTests` — defines the shape of the `OrderPlaced` SQS message that ProductService
  needs to receive. Exercises the real message handler against a Pact-generated message. Generates
  `ProductService-OrderService.json`.
- `Http/ProductServiceHttpProviderTests` — boots ProductService on a real Kestrel port using `WebApplicationFactory`,
  replays every HTTP interaction from `OrderService-ProductService.json` against the real API, and verifies the
  responses match.

**OrderService.ContractTests**

- `Http/ProductCatalogueHttpConsumerTests` — defines the HTTP interactions OrderService needs from ProductService's
  REST API. Exercises the real `ProductCatalogueHttpClient` adapter against Pact's mock server. Generates
  `OrderService-ProductService.json`.
- `Messaging/OrderPlacedMessageProviderTests` — verifies that the `OrderPlacedIntegrationEvent` OrderService actually
  produces matches the shape ProductService declared it needs in `ProductService-OrderService.json`.

### Provider state middleware

The HTTP provider verification test uses `ProviderStateMiddleware` to seed the in-memory repository with specific data
before each interaction is replayed. This middleware lives entirely in the test project — the API project has no
knowledge of it.

The middleware is registered via `IStartupFilter` so it is injected at the start of the pipeline without replacing the
pipeline configured in `Program.cs`. Before each interaction the Pact verifier sends a `POST /provider-states` request
with the state name and any parameters, and the middleware seeds the repository accordingly:

| Provider state                     | Data seeded                           |
|------------------------------------|---------------------------------------|
| `a product with ID {id} exists`    | Product with the given ID, in stock   |
| `a product with ID {id} is out of stock` | Product with the given ID, zero stock |
| `no product exists with ID {id}`   | No Product wih the given ID           |

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
- `ProductService.BaseUrl` in `OrderService.Api/appsettings.json` matches the port ProductService is listening on
- If running via a Compound Run Configuration in Rider and one service becomes unreachable, try restarting both
  services together

### Contract test pact file not found

Provider tests require the consumer's pact file to exist before they can run. If you see a message like
`Pact file not found at pacts/OrderService-ProductService.json`, run the consumer tests first.

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

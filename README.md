# BankApp — Bank Loan API

A .NET 9 Web API for a simple digital-lending flow: users sign up, get a wallet, pass KYC, and take loans that are paid straight into their wallet with a repayment schedule. Loan events are published to Kafka through a transactional outbox.

## What it does

- **Auth**: register, login (JWT), forgot/reset password
- **Wallets**: every new account gets a wallet automatically (balance, loan limit, KYC status)
- **Loans**: apply for a loan, which is disbursed into the wallet in one transaction (loan + balance credit + ledger entry + outbox message). Supports `SIMPLE` and `REDUCING` (EMI) amortization schedules.
- **Messaging**: a background dispatcher publishes outbox rows to Kafka; a consumer reads `loan-events` and `thirdparty-webhooks-received`

## Project structure

| Project | Responsibility |
|---|---|
| `BankApp.Domain` | Entities (`User`, `Wallet`, `Loan`, `LedgerEntry`, `OutboxMessage`) and amortization strategies |
| `BankApp.Application` | Use cases (`UserService`, `WalletService`, `LoanApplicationService`), DTOs, interfaces |
| `BankApp.Infrastructure` | EF Core + PostgreSQL, migrations, JWT/BCrypt, Kafka producer/consumer |
| `BankApp.WebApi` | Controllers, DI setup, authentication, API docs |

## Requirements

- .NET 9 SDK and the EF Core tool (`dotnet tool install --global dotnet-ef`)
- PostgreSQL running on `localhost:5432`
- Kafka on `localhost:9092` (optional; `make kafka-install` installs it with Homebrew). Without it the API still works and outbox messages wait until Kafka is available.

## Getting started

```bash
make setup                              # restore packages + generate a JWT signing key
make secrets-db DB_PASSWORD=<password>  # store your Postgres connection string
make kafka-install                      # once, if you want Kafka
make up                                 # start Kafka, create topics, migrate DB, run the API
```

Without Kafka, use `make db-update && make run` instead of `make up`.

The API runs on **http://localhost:5155**, with interactive docs at **http://localhost:5155/scalar/v1**.

## Configuration

Non-secret settings live in `BankApp.WebApi/appsettings*.json`. Secrets are never committed:

- **Development**: stored with `dotnet user-secrets` (`make secrets-db`, `make secrets-jwt`, `make secrets-list`)
- **Other environments**: environment variables, e.g. `Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Kafka__BootstrapServers`

## Using the API

1. `POST /api/auth/register` returns a JWT, and a wallet is created for you
2. Send `Authorization: Bearer <token>` on the calls below
3. `GET /api/wallets/me` returns your wallet and balance
4. `POST /api/wallets/me/verify-kyc` marks KYC as verified (demo stand-in for a real KYC provider)
5. `POST /api/loans` with `{ "amount": 5000, "termInMonths": 3, "methodType": "REDUCING" }` (the loan goes into your own wallet)
6. `GET /api/loans` or `GET /api/loans/{id}` shows your loans and repayment schedules

## Makefile reference

Run `make help` to see all commands.

| Command | Description |
|---|---|
| `make build` / `make run` / `make watch` | Build, run, or run with hot reload |
| `make docs` | Print the API docs URLs |
| `make db-update` | Apply EF Core migrations |
| `make migration-add NAME=X` | Create a new migration |
| `make migrations` | List migrations and their status |
| `make db-reset` | Drop and recreate the database (asks for confirmation) |
| `make secrets-db DB_PASSWORD=...` | Save the DB connection string to user-secrets |
| `make secrets-jwt` | Generate a new JWT key (existing tokens stop working) |
| `make secrets-list` | Show secrets with values masked |
| `make kafka-start` / `make kafka-stop` / `make kafka-status` | Manage the local Kafka broker |
| `make kafka-topics` | Create the topics the app uses |
| `make kafka-consume TOPIC=loan-events` | Watch messages on a topic |
| `make up` | Kafka + topics + migrations + run, all in one |

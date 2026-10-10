# BankApp developer commands. Run `make help` to list them.

API_PROJECT    := BankApp.WebApi
INFRA_PROJECT  := BankApp.Infrastructure
EF             := dotnet ef --project $(INFRA_PROJECT) --startup-project $(API_PROJECT)

API_URL        := http://localhost:5155
KAFKA_SERVER   := localhost:9092
KAFKA_TOPICS   := loan-events thirdparty-webhooks-received

DB_HOST        ?= localhost
DB_PORT        ?= 5432
DB_NAME        ?= bankapp
DB_USER        ?= postgres

.DEFAULT_GOAL := help
.PHONY: help restore build clean rebuild run watch \
        migration-add migration-remove migrations db-update db-reset \
        secrets-jwt secrets-db secrets-list setup \
        kafka-install kafka-start kafka-stop kafka-status kafka-wait kafka-topics kafka-consume kafka-logs \
        up docs

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-18s\033[0m %s\n", $$1, $$2}'

# ---------- Build & run ----------

restore: ## Restore NuGet packages
	dotnet restore

build: ## Build the solution
	dotnet build

clean: ## Remove build output
	dotnet clean

rebuild: clean build ## Clean, then build

run: ## Run the API on http://localhost:5155
	dotnet run --project $(API_PROJECT) --launch-profile http

watch: ## Run the API with hot reload
	dotnet watch --project $(API_PROJECT) run --launch-profile http

docs: ## Print the API documentation URLs
	@echo "Scalar:  $(API_URL)/scalar/v1"
	@echo "OpenAPI: $(API_URL)/openapi/v1.json"

# ---------- Database (EF Core) ----------

migration-add: ## Add a migration: make migration-add NAME=AddSomething
	@test -n "$(NAME)" || (echo "Usage: make migration-add NAME=<MigrationName>" && exit 1)
	$(EF) migrations add $(NAME)

migration-remove: ## Remove the last migration (only if not applied)
	$(EF) migrations remove

migrations: ## List migrations and whether they are applied
	$(EF) migrations list

db-update: ## Apply pending migrations to the database
	$(EF) database update

db-reset: ## DROP the database and recreate it from migrations (deletes all data)
	@read -p "This deletes ALL data in '$(DB_NAME)'. Type 'yes' to continue: " ans && [ "$$ans" = "yes" ]
	$(EF) database drop --force
	$(EF) database update

# ---------- Secrets (user-secrets, development only) ----------

secrets-jwt: ## Generate a new random JWT signing key (invalidates existing tokens)
	dotnet user-secrets set "Jwt:Key" "$$(openssl rand -base64 64 | tr -d '\n')" --project $(API_PROJECT)

secrets-db: ## Store the DB connection string: make secrets-db DB_PASSWORD=...
	@test -n "$(DB_PASSWORD)" || (echo "Usage: make secrets-db DB_PASSWORD=<password> [DB_USER=postgres DB_NAME=bankapp]" && exit 1)
	@dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
		"Host=$(DB_HOST);Port=$(DB_PORT);Database=$(DB_NAME);Username=$(DB_USER);Password=$(DB_PASSWORD)" \
		--project $(API_PROJECT)

secrets-list: ## Show configured secrets (values masked)
	@dotnet user-secrets list --project $(API_PROJECT) | sed -E 's/(Password=)[^;]*/\1***/; s/(Jwt:Key = ).*/\1***/'

# ---------- Kafka (Homebrew, KRaft mode) ----------

kafka-install: ## Install Kafka with Homebrew
	brew install kafka

kafka-start: ## Start Kafka in the background
	brew services start kafka

kafka-stop: ## Stop Kafka
	brew services stop kafka

kafka-status: ## Show whether Kafka is running and reachable
	@brew services list | grep -E '^(Name|kafka)' || true
	@nc -z $(subst :, ,$(KAFKA_SERVER)) && echo "Kafka reachable on $(KAFKA_SERVER)" || echo "Kafka NOT reachable on $(KAFKA_SERVER)"

kafka-wait: ## Wait up to 60s for Kafka to accept connections
	@for i in $$(seq 1 60); do nc -z $(subst :, ,$(KAFKA_SERVER)) && echo "Kafka is up" && exit 0; sleep 1; done; \
		echo "Kafka did not start; check 'make kafka-logs'"; exit 1

kafka-topics: kafka-wait ## Create the topics the app uses
	@for t in $(KAFKA_TOPICS); do \
		kafka-topics --bootstrap-server $(KAFKA_SERVER) --create --if-not-exists --topic $$t; \
	done
	kafka-topics --bootstrap-server $(KAFKA_SERVER) --list

kafka-consume: ## Tail a topic: make kafka-consume [TOPIC=loan-events]
	kafka-console-consumer --bootstrap-server $(KAFKA_SERVER) --topic $(or $(TOPIC),loan-events) --from-beginning

kafka-logs: ## Tail the Kafka broker log
	tail -f $$(brew --prefix)/var/log/kafka/kafka_output.log

# ---------- One-shot workflows ----------

setup: restore secrets-jwt ## First-time setup (then run: make secrets-db DB_PASSWORD=... && make db-update)
	@echo "Next: make secrets-db DB_PASSWORD=<your postgres password> && make db-update"

up: kafka-start kafka-topics db-update run ## Start Kafka, create topics, migrate the DB, run the API

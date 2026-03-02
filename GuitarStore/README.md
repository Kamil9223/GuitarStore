# 🔐 Local Configuration (Stripe + Docker)

## 1️⃣ .env File

This project uses a `.env` file to store local secrets.

The file is **not committed to the repository** (it is included in `.gitignore`).

### Step 1 -- Create the `.env` file

In the root directory (next to `docker-compose.yml`), create a file
named:

    .env

### Example `.env` content:

    # Stripe
    STRIPE_API_KEY=sk_test_xxxxxxxxx
    Stripe__SecretKey=sk_test_xxxxxxxxx
    Stripe__WebhookSecret=whsec_xxxxxxxxx
    Stripe__Url=https://api.stripe.com

-   `STRIPE_API_KEY` -- used by stripe-cli
-   `Stripe__SecretKey` -- used by the .NET application
-   `Stripe__WebhookSecret` -- used to verify incoming webhooks
-   `Stripe__Url` -- Stripe API base URL

------------------------------------------------------------------------

## 2️⃣ Start Infrastructure (Docker)

Docker is used only for:

-   SQL Server
-   RabbitMQ
-   Stripe CLI (webhook forwarder)

Run:

    docker compose up -d

------------------------------------------------------------------------

## 3️⃣ Run the Application

The API application runs locally (outside Docker).

Start it using:

    dotnet run

or via your IDE.

In `Development` mode, the application automatically loads environment
variables from the `.env` file.

------------------------------------------------------------------------

## 4️⃣ Verify Stripe CLI Configuration

To confirm that `STRIPE_API_KEY` was correctly loaded into the
stripe-cli container:

### Linux / Mac / Git Bash:

    docker exec -it stripe-cli printenv | grep STRIPE

### Windows PowerShell:

    docker exec -it stripe-cli printenv | Select-String STRIPE

You should see:

    STRIPE_API_KEY=sk_test_...

------------------------------------------------------------------------

## 5️⃣ How to Obtain WebhookSecret

After starting stripe-cli, check its logs:

    docker logs stripe-cli

You should see:

    Ready! Your webhook signing secret is whsec_...

Copy this value into:

    Stripe__WebhookSecret

in your `.env` file.

------------------------------------------------------------------------

# 🧠 Configuration Overview

Environment         Source of Secrets
  ------------------- ----------------------------------------
Local Development   `.env`
Docker              `.env`
Production          Environment Variables / Secret Manager

The project does **not use User Secrets**.\
All local secrets are managed through `.env` for clarity and
consistency.

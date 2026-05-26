# CLAUDE.md

## Project Overview

Hotel booking platform backend API built with .NET 9 / ASP.NET Core. Provides REST APIs for hotel search, room booking, payments, user management, and an admin dashboard.

## Tech Stack

- .NET 9, C#, ASP.NET Core Web API
- SQL Server with Entity Framework Core (code-first migrations)
- ASP.NET Identity + JWT authentication (+ Google OAuth)
- SignalR (real-time chat & notifications)
- Cloudinary (image uploads)
- VNPay (payment gateway, sandbox)
- MailKit (email/OTP)
- Ollama (LLM chatbot, model: qwen2.5:7b)
- EPPlus (Excel export)
- Swagger/OpenAPI

## Architecture

Repository + Service pattern with constructor-based DI. Separate admin and user-facing layers.

```
be-booking-hotel/
├── Controllers/        # API endpoints
├── Models/             # EF Core entities + DbContext
├── DTOs/               # Request/response data transfer objects
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
├── Services/
│   ├── Interfaces/
│   └── Implements/
├── Hub/                # SignalR hubs (Chat, Notification)
├── Migrations/         # EF Core migrations
├── Data/               # DbInitializer (role seeding)
├── Libraries/          # VnPay library
└── Helpers/            # OTP helper
```

## Common Commands

```powershell
# Run the project
dotnet run --project be-booking-hotel

# Build
dotnet build

# Add a migration
dotnet ef migrations add <Name> --project be-booking-hotel

# Update database
dotnet ef database update --project be-booking-hotel

# Docker build
docker build -t be-booking-hotel ./be-booking-hotel
```

## Development URLs

- HTTP: http://localhost:5160
- HTTPS: https://localhost:7035
- Swagger UI: http://localhost:5160/swagger (dev only)
- Docker port: 10000

## Key Conventions

- Namespace: `be_booking_hotel`
- Database: SQL Server (`HotelBookingDB`)
- Auth: JWT Bearer tokens (1440 min expiry). SignalR reads token from `access_token` query param.
- SignalR endpoints: `/hubs/chatHub`, `/hubs/notification`
- CORS: localhost:3000, localhost:5173, Vercel production domains
- File uploads: max 50MB multipart
- Timezone: SE Asia Standard Time

## Frontend

Separate repo deployed on Vercel. Runs on localhost:3000 or localhost:5173 during development.

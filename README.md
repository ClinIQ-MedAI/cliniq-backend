# ClinIQ Backend API

[![Production CI/CD Pipeline](https://github.com/ClinIQ-MedAI/cliniq-backend/actions/workflows/deploy.yml/badge.svg)](https://github.com/ClinIQ-MedAI/cliniq-backend/actions/workflows/deploy.yml)

A modular ASP.NET Core backend for clinic management, providing authentication, doctor/patient profiles, booking, real-time chat, AI-powered medical image analysis, and notifications.

## Features

- **Authentication & Authorization** – JWT-based user authentication with role-based access control.
- **Doctor & Patient Management** – Profiles, search, and administrative functions.
- **Booking System** – Scheduling appointments between doctors and patients.
- **Real-time Chat** – SignalR hubs for doctor-patient and chatbot communication.
- **AI Medical Image Analysis** – Queue-based analysis of X-rays (bone, chest, dental) via Redis.
- **Notification System** – Push notifications for users and management.
- **Contact Management** – Public contact form and management backend.
- **Health Checks** – Endpoint for monitoring service health.
- **API Documentation** – OpenAPI/Swagger with Scalar UI in development.
- **Structured Logging** – Serilog with rolling file output.
- **Localization** – Supports English and Arabic cultures.
- **Docker Support** – Compose file for SQL Server and Redis.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or later)
- [Docker](https://www.docker.com/get-started) (optional, for running dependencies)
- SQL Server (or use the Docker container)
- Redis (or use the Docker container)

## Installation

```bash
# Clone the repository
git clone https://github.com/ClinIQ-MedAI/cliniq-backend.git
cd cliniq-backend

# Restore dependencies
dotnet restore
```

### Using Docker Compose

Start SQL Server and Redis:

```bash
docker-compose up -d
```

The services will be available at:
- SQL Server: `localhost:1433`
- Redis: `localhost:6379`

### Database Setup

1. Update connection strings in `Clinic.API/appsettings.json` (or use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):
   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=ClinicMonoDB;User Id=sa;Password=<your_password>;TrustServerCertificate=True;",
       "Redis": "localhost:6379"
   }
   ```
2. Apply migrations and seed the database (runs automatically in development).

## Usage

```bash
# Run the API
dotnet run --project Clinic.API
```

### Access Points

| Service | URL | Description |
|---------|-----|-------------|
| API Controllers | `http://localhost:5000/api/...` | REST endpoints |
| SignalR Chat Hub | `/hubs/chat` | Real-time doctor-patient chat |
| Notification Hub | `/hubs/notifications` | Push notifications |
| AI Job Hub | `/hubs/jobs` | AI image analysis updates |
| Chatbot Hub | `/hubs/chatbot` | AI chatbot responses |
| Health Check | `/health` | Service health status |
| Scalar API Docs | `/scalar` (development) | Interactive API documentation |

## Configuration

Key settings in `appsettings.json`:

| Section | Description |
|---------|-------------|
| `ConnectionStrings` | SQL Server and Redis connection strings |
| `Jwt` | JWT token settings (key, issuer, audience, expiry) |
| `MailSettings` | SMTP configuration for email notifications |
| `AllowedOrigins` | CORS allowed origins |
| `HangfireSettings` | Background job processing credentials |
| `Serilog` | Logging configuration (levels, sinks) |

> [!WARNING]
> Never commit real secrets. Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables in production.

## Project Structure

```
ClinicAPI.sln
├── Clinic.API                  # Main API entry point, controllers, and configuration
├── Clinic.Infrastructure       # Shared infrastructure: DB context, hubs, localization
├── Clinic.Authentication       # JWT authentication and user management
├── Clinic.AIFeatures           # AI image analysis integration
├── Doctor.Profile/Doctor.Management  # Doctor domain logic
├── Patient.Profile/Patient.Management # Patient domain logic
├── Booking.Management/Booking.Doctor/Booking.Patient # Appointment scheduling
├── Chat.Management/Chat.Doctor/Chat.Patient # Real-time messaging
├── Notification.Management/Notification.User # Notification system
├── Contact.Management/Contact.Public # Contact form handling
├── Admin.Management            # Administrative functions
└── Roles.Management            # Role-based access control
```

Each module is a separate class library, promoting separation of concerns and testability.

## Integration Guides

- **Flutter Chatbot Integration**: See [`FLUTTER_CHATBOT_INTEGRATION.md`](FLUTTER_CHATBOT_INTEGRATION.md) for connecting a Flutter app to the SignalR chatbot hub.
- **AI Queue Integration**: See [`INTEGRATION_TEST_GUIDE.md`](INTEGRATION_TEST_GUIDE.md) for testing Redis-based AI job processing.

## Development

### API Documentation

In development mode, the API exposes an OpenAPI specification and Scalar UI:

- OpenAPI JSON: `http://localhost:5000/openapi/v1.json`
- Scalar UI: `http://localhost:5000/scalar`

### Logging

Logs are written to `./Logs/` with daily rolling files. Configure via `Serilog` section in `appsettings.json`.

### Health Checks

The `/health` endpoint returns a JSON response with the status of SQL Server and Redis connections.


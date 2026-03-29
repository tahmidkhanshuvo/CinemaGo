# CinemaGo - How to Run & Deploy

## Quick Start (Local Development)

### Prerequisites
- .NET 10 SDK
- Node.js 18+ 
- PostgreSQL (local or Neon cloud)

### 1. Backend (ASP.NET Core)

**Option A: Using local PostgreSQL**
```bash
cd CinemaGo.Api
dotnet restore
dotnet build
dotnet run
```

**Option B: Using Neon (Cloud PostgreSQL)**
1. Create a project at https://neon.tech
2. Get your connection string from the dashboard
3. Update `CinemaGo.Api/.env`:
```
ConnectionStrings__DefaultConnection=postgresql://username:password@ep-xyz.us-east-1.aws.neon.tech/cinemago?sslmode=require
```
4. Run: `dotnet run`

The API will automatically:
1. Create all database tables using EF Core
2. Seed demo data (admin, users, movies, halls, showtimes)
3. Start at `http://localhost:5000`

### 2. Frontend (Next.js)

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000`

---

## Demo Accounts (Pre-seeded)

The database is automatically seeded with:

| Email | Password | Role |
|-------|----------|------|
| admin@cinemago.com | admin123 | Admin |
| john@example.com | password123 | User |
| jane@example.com | password123 | User |

**Note:** Admin users can access `/admin` dashboard.

---

## Deployment Guide

### 1. PostgreSQL (Neon)
1. Create a Neon project at https://neon.tech
2. Copy the connection string
3. Set it as `ConnectionStrings__DefaultConnection` in your deployment

### 2. Backend (Render Docker)
1. Push your code to GitHub
2. Create a new Web Service on Render
3. Set the following environment variables:
   - `ConnectionStrings__DefaultConnection` = your Neon connection string
   - `Jwt__SecretKey` = your secret key
4. Use the following Dockerfile:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "CinemaGo.Api.dll"]
```

### 3. Frontend (Vercel)
1. Push your frontend code to GitHub
2. Import the project in Vercel
3. Set the environment variable:
   - `NEXT_PUBLIC_API_URL` = your Render backend URL (e.g., https://your-backend.onrender.com/api)
4. Deploy

---

## Environment Variables

### Backend (CinemaGo.Api/.env)
```
ConnectionStrings__DefaultConnection=postgresql://user:pass@host/db?sslmode=require
Jwt__SecretKey=YourSecretKey12345678901234567890
```

### Frontend (frontend/.env.local)
```
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

---

## Project Structure

```
CinemaGo/
├── CinemaGo.Api/          # ASP.NET Core Web API
│   ├── Controllers/        # API endpoints
│   ├── Models/            # Entity classes
│   ├── DTOs/              # Data transfer objects
│   ├── Data/              # Database context & migrations
│   └── .env               # Environment configuration
│
├── frontend/              # Next.js frontend
│   ├── app/               # App router pages
│   ├── components/        # React components
│   └── .env.local         # Frontend environment
│
├── RUNNING.md             # This file
└── Dockerfile             # For backend deployment
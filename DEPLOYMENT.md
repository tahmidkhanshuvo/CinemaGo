# CinemaGo Deployment Guide

This guide walks you through deploying CinemaGo to production.

## Architecture Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────┐
│   Vercel        │────▶│   Render        │────▶│  Neon DB    │
│  (Next.js)      │     │ (ASP.NET Core)  │     │ (Postgres)  │
└─────────────────┘     └─────────────────┘     └─────────────┘
```

## Prerequisites

- Node.js 18+ installed
- .NET 8 SDK installed
- Git installed
- Accounts for:
  - Vercel (for frontend)
  - Render (for backend)
  - Neon (for PostgreSQL)

---

## Step 1: Database Setup (Neon)

1. Go to [Neon](https://neon.tech) and create a free account
2. Create a new project named "cinemago"
3. Copy the connection string (format: `postgresql://user:pass@host.neon.tech/cinemago?sslmode=require`)

---

## Step 2: Backend Deployment (Render with Docker)

### Option A: Using GitHub (Recommended)

1. **Push your code to GitHub:**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   # Create a new repository on GitHub and push
   git remote add origin https://github.com/yourusername/cinemago.git
   git push -u origin main
   ```

2. **Deploy to Render:**
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Click "New +" → "Web Service"
   - Connect your GitHub repository
   - Configure:
     - Name: `cinemago-api`
     - Branch: `main`
     - Build Command: (leave empty - uses Dockerfile)
     - Start Command: (leave empty - uses Dockerfile)
   - Click "Create Web Service"

3. **Set Environment Variables:**
   Add these in Render's environment variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=postgresql://user:pass@host.neon.tech/cinemago?sslmode=require
   JWT__Key=YourSuperSecretKeyHere123!
   JWT__Issuer=CinemaGo
   JWT__Audience=CinemaGoUsers
   ```

### Option B: Using Docker CLI

```bash
# Build the image
docker build -t cinemago-api .

# Run with environment variables
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="postgresql://..." \
  -e JWT__Key="YourSecretKey" \
  cinemago-api
```

---

## Step 3: Frontend Deployment (Vercel)

1. **Push code to GitHub** (if not already done)

2. **Deploy to Vercel:**
   - Go to [Vercel Dashboard](https://vercel.com/dashboard)
   - Click "Add New..." → "Project"
   - Import your GitHub repository
   - Configure:
     - Framework Preset: Next.js
     - Build Command: `next build`
     - Output Directory: `.next`
   - Click "Deploy"

3. **Set Environment Variables:**
   ```
   NEXT_PUBLIC_API_URL=https://your-render-backend.onrender.com/api
   ```

---

## Step 4: Verify Deployment

1. **Test the API:**
   - Visit `https://your-render-backend.onrender.com/api/movies/home`
   - Should return JSON with `success: true`

2. **Test the Frontend:**
   - Visit `https://your-vercel-frontend.vercel.app`
   - Should see the CinemaGo homepage

---

## Default Accounts

After deployment, you can create an admin user via the registration page, then manually update their role in the database:

```sql
UPDATE "Users" SET "Role" = 'Admin' WHERE "Email" = 'your-admin-email@example.com';
```

---

## Troubleshooting

### Backend Issues
- **Database connection failed:** Check your Neon connection string
- **JWT errors:** Ensure JWT__Key is set to a strong random string

### Frontend Issues
- **API not connecting:** Check NEXT_PUBLIC_API_URL is correct
- **CORS errors:** Ensure backend allows requests from your Vercel URL

---

## Local Development

To run locally:

1. **Backend:**
   ```bash
   cd CinemaGo.Api
   dotnet run
   ```

2. **Frontend:**
   ```bash
   cd frontend
   npm run dev
   ```

3. **Configure .env.local in frontend:**
   ```
   NEXT_PUBLIC_API_URL=http://localhost:5000/api
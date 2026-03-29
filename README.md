# CinemaGo - Movie Platform

A full-stack movie platform with OTT streaming and cinema ticket booking capabilities, built with ASP.NET Core Web API and Next.js.

![CinemaGo](https://img.shields.io/badge/.NET-8.0-blue) ![Next.js](https://img.shields.io/badge/Next.js-14-black) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791)

## Features

### 🎬 OTT Platform
- Browse and search movies
- Purchase individual movies
- Watch purchased movies
- Rate movies (1-10 scale)

### 🎟️ Cinema Booking
- Browse currently showing movies
- View showtimes by date and branch
- Interactive seat selection
- Multiple seat categories (Regular, Premium, VIP, Recliner)
- Real-time seat availability

### 👥 User Features
- User registration and login
- Profile management
- Purchase history (OTT)
- Ticket history (Cinema)
- Movie ratings

### ⚙️ Admin Features
- Dashboard with statistics
- Movie management (OTT & Cinema)
- Hall management with seat layouts
- Showtime scheduling
- Booking management

## Tech Stack

### Backend
- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Role-based Authorization

### Frontend
- Next.js 14 (App Router)
- Tailwind CSS
- Zustand (State Management)
- TypeScript

## Project Structure

```
CinemaGo/
├── CinemaGo.Api/          # ASP.NET Core Backend
│   ├── Controllers/       # API Controllers
│   ├── Models/            # Entity Models
│   ├── DTOs/              # Data Transfer Objects
│   ├── Services/          # Business Logic
│   ├── Data/              # DbContext & Migrations
│   └── ...
├── frontend/              # Next.js Frontend
│   ├── src/
│   │   ├── app/          # Pages
│   │   ├── components/   # React Components
│   │   ├── lib/          # Utilities
│   │   └── store/        # State Management
│   └── ...
├── Dockerfile             # Docker configuration
├── DEPLOYMENT.md         # Deployment guide
└── README.md
```

## Getting Started

### Prerequisites
- Node.js 18+
- .NET 8 SDK
- PostgreSQL (local or cloud)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/cinemago.git
   cd cinemago
   ```

2. **Set up the Backend**
   ```bash
   cd CinemaGo.Api
   
   # Update appsettings.json with your database connection
   # "ConnectionStrings": {
   #   "DefaultConnection": "Host=localhost;Database=cinemago;Username=postgres;Password=yourpassword"
   # }
   
   dotnet restore
   dotnet run
   ```

3. **Set up the Frontend**
   ```bash
   cd frontend
   
   # Create .env.local
   # NEXT_PUBLIC_API_URL=http://localhost:5000/api
   
   npm install
   npm run dev
   ```

4. **Open the application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000/api

## Deployment

### Deploy to Production

See [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed deployment instructions.

**Quick Summary:**
1. **Database:** Create a PostgreSQL database on [Neon](https://neon.tech)
2. **Backend:** Deploy to [Render](https://render.com) using Docker
3. **Frontend:** Deploy to [Vercel](https://vercel.com)

### Environment Variables

**Backend (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://..."
  },
  "Jwt": {
    "Key": "YourSecretKey123!",
    "Issuer": "CinemaGo",
    "Audience": "CinemaGoUsers"
  }
}
```

**Frontend (.env.local):**
```
NEXT_PUBLIC_API_URL=https://your-backend-api.com/api
```

## Default Login

After seeding, you can log in with:

| Role  | Email              | Password |
|-------|--------------------|----------|
| Admin | admin@cinemago.com | admin123 |
| User  | user@cinemago.com  | user123  |

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Get current user

### Movies
- `GET /api/movies/home` - Get homepage data
- `GET /api/movies` - List movies
- `GET /api/movies/{slug}` - Get movie details

### Cinema
- `GET /api/cinema/showtimes` - Get showtimes
- `GET /api/cinema/showtimes/{id}` - Get seats
- `POST /api/cinema/book` - Book tickets

### OTT
- `POST /api/ott/purchase` - Purchase movie
- `GET /api/ott/library` - Get purchased movies
- `GET /api/ott/watch/{id}` - Get video URL

### Admin
- `GET /api/admin/dashboard` - Dashboard stats
- `GET /api/admin/movies` - Manage movies
- `GET /api/admin/halls` - Manage halls
- `GET /api/admin/showtimes` - Manage showtimes
- `GET /api/admin/bookings` - View bookings

## License

MIT License - see LICENSE for details.
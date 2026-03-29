# CinemaGo — Detailed System Documentation

## 1. Project Overview

**CinemaGo** is a movie platform with two core business areas inside one system:

1. **OTT Platform**  
   Users can browse, purchase, and watch movies online, similar to Netflix, Chorki, or Bongo.
2. **Cinema Ticket Booking Platform**  
   Users can browse currently running movies, view shows by hall and time, inspect seat maps, and book seats like Star Cineplex.

The platform has two main account types:
- **Admin**
- **User**

Guest users can browse public content, but **cannot purchase OTT movies or cinema tickets without logging in**.

---

## 2. Proposed Tech Stack

### Frontend
- **Next.js**
- Tailwind CSS
- Axios or Fetch API
- Zustand or Context API for lightweight state management

### Backend
- **ASP.NET Core Web API**
- Entity Framework Core
- JWT Authentication
- Role-based authorization

### Database
- **PostgreSQL**

### Dummy Payment
- Internal simulated payment service
- Payment success/failure handled by backend logic

---

## 3. High-Level Modules

### Public Modules
- Home page
- Movie discovery
- OTT movie details
- Cinema movie details
- Showtimes browsing
- Authentication pages

### User Modules
- Register / Login / Logout
- Profile page
- OTT purchase history
- Watched / purchased OTT library
- Cinema ticket history
- My bookings
- Rating movies

### Admin Modules
- Admin login
- Dashboard
- Manage OTT movies
- Manage cinema movies
- Manage halls
- Manage showtimes
- Manage seat layouts
- View bookings
- View payments
- Manage users

---

## 4. Core Business Features

## 4.1 OTT Section
Users can:
- Browse OTT movies
- Search and filter movies
- View movie details
- Purchase a movie
- Watch purchased movie
- Rate a movie

Admin can:
- Add OTT movie
- Update OTT movie
- Remove OTT movie
- Set price
- Upload poster/banner/video URL/trailer URL
- Mark movie as featured or published/unpublished

### Important Note
This system keeps OTT lightweight:
- No subscription plan in MVP
- Purchase is **single-movie based**
- No comments/reviews text, only **numeric rating**

---

## 4.2 Cinema Booking Section
Users can:
- Browse currently running cinema movies
- View available showtimes
- Select hall and date
- View seat map
- Select seats
- See occupied seats in **red** and available seats in **green**
- Confirm booking after dummy payment
- View booked ticket in profile

Admin can:
- Add cinema movie
- Add halls
- Define seat layouts
- Create showtimes
- Set prices by seat category
- View bookings

---

## 5. User Roles and Permissions

## 5.1 Guest
Can:
- Browse OTT movies
- Browse cinema movies
- View movie details
- View showtimes
- View seat availability

Cannot:
- Purchase OTT movie
- Book cinema ticket
- Rate movie
- Access profile

## 5.2 User
Can:
- Register/login
- Buy OTT movie
- Watch purchased OTT movie
- Rate movie
- Book cinema seats
- View purchase history
- View tickets
- Manage profile

## 5.3 Admin
Can do everything a user can, plus:
- Manage movies
- Manage showtimes
- Manage halls and seats
- View all payments
- View all orders/bookings
- Manage platform data

---

## 6. Suggested Database Entities

## 6.1 Users
- Id
- FullName
- Email
- PasswordHash
- Phone
- Role (Admin/User)
- IsActive
- CreatedAt
- UpdatedAt

## 6.2 Movies
Single movie table can be used with type flags.
- Id
- Title
- Slug
- Description
- DurationMinutes
- ReleaseDate
- Language
- Country
- Genre
- AgeRating
- IMDbLikeScore (cached average)
- PosterUrl
- BannerUrl
- TrailerUrl
- VideoUrl
- MovieType (`OTT`, `CINEMA`, `BOTH`)
- OttPrice
- IsPublished
- IsFeatured
- CreatedAt
- UpdatedAt

## 6.3 Ratings
- Id
- UserId
- MovieId
- Score (1-10 or 1-5, recommended 1-10)
- CreatedAt

**Rule:** one user can rate one movie once, but can update later.

## 6.4 Halls
- Id
- Name
- BranchName
- TotalRows
- TotalSeatsPerRow
- IsActive
- CreatedAt

## 6.5 SeatCategories
- Id
- HallId
- Name (`Regular`, `Premium`, `VIP`, `Recliner`)
- PriceMultiplier or FixedExtraPrice

## 6.6 HallSeats
- Id
- HallId
- SeatLabel (A1, A2, B1...)
- RowLabel
- SeatNumber
- CategoryId
- IsActive

## 6.7 ShowTimes
- Id
- MovieId
- HallId
- ShowDate
- StartTime
- EndTime
- BasePrice
- Status (`Scheduled`, `Running`, `Closed`, `Cancelled`)
- CreatedAt

## 6.8 SeatReservations
- Id
- ShowTimeId
- SeatId
- UserId
- Status (`Reserved`, `Booked`, `Cancelled`)
- LockedUntil (optional for temporary hold)
- Price
- CreatedAt

## 6.9 OttPurchases
- Id
- UserId
- MovieId
- Amount
- PaymentId
- PurchasedAt

## 6.10 TicketOrders
- Id
- UserId
- ShowTimeId
- BookingCode
- TotalAmount
- PaymentId
- Status (`Pending`, `Paid`, `Cancelled`)
- CreatedAt

## 6.11 TicketOrderItems
- Id
- TicketOrderId
- SeatId
- SeatLabel
- UnitPrice

## 6.12 Payments
- Id
- UserId
- PaymentType (`OTT`, `TICKET`)
- ReferenceType (`OttPurchase`, `TicketOrder`)
- ReferenceId
- Amount
- Method (`DummyCard`, `DummyWallet`, `DummyBkash`)
- Status (`Pending`, `Success`, `Failed`)
- TransactionCode
- CreatedAt

---

## 7. Recommended Folder Structure

## 7.1 Backend (ASP.NET Core)

```text
CinemaGo.Api/
 ┣ Controllers/
 ┃ ┣ AuthController.cs
 ┃ ┣ MoviesController.cs
 ┃ ┣ OttController.cs
 ┃ ┣ CinemaController.cs
 ┃ ┣ RatingsController.cs
 ┃ ┣ PaymentsController.cs
 ┃ ┣ ProfileController.cs
 ┃ ┗ Admin/
 ┃    ┣ AdminMoviesController.cs
 ┃    ┣ AdminHallsController.cs
 ┃    ┣ AdminShowTimesController.cs
 ┃    ┣ AdminBookingsController.cs
 ┃    ┗ AdminUsersController.cs
 ┣ Models/
 ┣ DTOs/
 ┣ Data/
 ┣ Services/
 ┣ Repositories/
 ┣ Mappings/
 ┣ Middleware/
 ┣ Helpers/
 ┗ Program.cs
```

## 7.2 Frontend (Next.js)

```text
src/
 ┣ app/
 ┃ ┣ page.tsx
 ┃ ┣ login/page.tsx
 ┃ ┣ register/page.tsx
 ┃ ┣ ott/page.tsx
 ┃ ┣ ott/[slug]/page.tsx
 ┃ ┣ watch/[movieId]/page.tsx
 ┃ ┣ cinema/page.tsx
 ┃ ┣ cinema/[slug]/page.tsx
 ┃ ┣ booking/[showTimeId]/page.tsx
 ┃ ┣ profile/page.tsx
 ┃ ┣ profile/purchases/page.tsx
 ┃ ┣ profile/tickets/page.tsx
 ┃ ┗ admin/
 ┃    ┣ page.tsx
 ┃    ┣ movies/page.tsx
 ┃    ┣ halls/page.tsx
 ┃    ┣ showtimes/page.tsx
 ┃    ┗ bookings/page.tsx
 ┣ components/
 ┃ ┣ movie/
 ┃ ┣ booking/
 ┃ ┣ layout/
 ┃ ┣ forms/
 ┃ ┗ ui/
 ┣ lib/
 ┃ ┣ api.ts
 ┃ ┣ auth.ts
 ┃ ┗ utils.ts
 ┗ store/
```

---

## 8. Authentication Design

- JWT-based auth
- Access token returned after login
- Role embedded inside JWT claims
- Frontend stores token securely (for MVP, localStorage is acceptable; production should prefer safer patterns)

### Auth Header
```http
Authorization: Bearer <jwt_token>
```

---

## 9. Standard API Response Format

Use one consistent response style for lightweight frontend integration.

### Success Response
```json
{
  "success": true,
  "message": "Request successful",
  "data": {}
}
```

### Error Response
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "email": ["Email is required"]
  }
}
```

---

## 10. API Documentation

# 10.1 Auth APIs

## POST /api/auth/register
Create a new user account.

### Request
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phone": "01700000000",
  "password": "123456",
  "confirmPassword": "123456"
}
```

### Response
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "user": {
      "id": 1,
      "fullName": "John Doe",
      "email": "john@example.com",
      "phone": "01700000000",
      "role": "User"
    },
    "token": "jwt_token_here"
  }
}
```

## POST /api/auth/login

### Request
```json
{
  "email": "admin@cinemago.com",
  "password": "123456"
}
```

### Response
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "fullName": "CinemaGo Admin",
      "email": "admin@cinemago.com",
      "role": "Admin"
    },
    "token": "jwt_token_here"
  }
}
```

## GET /api/auth/me
Returns current logged in user.

### Response
```json
{
  "success": true,
  "message": "Current user fetched",
  "data": {
    "id": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phone": "01700000000",
    "role": "User"
  }
}
```

---

# 10.2 Public Movie APIs

## GET /api/movies/home
Returns homepage sections.

### Response
```json
{
  "success": true,
  "message": "Home data fetched",
  "data": {
    "featured": [
      {
        "id": 1,
        "title": "The Silent Code",
        "slug": "the-silent-code",
        "posterUrl": "https://...",
        "bannerUrl": "https://...",
        "movieType": "BOTH",
        "averageRating": 8.4
      }
    ],
    "ottTrending": [],
    "cinemaRunning": []
  }
}
```

## GET /api/movies?type=OTT&search=code&genre=thriller&page=1&pageSize=12

### Response
```json
{
  "success": true,
  "message": "Movies fetched",
  "data": {
    "items": [
      {
        "id": 1,
        "title": "The Silent Code",
        "slug": "the-silent-code",
        "posterUrl": "https://...",
        "movieType": "OTT",
        "ottPrice": 299,
        "averageRating": 8.4,
        "genres": ["Thriller", "Drama"]
      }
    ],
    "page": 1,
    "pageSize": 12,
    "totalCount": 50
  }
}
```

## GET /api/movies/{id}

### Response
```json
{
  "success": true,
  "message": "Movie details fetched",
  "data": {
    "id": 1,
    "title": "The Silent Code",
    "slug": "the-silent-code",
    "description": "A suspense thriller...",
    "durationMinutes": 130,
    "releaseDate": "2026-01-15",
    "language": "Bangla",
    "country": "Bangladesh",
    "genre": ["Thriller", "Drama"],
    "ageRating": "PG-13",
    "posterUrl": "https://...",
    "bannerUrl": "https://...",
    "trailerUrl": "https://...",
    "movieType": "BOTH",
    "ottPrice": 299,
    "averageRating": 8.4,
    "ratingCount": 124
  }
}
```

---

# 10.3 Rating APIs

## POST /api/ratings
Authenticated user rates a movie.

### Request
```json
{
  "movieId": 1,
  "score": 9
}
```

### Response
```json
{
  "success": true,
  "message": "Rating submitted",
  "data": {
    "movieId": 1,
    "userScore": 9,
    "averageRating": 8.5,
    "ratingCount": 125
  }
}
```

## GET /api/ratings/movie/{movieId}

### Response
```json
{
  "success": true,
  "message": "Movie ratings fetched",
  "data": {
    "movieId": 1,
    "averageRating": 8.5,
    "ratingCount": 125,
    "userRating": 9
  }
}
```

---

# 10.4 OTT APIs

## GET /api/ott/library
Returns purchased OTT movies of current user.

### Response
```json
{
  "success": true,
  "message": "OTT library fetched",
  "data": [
    {
      "purchaseId": 1,
      "movieId": 1,
      "title": "The Silent Code",
      "posterUrl": "https://...",
      "purchasedAt": "2026-03-25T10:30:00Z",
      "videoUrl": "https://stream-url..."
    }
  ]
}
```

## POST /api/ott/purchase
Creates OTT purchase intent before payment.

### Request
```json
{
  "movieId": 1,
  "paymentMethod": "DummyBkash"
}
```

### Response
```json
{
  "success": true,
  "message": "OTT purchase initiated",
  "data": {
    "purchaseReference": "OTT-20260329-0001",
    "movieId": 1,
    "title": "The Silent Code",
    "amount": 299,
    "paymentStatus": "Pending"
  }
}
```

## POST /api/payments/process
Used for both OTT and ticket payment.

### Request
```json
{
  "referenceType": "OTT",
  "referenceCode": "OTT-20260329-0001",
  "method": "DummyBkash",
  "dummyAccount": "01711111111",
  "dummyPin": "1234",
  "shouldSucceed": true
}
```

### Response
```json
{
  "success": true,
  "message": "Payment successful",
  "data": {
    "paymentId": 19,
    "transactionCode": "TXN-OTT-934820",
    "status": "Success"
  }
}
```

## GET /api/ott/watch/{movieId}
Only accessible if movie was purchased.

### Response
```json
{
  "success": true,
  "message": "Streaming access granted",
  "data": {
    "movieId": 1,
    "title": "The Silent Code",
    "videoUrl": "https://streaming-url...",
    "subtitleUrl": "https://subtitle-url..."
  }
}
```

## GET /api/ott/purchases

### Response
```json
{
  "success": true,
  "message": "OTT purchase history fetched",
  "data": [
    {
      "purchaseId": 1,
      "movieId": 1,
      "title": "The Silent Code",
      "amount": 299,
      "paymentStatus": "Success",
      "purchasedAt": "2026-03-25T10:30:00Z"
    }
  ]
}
```

---

# 10.5 Cinema APIs

## GET /api/cinema/running
Returns running cinema movies.

### Response
```json
{
  "success": true,
  "message": "Running movies fetched",
  "data": [
    {
      "id": 5,
      "title": "Shadow Bridge",
      "slug": "shadow-bridge",
      "posterUrl": "https://...",
      "durationMinutes": 145,
      "averageRating": 7.9
    }
  ]
}
```

## GET /api/cinema/movies/{movieId}/showtimes?date=2026-03-30

### Response
```json
{
  "success": true,
  "message": "Showtimes fetched",
  "data": [
    {
      "showTimeId": 11,
      "hallId": 2,
      "hallName": "Hall 2",
      "branchName": "Bashundhara",
      "startTime": "2026-03-30T14:00:00",
      "endTime": "2026-03-30T16:25:00",
      "basePrice": 350,
      "availableSeats": 42,
      "totalSeats": 80
    }
  ]
}
```

## GET /api/cinema/showtimes/{showTimeId}/seatmap
Returns full seat map with booking status.

### Response
```json
{
  "success": true,
  "message": "Seat map fetched",
  "data": {
    "showTimeId": 11,
    "movieTitle": "Shadow Bridge",
    "hallName": "Hall 2",
    "branchName": "Bashundhara",
    "startTime": "2026-03-30T14:00:00",
    "seatLayout": [
      {
        "rowLabel": "A",
        "seats": [
          {
            "seatId": 101,
            "seatLabel": "A1",
            "category": "VIP",
            "price": 600,
            "status": "Available"
          },
          {
            "seatId": 102,
            "seatLabel": "A2",
            "category": "VIP",
            "price": 600,
            "status": "Booked"
          }
        ]
      },
      {
        "rowLabel": "B",
        "seats": []
      }
    ],
    "legend": {
      "available": "green",
      "booked": "red",
      "selected": "yellow"
    }
  }
}
```

## POST /api/cinema/bookings/initiate
Create temporary booking request.

### Request
```json
{
  "showTimeId": 11,
  "seatIds": [101, 103, 104],
  "paymentMethod": "DummyCard"
}
```

### Response
```json
{
  "success": true,
  "message": "Booking initiated",
  "data": {
    "bookingReference": "TKT-20260329-0007",
    "showTimeId": 11,
    "seats": ["A1", "A3", "A4"],
    "totalAmount": 1800,
    "paymentStatus": "Pending",
    "expiresInSeconds": 300
  }
}
```

## POST /api/payments/process
Cinema booking payment uses same payment endpoint.

### Request
```json
{
  "referenceType": "TICKET",
  "referenceCode": "TKT-20260329-0007",
  "method": "DummyCard",
  "dummyCardNumber": "4111111111111111",
  "dummyExpiry": "12/30",
  "dummyCvv": "123",
  "shouldSucceed": true
}
```

### Response
```json
{
  "success": true,
  "message": "Payment successful",
  "data": {
    "paymentId": 25,
    "transactionCode": "TXN-TKT-223344",
    "status": "Success",
    "ticketOrderId": 9
  }
}
```

## GET /api/cinema/bookings/my

### Response
```json
{
  "success": true,
  "message": "My bookings fetched",
  "data": [
    {
      "ticketOrderId": 9,
      "bookingCode": "CG-TKT-0009",
      "movieTitle": "Shadow Bridge",
      "hallName": "Hall 2",
      "branchName": "Bashundhara",
      "showDate": "2026-03-30",
      "startTime": "14:00",
      "seats": ["A1", "A3", "A4"],
      "totalAmount": 1800,
      "status": "Paid",
      "bookedAt": "2026-03-29T08:45:00Z"
    }
  ]
}
```

## GET /api/cinema/bookings/{ticketOrderId}

### Response
```json
{
  "success": true,
  "message": "Booking details fetched",
  "data": {
    "ticketOrderId": 9,
    "bookingCode": "CG-TKT-0009",
    "movieTitle": "Shadow Bridge",
    "posterUrl": "https://...",
    "branchName": "Bashundhara",
    "hallName": "Hall 2",
    "showDate": "2026-03-30",
    "startTime": "14:00",
    "endTime": "16:25",
    "seats": ["A1", "A3", "A4"],
    "totalAmount": 1800,
    "status": "Paid",
    "qrText": "CG-TKT-0009"
  }
}
```

---

# 10.6 Profile APIs

## GET /api/profile

### Response
```json
{
  "success": true,
  "message": "Profile fetched",
  "data": {
    "id": 3,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phone": "01700000000",
    "role": "User",
    "joinedAt": "2026-03-01T00:00:00Z"
  }
}
```

## PUT /api/profile

### Request
```json
{
  "fullName": "John D.",
  "phone": "01800000000"
}
```

### Response
```json
{
  "success": true,
  "message": "Profile updated",
  "data": {
    "id": 3,
    "fullName": "John D.",
    "email": "john@example.com",
    "phone": "01800000000"
  }
}
```

## GET /api/profile/history
Combined purchase history.

### Response
```json
{
  "success": true,
  "message": "History fetched",
  "data": {
    "ottPurchases": [
      {
        "purchaseId": 1,
        "movieTitle": "The Silent Code",
        "amount": 299,
        "purchasedAt": "2026-03-25T10:30:00Z"
      }
    ],
    "ticketBookings": [
      {
        "ticketOrderId": 9,
        "movieTitle": "Shadow Bridge",
        "bookingCode": "CG-TKT-0009",
        "totalAmount": 1800,
        "bookedAt": "2026-03-29T08:45:00Z"
      }
    ]
  }
}
```

---

# 10.7 Admin APIs

All admin routes require:
- Authenticated admin user
- Role = Admin

## POST /api/admin/movies
Create a new movie.

### Request
```json
{
  "title": "Shadow Bridge",
  "description": "Thriller movie description",
  "durationMinutes": 145,
  "releaseDate": "2026-02-20",
  "language": "English",
  "country": "USA",
  "genre": ["Thriller", "Mystery"],
  "ageRating": "PG-13",
  "posterUrl": "https://...",
  "bannerUrl": "https://...",
  "trailerUrl": "https://...",
  "videoUrl": "https://...",
  "movieType": "BOTH",
  "ottPrice": 299,
  "isPublished": true,
  "isFeatured": true
}
```

### Response
```json
{
  "success": true,
  "message": "Movie created",
  "data": {
    "id": 15,
    "title": "Shadow Bridge"
  }
}
```

## PUT /api/admin/movies/{id}

### Request
```json
{
  "title": "Shadow Bridge Reloaded",
  "ottPrice": 349,
  "isFeatured": false
}
```

### Response
```json
{
  "success": true,
  "message": "Movie updated",
  "data": {
    "id": 15,
    "title": "Shadow Bridge Reloaded"
  }
}
```

## DELETE /api/admin/movies/{id}

### Response
```json
{
  "success": true,
  "message": "Movie deleted",
  "data": null
}
```

## POST /api/admin/halls
Create a hall.

### Request
```json
{
  "name": "Hall 1",
  "branchName": "Bashundhara",
  "totalRows": 8,
  "totalSeatsPerRow": 10
}
```

### Response
```json
{
  "success": true,
  "message": "Hall created",
  "data": {
    "id": 2,
    "name": "Hall 1"
  }
}
```

## POST /api/admin/halls/{hallId}/seats/generate
Generate seat map for a hall.

### Request
```json
{
  "categories": [
    {
      "name": "Regular",
      "rows": ["D", "E", "F", "G", "H"],
      "priceMultiplier": 1.0
    },
    {
      "name": "Premium",
      "rows": ["B", "C"],
      "priceMultiplier": 1.3
    },
    {
      "name": "VIP",
      "rows": ["A"],
      "priceMultiplier": 1.8
    }
  ]
}
```

### Response
```json
{
  "success": true,
  "message": "Seats generated successfully",
  "data": {
    "hallId": 2,
    "totalSeats": 80
  }
}
```

## POST /api/admin/showtimes
Create a showtime.

### Request
```json
{
  "movieId": 15,
  "hallId": 2,
  "showDate": "2026-03-30",
  "startTime": "14:00",
  "basePrice": 350
}
```

### Response
```json
{
  "success": true,
  "message": "Showtime created",
  "data": {
    "id": 11,
    "movieId": 15,
    "hallId": 2
  }
}
```

## GET /api/admin/bookings

### Response
```json
{
  "success": true,
  "message": "Bookings fetched",
  "data": {
    "items": [
      {
        "ticketOrderId": 9,
        "bookingCode": "CG-TKT-0009",
        "userName": "John Doe",
        "movieTitle": "Shadow Bridge",
        "showDate": "2026-03-30",
        "startTime": "14:00",
        "status": "Paid",
        "totalAmount": 1800
      }
    ],
    "totalCount": 1
  }
}
```

## GET /api/admin/payments

### Response
```json
{
  "success": true,
  "message": "Payments fetched",
  "data": {
    "items": [
      {
        "paymentId": 25,
        "userName": "John Doe",
        "paymentType": "TICKET",
        "amount": 1800,
        "method": "DummyCard",
        "status": "Success",
        "transactionCode": "TXN-TKT-223344",
        "createdAt": "2026-03-29T08:45:00Z"
      }
    ]
  }
}
```

---

## 11. Dummy Payment Gateway Design

To keep it simple:
- Payment is not integrated with real provider in MVP
- Frontend shows fake payment form
- Backend receives payment payload
- If `shouldSucceed = true`, backend marks success
- If false, backend marks failure

### Supported Dummy Methods
- DummyCard
- DummyBkash
- DummyWallet

### Dummy Logic
- Valid form + `shouldSucceed=true` => success
- Valid form + `shouldSucceed=false` => fail

This is enough for demo, testing, university project, or MVP proof-of-concept.

---

## 12. Booking Flow Logic

## 12.1 OTT Purchase Flow
1. User opens OTT movie details
2. Clicks **Buy Now**
3. Frontend calls `POST /api/ott/purchase`
4. Backend creates pending purchase reference
5. Frontend opens dummy payment modal/page
6. Frontend calls `POST /api/payments/process`
7. Backend marks payment success
8. Purchase becomes available in user library
9. User can watch movie from profile or watch page

## 12.2 Cinema Booking Flow
1. User opens cinema movie details
2. Selects date/showtime
3. Frontend loads seat map
4. User selects seats
5. Frontend calls `POST /api/cinema/bookings/initiate`
6. Backend verifies seats are available and creates pending booking
7. Frontend opens dummy payment page
8. Frontend calls `POST /api/payments/process`
9. Backend confirms booking and locks seats as booked
10. Ticket saved in user profile

---

## 13. Validation Rules

### Auth
- Email must be unique
- Password minimum 6 characters

### Ratings
- Score range: 1 to 10
- One user rating per movie

### OTT Purchase
- User cannot purchase same movie twice unless business rule allows repurchase
- Recommended: if already purchased, return existing access

### Booking
- Must select at least 1 seat
- Cannot book already booked seats
- Showtime must not be closed/cancelled
- Booking expires if payment not completed within configured time

---

## 14. Error Handling Cases

### Example: seat already booked
```json
{
  "success": false,
  "message": "One or more seats are no longer available",
  "errors": {
    "seatIds": ["A2 is already booked"]
  }
}
```

### Example: OTT access denied
```json
{
  "success": false,
  "message": "You have not purchased this movie"
}
```

### Example: unauthorized
```json
{
  "success": false,
  "message": "Unauthorized"
}
```

---

## 15. UI Plan

# 15.1 Public UI Structure

## Navbar
- Logo: CinemaGo
- Home
- OTT
- Cinema
- Search bar
- Login/Register or User Avatar

## Home Page Sections
- Hero banner with featured movie
- OTT Trending
- Running in Cinemas
- New Releases
- Popular by rating

### Style Direction
- Modern dark UI
- Netflix + IMDb inspired layout
- Large banners
- Poster carousels
- Minimal but premium look

---

# 15.2 OTT UI Pages

## OTT Listing Page
Cards should show:
- Poster
- Title
- Genre
- Rating
- Price
- Watch/Buy badge

Filters:
- Genre
- Language
- Rating
- Search

## OTT Movie Details Page
Sections:
- Banner backdrop
- Poster
- Title
- Rating
- Duration
- Genre
- Description
- Trailer button
- Buy Now button
- Watch Now button (if already purchased)
- Rating input component

## Watch Page
- Video player area
- Movie title
- Subtitle selector placeholder
- Continue watching info (optional later)

---

# 15.3 Cinema UI Pages

## Cinema Listing Page
Cards show:
- Poster
- Title
- Duration
- Rating
- Running now badge
- View Showtimes button

## Cinema Movie Details Page
Sections:
- Banner/poster
- Description
- Available dates
- Showtime cards
- Hall name
- Branch
- Starting price

## Seat Booking Page
Layout:
- Movie summary at top
- Hall + showtime info
- Screen indicator at top
- Seat grid in center
- Legend below:
  - Green = available
  - Red = occupied
  - Yellow = selected
- Price summary on right or bottom
- Proceed to payment button

### Seat UI Behavior
- Tap/click seat to select
- Booked seats disabled
- Different chair quality shown by label or border text:
  - Regular
  - Premium
  - VIP
  - Recliner

---

# 15.4 Profile UI

## Profile Dashboard
Tabs:
- My Profile
- OTT Library
- Purchase History
- My Tickets

## OTT Library Page
- Purchased movie cards
- Watch button

## Ticket History Page
- Ticket cards
- Booking code
- Seats
- Showtime
- Download/Print button (optional later)

---

# 15.5 Admin UI

## Admin Dashboard
Cards:
- Total users
- Total OTT purchases
- Total ticket bookings
- Total revenue
- Running movies

## Admin Movies Management
- Table view
- Add movie button
- Edit/Delete actions
- Filter by type: OTT / CINEMA / BOTH

## Admin Halls Management
- Create hall
- Generate seat layout
- Edit hall

## Admin Showtimes Management
- Create showtime form
- List of scheduled shows
- Edit/cancel showtime

## Admin Bookings & Payments
- Booking table
- Payment table
- Search by booking code / user / movie

---

## 16. Suggested MVP Scope

For first version, build only:
- User auth
- Movie listing/details
- OTT purchase with dummy payment
- Watch purchased OTT movie
- Cinema showtime list
- Seat map booking
- Ticket history
- Admin movie management
- Admin showtime management
- Admin hall/seat generation

Do not build in MVP:
- Subscription plans
- Real payment gateway
- Coupons
- Notifications
- Comments
- Refund system
- Advanced analytics
- Multi-language UI

---

## 17. Recommended Seed Data

### Admin Account
- Email: `admin@cinemago.com`
- Password: `123456`

### Sample Users
- `user1@test.com`
- `user2@test.com`

### Seed Movies
- 5 OTT only
- 5 Cinema only
- 3 Both

### Seed Halls
- Hall 1 – Bashundhara
- Hall 2 – Bashundhara
- Hall 1 – Dhanmondi

---

## 18. Security Notes for MVP

- Passwords must be hashed
- Admin routes protected by role-based authorization
- Watch API must verify purchase
- Booking API must verify seat availability server-side
- Payment finalization must happen only in backend

---

## 19. Recommended Development Order

### Phase 1
- Setup backend project
- Setup Next.js frontend
- Setup PostgreSQL schema
- JWT authentication

### Phase 2
- Public movie browsing
- Movie details page
- Admin movie CRUD

### Phase 3
- OTT purchase flow
- Dummy payment
- OTT library and watch page

### Phase 4
- Hall creation
- Seat generation
- Showtime creation
- Seat map API
- Cinema ticket booking

### Phase 5
- Profile pages
- Booking history
- Purchase history
- Admin reporting pages

---

## 20. Final Architectural Recommendation

To keep CinemaGo **easy and lightweight**, use these principles:

- One backend API project only
- One PostgreSQL database only
- Single `Movies` table with `movieType`
- Shared payment table for OTT and tickets
- Shared profile/history pages
- Dummy gateway instead of real payment integration
- Simple role system: `Admin`, `User`

This keeps the project clean, easier to build, and ideal for MVP or academic/demo use.

---

## 21. Optional Future Enhancements

Later you can add:
- Real payment integration (bKash, SSLCommerz, card)
- OTT subscription plans
- Promo codes
- Ticket QR scanner
- Email confirmation
- Push notifications
- Recommendation engine
- Favorites/watchlist
- Trailer autoplay
- Branch-specific admin roles

---

## 22. Final Summary

**CinemaGo** is a combined OTT + Cinema Ticket Booking platform with:
- ASP.NET backend
- Next.js frontend
- PostgreSQL database
- JWT auth
- Admin/user roles
- OTT purchase and watching
- Cinema seat booking with seat map
- Dummy payment gateway
- Profile with purchase and ticket history

This design is intentionally simple, practical, and lightweight while still feeling like a real product.


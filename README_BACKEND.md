# Nomadic backend setup (.NET 10)

The existing ASP.NET Core MVC application now uses SQL Server and ASP.NET Core Identity. The existing MVC pages and visual design are retained.

## Technology

- .NET 10 / ASP.NET Core MVC
- SQL Server LocalDB
- Entity Framework Core 10
- ASP.NET Core Identity for registration, login, password hashing, cookies, and logout
- JSON REST endpoints for travel destinations and packages

## Requirements

1. .NET 10 SDK (x64).
2. SQL Server Express LocalDB, normally available with Visual Studio's SQL Server data components. If LocalDB is not installed, install SQL Server Express LocalDB or change the connection string to your SQL Server instance.
3. Visual Studio 2026 or Visual Studio Code.

## Run

Open a terminal in the folder containing `Nomadic.csproj` and run:

```powershell
dotnet restore
dotnet run
```

The connection string is in `appsettings.json` under `ConnectionStrings:NomadicDatabase`:

```text
Server=(localdb)\MSSQLLocalDB;Database=NomadicDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

On first startup, `EnsureCreated()` creates the database and Identity tables. The application seeds the existing destination and package catalog into the database. Existing JSON bookings in `App_Data/bookings.json` are imported when the bookings table is empty.

For a different SQL Server instance, replace the connection string, for example:

```text
Server=localhost;Database=NomadicDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Account behavior

- Registration creates an Identity user with a unique email.
- Passwords are hashed by ASP.NET Core Identity and are never stored as plain text.
- Password policy: at least 8 characters, including uppercase, lowercase, and a digit.
- Login uses an authentication cookie; logout is a POST request protected by antiforgery validation.

## Database tables

- Identity tables: users, roles, claims, logins, and tokens managed by ASP.NET Core Identity.
- `Bookings`: customer and booking records.
- `TravelCatalog`: existing destinations and packages stored as JSON records so itinerary details and inclusions remain intact.

## REST endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/health` | API health |
| GET | `/api/destinations` | List destinations |
| GET | `/api/destinations/{id}` | Destination details |
| GET | `/api/packages` | List packages; optional `?destination=Goa` |
| GET | `/api/packages/{id}` | Package details |
| GET | `/api/bookings/{bookingId}` | Read a booking after login; account email must match the booking email |

## Important payment limitation

The current checkout remains a demo flow. It saves booking requests with `Pending Payment (Demo)` status. No card, UPI, bank, or wallet payment is processed. Do not use this demo flow to collect real payments. A verified payment gateway integration and server-side payment verification are required before production use.

## Notes

- The project targets `net10.0`.
- The app creates the initial database schema with `EnsureCreated()`. If you later change the schema for a deployed database, move to EF Core migrations and apply them through a controlled deployment process.
- Do not commit production connection strings, credentials, or payment secrets. Use environment variables or a secrets manager for deployment.

# Insurance Claims Processing & Settlement System
### ASP.NET Core MVC | EF Core 10.0.0 | SQL Server | Bootstrap 5

---

## Complete Claim Workflow

```
Customer registers → Buys Policy → Files Claim (REGISTERED)
       ↓
Customer uploads documents → Claim moves to UNDER_REVIEW
       ↓
Officer verifies documents (VERIFIED / REJECTED)
       ↓
Surveyor submits assessment (assessed amount + remarks)
       ↓
Officer runs Fraud Check (score 0-100, flag: LOW/MEDIUM/HIGH)
       ↓
Officer APPROVES claim → Status: APPROVED
       ↓
Officer processes Settlement → Payment disbursed
       OR
Officer REJECTS claim → Status: REJECTED (with reason)
```

---

## Project Structure

```
InsuranceClaims/
├── Controllers/           9 controllers (1 per feature)
├── Data/                  AppDbContext + DbInitializer
├── Database/              schema.sql (manual alternative)
├── Enums/                 AppEnums.cs (ClaimStatus, RiskFlag etc.)
├── Helpers/               PasswordHelper + SessionHelper
├── Models/
│   ├── Entities/          13 separate entity classes
│   └── ViewModels/        8 separate view model classes
├── Repositories/          10 repository classes
├── Services/
│   ├── Interfaces/        8 service interfaces
│   └── Implementations/   8 service implementations
├── Views/                 All Razor views per controller
├── wwwroot/uploads/       Uploaded documents stored here
├── Program.cs
└── appsettings.json
```

---

## Setup Instructions

### Prerequisites
- Visual Studio 2022 (v17.8+)
- .NET 10 SDK
- SQL Server / LocalDB

### Step 1 – Connection String
Edit `appsettings.json`:
```json
"DefaultConnection": "Server=localhost;Database=InsuranceClaimsDB;Trusted_Connection=True;TrustServerCertificate=True;"
```
For LocalDB:
```
Server=(localdb)\\mssqllocaldb;Database=InsuranceClaimsDB;Trusted_Connection=True;
```

### Step 2 – EF Migrations
```powershell
# In Package Manager Console
Add-Migration InitialCreate
Update-Database
```
OR via terminal:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 3 – Run
Press **F5** in Visual Studio or:
```bash
dotnet run
```
App auto-seeds admin account and 4 sample policies on first run.

---

## Login Credentials

| Role     | Email                   | Password  |
|----------|-------------------------|-----------|
| Admin    | admin@insurance.com     | Admin@123 |
| Officer  | Add via Admin Dashboard | Set by Admin |
| Surveyor | Add via Admin Dashboard | Set by Admin |
| Customer | Register yourself       | Your choice |

---

## Role Permissions

| Feature              | Customer | Officer | Surveyor | Admin |
|----------------------|----------|---------|----------|-------|
| Browse Policies      | ✅ | ✅ | ✅ | ✅ |
| Buy / Renew Policy   | ✅ | ❌ | ❌ | ❌ |
| File a Claim         | ✅ | ❌ | ❌ | ❌ |
| Upload Documents     | ✅ | ❌ | ❌ | ❌ |
| Track Claim          | ✅ | ❌ | ❌ | ❌ |
| Verify Documents     | ❌ | ✅ | ❌ | ❌ |
| View All Claims      | ❌ | ✅ | ❌ | ✅ |
| Approve/Reject       | ❌ | ✅ | ❌ | ✅ |
| Run Fraud Check      | ❌ | ✅ | ❌ | ✅ |
| Process Settlement   | ❌ | ✅ | ❌ | ✅ |
| Assess Claims        | ❌ | ❌ | ✅ | ❌ |
| Manage Officers      | ❌ | ❌ | ❌ | ✅ |
| Manage Surveyors     | ❌ | ❌ | ❌ | ✅ |

---

## Tech Stack
- **Framework:** ASP.NET Core MVC (.NET 10)
- **ORM:** Entity Framework Core 10.0.0
- **Database:** SQL Server
- **Auth:** Session-based + HMACSHA512 password hashing
- **UI:** Razor Views + Bootstrap 5.3 + Bootstrap Icons
- **Architecture:** Controller → Service Interface → Service Implementation → Repository → DbContext

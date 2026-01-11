# User Management API

A RESTful API built with ASP.NET Core to manage users.  
This project was developed, debugged, and enhanced with the help of Microsoft Copilot.

---

## ✨ Features
- **CRUD Endpoints**: Create, Read, Update, Delete users
- **Validation**: Ensures only valid user data is processed (e.g., required fields, valid email format)
- **Middleware**:
  - Logging middleware → logs all incoming requests and outgoing responses
  - Error-handling middleware → catches unhandled exceptions and returns JSON error responses
  - Authentication middleware → validates Bearer tokens and secures endpoints
- **Thread-safe storage** using `ConcurrentDictionary`
- **Pagination & Search** support in `GET /api/users`

---

## 🚀 Getting Started

### Prerequisites
- [.NET 6+ SDK](https://dotnet.microsoft.com/download)

### Run the API
```bash
dotnet run

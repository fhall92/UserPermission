# UserPermission API

A simple access management platform (Mini IAM) built with .NET 9, Entity Framework, and ASP.NET Core.  
This project provides RESTful endpoints for user registration, authentication, role assignment, and user querying.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

### Running Locally

1. **Clone the repository:**
```gh repo clone fhall92/UserPermission```

2. **Restore dependencies:**
```dotnet restore```

3. **Run the API:**
```dotnet run --project UserPermission.Api```

4. **Access Swagger UI:**  
   Open [http://localhost:5265/swagger](http://localhost:5265/swagger) in your browser (or the port shown in your console).

---

## 🧪 Running Tests
```dotnet test```


---

## 📖 API Endpoints

All endpoints accept and return `application/json`.

### 1. **Register a New User**
- **POST** `/users`
- **Body:**
  ```{ "name": "John Doe", "email": "john@example.com", "password": "password123" }```
- **Returns:**  
  `201 Created` with user info (excluding password).  
  `400 Bad Request` for validation errors.  
  `409 Conflict` if email already exists.

---

### 2. **Login (Simulated Authentication)**
- **POST** `/auth/login`
- **Body:**
  ```{ "email": "john@example.com", "password": "password123" }```
- **Returns:**  
  `200 OK` with user info if credentials are valid.  
  `401 Unauthorized` if invalid.

---

### 3. **Assign Role to User**
- **POST** `/users/{id}/roles`
- **Body:**
  ```{ "roleName": "admin" }```
- **Returns:**  
  `204 No Content` on success.  
  `404 Not Found` if user does not exist.  
  `400 Bad Request` for validation errors.

---

### 4. **Get User Info and Roles**
- **GET** `/users/{id}`
- **Returns:**  
  `200 OK` with user info and roles:
  ```{ "id": "user-guid", "name": "John Doe", "email": "john@example.com", "roles": ["admin", "user"] }```

  `404 Not Found` if user does not exist.

---

## 🛠️ Project Structure

- `UserPermission.Api` - API project (controllers, startup)
- `UserPermission.Core` - Core domain (entities, interfaces, DTOs)
- `UserPermission.Infrastructure` - Data access, repositories, security
- `UserPermission.Tests` - Unit tests

---

## 🔗 Helpful Links

- [Swagger UI (when running)](http://localhost:5265/swagger)

---

## 📝 Notes

- Uses in-memory database; no external DB required.
- Passwords are hashed using SHA256 (for demo purposes only).
- All endpoints require no authentication for simplicity.
- HTML Test Coverage Report available as a downloadable artifact from the latest Built & Test Actions Workflow.

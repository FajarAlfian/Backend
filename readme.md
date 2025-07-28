
# 🛠️ Online Course Platform - Backend API (Group 1)

This repository contains the **Backend API** for the Online Course Platform developed by **Group 1: Fajar Alfian S & Muti Salsabila**. The backend is built using **.NET** and connected to a **MySQL** database, handling authentication, course management, payments, and invoice generation.

---

## 🚀 Tech Stack

- **Backend Framework**: .NET (ASP.NET Core)
- **Language**: C#
- **Database**: MySQL
- **Authentication**: JWT (JSON Web Tokens)
- **ORM**: Entity Framework Core

---

## 📌 Key Features

- **Authentication**
  - User registration, login, logout
  - Email confirmation
  - Password reset

- **Course Management**
  - CRUD operations for courses
  - Retrieve course details, categories

- **User Dashboard**
  - View and manage enrolled classes
  - Class scheduling and history

- **Transaction System**
  - Checkout process
  - Payment method API integration
  - Invoice creation and retrieval

---

## 🔗 API Endpoints (Examples)

```
POST   /api/auth/register           # Register a new user
POST   /api/auth/login              # Authenticate user and issue JWT
GET    /api/courses                 # Retrieve all courses
GET    /api/courses/{id}            # Get a specific course by ID
POST   /api/checkout                # Handle course purchase
GET    /api/invoices/user/{userId}  # List user's invoices
```

---

## ⚙️ Running the Project Locally

```bash
# Clone the repository
git clone https://github.com/FajarAlfian/Backend.git
cd Backend

# Restore NuGet packages
dotnet restore

# Update appsettings.json with your MySQL connection string

# Run database in Database/db.sql

# Run the application
dotnet run
```

---

## 📁 Suggested Folder Structure

```
backend/
├── Controllers/         # API route handlers
├── Models/              # Entity models
├── Data/                # DB context and configurations
├── Services/            # Business logic services
├── Helpers/             # Utility functions and helpers
├── appsettings.json     # Configuration settings
└── Program.cs           # Main entry point
```

---

## 🎯 Purpose

This backend is the core service that powers the Online Course Platform frontend. It demonstrates the use of:

- Secure and scalable API development with ASP.NET Core
- Integration with MySQL databases using EF Core
- Full-featured authentication and role management
- Invoice and transaction tracking via RESTful endpoints

---

## 🤝 Team Members

- **Fajar Alfian S** 
- **Muti Salsabila** 

---

## 📬 Contact

For technical support or questions, feel free to reach out via [GitHub](https://github.com/FajarAlfian).

---

EduCore College Learning Management API

EduCore is a RESTful API for managing college learning management tasks. It supports role-based users (Admin, Lecturer, Student), secure authentication, course management, enrollment tracking, and email notifications.

✨ Features

Role-based user management: Admin, Lecturer, Student via ASP.NET Identity

User login and password management

Course creation and retrieval (Lecturer/Admin)

Student enrollment and progress tracking (Student/Admin)

JWT-secured endpoints for role-based access

Email notifications via SMTP

Database management with Entity Framework Core migrations

🛠 Tech Stack
Backend	ASP.NET Web API
Authentication	ASP.NET Identity + JWT
Database	Azure SQL (EF Core migrations)
Email Service	SMTP
Documentation	Swagger (OpenAPI)

⚡ Setup
1. Clone the repository
git clone https://github.com/yourusername/educore-api.git
cd educore-api

2. Restore & build
dotnet restore
dotnet build

3. Configure your appsettings.json

Add your DB connection string, JWT settings, and SMTP configuration.

4. Apply EF migrations
dotnet ef database update

5. Run API
dotnet run

6. Swagger Documentation

Access at:

https://localhost:5001/swagger

🔑 API Endpoints
Admin

CRUD operations for students, lecturers, courses, modules

Assign lecturers or students to courses/modules

Get course enrollments, student courses, and search endpoints

Examples:

GET /api/Admin/students
POST /api/Admin/courses
POST /api/Admin/courses/{courseId}/assign-lecturer/{lecturerId}
GET /api/Admin/students/{studentId}/courses

Auth
POST /api/Auth/login
POST /api/Auth/change-password
POST /api/Auth/reset-password
POST /api/Auth/refresh

Lecturer

View own courses and modules

Manage tasks within modules

Examples:

GET /api/Lecturer/courses/{lecturerId}
POST /api/Lecturer/modules/{moduleId}/tasks
PUT /api/Lecturer/tasks/{taskId}

Student

View own courses, modules, and tasks

Update task status

Examples:

GET /api/Student/{studentId}/courses
PUT /api/Student/{studentId}/tasks/{taskId}/status


Note: JWT is required for protected routes via Authorization: Bearer <token> header.

🎯 Roles & Permissions
Role	Permissions
Admin	Full access: manage users, courses, enrollments
Lecturer	Manage courses & tasks, view enrollments
Student	View courses, modules, tasks, update own tasks
📦 License

MIT License – See LICENSE

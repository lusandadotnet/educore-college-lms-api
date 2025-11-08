EduCore College Learning Management API

EduCore is a RESTful API for managing college learning management tasks, built with ASP.NET Web API, ASP.NET Identity, JWT authentication, Azure SQL, Entity Framework, and SMTP email notifications.

#Features:

Role-based user management: Admin, Lecturer, Student via ASP.NET Identity

User login

Course creation, updates, and retrieval (Lecturer/Admin)

Student enrollment and progress tracking (Student/Admin)

JWT-secured endpoints for role-based access

Email notifications via SMTP

Database management with Entity Framework migrations

#Tech Stack:

Backend: ASP.NET Web API

Authentication: ASP.NET Identity + JWT

Database: Azure SQL (EF Core for migrations)

Email Service: SMTP
Documentation: Swagger (OpenApi) for testing

Setup

Clone repo:

git clone https://github.com/yourusername/educore-api.git
cd educore-api


Restore & build:

dotnet restore
dotnet build


Configure appsettings.json with your DB, JWT, and SMTP settings.

Apply EF migrations:

dotnet ef database update


Run API:

dotnet run


Access Swagger: https://localhost:5001/swagger

API Endpoints: 

Admin


GET
/api/Admin/students



POST
/api/Admin/students



PUT
/api/Admin/students/{id}



DELETE
/api/Admin/students/{id}



GET
/api/Admin/lecturers



POST
/api/Admin/lecturers



PUT
/api/Admin/lecturers/{id}



DELETE
/api/Admin/lecturers/{id}



GET
/api/Admin/courses



POST
/api/Admin/courses



PUT
/api/Admin/courses/{id}



DELETE
/api/Admin/courses/{id}



GET
/api/Admin/modules



POST
/api/Admin/modules



PUT
/api/Admin/modules/{id}



DELETE
/api/Admin/modules/{id}



POST
/api/Admin/courses/{courseId}/assign-lecturer/{lecturerId}



POST
/api/Admin/modules/{moduleId}/assign-lecturer/{lecturerId}



POST
/api/Admin/courses/{courseId}/assign-student/{studentId}



POST
/api/Admin/modules/{moduleId}/assign-student/{studentId}



GET
/api/Admin/courses/{courseId}/students



GET
/api/Admin/students/{studentId}/courses



GET
/api/Admin/students/search



GET
/api/Admin/lecturers/search


Auth


POST
/api/Auth/login



POST
/api/Auth/change-password



POST
/api/Auth/reset-password



POST
/api/Auth/refresh


Lecturer


GET
/api/Lecturer/lecturers/{lecturerId}



GET
/api/Lecturer/courses/{lecturerId}



GET
/api/Lecturer/courses/{lecturerId}/{courseId}/modules



POST
/api/Lecturer/modules/{moduleId}/tasks



GET
/api/Lecturer/modules/{moduleId}/tasks



PUT
/api/Lecturer/tasks/{taskId}



DELETE
/api/Lecturer/tasks/{taskId}


Password


POST
/api/Password/forgot



POST
/api/Password/reset


Student


GET
/api/Student/students/{studentId}



GET
/api/Student/{studentId}/courses



GET
/api/Student/{studentId}/modules



GET
/api/Student/{studentId}/tasks



GET
/api/Student/courses/{courseId}



PUT
/api/Student/{studentId}/tasks/{taskId}/status



GET
/api/Student/{studentId}/tasks/filter

Note: JWT is required for protected routes via Authorization: Bearer <token> header.

License

MIT License

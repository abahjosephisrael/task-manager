# Task Manager
Task Manager

This is a task management system.

Technology Used:
- PostgreSQL database
- .NET 7
- Hangfire
- Swagger
- CQRS
- MediatR

To run this application:
- Clone and open with Visual Studio
- Restore packages. Visual Studio restores the packages automatically.
- Update the database connection. Specify the connection password.
- Build solution
- Run TaskManager.Presentation.Api
- http://localhost:2023/swagger/index.html -> Loads up the swagger documentation
- Login using the default user account, email: admin@test.email, password: Admin.1@
- http://localhost:2023/background -> Loads up background service dashboard

Note: Migration is automatic.

😉

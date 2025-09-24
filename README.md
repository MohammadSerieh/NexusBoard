# NexusBoard - Collaboration Hub

A modern team collaboration platform built with C# .NET and Angular.

## Features
- User authentication with JWT
- Role-based access control (Admin, Manager, Member)
- Team and project management
- Task tracking with Kanban boards
- File attachments
- Real-time notifications

## Tech Stack
- **Backend**: .NET 9, Entity Framework Core, PostgreSQL
- **Frontend**: Angular 18 (coming soon)
- **Authentication**: JWT with BCrypt password hashing
- **Database**: PostgreSQL with Docker
- **Architecture**: Clean Architecture pattern

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Node.js (for Angular)

### Setup
1. Clone the repository
2. Start PostgreSQL: `docker run --name postgres-collab -e POSTGRES_DB=CollabHub -e POSTGRES_PASSWORD=devpassword123 -p 5432:5432 -d postgres:16`
3. Run the API: `cd NexusBoard.API && dotnet run`
4. Open Swagger: `http://localhost:5058/swagger`

## Project Status
Currently in Week 1 of development - Authentication system complete.
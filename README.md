# NexusBoard - Team Collaboration Platform

A modern full-stack team collaboration platform built with .NET and Angular, featuring JWT authentication, role-based access control, and real-time project management capabilities.

## 🚀 Live Demo

**Backend API:** `http://localhost:5058/swagger`  
**Frontend:** `http://localhost:4200`

**Test Credentials:**
- Email: `test@example.com`
- Password: `TestPassword123!`

## ✨ Features

### Authentication & Security
- JWT-based authentication with refresh tokens
- Role-based access control (Admin, Manager, Member)
- BCrypt password hashing
- SQL injection prevention
- XSS attack prevention
- Input validation and sanitization

### Team Management
- Create and manage teams
- Invite team members by email
- Role assignments within teams (Team Lead, Member)
- Team-based project access control

### Project Management
- Create projects within teams
- Project status tracking (Planning, Active, OnHold, Completed, Cancelled)
- Priority levels (Low, Medium, High, Critical)
- Project timeline management

### Task Management
- Create and assign tasks within projects
- Task status workflow (Todo, InProgress, Review, Done)
- Task priority management
- File attachments support
- Task assignment to team members

## 🛠 Tech Stack

### Backend (.NET 9)
- **Framework:** ASP.NET Core Web API
- **Database:** PostgreSQL with Entity Framework Core
- **Authentication:** JWT Bearer tokens
- **Architecture:** Clean Architecture pattern
- **Security:** BCrypt, parameterized queries, CORS
- **Documentation:** Swagger/OpenAPI

### Frontend (Angular 18)
- **Framework:** Angular with TypeScript
- **UI Library:** Angular Material
- **State Management:** RxJS Observables
- **Authentication:** JWT interceptors and guards
- **Forms:** Reactive Forms with validation

### DevOps & Tools
- **Database:** PostgreSQL in Docker
- **Version Control:** Git with GitHub
- **Development:** Hot reload for both frontend and backend

## 🏗 Architecture
├── NexusBoard.API/              # Web API Controllers
├── NexusBoard.Core/             # Domain Models & Interfaces
├── NexusBoard.Infrastructure/   # Data Access & Services
├── NexusBoard.Tests/           # Unit & Integration Tests
└── nexusboard-ui/              # Angular Frontend
├── src/app/core/           # Services, Guards, Interceptors
├── src/app/features/       # Feature Components
└── src/app/shared/         # Shared Components

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 18+ and npm
- Docker Desktop
- Git

### Backend Setup

1. **Clone the repository**
```bash
   git clone https://github.com/MohammadSerieh/NexusBoard.git
   cd NexusBoard

Start PostgreSQL

bash   docker run --name postgres-collab \
     -e POSTGRES_DB=CollabHub \
     -e POSTGRES_PASSWORD=devpassword123 \
     -p 5432:5432 \
     -d postgres:16

Run the API

bash   cd NexusBoard.API
   dotnet run
The API will be available at http://localhost:5058
Frontend Setup

Install dependencies

bash   cd nexusboard-ui
   npm install

Start the development server

bash   ng serve
The app will be available at http://localhost:4200
📝 API Documentation
Once the backend is running, visit http://localhost:5058/swagger for interactive API documentation.
Key Endpoints
Authentication:

POST /api/auth/register - User registration
POST /api/auth/login - User login

Teams:

GET /api/teams - Get user's teams
POST /api/teams - Create team
POST /api/teams/{id}/members - Add team member

Projects:

GET /api/projects - Get user's projects
POST /api/projects - Create project
PUT /api/projects/{id} - Update project

Tasks:

GET /api/workitems/project/{id} - Get project tasks
POST /api/workitems - Create task
PUT /api/workitems/{id} - Update task

🔐 Security Features

Authentication: JWT tokens with 7-day expiration
Authorization: Role-based and resource-based access control
Data Protection: BCrypt password hashing with salt
Input Validation: Server-side validation on all endpoints
SQL Injection Prevention: Parameterized queries via Entity Framework
XSS Prevention: Input sanitization and output encoding
CORS: Configured for development and production environments

🗄 Database Schema
The application uses PostgreSQL with the following main entities:

Users - Authentication and user profiles
Teams - Team organization and management
TeamMembers - Many-to-many relationship with roles
Projects - Project information and status
WorkItems - Tasks within projects
TaskFiles - File attachments for tasks

🧪 Testing
Backend Testing
bashcd NexusBoard.API
dotnet test
Manual Testing

Register a new user via the frontend
Create a team and add members
Create projects within teams
Add and manage tasks
Test role-based permissions

📈 Development Progress

✅ Week 1: Backend foundation with authentication
✅ Week 2: Teams, projects, and tasks APIs
✅ Week 3: Angular frontend with authentication integration
🔄 Week 4: Teams and projects UI (In Progress)
📋 Week 5: Task management and Kanban board
🔄 Week 6: File uploads and real-time features

🎯 Learning Outcomes
This project demonstrates:

Full-stack development with modern technologies
Clean Architecture and separation of concerns
Security best practices for web applications
RESTful API design and documentation
Database design and Entity Framework usage
Authentication and authorization implementation
Modern frontend development with Angular
Git workflow and project organization

🤝 Contributing
This is a portfolio project, but feedback and suggestions are welcome!
📄 License
This project is open source and available under the MIT License.

Built by Mohammad Serieh as a full-stack development portfolio project.

# CampusLearn
### *A Web Based Course Delivery and Progress Tracking Portal*

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=aspnetcore&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/mvc)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![Bootstrap 5.3](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Database-SQLite%20%7C%20SQL%20Server-blue](#database-configuration)](#database-configuration)

---

## Table of Contents
1. [Project Overview](#project-overview)
2. [Project Objectives](#project-objectives)
3. [Key Features](#key-features)
   - [Public Features](#public-features)
   - [Learner Features](#learner-features)
   - [Administrator Features](#administrator-features)
   - [Authentication & Security Features](#authentication--security-features)
4. [Technology Stack](#technology-stack)
5. [System Requirements](#system-requirements)
6. [Project Installation and Setup](#project-installation-and-setup)
7. [Database Configuration](#database-configuration)
8. [Administrator and Learner Access](#administrator-and-learner-access)
9. [Project Structure](#project-structure)
10. [Application Workflow](#application-workflow)
11. [Security Considerations](#security-considerations)
12. [Testing and Build](#testing-and-build)
13. [Future Enhancements](#future-enhancements)
14. [Author](#author)
15. [License](#license)

---

## Project Overview

**CampusLearn** is a modern, responsive, web-based course delivery and academic progress tracking portal engineered for educational institutions, college curricula, and modern web learners.

The platform provides a centralized, interactive learning management environment:
* **Learners** can explore academic courses, enroll in structured curricula, consume sequential lesson modules, verify comprehension through interactive chapter quizzes, track real-time completion analytics, and review detailed quiz submissions.
* **Administrators** have complete oversight through a protected institutional management console to author courses, publish lessons, build quizzes with dynamic question banks, monitor learner registrations, manage enrollments, evaluate quiz performance, and review student contact inquiries.

---

## Project Objectives

* **Structured Course Delivery:** Deliver modular learning content including structured curricula, rich lesson descriptions, and organized learning materials through an intuitive web interface.
* **Granular Progress Tracking:** Enable learners to track their individual learning milestones through automated lesson progress calculation and real-time completion metrics.
* **Interactive Assessment Engine:** Evaluate student mastery with objective, automated quizzes featuring instant scoring, answer review, and historical attempt analytics.
* **Role-Based Institutional Administration:** Provide academic administrators with dedicated tooling for course authoring, question management, learner oversight, and enrollment auditing.
* **Enterprise Security Standards:** Implement industry-standard authentication, role-based authorization, account lockout protection, anti-forgery defense, and sanitized configuration management using ASP.NET Core Identity.

---

## Key Features

### Public Features
* **Landing Page & Platform Overview:** Highlighting featured courses, academic statistics, curriculum advantages, and learner value propositions.
* **Interactive Course Catalogue:** Browse available courses with category filters, detailed syllabi, lesson counts, duration metrics, and enrollment statuses.
* **Institutional Information Pages:** Detailed About page outlining pedagogical goals, and an interactive Contact page allowing prospective students and visitors to submit inquiries directly to administration.
* **Responsive Modern UI:** Modern design featuring custom gradient palettes, glassmorphism cards, micro-animations, and mobile-friendly responsive navigation.

### Learner Features
* **Personalized Learner Dashboard:** Real-time overview displaying active course enrollments, average quiz performance, completed lessons, and quick-resume shortcuts.
* **Self-Service Course Enrollment:** One-click enrollment into academic tracks with immediate syllabus access.
* **Modular Lesson Viewer:** Sequential lesson viewer with previous/next navigation, rich markdown/text lecture materials, and instant "Mark as Completed" progress synchronization.
* **Quiz Assessment System:** Timed or untimed multi-question assessments with automatic grading, immediate feedback on correct/incorrect choices, and comprehensive score breakdown.
* **Learning Analytics & History:** Detailed overview of all quiz attempts, percentage scores, and course-by-course progress statistics.

### Administrator Features
* **Administrative Control Panel:** High-level institutional dashboard reporting total courses, active lessons, quiz submissions, registered learners, and pending inquiries.
* **Curriculum & Course Management:** Full CRUD operations for courses (title, category, duration, description, thumbnail URL, and active status).
* **Lesson Publishing Suite:** Create, edit, reorder, and manage individual lesson modules linked to parent courses.
* **Quiz & Question Authoring Engine:** Build multi-question quizzes, configure passing criteria, add/edit/delete multiple-choice questions, and define correct answer options.
* **Learner & Enrollment Oversight:** Inspect enrolled students, track individual learner completion percentages, and manage course enrollments.
* **Assessment & Grade Auditing:** Review all learner quiz submissions, timestamps, raw scores, and percentage marks.
* **Inquiry Management:** Centralized repository to inspect contact messages submitted by public visitors and students.

### Authentication & Security Features
* **ASP.NET Core Identity Integration:** Native password hashing using PBKDF2 with HMAC-SHA256.
* **Role-Based Access Control (RBAC):** Strict partitioning separating `Administrator` and `Learner` capabilities using `[Authorize(Roles = "...")]`.
* **Institutional Admin Login:** Dedicated administrative entry point enforcing elevated role authorization.
* **Authenticated Password Change:** In-app administrative password management requiring validation of current credentials against ASP.NET Core Identity policies.
* **Brute-Force Lockout Defense:** Automatic account lockout after 5 consecutive failed access attempts with a 15-minute cooldown.
* **Cross-Site Request Forgery (CSRF) Mitigation:** Anti-forgery tokens enforced on all state-modifying POST actions.
* **HTTP Security Headers:** Integrated middleware enforcing `X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN`, and strict referrer policies.

---

## Technology Stack

| Layer | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | **ASP.NET Core 10 (net10.0)** | High-performance, cross-platform enterprise web framework |
| **Architecture** | **MVC (Model-View-Controller)** | Clean separation of business models, razor views, and controllers |
| **Language** | **C# 14 / .NET 10** | Modern, strongly-typed object-oriented language |
| **Data Access** | **Entity Framework Core 10** | Modern Object-Relational Mapper (ORM) using Code-First migrations |
| **Databases** | **SQLite** *(Local Dev)*<br>**Microsoft SQL Server** *(Production)* | Dual-provider support configured dynamically at startup |
| **Security** | **ASP.NET Core Identity** | Membership, secure hashing, cookie authentication, and role authorization |
| **Frontend** | **Razor Views, HTML5, CSS3, JS** | Server-rendered views with client-side interactivity |
| **UI Styling** | **Bootstrap 5.3 & Bootstrap Icons** | Responsive grid system, modern typography, and vector iconography |
| **Fonts** | **Google Fonts (Outfit & Plus Jakarta Sans)** | Contemporary academic web typography |

---

## System Requirements

* **.NET SDK:** .NET 10.0 SDK (v10.0.100 or higher)
* **Operating System:** Windows 10/11, macOS, or Linux (cross-platform compatible)
* **IDE / Editor:** Visual Studio 2022 / 2026, Visual Studio Code (with C# Dev Kit), or JetBrains Rider
* **Database Engine:**
  * Local Development: No external database engine required (uses built-in SQLite).
  * Production Deployment: Microsoft SQL Server 2019+, Azure SQL Database, or AWS RDS for SQL Server.

---

## Project Installation and Setup

### 1. Clone the Repository
Open Windows PowerShell or your terminal and clone the repository:
```powershell
git clone https://github.com/your-username/CampusLearn.git
cd CampusLearn
```
*(Replace the placeholder URL with your actual repository remote if already published).*

### 2. Restore Dependencies
Restore all required NuGet packages:
```powershell
dotnet restore
```

### 3. Build the Solution
Verify that the project builds cleanly without errors or warnings:
```powershell
dotnet build
```

### 4. Apply Database Migrations & Seed Initial Data
The application automatically checks, applies EF Core migrations, and seeds initial academic roles, courses, lessons, and quizzes upon startup. To manually verify migrations via the CLI:
```powershell
dotnet ef database update
```

### 5. Run the Application
Start the local development server:
```powershell
dotnet run
```

### 6. Access the Portal
Once started, the application will output the active local listener URLs (typically `http://localhost:5101` or `https://localhost:7286`). Open your web browser and navigate to:
```text
http://localhost:5101
```

---

## Database Configuration

CampusLearn features dynamic database provider resolution configured in `Program.cs`.

### Local SQLite (Default)
In local development, the application connects to a file-based SQLite database (`CampusLearn.db`) in the project root as specified in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=CampusLearn.db"
  }
}
```

### Microsoft SQL Server (Production)
To use Microsoft SQL Server in staging or production environments, supply a standard SQL Server connection string via the `ConnectionStrings__DefaultConnection` environment variable without modifying code or committing credentials:

```powershell
# Windows PowerShell Example
$env:ConnectionStrings__DefaultConnection = "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=CampusLearnDb;User ID=your-username;Password=your-strong-password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

When the application detects `Server=` or `Database=` in the connection string, it automatically activates `Microsoft.EntityFrameworkCore.SqlServer` and migrates the schema.

---

## Administrator and Learner Access

### Learner Registration & Login
* Any visitor can create a learner account by clicking **Sign Up Free** or navigating to `/Account/Register`.
* New registrations are automatically assigned the `Learner` role and redirected to their personal **Learner Dashboard**.

### Administrator Access
* Administrative management is accessed via the **Administrator Portal** at `/Account/AdminLogin` (or the link in the site footer).
* The initial administrator account (`admin@campuslearn.com`) is initialized by the database seeder on startup.
* To configure or update the administrator password during deployment, supply the password through an environment variable:
  ```powershell
  $env:AdminUser__Password = "YourSecureAdminPassword123!"
  ```

### Password Management Policy
* **In-App Change Password:** Authenticated administrators can safely update their credentials at any time by navigating to the **Change Password** option located in the **Admin Dashboard** and navigation user menu.
* **Email Recovery Notice:** In accordance with institutional security policies, **public email-based password recovery has been intentionally omitted**. Administrator password updates are restricted exclusively to authenticated dashboard sessions and hosting-level environment configuration.

---

## Project Structure

```text
CampusLearn/
├── Controllers/                 # MVC Controllers handling requests and business logic
│   ├── AccountController.cs     # Authentication, registration, login, and admin password change
│   ├── AdminController.cs       # Administrative dashboard metrics and management hub
│   ├── AdminCoursesController.cs# Course authoring and curriculum management
│   ├── AdminLessonsController.cs# Lesson authoring and content ordering
│   ├── AdminQuizzesController.cs# Quiz creation, question bank, and scoring rules
│   ├── AdminLearnersController.cs# Learner profile oversight
│   ├── AdminEnrollmentsController.cs # Student course enrollment tracking
│   ├── AdminQuizResultsController.cs # Assessment submissions and grade auditing
│   ├── AdminContactMessagesController.cs # Inbound public contact inquiry inbox
│   ├── CoursesController.cs     # Public course catalogue and syllabus viewer
│   ├── HomeController.cs        # Public landing, About, Contact, and Privacy pages
│   ├── LearnerController.cs     # Student dashboard, my courses, and learning analytics
│   ├── LessonsController.cs     # Lesson content presentation and progress toggle
│   └── QuizzesController.cs     # Interactive quiz taking and submission processing
├── Data/                        # EF Core database context, seeders, and migration helpers
│   ├── ApplicationDbContext.cs  # DbSets, relationship configurations, and delete behaviors
│   ├── DbSeeder.cs              # Idempotent database, role, admin user, and sample course seeder
│   └── SqlDataMigrator.cs       # Utility for SQLite-to-SQL Server record transfers
├── Migrations/                  # EF Core database schema migration snapshots
├── Models/                      # Core domain entities and database models
│   ├── ApplicationUser.cs       # Identity user extension (FullName, CreatedAt, ProfileImage)
│   ├── Course.cs                # Course metadata, category, duration, and relations
│   ├── Lesson.cs                # Individual learning modules within courses
│   ├── Enrollment.cs            # Learner-to-Course registration and overall progress
│   ├── LessonProgress.cs        # Granular completed/in-progress tracking per lesson
│   ├── Quiz.cs                  # Assessment configuration and passing threshold
│   ├── Question.cs              # Multiple-choice questions, options, and correct answers
│   ├── QuizResult.cs            # Historical quiz submissions and percentage scores
│   └── ContactMessage.cs        # Public inquiries submitted via contact form
├── ViewModels/                  # Strongly-typed data transfer view models for UI views
│   ├── AccountViewModels.cs     # Login, Register, AdminLogin, AdminChangePassword models
│   ├── CourseViewModels.cs      # Catalogue search, filter, and course detail view models
│   ├── LearnerViewModels.cs     # Learner dashboard, progress analytics, and quiz models
│   └── AdminViewModels.cs       # Administrative dashboard aggregation models
├── Views/                       # Razor View templates
│   ├── Account/                 # Login, Register, AdminLogin, AdminChangePassword
│   ├── Admin/                   # Administrator dashboard
│   ├── AdminCourses/            # Course management views
│   ├── AdminLessons/            # Lesson management views
│   ├── AdminQuizzes/            # Quiz & Question management views
│   ├── AdminLearners/           # Learner management views
│   ├── AdminEnrollments/        # Enrollment auditing views
│   ├── AdminQuizResults/        # Quiz submission review views
│   ├── AdminContactMessages/    # Contact inquiry views
│   ├── Courses/                 # Public course catalogue views
│   ├── Home/                    # Index, About, Contact views
│   ├── Learner/                 # Dashboard, MyCourses, Progress analytics views
│   ├── Lessons/                 # Interactive lesson viewer
│   ├── Quizzes/                 # Quiz assessment and result views
│   └── Shared/                  # _Layout, _LoginPartial, _AlertMessages, _ValidationScripts
├── wwwroot/                     # Static client assets (CSS, JavaScript, vendor libraries)
│   ├── css/                     # Custom site styling and responsive theme rules
│   └── lib/                     # Bootstrap, jQuery, and validation scripts
├── appsettings.json             # Application configuration (free of sensitive credentials)
├── appsettings.Development.json # Development-specific logging configuration
├── CampusLearn.csproj           # Project configuration, target framework, and package references
└── Program.cs                   # Application entry point, dependency injection, and middleware pipeline
```

---

## Application Workflow

```
+-------------------------------------------------------------------------------+
|                                PUBLIC VISITOR                                 |
+-------------------------------------------------------------------------------+
         |                                                       |
         v                                                       v
 [Explore Courses / About]                             [Register Account]
         |                                                       |
         v                                                       v
 [Submit Contact Inquiry]                                [Learner Login]
                                                                 |
+-------------------------------------------------------------------------------+
|                               LEARNER WORKFLOW                                |
+-------------------------------------------------------------------------------+
                                 |
         +-----------------------+-----------------------+
         |                       |                       |
         v                       v                       v
 [Browse Catalogue]      [Enroll in Course]     [Learner Dashboard]
         |                       |                       |
         v                       v                       v
 [Read Lessons] -------> [Complete Lesson] ----> [Take Chapter Quiz]
                                                         |
                                                         v
                                              [Review Score & History]
                                                         |
                                                         v
                                             [Track Analytics (Progress)]

+-------------------------------------------------------------------------------+
|                            ADMINISTRATOR WORKFLOW                             |
+-------------------------------------------------------------------------------+
                                 |
                                 v
                        [Admin Portal Login]
                                 |
                                 v
                     [Administrator Dashboard]
                                 |
     +-------------------+-------+-------+-------------------+
     |                   |               |                   |
     v                   v               v                   v
[Manage Courses]  [Publish Lessons] [Author Quizzes] [Audit Enrollments]
     |                   |               |                   |
     v                   v               v                   v
[Inspect Learners] [Review Grades] [View Inquiries] [Change Admin Password]
```

---

## Security Considerations

1. **Password Hashing:** All user passwords are encrypted using ASP.NET Core Identity's standard PBKDF2 hashing algorithm with cryptographic salts. Plaintext passwords are never stored or logged.
2. **Strict Authorization Boundaries:** Administrative endpoints are guarded with `[Authorize(Roles = "Administrator")]`, ensuring that regular learners and anonymous visitors cannot access administrative routes or actions.
3. **Session Hardening & Cookie Security:** Cookies are configured with `HttpOnly = true`, `SameSite = SameSiteMode.Lax`, and slide expiration policies to protect against Cross-Site Scripting (XSS) session hijacking.
4. **No Secrets in Source Control:** No database passwords, production connection strings, or administrative credentials are hard-coded in source files or configuration files.
5. **Form Tamper Defense:** Anti-forgery validation (`[ValidateAntiForgeryToken]`) is enforced across all data mutation endpoints to prevent Cross-Site Request Forgery (CSRF).

---

## Testing and Build

The application compiles with zero build errors and zero warnings across all controllers, view models, and database context configurations.

### Build Verification
Execute the standard .NET build command:
```powershell
dotnet build
```
**Expected Output:**
```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Future Enhancements

* **Automated Certificate Generation:** PDF completion certificates issued dynamically upon achieving 100% course progress and passing all associated quizzes.
* **Rich Media Content Integration:** Embedded video player support (YouTube, Vimeo, or cloud storage) for multimedia lesson delivery.
* **Discussion Forums & Peer Feedback:** Course-level discussion boards allowing learners to ask questions and interact with instructors.
* **Two-Factor Authentication (2FA):** Time-based One-Time Password (TOTP) authenticator app integration for administrative accounts.
* **Dark / Light Theme Toggle:** User-selectable interface themes with persistent browser preference storage.

---

## Author

* **Author:** Biwash Thapa
* **Project Context:** College Semester Web Development Project — *CampusLearn: A Web Based Course Delivery and Progress Tracking Portal*

---

## License

No license has been specified yet. All rights reserved.

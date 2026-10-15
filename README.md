# ClaimSystem — Independent Contractor Claims Management

A role-based web application that streamlines the submission, review, approval, and management of monthly claims for Independent Contractor (IC) lecturers.

## 📌 Project Overview

ClaimSystem is a web-based application developed as part of the PROG6212 Programming 2A Portfolio of Evidence (POE).

The system aims to simplify the monthly claims process by allowing lecturers to submit claims and enabling authorised staff members to review and approve or reject them through a structured workflow.

The Part 3 implementation extends the system with Human Resources (HR) functionality, automated hourly-rate management, user administration, and additional security measures.

## ✨ Key Features

### 📝 Claims Management
- Allows lecturers to submit monthly claims.
- Supports the management and review of submitted claims.
- Includes claim approval and rejection functionality.
- Uses claim statuses to support the approval workflow.

### 👥 Role-Based Access Control

The application supports four user roles, each with different responsibilities.

| Role | Responsibilities |
|---|---|
| Lecturer | Submit monthly claims and access lecturer-related functionality. |
| Programme Coordinator | Review claims and perform authorised approval workflow actions. |
| Academic Manager | Participate in the claim management and approval workflow according to assigned permissions. |
| Human Resources (HR) | Manage users, create user accounts, configure hourly rates, and access HR-related functionality. |

Access to functionality is controlled according to the user's role.

### 💰 Automated Hourly-Rate Management
- Allows HR to manage lecturer hourly rates.
- Supports the use of configured hourly rates in claim calculations.
- Reduces the need for manual rate entry during the claims process.

### 🔐 Security Enhancements
- Implements role-based access restrictions.
- Includes custom role authorisation functionality.
- Uses server-side application logic to manage protected operations.
- Supports a more controlled workflow for user and claim management.

### 📊 HR Management and Reporting
- Provides HR-related user management functionality.
- Supports lecturer management.
- Includes an HR dashboard with claim and lecturer statistics.
- Includes functionality for generating reports from approved claims.

## 🛠️ Technology Stack

- **C#** — application logic.
- **ASP.NET Core MVC** — web application architecture.
- **Entity Framework Core** — database access and data management.
- **Microsoft SQL Server** — relational database.
- **Razor Views** — server-rendered user interface.
- **Git and GitHub** — version control and source code management.
- **Visual Studio** — development environment.

## 🏗️ Application Architecture

The project separates application responsibilities into controllers, views, data-access components, and supporting classes.

- **Controllers:** Handle incoming requests and coordinate application operations.
- **Views:** Provide the user interface for claims, reviews, and administrative functionality.
- **Database context:** Manages database access through Entity Framework Core.
- **Authorisation components:** Restrict access to functionality based on assigned user roles.
- **Testing project:** Contains unit tests for supported application functionality.

## 🚀 Getting Started

### Prerequisites

Before running the application, ensure that you have:

- Visual Studio with the ASP.NET and web development workload.
- The .NET SDK version specified by the project.
- Microsoft SQL Server.
- SQL Server Management Studio (SSMS) or another compatible database management tool.
- Git installed if you want to clone the repository.

### 1. Clone the repository

```bash
git clone https://github.com/Ad30000/ClaimSystem.git
cd ClaimSystem
```

### 2. Open the solution

Open `ClaimSystem.sln` in Visual Studio.

### 3. Configure the database

Configure the SQL Server connection string in the appropriate application configuration file.

Use your own local database credentials and avoid committing passwords or other secrets to GitHub.

The repository includes the document `Programming 6212 Part 3.docx`, which contains assessment documentation and the database script.

**Database script note:** The document contains the table definitions, views, and function. Check the notes separating the sections and execute the relevant SQL statements in the correct order when setting up the database.

### 4. Restore dependencies

```bash
dotnet restore ClaimSystem.sln
```

### 5. Build the application

```bash
dotnet build ClaimSystem.sln
```

### 6. Run the application

Run the web application from Visual Studio using the configured startup project, or navigate to the directory containing the web project's `.csproj` file and execute:

```bash
dotnet run
```

Use the local URL displayed by the application to open it in your browser.

**Note:** The exact database initialization and migration steps depend on the current project configuration.

## 🧪 Testing

The repository includes an `MsTestProject` test project.

Unit-testing documentation is also included in the Part 3 assessment document.

To run the automated tests, open a terminal in the repository directory and execute:

```bash
dotnet test ClaimSystem.sln
```

Review the test results to determine which tests passed or failed.

## 🎥 Demonstrations and Presentation

Explore the project through the following assessment videos.

- **PowerPoint presentation:** [Watch the project presentation](https://youtu.be/uPqdQU7hfV4)
- **Live demonstration:** [Watch the live application demo](https://youtu.be/a-sgY9mZUEw)

## 📚 Assessment Documentation

The repository includes supporting documentation for the PROG6212 Part 3 submission.

- **Part 3 documentation:** [`Programming 6212 Part 3.docx`](https://github.com/Ad30000/ClaimSystem/blob/main/Programming%206212%20Part%203.docx)
- **Presentation:** [`Prog 6212 POE.pptx`](https://github.com/Ad30000/ClaimSystem/blob/main/Prog%206212%20POE.pptx)
- **Automated tests:** `MsTestProject`
- **Database script:** Included in the Part 3 Word document, with tables, views, and a function.

## 🔑 Demo Access

The original assessment documentation identifies the following HR demo account:

- **Email:** `hr@test.com`

The password value previously listed as `hashed_password` should not be assumed to be a working login password. Use the actual configured demo password or recreate the account through the application's supported setup process.

For security, avoid publishing working credentials or real user passwords in a public repository.

## 🎯 Learning Objectives

This project demonstrates practical application of:

- ASP.NET Core MVC development.
- Relational database integration using Entity Framework Core and SQL Server.
- Role-based authorisation and access control.
- User and claim workflow management.
- Automated financial calculations using configured hourly rates.
- Unit testing and software verification.
- Version control and technical documentation.

## 👨‍💻 Author

**Ad30000**

GitHub: [@Ad30000](https://github.com/Ad30000)

## 📂 Repository

[View ClaimSystem on GitHub](https://github.com/Ad30000/ClaimSystem)

---

*Developed as part of the PROG6212 Programming 2A Portfolio of Evidence.*


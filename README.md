# Student Grades Management System

## Project Overview

The Student Grades Management System is a comprehensive F# application designed to streamline student grade management and analysis. Built with Windows Forms and MySQL, this system provides robust functionality for educational administrators and students to track and manage academic performance.

## Key Features

### 1. User Authentication
- Secure login system with role-based access control
- Two user roles: Admin and Reader
- Password hashing using BCrypt for enhanced security

### 2. Admin Capabilities
- Full database management
- Add new students
- Update existing student information
- Delete student records
- View comprehensive class statistics

### 3. Student Grade Management
- Store and manage student records with multiple grades
- Calculate individual student averages
- Generate class-wide statistical insights
- Identify highest and lowest grades

### 4. Reader Access
- View personal grade information
- Refresh grade details

## Technology Stack
- Language: F#
- UI Framework: Windows Forms
- Database: MySQL
- Authentication: BCrypt.Net

## Prerequisites
- .NET Framework
- MySQL Server
- MySql.Data NuGet Package
- BCrypt.Net NuGet Package

## Database Setup

### Users Table
```sql
CREATE TABLE Users (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role ENUM('Admin', 'Reader') NOT NULL
);
```

### Students Table
```sql
CREATE TABLE Students (
    ID INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    Grades VARCHAR(255) NOT NULL,
    user_id INT,
    FOREIGN KEY (user_id) REFERENCES Users(ID)
);
```

### Initial Admin User Setup
```sql
INSERT INTO Users (Username, PasswordHash, Role) VALUES
('admin', '$2a$11$m81sHWtH.1HIcRnQRb75UOI3TIG./2Yw/GmOXDnZdJQXoUjIPQSNG', 'Admin');
```

## Installation Steps

1. Clone the repository
2. Install required NuGet packages
3. Configure MySQL connection string in the code
4. Run the database setup scripts
5. Build and run the application

## Usage

### Admin Login
- Username: admin
- Password: laughtale

### Functionality
- Admins can add, edit, and delete student records
- Readers can view their personal grade information
- Class-wide statistics are available for administrators

## Security Notes
- Passwords are securely hashed
- Role-based access control implemented
- Sensitive information is protected

## Future Enhancements
- Implement more advanced reporting
- Add data export functionality
- Enhance user interface
- Implement more granular permission levels

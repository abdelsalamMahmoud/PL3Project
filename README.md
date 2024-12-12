# Student Grades Management System

## Setup Instructions

To get started with the project, follow these steps:

1. **Create the Users Table:** (with the existing table -students-)
   To store the users, you'll need to create a `Users` table in your MySQL database. Run the following SQL command to create the table:

   ```sql
    CREATE TABLE Users (
    ID INT AUTO_INCREMENT PRIMARY KEY,       -- Unique identifier for each user
    Username VARCHAR(50) NOT NULL UNIQUE,    -- Username
    PasswordHash VARCHAR(255) NOT NULL,      -- Hashed password for security
    Role ENUM('Admin', 'Reader') NOT NULL    -- User role (Admin or Reader)
    );

    -- Insert an admin user
    INSERT INTO Users (Username, PasswordHash, Role) VALUES
    ('admin', '$2a$11$m81sHWtH.1HIcRnQRb75UOI3TIG./2Yw/GmOXDnZdJQXoUjIPQSNG', 'Admin');

    
   CREATE TABLE Students (
      ID INT PRIMARY KEY AUTO_INCREMENT,
      Name VARCHAR(255) NOT NULL,
      Grades VARCHAR(255) NOT NULL,
      user_id int,
      FOREIGN KEY (user_id) REFERENCES Users(ID)
   );
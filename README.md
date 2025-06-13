# Thwala Attorneys Web Application

-----

## Overview

Thwala Attorneys is a modern, full-stack **ASP.NET Core 8** web application designed to streamline legal case and client management. Built with **MVC architecture**, backed by **SQL Server 2022 Express**, and containerized using **Docker**, this application ensures efficient deployment and robust operation.

Key features include:

  * A robust **database schema** leveraging Entity Framework Core.
  * Automated **database migration** with built-in retry logic.
  * Seamless **email integration** via SMTP (Gmail) for client notifications.
  * Comprehensive **logging and error handling** for application stability.
  * A **Dockerized multi-stage build** for lightweight production images.
  * **Docker Compose setup** for orchestrating the application and SQL Server.

-----

## ✨ Features

  * **ASP.NET Core MVC architecture** ensures clear separation of concerns, promoting maintainable and scalable code.
  * **Entity Framework Core with SQL Server** provides persistent and reliable data storage.
  * **Robust database migration** with retry logic enhances application resilience during startup.
  * **Integrated email service** with SMTP facilitates critical notifications and communications.
  * A fully **Dockerized setup** enables seamless, containerized deployment across various environments.
  * **Comprehensive logging capabilities** assist in in-depth application diagnostics and troubleshooting.

-----

## 🛠️ Technologies Used

  * **.NET 8.0 (ASP.NET Core)**: The core framework for a high-performance foundation.
  * **Entity Framework Core**: Simplifies database interactions through an ORM.
  * **Microsoft SQL Server 2022 (Docker container)**: The relational database management system deployed efficiently via Docker.
  * **Docker & Docker Compose**: Tools for consistent containerization and orchestration.
  * **SMTP Email (Gmail SMTP configured)**: For direct email sending and notifications.
  * **C\#**: The primary programming language for backend logic.
  * **HTML, CSS, JavaScript (Razor Views)**: For building dynamic and responsive user interfaces.

-----

## 🚀 Getting Started

### Prerequisites

To get started with Thwala Attorneys, ensure you have the following software installed on your system. Links are provided for direct access and can be copied.

  * [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0): Essential for building and running .NET 8 applications.
  * [Docker Desktop](https://www.docker.com/get-started): Includes Docker Engine and Docker Compose for containerization.
  * [Git](https://git-scm.com/downloads): For cloning the project repository.

### Setup and Run

Follow these steps to set up and launch the Thwala Attorneys Web Application. All commands are provided in copyable code blocks, allowing for easy "copy to clipboard" functionality on platforms like GitHub.

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/PixieStack/thwala-attorneys.git
    cd thwala-attorneys
    ```

    *Note: This project's code is hosted under the **PixieStack** GitHub account.*

2.  **Configure environment variables:**
    Open the `docker-compose.yml` file. Adjust any necessary environment variables within this file, particularly the **database password** and **email settings**, to match your local environment or desired configuration.

3.  **Build and start the application:**
    Ensure Docker Desktop is running. Then, from the `thwala-attorneys` root directory, use Docker Compose to build the application images and start all defined services:

    ```bash
    docker-compose up --build
    ```

    *This command will build the necessary Docker images and then start the application container along with the SQL Server container.*

4.  **Access the application:**
    Once all services are running and initialized (this might take a moment for database migrations), open your web browser and navigate to the application using this URL:

    ```
    http://localhost:8080
    ```

    *The application should now be accessible and ready for use.*

-----

## Application Configuration

### Database

The application leverages **SQL Server** running within a Docker container. The connection string is configured via **environment variables** in `docker-compose.yml`, and it features **automatic database migration** with a retry mechanism on startup.

### Email Service

An integrated email service handles notifications. It's configured with **SMTP details** via environment variables and sends a test email during startup for verification. By default, it uses Gmail SMTP.

### Logging

The application includes robust logging. **Console and Debug logging** are enabled, capturing crucial events like database connection attempts, migration status, and email service tests for diagnostics.

-----

## Project Structure

The project is organized logically to ensure clarity and maintainability:

  * `Controllers`: Contains **MVC controllers** for handling web requests.
  * `Data`: Houses the **Entity Framework DbContext** and database migrations.
  * `Models`: Defines **data models** representing database entities.
  * `Services`: Contains business logic and specific service implementations (e.g., email service).
  * `Views`: Stores **Razor views** for UI rendering.
  * `wwwroot`: Serves **static files** (CSS, JavaScript, images).

-----

## Troubleshooting

If you encounter any issues, consider the following:

  * **Docker Status**: Ensure **Docker Desktop** is correctly installed and actively running.
  * **Port Availability**: Verify that ports `8080`, `8081` (application) and `1433` (database) are not in use by other applications. If they are, you might need to stop the conflicting applications or adjust the port mappings in your `docker-compose.yml` file.
  * **Check Logs**: Always consult the application logs, which can be viewed in your terminal after running `docker-compose up --build` or by using `docker logs <container_name>`. These logs provide crucial insights into any errors or issues during startup or operation.
  * **Environment Variables**: Double-check that all necessary environment variables, especially database credentials and email settings in `docker-compose.yml`, are correctly set.

-----

## ⚖️ Legal Notice

This project, **Thwala Attorneys**, is part of the personal portfolio of **Thembinkosi Eden Thwala** (PixieStack). While the code is publicly visible for demonstration purposes, it is **NOT open source**. All rights are reserved. Unauthorized copying, modification, or distribution of this code is strictly prohibited and may result in legal action.

© 2025 PixieStack. All Rights Reserved.

-----

## Contact

For support or inquiries regarding **Thwala Attorneys**, please contact:

**Thembinkosi Eden Thwala**

Email: thwalathembinkosi16@gmail.com
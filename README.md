# SaigonRide

**Course:** Software Engineering (Semester 2 - 2025-2026)
**Team:** F2
**Tier:** 4 (Advanced)

---

## 1. Deployment Guide

### Prerequisites
* **IDE:** Visual Studio 2022 (v17.4 or higher).
* **Framework:** .NET 8.0.
* **Database:** SQL Server (LocalDB).
* **Browser:** Google Chrome or Microsoft Edge (required for Automation Tests).

### Database Setup
The team has provided the necessary SQL scripts in the `Database/` directory. Please follow these steps to initialize the database:
1. Open SQL Server Object Explorer in Visual Studio or SQL Server Management Studio (SSMS).
2. Execute **`SaigonRide_Database.sql`** to create the schema and tables.
3. Execute **`SeedData.sql`** to insert sample data (Stations, Vehicles, and Accounts).
4. Verify the connection string in **`appsettings.json`** to ensure it matches your local server instance configuration.

### How to Run the Project
1. Open the **`SaigonRide.sln`** solution file in Visual Studio.
2. Press **F5** or click the Start button to run the application using IIS Express or the SaigonRide profile.
3. The system will automatically launch in your default web browser at `localhost`.

---

## 2. Quality Assurance (Tier 4 Only)

This project includes a comprehensive Automation Testing suite utilizing Selenium WebDriver to guarantee system stability and core functionalities.

**To execute the tests:**
1. Open the **Test Explorer** window in Visual Studio.
2. Right-click on the **`SaigonRide.Tests`** project.
3. Select **Run**.
4. **Result Expected:** 8/8 Test Cases passed successfully (covering Login, Start Rental, and Payment workflows).

---

## 3. Test Credentials

To facilitate the grading process, please use the following pre-configured accounts:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | admin@ex.com | admin123 |
| **User** | user1@ex.com | 123456 |

---

## 4. Technologies Used

* **Backend:** ASP.NET MVC, Entity Framework Core (Code First).
* **Frontend:** Bootstrap 5, Razor View Engine.
* **Payment Integration:** VNPAY API & PayPal API.
* **Testing:** Selenium WebDriver, xUnit.
* **Cloud Hosting:** Render (PostgreSQL).

---

## 5. Team F2 Members

1. **Le Duy Khang** (Leader) - 524H0097
2. **Nguyen Minh Hoang** - 524H0206
3. **Ly Phuc Hai** - 524H0013

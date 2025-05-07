<file name=0 path=/Users/fatihguzelkara/StockManagemant/Readme.MD># Stock Tracking System (ASP.NET Core MVC)

This project is a stock tracking system with warehouse and product management functionalities. It includes features such as user roles, authorization, bulk product upload via Excel, and receipt operations.

## 📁 Project Folder Structure

- **Controllers/**: Contains MVC controller classes.
- **Views/**: Razor-based UI pages are located here.
- **Models/**: Contains entity and DTO classes.
- **Repositories/**: Repository classes for database operations.
- **Managers/**: Business rules are defined here.
- **wwwroot/**: Static files (CSS, JS, images).

## 👤 User Roles and Permissions

- **Admin**
  - Access to all pages
  - User, product, category, warehouse, location, and receipt operations
- **Operator**
  - Product entry/exit to/from warehouse
  - Receipt creation, product upload via Excel
- **Basic User**
  - Viewing limited to assigned warehouse

## 📦 Core Features

- Product upload via Excel (products or warehouse products)
- Warehouse selection and location-based product placement
- Receipt creation and receipt detail operations
- User and role management
- Warehouse entry and exit operations
- User management
- Multi-warehouse management

## 🚀 Setup

1. Clone the project.
2. Configure the database connection in `appsettings.json`.
3. Create the database using the `dotnet ef database update` command.
4. dotnet restore.
5. dotnet run

## 📊 Excel Templates

- **Product Addition Template**: [Uploads/ProductTemplate.xlsx](Uploads/ProductTemplate.xlsx)
- **Warehouse Products Template**: [Uploads/WarehouseProductTemplate.xlsx](Uploads/WarehouseProductTemplate.xlsx)

## 📝 License

This project is for educational purposes and is shared as open source.

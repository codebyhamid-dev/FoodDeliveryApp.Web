# FoodDeliveryApp.Web 🍴

A modern **Food Delivery Web Application** built with **ASP.NET Core MVC**, designed to manage food orders, users, and menus efficiently.  
This project implements **cookie-based authentication** and **role-based authorization**, and supports full **CRUD operations** including **image uploads**.

---

## 🚀 Features

- 🔐 **User Authentication** — Secure login and registration using cookies.  
- 🧑‍🍳 **Role-Based Authorization** — Separate access for Admins, Delivery Staff, and Customers.  
- 🥗 **Menu Management** — Add, edit, update, and delete food items with image uploads.  
- 📦 **Order Handling** — Users can browse items, place orders, and view order history.  
- 🖼️ **Image Uploads** — Store and manage product images efficiently.  
- ⚙️ **Clean Architecture** — Built with layered architecture for easy maintenance and scalability.

---

## 🧩 Tech Stack

- **Backend:** ASP.NET Core MVC  
- **Frontend:** Razor Views, HTML5, CSS3, Bootstrap  
- **Database:** SQL Server (Entity Framework Core ORM)  
- **Authentication:** Cookie-based Authentication  
- **Authorization:** Role-based Access Control  
- **File Handling:** Image upload & storage in the `wwwroot` folder

  
- ## 🏗️ Project Structure
FoodDeliveryApp.Web/
│
├── Controllers/ # MVC Controllers for handling requests
├── Models/ # Entity models and view models
├── Views/ # Razor views for UI
├── wwwroot/ # Static files (images, CSS, JS)
├── Data/ # Database context and seed data
└── Program.cs # Application entry point

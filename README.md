# 🎬 MuvyHub

MuvyHub is a lightweight media management and streaming application built with modern .NET technologies and cloud-native storage.  
It is designed for managing, uploading, and streaming media files efficiently — with **Wasabi** used as the object storage backend.

---

## ✨ Features

- 📁 Upload and manage media files in Wasabi storage  
- 🎥 Stream videos with secure links  
- 🔐 Authentication & role-based access  
- 📊 Basic analytics (views, uploads, etc.)  
- ⚡ Built with performance in mind (async APIs, efficient file handling)  
- 🧩 Modular and open for contributions  

---

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8 (Web API + Razor Pages)  
- **Frontend**: Razor Pages / JavaScript (optional SPA integration)  
- **Storage**: [Wasabi Cloud Storage](https://wasabi.com/) (S3-compatible)  
- **Database**: SQL Server / PostgreSQL (configurable)  
- **ORM**: Entity Framework Core  
- **Authentication**: JWT / Identity  

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- A database (SQL Server or PostgreSQL)  
- A Wasabi account with access keys  

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/muvyhub.git
   cd muvyhub

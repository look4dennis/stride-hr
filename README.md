# StrideHR - Enterprise Human Resource Management System

<div align="center">
  <h1>Stride<span style="color: #007bff;">HR</span></h1>
  <p><em>Empowering Organizations Through Intelligent HR Management</em></p>
</div>

## 🚀 Overview

StrideHR is a comprehensive, full-stack Human Resource Management System designed to meet international standards for global organizations. Built with modern web technologies, it provides a unified platform for all HR operations while accommodating regional compliance requirements and varying organizational structures.

## ✨ Key Features

- 🌍 **Global Multi-Branch Support** - Manage operations across multiple countries
- 👥 **Complete Employee Lifecycle** - From recruitment to exit management
- 💰 **Advanced Payroll System** - Custom formulas and multi-currency support
- ⏰ **Real-time Attendance Tracking** - Location-based check-in/out with break management
- 📊 **Performance Management** - Including PIP (Performance Improvement Plans)
- 🎯 **Project Management** - Kanban boards with time tracking
- 📱 **Mobile-First Design** - Responsive Bootstrap 5 interface
- 🔒 **Enterprise Security** - Role-based access control and audit trails
- 🤖 **AI-Powered Insights** - Intelligent analytics and chatbot support

## 🛠️ Technology Stack

- **Frontend**: Angular 17+ with Bootstrap 5
- **Backend**: .NET 8 Web API with Entity Framework Core
- **Database**: MySQL 8.0+
- **Caching**: Redis
- **Real-time**: SignalR
- **Authentication**: JWT with role-based access control

## 🏗️ Architecture

strideHR follows a modern, scalable architecture designed for enterprise use:

- **Microservices-Ready**: Modular design for future scalability
- **Multi-Tenancy**: Branch-based data isolation
- **Performance Optimized**: Caching, lazy loading, and efficient queries
- **Security-First**: Comprehensive audit trails and data protection
- **International Standards**: Multi-currency, timezone, and compliance support

## 📋 System Requirements

### Development Environment
- Node.js 18+ and npm
- .NET 8 SDK
- MySQL 8.0+
- Redis (optional for development)
- Visual Studio Code or Visual Studio 2022

### Production Environment
- Linux/Windows Server
- Docker and Docker Compose (recommended)
- MySQL 8.0+ or compatible
- Redis for caching
- Reverse proxy (Nginx/Apache)

## 🚀 Quick Start

### Using Docker (Recommended)
```bash
# Clone the repository
git clone https://github.com/look4dennis/stride-hr.git
cd stride-hr

# Start all services
docker-compose up -d

# Access the application
# Frontend: http://localhost:4200
# API: http://localhost:5000
# API Documentation: http://localhost:5000/swagger
```

### Manual Setup
```bash
# Backend setup
cd src/StrideHR.API
dotnet restore
dotnet ef database update
dotnet run

# Frontend setup (in another terminal)
cd frontend
npm install
ng serve
```

## 📚 Documentation

- [Requirements Document](.kiro/specs/stride-hr/requirements.md) - Complete system requirements
- [Technical Design](.kiro/specs/stride-hr/design.md) - Architecture and system design
- [Implementation Tasks](.kiro/specs/stride-hr/tasks.md) - Development roadmap

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Server=localhost;Database=stridehr;Uid=root;Pwd=password;"

# Redis
Redis__ConnectionString="localhost:6379"

# JWT
JWT__SecretKey="your-secret-key"
JWT__Issuer="strideHR"
JWT__Audience="strideHR-users"
JWT__ExpirationHours=24

# Email
Email__SmtpServer="smtp.gmail.com"
Email__SmtpPort=587
Email__Username="your-email@gmail.com"
Email__Password="your-app-password"
```

## 🤝 Contributing

We welcome contributions! Please read our contributing guidelines for details on our code of conduct and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Developer

**Dennis Charles Dcruz**  
Email: look4dennis@hotmail.com

## 🙏 Acknowledgments

- Built with modern web technologies and best practices
- Designed for international compliance and scalability
- Focused on user experience and performance

---

<div align="center">
  <p>Made with ❤️ for the global HR community</p>
</div>
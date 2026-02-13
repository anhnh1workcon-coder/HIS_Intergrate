# HIS Integration - Blood Bank Management System

A mockup test project for **Kho Máu Minh Tâm** (Minh Tam Blood Bank) integration with HIS (Hospital Information System).

## Overview

This project provides an **ASP.NET Core Web API** backend that simulates blood bank inventory management and patient order processing. It includes:

- RESTful API endpoints for blood inventory and patient orders
- Mock JSON database for persistent data storage
- Request/response logging with timestamps
- Web-based dashboard for inventory and order management
- Comprehensive validation and inventory deduction logic
- Swagger/OpenAPI documentation

## Features

### Core Functionality
- **Inventory Management**: Track blood types (ABO, Rh), volumes, quantities, and blood plans
- **Patient Orders**: Process patient transfusion orders with automatic inventory validation and deduction
- **Logging Service**: File-based request/response logging for audit trails
- **Dashboard UI**: Interactive web interface for managing inventory and orders
- **Mock Database**: JSON-based data storage (no external database required)

### API Endpoints (Swagger-filtered)
- `POST /LisReceiver/web/SavePatient` - Save patient order with inventory deduction
- `GET /LisReceiver/web/GetInventory` - Retrieve blood inventory (flexible filtering)

## Project Structure

```
WebTestKhoMau/
├── Controllers/
│   ├── InventoryController.cs       # Blood inventory endpoints
│   └── PatientOrderController.cs    # Patient order endpoints
├── Models/
│   ├── PatientOrderRequest.cs
│   ├── PatientOrderResponse.cs
│   ├── InventoryInfo.cs
│   ├── InventoryRequest.cs
│   ├── InventoryResponse.cs
│   └── OrderItem.cs
├── Services/
│   ├── MockDatabaseService.cs       # CRUD operations on JSON DB
│   ├── ValidationService.cs         # Order and inventory validation
│   └── LogService.cs                # File-based logging
├── Data/
│   └── mockdb.json                  # Mock database (21 blood types)
├── Properties/
│   └── launchSettings.json          # App configuration & network binding
├── wwwroot/
│   └── dashboard.html               # Web dashboard UI
├── Program.cs                       # Dependency injection & config
├── appsettings.json                 # App settings
└── WebTestKhoMau.csproj             # Project file

_Document/
├── _HIS - Kho máu Minh Tâm Integration.postman_collection.json  # Postman API spec
└── HIS-Tủ máu.docx                  # Documentation
```

## Technology Stack

- **Framework**: ASP.NET Core 8 (.NET 8)
- **Language**: C# 12
- **Serialization**: System.Text.Json
- **API Documentation**: Swashbuckle (Swagger/OpenAPI)
- **Logging**: .NET Logging + File I/O
- **Network**: 0.0.0.0:5268 (CORS enabled for development)
- **Frontend**: HTML5 + JavaScript (dashboard)

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022 / VS Code / Rider
- Git

### Installation & Run

1. **Clone the repository**:
   ```bash
   git clone https://github.com/anhnh1workcon-coder/HIS_Intergrate.git
   cd HIS_Intergrate
   ```

2. **Build the project**:
   ```bash
   cd WebTestKhoMau
   dotnet build
   ```

3. **Run the server**:
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5268`

4. **Access the dashboard**:
   Open browser → `http://localhost:5268/dashboard.html`

5. **View API documentation**:
   Open browser → `http://localhost:5268/swagger`

## API Usage Examples

### Get Blood Inventory
```bash
POST http://localhost:5268/LisReceiver/web/GetInventory
Content-Type: application/json

{
  "ABO": "A",
  "Rh": "+"
}
```

### Save Patient Order (with automatic inventory deduction)
```bash
POST http://localhost:5268/LisReceiver/web/SavePatient
Content-Type: application/json

{
  "PID": "P001",
  "OrderID": "ORD001",
  "PatientName": "Nguyễn Văn A",
  "TREATMENT_CODE": "TRANSFUSION",
  "ListOrder": [
    {
      "ElementID": "1",
      "Volume": 500,
      "Quantity": 2
    }
  ]
}
```

## Database Schema

### Inventory (mockdb.json)
```json
{
  "ABO": "A",
  "Rh": "+",
  "ElementID": "1",
  "ElementName": "RBC",
  "Volume": 500,
  "Quantity": 100,
  "BooldPlan": "Normal"
}
```

### Patient Orders (mockdb.json)
```json
{
  "PID": "P001",
  "OrderID": "ORD001",
  "PatientName": "Nguyễn Văn A",
  "TREATMENT_CODE": "TRANSFUSION",
  "ListOrder": [
    {
      "ElementID": "1",
      "Volume": 500,
      "Quantity": 2
    }
  ]
}
```

## Logging

Request/response logs are stored in `WebTestKhoMau/Logs/` directory:
- **File format**: `API_SavePatient_YYYY-MM-DD.log`
- **Contents**: JSON-formatted log entries with timestamp, input, output, status, and error details

Example log entry:
```json
{
  "Time": "2026-02-13 10:30:45.123",
  "API": "SavePatient",
  "Status": "Success",
  "Input": {
    "PID": "P001",
    "OrderID": "ORD001",
    ...
  },
  "Output": {
    "IsSuccess": true,
    "ErrorMessage": ""
  }
}
```

## Dashboard Features

- **Two-tab interface**:
  - 📦 **Kho Máu** (Inventory): View and manage blood inventory
  - 🏥 **Đơn Hàng Bệnh Nhân** (Patient Orders): View and manage patient orders
- **CRUD operations**: Add, edit, delete inventory items and orders
- **Auto-persistence**: Active tab state saved in localStorage
- **Real-time updates**: Dashboard refreshes after API operations
- **Validation feedback**: Visual error messages for failed operations

## Configuration

### Network Binding (launchSettings.json)
```json
"https://localhost:7062": {
  "commandName": "Project",
  "launchBrowser": false,
  "applicationUrl": "https://localhost:7062;http://0.0.0.0:5268"
}
```

### CORS (Program.cs)
Development-enabled CORS allows requests from any origin (for testing).

## Known Limitations

- Mock database (JSON) — not suitable for production
- No authentication/authorization
- Data persists only during session (unless mockdb.json is manually saved)
- Swagger documentation filtered to show only GetInventory and SavePatient

## Future Enhancements

- [ ] Integration with real HIS database
- [ ] User authentication & role-based access
- [ ] Advanced reporting & analytics
- [ ] Multi-location blood bank support
- [ ] Real-time inventory alerts
- [ ] Email notifications for low stock

## Support & Contact

For questions or issues regarding this mockup project, contact the development team.

---

**Project Type**: ASP.NET Core Web API Mock  
**Target System**: HIS Blood Bank Integration (Kho Máu Minh Tâm)  
**Status**: Development / Testing Phase  
**Last Updated**: February 2026

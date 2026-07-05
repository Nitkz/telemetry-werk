# TelemetryWerk

High-Performance Industrial IoT Telemetry Monitoring Platform.

## Overall Architecture 🏗️

The project strictly follows **Clean Architecture** principles and **Domain-Driven Design (DDD)** to ensure a high level of **Separation of Concerns**. The solution is divided into two main parts: the Backend API and the Frontend UI. They communicate exclusively via REST APIs and real-time WebSockets (SignalR), keeping both systems completely decoupled.

## Server Side (Backend API) ⚙️

- **Architecture Layout:** Structured with strict Clean Architecture layers (`Api.Domain`, `Api.Application`, `Api.Infrastructure`, `Api.Host`).
- **REST & Real-time:** Uses standard `ControllerBase` for HTTP REST endpoints and **SignalR** for real-time telemetry streaming at high frequencies (e.g., 60ms intervals).
- **Background Workers:** Employs `BackgroundService` (`IHostedService`) for continuous data ingestion and broadcasting.
- **Middleware:** Custom `ApiKeyMiddleware` for API Key authentication, supporting both Standard HTTP headers and WebSockets query strings.

## Client Side (Frontend UI) 💻

- **Framework:** Built with **Blazor Web App** supporting both Server-side Pre-rendering and WebAssembly (WASM) interactive modes (`InteractiveWebAssembly` / `InteractiveServer`).
- **UI Component Library:** Uses **MudBlazor** for clean, modern, and responsive UI components.
- **Anti-Corruption Layer:** UI Domain Models (`Ui.Core`) have zero dependencies on the Backend models. We treat API integrations as external third-party services.
- **NSwag Code Generation:** Utilizes **NSwag** to automatically generate strongly-typed API Clients (`TelemetryApiClient`) during the build process (`dotnet build`) by reading the Server's Swagger JSON. This ensures type safety without manual duplication.

## Modern C# & .NET Features Utilized 🚀

- **Primary Constructors (C# 12 / .NET 8+):** Used extensively across the entire project (both Server and Client) to drastically simplify Dependency Injection and eliminate boilerplate constructor code.
- **Extension Methods for Clean Architecture:** We extract UI logic (like mapping statuses to UI colors) into Extension Methods (`MachineStatusExtensions.cs`). This prevents the UI Domain Core from depending on UI frameworks (like MudBlazor), preserving the single source of truth while respecting Clean Architecture principles.
- **Single Source of Truth Configuration:** Centralized configuration (`ApiServiceOptions`) ensures consistency across Pre-rendered Server contexts and WASM Client contexts.

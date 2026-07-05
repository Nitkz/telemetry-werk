# TelemetryWerk

[![CI](https://github.com/Nitkz/telemetry-werk/actions/workflows/ci.yml/badge.svg)](https://github.com/Nitkz/telemetry-werk/actions/workflows/ci.yml)

High-Performance Industrial IoT Telemetry Monitoring Platform.

## Overall Architecture 🏗️

The project follows **Clean Architecture** principles and **Domain-Driven Design (DDD)** to ensure a clear **Separation of Concerns**. The solution is divided into two main parts: the Backend API and the Frontend UI. They communicate only via REST APIs and real-time WebSockets (SignalR), keeping both systems completely decoupled.

## Server Side (Backend API) ⚙️

- **Architecture Layout:** Structured with clear Clean Architecture layers (`Api.Domain`, `Api.Application`, `Api.Infrastructure`, `Api.Host`).
- **REST & Real-time:** Uses standard `ControllerBase` for HTTP REST endpoints and **SignalR** for fast real-time telemetry streaming (e.g., 60ms intervals).
- **Background Workers:** Uses `BackgroundService` (`IHostedService`) as a pseudo-generator for testing. The core telemetry ingestion logic is separated into the Application layer (`ITelemetryIngestionService`), making the system ready to accept data from real IoT devices via various connection types (HTTP, MQTT, WebSockets) seamlessly.
- **Security & Resilience:** Implements Built-in Rate Limiting (Fixed Window), dynamic environment-based CORS policies, and a custom `SecurityHeadersMiddleware` (XSS Protection, NoSniff, Frame Options, Strict Referrer) to harden the API.
- **Middleware:** Custom `ApiKeyMiddleware` for secure API Key authentication (supports standard HTTP headers and WebSockets). Validates keys securely without leaking them in logs and records client IPs on failure for rate limit awareness.
- **Observability:** Centralized structured logging using **Serilog**. Employs a custom **CorrelationIdMiddleware** for end-to-end request tracing and robust visibility across the system.
- **Quality Assurance:** Comprehensive Unit Testing suite built with **xUnit**, **NSubstitute** (for mocking), and **FluentAssertions**. Tests are fully isolated using the AAA pattern and provide extensive coverage for business logic, repositories, and custom middlewares.

## Client Side (Frontend UI) 💻

- **Framework:** Built with **Blazor Web App** supporting both Server-side Pre-rendering and WebAssembly (WASM) interactive modes (`InteractiveWebAssembly` / `InteractiveServer`).
- **UI Component Library:** Uses **MudBlazor** for clean, modern, and responsive UI components.
- **Data Isolation (Anti-Corruption Layer):** UI Domain Models (`Ui.Core`) have zero dependencies on the Backend models. We treat API integrations as external third-party services.
- **NSwag Code Generation:** Uses **NSwag** to automatically generate strongly-typed API Clients (`TelemetryApiClient`) during the build process (`dotnet build`) by reading the Server's Swagger JSON. This ensures type safety without writing duplicate code.

## Modern C# & .NET Features Utilized 🚀

- **Primary Constructors (C# 12 / .NET 8+):** Used across the entire project (both Server and Client) to simplify Dependency Injection and remove repetitive constructor code.
- **In-Memory Pub/Sub with Channels:** Uses `System.Threading.Channels` for high-performance, thread-safe messaging between REST API controllers and background workers, completely eliminating database polling bottlenecks.
- **Extension Methods for Clean Architecture:** We extract UI logic (like mapping statuses to UI colors) into Extension Methods (`MachineStatusExtensions.cs`). This prevents the UI Domain Core from depending on UI frameworks (like MudBlazor), keeping a single source of truth while respecting Clean Architecture principles.
- **Single Source of Truth Configuration:** Centralized configuration (`ApiServiceOptions`) ensures consistency across Pre-rendered Server contexts and WASM Client contexts.

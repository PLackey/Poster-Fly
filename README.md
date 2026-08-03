# Poster Fly

A .NET MAUI application for creating and managing poster collections.

## Description

Poster Fly is a cross-platform mobile application built with .NET MAUI that allows users to create, manage, and organize poster collections with variable support and API integration.

## Features

- Collection management
- Variable system with auto-complete
- API request/response handling
- Cross-platform support (Android, iOS, Windows, macOS)

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 (version 17.8 or later) with .NET MAUI workload
- For Android development: Android SDK
- For iOS development: Xcode (macOS only)

### Installation

1. Clone the repository:
   ```
   git clone <repository-url>
   cd Poster-Fly
   ```

2. Restore NuGet packages:
   ```
   dotnet restore
   ```

3. Build the project:
   ```
   dotnet build
   ```

4. Run the application:
   ```
   dotnet run --framework net8.0-android
   ```
   or
   ```
   dotnet run --framework net8.0-ios
   ```

## Project Structure

- `Models/` - Data models (ApiRequest, ApiResponse, Collection, Variable)
- `Controls/` - Custom UI controls (VariableAutoCompleteEntry)
- `Converters/` - Value converters for data binding
- `Examples/` - Sample data files
- `Platforms/` - Platform-specific code

## Configuration

The application uses sample data files located in the `Examples/` folder:
- `sample-collection.json` - Sample collection data
- `sample-variables.json` - Sample variable definitions

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[Add your license information here]
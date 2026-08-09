# Changelog

All notable changes to Poster Fly will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project setup with .NET MAUI
- MIT License implementation
- Comprehensive project documentation

### Changed
- Updated README with complete project documentation
- Added development guidelines and contribution guide

### Fixed
- N/A

### Removed
- N/A

## [1.0.0] - 2026-08-08

### Added
- **Core Features**
  - Multi-protocol API support (HTTP/REST, GraphQL, gRPC)
  - Collection and folder-based request organization
  - Intelligent variable system with auto-completion
  - Request history and response analysis
  - Cross-platform support (Android, iOS, Windows, macOS)

- **Models & Architecture**
  - `ApiRequest` model with comprehensive configuration options
  - `ApiResponse` model with detailed response analysis
  - `Collection` and `CollectionFolder` for hierarchical organization
  - `Variable` and `Environment` models for dynamic data management
  - MVVM architecture with dependency injection

- **UI Components**
  - `VariableAutoCompleteEntry` custom control
  - Value converters for data binding
  - Responsive design optimized for mobile and desktop

- **Services**
  - `ApiService` for HTTP/GraphQL/gRPC request handling
  - `VariableService` for variable resolution and management
  - `StorageService` for data persistence

- **Development Tools**
  - Comprehensive `.gitignore` for .NET MAUI projects
  - `.editorconfig` with C# coding standards
  - `Directory.Build.props` for shared MSBuild properties
  - `global.json` for SDK version management
  - `nuget.config` for package source management

- **Documentation**
  - Complete README with setup and usage instructions
  - Contributing guidelines
  - MIT License
  - Sample data and examples

- **Dependencies**
  - Microsoft.Maui.Controls (Core MAUI framework)
  - CommunityToolkit.Mvvm (MVVM helpers)
  - Newtonsoft.Json (JSON serialization)
  - Grpc.Net.Client (gRPC support)
  - GraphQL.Client (GraphQL support)

### Technical Details
- **Target Framework**: .NET 10.0
- **Platform Support**: 
  - Android 21+ (API Level 21)
  - iOS 15.0+
  - Windows 10 (19041+)
  - macOS 12.0+ (Monterey)
- **Architecture**: MVVM with dependency injection
- **UI Framework**: .NET MAUI with XAML

---

## Version History Template

Use this template for future releases:

## [Version] - YYYY-MM-DD

### Added
- New features

### Changed
- Changes in existing functionality

### Deprecated
- Soon-to-be removed features

### Removed
- Now removed features

### Fixed
- Bug fixes

### Security
- Vulnerability fixes

---

## Release Process

1. Update version numbers in:
   - `PosterFly.csproj` (ApplicationVersion, ApplicationDisplayVersion)
   - `Directory.Build.props` (Version, AssemblyVersion, FileVersion)

2. Update this changelog with all changes since last release

3. Create a release tag:
   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   git push origin v1.0.0
   ```

4. Create GitHub release with:
   - Release notes from changelog
   - Binary attachments (if applicable)
   - Migration notes (if breaking changes)

## Breaking Changes Policy

- Major version bumps for breaking changes
- Detailed migration guides for breaking changes
- Deprecation warnings before removal (minimum 1 minor version)
- Clear documentation of API changes
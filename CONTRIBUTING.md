# Contributing to Poster Fly

Thank you for your interest in contributing to Poster Fly! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2022 with .NET MAUI workload OR Visual Studio Code with C# Dev Kit
- Git

### Setting Up Your Development Environment

1. **Fork and Clone**
   ```bash
   git clone https://github.com/your-username/Poster-Fly.git
   cd Poster-Fly
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build and Test**
   ```bash
   dotnet build
   dotnet test # Run tests
   ```

## 📋 Development Guidelines

### Code Style
- Follow Microsoft's C# coding conventions
- Use the provided `.editorconfig` for consistent formatting
- Enable nullable reference types
- Use `var` for local variables when type is obvious
- Prefer expression-bodied members for simple properties and methods

### Architecture Patterns
- **MVVM**: Use Model-View-ViewModel pattern consistently
- **Dependency Injection**: Register all services in `MauiProgram.cs`
- **Async/Await**: Use async patterns for I/O operations
- **Error Handling**: Use structured exception handling with proper logging

### Project Structure
```
├── Models/           # Data models and entities
├── Services/         # Business logic and API services
├── ViewModels/       # MVVM view models
├── Views/           # XAML pages and views
├── Controls/        # Custom UI controls
├── Converters/      # Value converters
├── Resources/       # Images, fonts, styles
└── Platforms/       # Platform-specific code
```

## 🧪 Testing

### Unit Tests
- Write unit tests for all business logic
- Use xUnit for testing framework
- Mock external dependencies
- Aim for >80% code coverage

### Integration Tests
- Test API endpoints with real services
- Test data persistence layers
- Validate cross-platform compatibility

### UI Tests
- Test critical user workflows
- Verify responsive design on different screen sizes
- Test accessibility features

## 🔀 Branching Strategy

### Branch Naming
- `feature/short-description` - New features
- `bugfix/issue-number-description` - Bug fixes
- `hotfix/critical-issue` - Critical production fixes
- `docs/documentation-update` - Documentation changes

### Workflow
1. Create feature branch from `main`
2. Make changes following coding guidelines
3. Write/update tests
4. Update documentation
5. Submit pull request

## 📝 Commit Guidelines

### Commit Message Format
```
type(scope): short description

Longer description if needed.

Fixes #issue-number
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Build process, auxiliary tools, etc.

### Examples
```bash
feat(api): add GraphQL query support
fix(variables): resolve auto-complete performance issue
docs(readme): update installation instructions
```

## 🚦 Pull Request Process

### Before Submitting
- [ ] Code follows style guidelines
- [ ] Self-review of the code
- [ ] Comments added for complex logic
- [ ] Tests added/updated and passing
- [ ] Documentation updated
- [ ] No merge conflicts with main branch

### PR Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Screenshots (if applicable)
Add screenshots for UI changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-reviewed the code
- [ ] Added tests
- [ ] Updated documentation
```

## 🐛 Bug Reports

### Before Reporting
- Search existing issues to avoid duplicates
- Try to reproduce the issue consistently
- Test on multiple platforms if possible

### Bug Report Template
```markdown
**Describe the Bug**
Clear and concise description of the bug.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected Behavior**
What you expected to happen.

**Screenshots**
If applicable, add screenshots.

**Environment:**
- OS: [e.g. Android 12, iOS 16, Windows 11]
- Device: [e.g. Pixel 7, iPhone 14, Surface Pro]
- App Version: [e.g. 1.2.0]

**Additional Context**
Any other context about the problem.
```

## 💡 Feature Requests

### Feature Request Template
```markdown
**Is your feature request related to a problem?**
Clear description of the problem.

**Describe the solution you'd like**
Clear description of what you want to happen.

**Describe alternatives you've considered**
Alternative solutions or features considered.

**Additional context**
Any other context, screenshots, or mockups.
```

## 🏷️ Issue Labels

- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements to documentation
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention is needed
- `question` - Further information is requested
- `platform:android` - Android-specific issue
- `platform:ios` - iOS-specific issue
- `platform:windows` - Windows-specific issue

## 📚 Resources

### Learning Resources
- [.NET MAUI Documentation](https://docs.microsoft.com/en-us/dotnet/maui/)
- [MVVM Pattern Guide](https://docs.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### Tools
- [Visual Studio](https://visualstudio.microsoft.com/)
- [VS Code with C# extension](https://code.visualstudio.com/)
- [Android Studio](https://developer.android.com/studio) (for Android debugging)
- [Xcode](https://developer.apple.com/xcode/) (for iOS development)

## 🤝 Community Guidelines

### Code of Conduct
- Be respectful and inclusive
- Provide constructive feedback
- Help newcomers learn and contribute
- Focus on the technical aspects

### Communication
- Use clear, concise language
- Provide context and examples
- Be patient with questions
- Celebrate contributions from all skill levels

## 🎉 Recognition

Contributors will be recognized in:
- README.md acknowledgments
- Release notes for significant contributions
- GitHub contributor graphs

Thank you for helping make Poster Fly better! 🚀
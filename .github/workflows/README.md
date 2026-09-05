# 🚀 GitHub Actions CI/CD Workflows

This directory contains GitHub Actions workflows for automating the build, test, and deployment processes of the Poster Fly .NET MAUI application.

## 📋 Available Workflows

### 1. `ci-cd.yml` - Main CI/CD Pipeline
**Triggers:** Push to main/develop, Pull requests, Manual dispatch

**Features:**
- ✅ Cross-platform builds (Android, iOS, Windows, macOS)
- 🧪 Automated testing (when tests exist)
- 📦 Artifact generation for all platforms
- 🏷️ Automatic GitHub releases on tags
- 🔍 Code quality analysis on PRs

**Platforms Built:**
- **Android**: APK + AAB (Play Store ready)
- **Windows**: EXE + MSIX packages
- **macOS**: APP + PKG bundles
- **iOS**: IPA files (requires certificates)

### 2. `release.yml` - Production Release Pipeline
**Triggers:** Git tags (v*.*.*), Manual dispatch

**Features:**
- 🏗️ Production-ready builds with version updates
- 📱 Platform-specific optimizations
- 📝 Automatic changelog generation
- 🚀 GitHub release creation with assets
- 🔢 Version management

**Usage:**
```bash
# Create a new release
git tag v1.0.0
git push origin v1.0.0
```

### 3. `pr-validation.yml` - Pull Request Validation
**Triggers:** Pull requests to main/develop

**Features:**
- 🔍 Code formatting validation
- 🏗️ Build verification on multiple platforms
- 🛡️ Security scanning (basic)
- 💬 Automatic PR comments with build status
- ⚠️ Detection of debugging code and TODOs

### 4. `dependency-update.yml` - Dependency Management
**Triggers:** Weekly schedule (Mondays 9 AM UTC), Manual dispatch

**Features:**
- 📦 Automatic NuGet package updates
- 🧪 Build verification with new dependencies
- 🔄 Automatic PR creation for updates
- 📋 Detailed change documentation

## 🔧 Setup Requirements

### MAUI Workload Installation (.NET 8+)
The workflows use platform-specific workload installation and target framework selection:

- **Ubuntu runners**: Only install `android` workload and build `net10.0-android` target
- **Windows runners**: Only install `windows` workload and build `net10.0-windows10.0.19041.0` target  
- **macOS runners**: Install `ios` and `maccatalyst` workloads and build both `net10.0-ios` and `net10.0-maccatalyst` targets

This approach avoids the NETSDK1178 error that occurs when trying to build iOS targets on Ubuntu or Windows runners.

The workflows automatically handle workload installation failures and continue with graceful fallbacks.

### Required Secrets
Add these secrets to your GitHub repository settings:

#### For Android Releases (Optional):
```
ANDROID_KEYSTORE          # Base64 encoded keystore file
ANDROID_KEY_ALIAS         # Keystore key alias
ANDROID_KEY_PASS          # Key password
ANDROID_STORE_PASS        # Keystore password
```

#### For iOS Releases (Optional):
```
IOS_CERTIFICATE           # Base64 encoded .p12 certificate
IOS_CERTIFICATE_PASSWORD  # Certificate password  
IOS_PROVISIONING_PROFILE  # Base64 encoded provisioning profile
```

### Repository Settings
1. **Actions Permissions**: Enable "Read and write permissions" for GITHUB_TOKEN
2. **Dependabot**: Configure `.github/dependabot.yml` with your GitHub usernames
3. **Branch Protection**: Set up branch protection rules for `main` branch

## 🎯 Usage Examples

### Creating a Release
```bash
# 1. Update version in your code
# 2. Commit changes
git add .
git commit -m "chore: bump version to 1.0.0"

# 3. Create and push tag
git tag v1.0.0
git push origin main
git push origin v1.0.0

# 4. Release workflow will automatically run
```

### Manual Workflow Dispatch
```yaml
# Go to Actions tab in GitHub
# Select "Release Build"
# Click "Run workflow" 
# Enter version (e.g., v1.0.0)
```

### Monitoring Builds
- **Pull Requests**: Check the "Checks" tab on your PR
- **Main Builds**: Visit the "Actions" tab in your repository
- **Releases**: Check the "Releases" section for published builds

## 📦 Artifacts

### Development Builds (ci-cd.yml)
- **Retention**: 30 days
- **Access**: Available in workflow run page
- **Formats**: APK, EXE, APP files

### Release Builds (release.yml)
- **Retention**: 90 days  
- **Access**: GitHub Releases page
- **Formats**: All platform packages + installers

## 🔍 Troubleshooting

### Common Issues

#### 1. Platform-Specific Workload Issues (.NET 8+)
```bash
# The error NETSDK1178 occurs when trying to build iOS targets on Ubuntu runners
# The workflows now handle this by building only compatible targets on each runner:

# Ubuntu runners (build-android, test-android-build):
dotnet build PosterFly.csproj -f net10.0-android

# Windows runners (build-windows, test-windows-build):  
dotnet build PosterFly.csproj -f net10.0-windows10.0.19041.0

# macOS runners (build-macos, build-ios):
dotnet build PosterFly.csproj -f net10.0-maccatalyst
dotnet build PosterFly.csproj -f net10.0-ios
```

#### 2. Build Fails on Specific Platform
```bash
# Check platform-specific logs in Actions tab
# Verify target framework versions match your project
# Check for platform-specific compilation errors
```

#### 3. Android Signing Fails
```bash
# Verify ANDROID_* secrets are properly set
# Check keystore file is valid base64
# Ensure key alias exists in keystore
```

#### 4. iOS Build Fails
```bash
# iOS builds require macOS runners
# Certificates and provisioning profiles needed for distribution
# May need Xcode command line tools
```

### Debug Workflow Issues
1. Check workflow logs in Actions tab
2. Verify secrets are properly configured  
3. Test builds locally first
4. Check .NET and MAUI workload versions

## 🛡️ Security

### Best Practices
- 🔒 Never commit secrets to repository
- 🔐 Use GitHub secrets for sensitive data
- 🛡️ Regular security updates via Dependabot
- 🔍 Automated security scanning in PRs

### Secret Management
```bash
# Add secrets via GitHub UI:
# Repository → Settings → Secrets and Variables → Actions
```

## 🎨 Customization

### Adding New Platforms
1. Add platform to `build-*` jobs in workflows
2. Update artifact upload steps
3. Add platform-specific build commands
4. Update documentation

### Modifying Build Configuration
1. Edit `env` section in workflow files
2. Update .NET/MAUI versions as needed
3. Modify build flags and parameters
4. Test changes in feature branch

### Custom Deployment Targets
1. Add deployment jobs to `release.yml`
2. Configure store-specific secrets
3. Add upload steps for app stores
4. Update release notes templates

## ⚡ Recent Updates

### Fixed Platform-Specific Build Issues (.NET 8+)
- **Platform-appropriate targets**: Each runner type builds only compatible target frameworks
- **Ubuntu runners**: Build Android targets only (`net10.0-android`)
- **Windows runners**: Build Windows targets only (`net10.0-windows10.0.19041.0`) 
- **macOS runners**: Build iOS and macCatalyst targets (`net10.0-ios`, `net10.0-maccatalyst`)
- **Error handling**: Eliminates NETSDK1178 errors from cross-platform workload conflicts

### Improved Error Handling
- **Platform compatibility**: No more iOS workload errors on Ubuntu runners
- **Build logging**: Enhanced logging for better troubleshooting
- **Target framework selection**: Automatic target framework selection based on runner type

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET MAUI CI/CD Guide](https://docs.microsoft.com/dotnet/maui/deployment/)
- [MAUI Workload Installation](https://docs.microsoft.com/dotnet/core/tools/dotnet-workload-install)
- [Dependabot Configuration](https://docs.github.com/en/code-security/dependabot)
- [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github)

---

💡 **Tip**: The workflows now build platform-specific targets on each runner type to avoid workload conflicts. Ubuntu builds Android, Windows builds Windows, and macOS builds both iOS and macCatalyst targets. If you encounter NETSDK1178 errors locally, specify the target framework: `dotnet build -f net10.0-android`
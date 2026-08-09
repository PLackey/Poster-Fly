# Security Policy

## Supported Versions

We provide security updates for the following versions of Poster Fly:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

The Poster Fly team takes security issues seriously. We appreciate your efforts to responsibly disclose security vulnerabilities.

### How to Report

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report security vulnerabilities by:

1. **Email**: Send details to [security@posterfly.com] (replace with actual email)
2. **Private GitHub Report**: Use GitHub's private vulnerability reporting feature
3. **Encrypted Communication**: For sensitive reports, request our PGP key

### What to Include

Please include the following information in your report:

- **Vulnerability Description**: Clear description of the security issue
- **Impact Assessment**: Potential impact and affected components
- **Reproduction Steps**: Detailed steps to reproduce the vulnerability
- **Proof of Concept**: Code or screenshots demonstrating the issue
- **Suggested Fix**: If you have ideas for remediation
- **Environment Details**: 
  - Platform (Android/iOS/Windows/macOS)
  - App version
  - Device/OS version

### Response Timeline

- **Initial Response**: Within 48 hours
- **Assessment**: Within 1 week
- **Fix Development**: 2-4 weeks (depending on complexity)
- **Public Disclosure**: After fix is released and deployed

### Security Measures in Poster Fly

#### Data Protection
- **Secure Storage**: Platform-native secure storage for sensitive data
- **Encryption**: All API tokens and secrets are encrypted at rest
- **No Plain Text**: Sensitive variables marked as secrets are never logged
- **Memory Protection**: Sensitive data cleared from memory after use

#### Network Security
- **TLS/SSL**: All network communications use TLS 1.2 or higher
- **Certificate Validation**: Full SSL certificate validation enabled by default
- **Request Validation**: Input validation and sanitization
- **Timeout Protection**: Configurable request timeouts to prevent DoS

#### Authentication & Authorization
- **Token Management**: Secure token storage and automatic refresh
- **Multiple Auth Types**: Support for various authentication methods
- **Scope Isolation**: Variable scoping prevents data leakage between collections

#### Input Validation
- **Parameter Sanitization**: All user inputs are validated and sanitized
- **URL Validation**: Request URLs are validated before execution
- **File Type Validation**: Imported files are validated for type and structure
- **Size Limits**: File uploads and requests have size limits

## Security Best Practices for Users

### API Key Management
- Never commit API keys or tokens to version control
- Use environment variables or secure storage for sensitive data
- Regularly rotate API keys and authentication tokens
- Mark sensitive variables as "secret" in the app

### Collection Sharing
- Review collections before sharing to ensure no sensitive data
- Use collection-scoped variables instead of global for sensitive data
- Export collections without including secret variables

### Network Security
- Use HTTPS endpoints whenever possible
- Verify SSL certificates are properly validated
- Be cautious when testing against development/staging environments
- Use VPN when testing internal APIs on public networks

### Device Security
- Use device lock screens and biometric authentication
- Keep the app and operating system updated
- Don't install the app on compromised or rooted devices
- Log out of shared devices

## Known Security Considerations

### Platform-Specific
- **Android**: App data stored in app-specific directory (requires root to access)
- **iOS**: Keychain services used for secure storage
- **Windows**: DPAPI used for credential protection
- **macOS**: Keychain services used for secure storage

### Third-Party Dependencies
- Regular security audits of NuGet packages
- Automated dependency vulnerability scanning
- Prompt updates for security-related package updates

### Limitations
- **Local Storage**: Data is stored locally on device (not cloud-synced by default)
- **Network Inspection**: Users can inspect network traffic (intended behavior)
- **Debug Builds**: Debug builds may include additional logging (development only)

## Security Updates

### Notification Channels
- GitHub Security Advisories
- Release notes with security indicators
- In-app notifications for critical updates

### Update Process
1. Security vulnerability identified
2. Fix developed and tested
3. Security advisory published (after fix available)
4. App store updates released
5. Users notified through multiple channels

## Compliance & Standards

### Privacy
- No telemetry or analytics collection without explicit consent
- All data processing happens locally on device
- No automatic cloud synchronization of API collections
- Users control all data sharing and export

### Industry Standards
- Follow OWASP Mobile Security guidelines
- Implement secure coding practices
- Regular security code reviews
- Automated security testing in CI/CD pipeline

## Security Research

We welcome security research on Poster Fly. Please follow responsible disclosure:

### Scope
**In Scope:**
- Authentication bypass
- Data injection attacks
- Privilege escalation
- Sensitive data exposure
- Network security issues

**Out of Scope:**
- Social engineering attacks
- Physical device attacks
- Denial of service (unless critical)
- Issues requiring physical access to unlocked device

### Recognition
- Security researchers will be credited in release notes (with permission)
- Hall of fame page for significant contributions
- Coordinated disclosure timeline respecting researcher preferences

## Contact Information

- **Security Team**: security@posterfly.com (replace with actual contact)
- **General Contact**: support@posterfly.com
- **GitHub Issues**: For non-security bugs only
- **Documentation**: This security policy and related docs

---

**Last Updated**: August 8, 2026
**Version**: 1.0

This security policy may be updated periodically. Please check back regularly for the latest version.
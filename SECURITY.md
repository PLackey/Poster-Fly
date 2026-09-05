# Security and Privacy Notice

## 🔒 Security Considerations

### Your Responsibility for API Security

**IMPORTANT**: As an API testing tool, Poster Fly handles sensitive information. You are solely responsible for:

#### 🔑 **API Credentials & Authentication**
- **Secure Storage**: Store API keys, tokens, and credentials securely
- **Access Control**: Limit access to devices/accounts with API credentials
- **Rotation**: Regularly rotate API keys and authentication tokens
- **Scope Limitation**: Use minimum required permissions for API access

#### 📊 **Data Protection**
- **Sensitive Data**: Be cautious when testing APIs with personal or sensitive data
- **Data Retention**: Clear request/response history containing sensitive information
- **Local Storage**: Data is stored locally on your device - secure your device accordingly
- **Third-Party APIs**: Ensure you have permission to test third-party APIs

### Security Best Practices

#### ✅ **Recommended Practices**
- Use test/sandbox environments when possible
- Implement proper access controls on testing devices
- Regularly review and clean stored request history
- Use environment-specific variables for different deployment stages
- Keep the application updated to the latest version

#### ❌ **Avoid These Practices**  
- Don't store production API keys in the application
- Don't test with real customer data unless necessary
- Don't share devices with stored API credentials
- Don't ignore certificate validation errors in production APIs

## 🛡️ Privacy Information

### Data Collection and Storage

**LOCAL STORAGE ONLY**: Poster Fly stores all data locally on your device:
- ✅ API requests and responses are stored locally
- ✅ Collections and variables are saved to local storage
- ✅ No data is transmitted to external servers by Poster Fly
- ✅ No analytics or tracking data is collected

### Third-Party Data Transmission

When you use Poster Fly to test APIs:
- **API Requests**: Data is sent directly from your device to the target API servers
- **No Intermediary**: Poster Fly does not intercept, log, or store your API communications on external servers
- **Third-Party Privacy**: Review the privacy policies of APIs you test

## 🔐 Compliance Considerations

### Data Protection Regulations

#### GDPR (European Union)
If testing APIs with EU personal data:
- Ensure you have lawful basis for processing
- Implement appropriate data protection measures
- Consider data minimization principles
- Document processing activities

#### CCPA (California)
If testing APIs with California resident data:
- Ensure compliance with consumer privacy rights
- Implement appropriate security measures
- Consider data retention and deletion policies

#### Other Jurisdictions
Consult local data protection laws and regulations applicable to your testing activities.

## 🚨 Reporting Security Issues

### Vulnerability Disclosure

If you discover a security vulnerability in Poster Fly:

1. **Do NOT** create a public GitHub issue
2. **Do NOT** disclose the vulnerability publicly
3. **DO** email security details to: [your-security-email]
4. **DO** provide detailed information about the vulnerability
5. **DO** allow reasonable time for response and fixes

### What to Include in Security Reports

- Description of the vulnerability
- Steps to reproduce the issue
- Potential impact assessment
- Suggested fix (if available)
- Your contact information for follow-up

## 📋 Security Checklist for Users

Before using Poster Fly in production or with sensitive data:

### Environment Setup
- [ ] Testing on secure, updated devices
- [ ] Using test/sandbox API environments when possible
- [ ] Proper network security (avoid public WiFi for sensitive testing)
- [ ] Updated operating system and security patches

### Credential Management  
- [ ] API keys stored securely and not shared
- [ ] Using least-privilege access for API credentials
- [ ] Regular rotation of API keys and tokens
- [ ] Separate credentials for development/testing/production

### Data Handling
- [ ] Understanding what data will be processed during testing
- [ ] Compliance with applicable data protection regulations
- [ ] Proper data retention and deletion policies
- [ ] Permission to test with third-party APIs

### Application Security
- [ ] Using the latest version of Poster Fly
- [ ] Regular review and cleanup of stored request history
- [ ] Proper device security (screen locks, encryption)
- [ ] Understanding of local data storage implications

## 📞 Additional Resources

- **General Security**: Review your organization's security policies
- **API Security**: Consult OWASP API Security guidelines
- **Data Protection**: Seek legal advice for compliance requirements
- **Privacy Policies**: Review third-party API privacy policies

---

**Remember**: Security is a shared responsibility. While we strive to build secure software, your implementation and usage practices are crucial for maintaining security and privacy.

**Last Updated**: December 2024
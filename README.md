# ASP.NET Core API - IIS Deployment Guide

## Overview

This guide provides comprehensive solutions for deploying ASP.NET Core API to IIS with proper CORS and Swagger configuration.

## Common Issues & Solutions

### 1. CORS Issues
❌ **Problem**: Frontend (localhost:3000) cannot access API after IIS deployment
✅ **Solution**: Environment-specific CORS policies implemented

### 2. Swagger Not Accessible
❌ **Problem**: Swagger UI not working in production/IIS
✅ **Solution**: Production-ready Swagger configuration with custom styling

## Files Created/Modified

### Core Application Files
- `Program.cs` - Main application configuration
- `web.config` - IIS-specific configuration
- `appsettings.Production.json` - Production settings

### Swagger Configuration
- `wwwroot/swagger-ui/custom.css` - Custom styling for production
- `Middleware/SwaggerBasicAuthMiddleware.cs` - Optional authentication

### Documentation
- `CORS_CONFIGURATION_GUIDE.md` - Comprehensive CORS troubleshooting
- `SWAGGER_GUIDE.md` - Detailed Swagger configuration guide

### Deployment Scripts
- `deploy-to-iis.ps1` - PowerShell script for IIS setup
- `test-swagger.ps1` - PowerShell script for testing endpoints
- `test-swagger.sh` - Bash script for testing endpoints

## Quick Start

### 1. Build & Publish
```bash
dotnet publish -c Release -o ./publish
```

### 2. Deploy to IIS
```powershell
# Run as Administrator
.\deploy-to-iis.ps1 -SiteName "ProductAPI" -Port 80
```

### 3. Set Environment Variable
```
ASPNETCORE_ENVIRONMENT = Production
```

### 4. Test Deployment
```powershell
.\test-swagger.ps1 -IISBase "http://your-domain"
```

## Access URLs

### Development
- API: `http://localhost:5009`
- Swagger: `http://localhost:5009/swagger`

### Production (IIS)
- API: `http://your-domain`
- Swagger: `http://your-domain/api-docs`

## Key Features

### ✅ CORS Configuration
- Environment-specific policies
- Supports localhost:3000 for development
- Production-ready with specific origins
- Credentials support for authentication

### ✅ Swagger Documentation
- Available in both Development and Production
- Custom styling for production
- Optional Basic Authentication
- Mobile-responsive design

### ✅ IIS Integration
- Proper static files handling
- URL rewriting for SPA routing
- Security headers configuration
- Compression and caching

### ✅ Security
- Environment-specific configurations
- Optional Swagger authentication
- CORS origin restrictions
- Security headers (XSS, CSRF, etc.)

## Troubleshooting

### CORS Issues
1. Check environment variable in IIS
2. Verify frontend URL in CORS policy
3. Test with browser developer tools
4. Check preflight OPTIONS requests

### Swagger Issues
1. Verify `/swagger/v1/swagger.json` endpoint
2. Check static files deployment
3. Test `/api-docs` URL in production
4. Verify environment detection

### IIS Issues
1. Check Application Pool settings
2. Verify ASP.NET Core module installation
3. Check IIS logs for errors
4. Restart Application Pool after changes

## Production Checklist

- [ ] Environment variable set: `ASPNETCORE_ENVIRONMENT=Production`
- [ ] SSL certificate configured (recommended)
- [ ] Static files deployed (`wwwroot` folder)
- [ ] Database connection string updated
- [ ] JWT keys configured securely
- [ ] CORS origins set to production domains
- [ ] Swagger authentication enabled (optional)
- [ ] IIS logs monitoring configured
- [ ] Health check endpoint working

## Support

For detailed troubleshooting, refer to:
- `CORS_CONFIGURATION_GUIDE.md` - CORS issues
- `SWAGGER_GUIDE.md` - Swagger configuration
- IIS logs at `C:\inetpub\logs\LogFiles\W3SVC1\`

## Architecture

```
Frontend (localhost:3000)
    ↓ HTTP Requests
IIS Server
    ↓ ASP.NET Core Module
Your API Application
    ↓ Controllers
Database
```

## Security Considerations

1. **Never use `AllowAnyOrigin()` in production**
2. **Always use HTTPS in production**
3. **Implement proper authentication**
4. **Monitor and log security events**
5. **Keep dependencies updated**

---

*Last updated: July 2025*

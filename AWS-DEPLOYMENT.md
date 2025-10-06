# AWS App Runner Deployment Guide

This guide explains how to deploy your HotelBooking .NET 9 API to AWS App Runner with RDS MySQL database.

## Prerequisites

1. **AWS CLI installed and configured**
   ```powershell
   # Install AWS CLI if not already installed
   winget install Amazon.AWSCLI
   
   # Configure AWS CLI with your credentials
   aws configure
   ```

2. **Required permissions**
   Your AWS user needs the following permissions:
   - `AmazonRDSFullAccess`
   - `AWSAppRunnerFullAccess`
   - `IAMFullAccess`
   - `AmazonEC2ReadOnlyAccess`

3. **GitHub repository**
   - Your code must be in a public GitHub repository
   - The repository must contain the `Dockerfile` and `apprunner.yaml`

## Deployment Steps

### 1. Prepare your repository

Make sure your GitHub repository contains:
- `Dockerfile` (already configured)
- `apprunner.yaml` (configuration file for App Runner)
- Your .NET 9 application code

### 2. Run the deployment script

```powershell
# Navigate to your project directory
cd "c:\Users\Влад\source\repos\HotelBooking"

# Run the deployment script
.\deploy-aws.ps1 -AppName "hotelbooking" -GitHubRepo "yourusername/HotelBooking"

# Or with custom parameters
.\deploy-aws.ps1 `
    -AppName "hotelbooking" `
    -GitHubRepo "yourusername/HotelBooking" `
    -Region "us-west-2" `
    -DatabasePassword "YourSecurePassword123!" `
    -JwtSecret "your-super-secret-jwt-key-that-is-at-least-32-characters-long"
```

### 3. What the script does

The deployment script automatically:

1. **Creates RDS MySQL database**
   - Sets up VPC subnet group
   - Creates security group for database access
   - Provisions MySQL 8.0 instance (db.t3.micro)
   - Configures public accessibility

2. **Sets up IAM roles**
   - Creates App Runner instance role
   - Creates GitHub access role for ECR
   - Attaches necessary policies

3. **Deploys App Runner service**
   - Connects to your GitHub repository
   - Configures auto-deployment on code changes
   - Sets up environment variables
   - Provisions 0.25 vCPU / 0.5 GB instance

## Configuration Files

### apprunner.yaml
```yaml
version: 1.0
runtime: docker
build:
  commands:
    build:
      - echo "Building .NET application with Docker"
      - docker build -t hotel-booking-api .
run:
  runtime-version: latest
  command: dotnet HotelBooking.API.dll
  network:
    port: 8080
    env: PORT
  env:
    - name: ASPNETCORE_URLS
      value: http://+:8080
    - name: ASPNETCORE_ENVIRONMENT
      value: Production
    - name: PORT
      value: "8080"
```

### Environment Variables

The following environment variables are automatically configured:
- `DATABASE_URL`: MySQL connection string
- `JWT_SECRET_KEY`: JWT signing key
- `JWT_ISSUER`: JWT issuer (HotelBookingAPI)
- `JWT_AUDIENCE`: JWT audience (HotelBookingClients)
- `ASPNETCORE_URLS`: App binding URL
- `ASPNETCORE_ENVIRONMENT`: Production
- `PORT`: Application port (8080)

## Cost Estimation

### App Runner
- **0.25 vCPU / 0.5 GB**: ~$7-10/month
- **Request charges**: $0.40 per million requests

### RDS MySQL (db.t3.micro)
- **Instance**: ~$13-15/month
- **Storage (20 GB)**: ~$2.3/month
- **Total RDS**: ~$15-17/month

**Total estimated cost**: ~$22-27/month

## Monitoring and Management

### Check service status
```powershell
# Get service details
aws apprunner describe-service --service-arn YOUR_SERVICE_ARN --region us-east-1

# View logs
aws logs describe-log-groups --log-group-name-prefix "/aws/apprunner/hotelbooking"
```

### Database management
```powershell
# Connect to RDS MySQL
mysql -h YOUR_RDS_ENDPOINT -u admin -p hotelbookingdb

# Check database status
aws rds describe-db-instances --db-instance-identifier hotelbooking-mysql-db
```

## Updating Your Application

App Runner automatically deploys when you push changes to your GitHub repository's main branch.

### Manual deployment
```powershell
# Trigger manual deployment
aws apprunner start-deployment --service-arn YOUR_SERVICE_ARN --region us-east-1
```

## Custom Domain Setup

To use a custom domain:

1. **Add domain to App Runner**
   ```powershell
   aws apprunner associate-custom-domain `
       --service-arn YOUR_SERVICE_ARN `
       --domain-name yourdomain.com `
       --region us-east-1
   ```

2. **Configure DNS**
   - Add CNAME records as provided by App Runner
   - Certificate is automatically managed by AWS

## Frontend Configuration

Update your frontend to use the new API URL:

```javascript
// In your frontend configuration
const API_BASE_URL = 'https://your-apprunner-url.us-east-1.awsapprunner.com';
```

## Troubleshooting

### Common Issues

1. **Build failures**
   - Check Dockerfile syntax
   - Verify .NET 9 runtime in base image
   - Ensure proper project structure

2. **Database connection issues**
   - Verify security group settings
   - Check connection string format
   - Ensure RDS is publicly accessible

3. **Permission errors**
   - Verify IAM roles and policies
   - Check AWS CLI configuration
   - Ensure sufficient permissions

### Logs and Debugging

```powershell
# View App Runner logs
aws logs tail /aws/apprunner/hotelbooking --follow --region us-east-1

# Check service events
aws apprunner describe-service --service-arn YOUR_SERVICE_ARN --query "Service.ServiceUrl" --region us-east-1
```

## Security Considerations

1. **Database security**
   - Consider restricting RDS access to App Runner only
   - Use strong passwords
   - Enable encryption at rest

2. **Application security**
   - Keep JWT secrets secure
   - Use HTTPS only
   - Implement rate limiting

3. **IAM security**
   - Follow principle of least privilege
   - Regularly rotate access keys
   - Monitor CloudTrail logs

## Cleanup

To remove all resources:

```powershell
# Delete App Runner service
aws apprunner delete-service --service-arn YOUR_SERVICE_ARN --region us-east-1

# Delete RDS instance
aws rds delete-db-instance `
    --db-instance-identifier hotelbooking-mysql-db `
    --skip-final-snapshot `
    --region us-east-1

# Delete IAM roles
aws iam delete-role --role-name hotelbooking-apprunner-role
aws iam delete-role --role-name hotelbooking-apprunner-access-role

# Delete security group and subnet group
aws ec2 delete-security-group --group-name hotelbooking-rds-sg --region us-east-1
aws rds delete-db-subnet-group --db-subnet-group-name hotelbooking-subnet-group --region us-east-1
```

## Support

- AWS App Runner Documentation: https://docs.aws.amazon.com/apprunner/
- AWS RDS Documentation: https://docs.aws.amazon.com/rds/
- .NET on AWS: https://aws.amazon.com/developer/language/net/
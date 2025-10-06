# AWS App Runner + RDS MySQL Deployment Script
# Run this script to deploy your HotelBooking API to AWS

param(
    [Parameter(Mandatory=$true)][string]$AppName,
    [Parameter(Mandatory=$true)][string]$GitHubRepo,
    [Parameter(Mandatory=$false)][string]$Region = "us-east-1",
    [Parameter(Mandatory=$false)][string]$DatabasePassword = "HotelBooking123!",
    [Parameter(Mandatory=$false)][string]$JwtSecret = "your-super-secret-jwt-key-at-least-32-characters-long"
)

Write-Host "🚀 Starting AWS App Runner + RDS MySQL deployment..." -ForegroundColor Green
Write-Host "App Name: $AppName" -ForegroundColor Cyan
Write-Host "GitHub Repo: $GitHubRepo" -ForegroundColor Cyan
Write-Host "Region: $Region" -ForegroundColor Cyan

# Step 1: Create RDS MySQL Database
Write-Host "`n📊 Creating RDS MySQL database..." -ForegroundColor Yellow

$dbInstanceId = "$AppName-mysql-db"
$dbName = "hotelbookingdb"

try {
    # Create DB subnet group
    Write-Host "Creating DB subnet group..."
    $vpcId = (aws ec2 describe-vpcs --filters "Name=is-default,Values=true" --query "Vpcs[0].VpcId" --output text --region $Region)
    $subnetIds = (aws ec2 describe-subnets --filters "Name=vpc-id,Values=$vpcId" --query "Subnets[*].SubnetId" --output text --region $Region) -split "`t"
    
    aws rds create-db-subnet-group `
        --db-subnet-group-name "$AppName-subnet-group" `
        --db-subnet-group-description "Subnet group for $AppName MySQL database" `
        --subnet-ids $subnetIds `
        --region $Region

    # Create security group for RDS
    Write-Host "Creating security group for RDS..."
    $sgId = aws ec2 create-security-group `
        --group-name "$AppName-rds-sg" `
        --description "Security group for $AppName RDS MySQL" `
        --vpc-id $vpcId `
        --query "GroupId" `
        --output text `
        --region $Region

    # Allow MySQL connections from anywhere (you may want to restrict this)
    aws ec2 authorize-security-group-ingress `
        --group-id $sgId `
        --protocol tcp `
        --port 3306 `
        --cidr 0.0.0.0/0 `
        --region $Region

    # Create RDS MySQL instance
    Write-Host "Creating RDS MySQL instance (this may take 10-15 minutes)..."
    aws rds create-db-instance `
        --db-instance-identifier $dbInstanceId `
        --db-instance-class db.t3.micro `
        --engine mysql `
        --engine-version "8.0" `
        --master-username admin `
        --master-user-password $DatabasePassword `
        --allocated-storage 20 `
        --db-name $dbName `
        --vpc-security-group-ids $sgId `
        --db-subnet-group-name "$AppName-subnet-group" `
        --backup-retention-period 7 `
        --no-multi-az `
        --storage-type gp2 `
        --publicly-accessible `
        --region $Region

    Write-Host "Waiting for RDS instance to be available..." -ForegroundColor Yellow
    aws rds wait db-instance-available --db-instance-identifier $dbInstanceId --region $Region

    # Get RDS endpoint
    $rdsEndpoint = aws rds describe-db-instances `
        --db-instance-identifier $dbInstanceId `
        --query "DBInstances[0].Endpoint.Address" `
        --output text `
        --region $Region

    $connectionString = "Server=$rdsEndpoint;Database=$dbName;Uid=admin;Pwd=$DatabasePassword;"
    Write-Host "✅ RDS MySQL created successfully!" -ForegroundColor Green
    Write-Host "Endpoint: $rdsEndpoint" -ForegroundColor Cyan
    Write-Host "Connection String: $connectionString" -ForegroundColor Cyan

} catch {
    Write-Host "❌ Failed to create RDS instance: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Create IAM roles for App Runner
Write-Host "`n🔐 Creating IAM roles for App Runner..." -ForegroundColor Yellow

$roleName = "$AppName-apprunner-role"
$accessRoleName = "$AppName-apprunner-access-role"

# Create App Runner instance role
$trustPolicy = @{
    Version = "2012-10-17"
    Statement = @(
        @{
            Effect = "Allow"
            Principal = @{
                Service = "tasks.apprunner.amazonaws.com"
            }
            Action = "sts:AssumeRole"
        }
    )
} | ConvertTo-Json -Depth 3

$trustPolicyFile = "trust-policy.json"
$trustPolicy | Out-File -FilePath $trustPolicyFile -Encoding utf8

aws iam create-role `
    --role-name $roleName `
    --assume-role-policy-document file://$trustPolicyFile `
    --region $Region

# Create GitHub access role
$githubTrustPolicy = @{
    Version = "2012-10-17"
    Statement = @(
        @{
            Effect = "Allow"
            Principal = @{
                Service = "build.apprunner.amazonaws.com"
            }
            Action = "sts:AssumeRole"
        }
    )
} | ConvertTo-Json -Depth 3

$githubTrustPolicyFile = "github-trust-policy.json"
$githubTrustPolicy | Out-File -FilePath $githubTrustPolicyFile -Encoding utf8

aws iam create-role `
    --role-name $accessRoleName `
    --assume-role-policy-document file://$githubTrustPolicyFile `
    --region $Region

# Attach managed policy for ECR access
aws iam attach-role-policy `
    --role-name $accessRoleName `
    --policy-arn "arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess" `
    --region $Region

# Get role ARNs
$instanceRoleArn = aws iam get-role --role-name $roleName --query "Role.Arn" --output text --region $Region
$accessRoleArn = aws iam get-role --role-name $accessRoleName --query "Role.Arn" --output text --region $Region

Write-Host "✅ IAM roles created successfully!" -ForegroundColor Green

# Step 3: Create App Runner service
Write-Host "`n🏃 Creating App Runner service..." -ForegroundColor Yellow

$serviceConfig = @{
    ServiceName = $AppName
    SourceConfiguration = @{
        CodeRepository = @{
            RepositoryUrl = "https://github.com/$GitHubRepo"
            SourceCodeVersion = @{
                Type = "BRANCH"
                Value = "main"
            }
            CodeConfiguration = @{
                ConfigurationSource = "API"
                CodeConfigurationValues = @{
                    Runtime = "DOCKER"
                    BuildCommand = "docker build -t hotel-booking-api ."
                    StartCommand = "dotnet HotelBooking.API.dll"
                    RuntimeEnvironmentVariables = @{
                        "ASPNETCORE_URLS" = "http://+:8080"
                        "ASPNETCORE_ENVIRONMENT" = "Production"
                        "PORT" = "8080"
                        "DATABASE_URL" = $connectionString
                        "JWT_SECRET_KEY" = $JwtSecret
                        "JWT_ISSUER" = "HotelBookingAPI"
                        "JWT_AUDIENCE" = "HotelBookingClients"
                    }
                }
            }
        }
        AutoDeploymentsEnabled = $true
    }
    InstanceConfiguration = @{
        Cpu = "0.25 vCPU"
        Memory = "0.5 GB"
        InstanceRoleArn = $instanceRoleArn
    }
} | ConvertTo-Json -Depth 10

$configFile = "apprunner-config.json"
$serviceConfig | Out-File -FilePath $configFile -Encoding utf8

try {
    $serviceArn = aws apprunner create-service `
        --cli-input-json file://$configFile `
        --query "Service.ServiceArn" `
        --output text `
        --region $Region

    Write-Host "Waiting for App Runner service to be running..." -ForegroundColor Yellow
    aws apprunner wait service-running --service-arn $serviceArn --region $Region

    $serviceUrl = aws apprunner describe-service `
        --service-arn $serviceArn `
        --query "Service.ServiceUrl" `
        --output text `
        --region $Region

    Write-Host "✅ App Runner service created successfully!" -ForegroundColor Green
    Write-Host "Service URL: https://$serviceUrl" -ForegroundColor Cyan
    Write-Host "Service ARN: $serviceArn" -ForegroundColor Cyan

} catch {
    Write-Host "❌ Failed to create App Runner service: $_" -ForegroundColor Red
    exit 1
}

# Cleanup temporary files
Remove-Item -Path $trustPolicyFile, $githubTrustPolicyFile, $configFile -Force -ErrorAction SilentlyContinue

Write-Host "`n🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "Your HotelBooking API is now available at: https://$serviceUrl" -ForegroundColor Cyan
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Test your API endpoints" -ForegroundColor White
Write-Host "2. Update your frontend configuration to use the new API URL" -ForegroundColor White
Write-Host "3. Set up custom domain if needed" -ForegroundColor White
Write-Host "`nDatabase details:" -ForegroundColor Yellow
Write-Host "RDS Endpoint: $rdsEndpoint" -ForegroundColor White
Write-Host "Database Name: $dbName" -ForegroundColor White
Write-Host "Username: admin" -ForegroundColor White
Write-Host "Password: $DatabasePassword" -ForegroundColor White
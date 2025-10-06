# PowerShell deployment script for S3
param(
    [Parameter(Mandatory=$true)]
    [string]$BucketName,
    
    [Parameter(Mandatory=$false)]
    [string]$BackendUrl = "https://your-backend-api.herokuapp.com/api",
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "us-east-1"
)

Write-Host "Deploying HotelBooking Frontend to S3..." -ForegroundColor Green

# Check if AWS CLI is installed
try {
    aws --version | Out-Null
} catch {
    Write-Error "AWS CLI not found. Please install AWS CLI and configure credentials."
    exit 1
}

# Update API URL
Write-Host "Updating API URL to: $BackendUrl" -ForegroundColor Yellow
$apiClientContent = Get-Content "apiClient.js" -Raw
$updatedContent = $apiClientContent -replace "const BASE_URL = '[^']*';", "const BASE_URL = '$BackendUrl';"
Set-Content "apiClient.js" -Value $updatedContent

# Create bucket if it doesn't exist
Write-Host "Creating S3 bucket: $BucketName" -ForegroundColor Yellow
aws s3 mb "s3://$BucketName" --region $Region 2>$null

# Configure static website hosting
Write-Host "Configuring static website hosting..." -ForegroundColor Yellow
aws s3 website "s3://$BucketName" --index-document login.html --error-document login.html

# Update bucket policy with correct bucket name
Write-Host "Setting bucket policy..." -ForegroundColor Yellow
$policyContent = Get-Content "bucket-policy.json" -Raw
$updatedPolicy = $policyContent -replace "your-hotel-booking-frontend", $BucketName
Set-Content "temp-policy.json" -Value $updatedPolicy
aws s3api put-bucket-policy --bucket $BucketName --policy file://temp-policy.json
Remove-Item "temp-policy.json"

# Sync files to S3
Write-Host "Uploading files to S3..." -ForegroundColor Yellow
aws s3 sync . "s3://$BucketName" --exclude "node_modules/*" --exclude ".git/*" --exclude "*.md" --exclude "*.ps1" --exclude "*.json" --exclude "package*"

# Set correct content types for JS files
Write-Host "Setting content types..." -ForegroundColor Yellow
aws s3 cp "s3://$BucketName/apiClient.js" "s3://$BucketName/apiClient.js" --content-type "application/javascript" --metadata-directive REPLACE
aws s3 cp "s3://$BucketName/app.js" "s3://$BucketName/app.js" --content-type "application/javascript" --metadata-directive REPLACE

$websiteUrl = "http://$BucketName.s3-website-$Region.amazonaws.com"
Write-Host "Deployment completed!" -ForegroundColor Green
Write-Host "Website URL: $websiteUrl" -ForegroundColor Cyan
Write-Host "Don't forget to update CORS in your backend to include: $websiteUrl" -ForegroundColor Yellow
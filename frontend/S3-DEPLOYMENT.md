# S3 Static Website Deployment

This guide shows how to deploy the HotelBooking frontend to Amazon S3 as a static website.

## Prerequisites

1. AWS CLI installed and configured:
```bash
aws configure
```

2. Your backend API deployed and accessible (Heroku, EC2, etc.)

## Deployment Steps

### 1. Update API Configuration

Update `apiClient.js` with your production backend URL:
```javascript
const BASE_URL = 'https://your-backend-api.herokuapp.com/api';
// or your backend domain
```

### 2. Create S3 Bucket

```bash
# Replace 'your-hotel-booking-frontend' with your bucket name
aws s3 mb s3://your-hotel-booking-frontend --region us-east-1
```

### 3. Configure Static Website Hosting

```bash
aws s3 website s3://your-hotel-booking-frontend --index-document login.html --error-document login.html
```

### 4. Set Bucket Policy for Public Access

Create `bucket-policy.json`:
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::your-hotel-booking-frontend/*"
        }
    ]
}
```

Apply the policy:
```bash
aws s3api put-bucket-policy --bucket your-hotel-booking-frontend --policy file://bucket-policy.json
```

### 5. Deploy Files

```bash
# Sync all files to S3
aws s3 sync . s3://your-hotel-booking-frontend --exclude "node_modules/*" --exclude ".git/*" --exclude "*.md"

# Set correct content types
aws s3 cp s3://your-hotel-booking-frontend/apiClient.js s3://your-hotel-booking-frontend/apiClient.js --content-type "application/javascript" --metadata-directive REPLACE
aws s3 cp s3://your-hotel-booking-frontend/app.js s3://your-hotel-booking-frontend/app.js --content-type "application/javascript" --metadata-directive REPLACE
```

### 6. Your Website is Live!

URL: `http://your-hotel-booking-frontend.s3-website-us-east-1.amazonaws.com`

## Optional: CloudFront CDN

For better performance and HTTPS:

### 1. Create CloudFront Distribution
```bash
aws cloudfront create-distribution --distribution-config file://cloudfront-config.json
```

### 2. Update CORS in Backend
Add your CloudFront domain to CORS policy in your backend:
```csharp
.WithOrigins("https://d1234567890.cloudfront.net", "http://localhost:5500")
```

## Environment-Specific Configuration

For different environments, you can create multiple versions:

**Development**: `apiClient.dev.js`
```javascript
const BASE_URL = 'http://localhost:5127/api';
```

**Production**: `apiClient.prod.js`  
```javascript
const BASE_URL = 'https://your-backend-api.herokuapp.com/api';
```

## Automated Deployment Script

See `deploy.ps1` for automated deployment.

## Costs
- S3 hosting: ~$0.50-2.00/month for small sites
- CloudFront: ~$1-5/month depending on traffic
- Much cheaper than Elastic Beanstalk ($10-50+/month)
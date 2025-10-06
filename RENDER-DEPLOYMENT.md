# Deploy HotelBooking Backend to Render

Render is the simplest way to deploy your .NET backend - no Docker, no complex configs, just connect GitHub!

## 🚀 Quick Deploy Steps

### 1. Push Your Code to GitHub
```bash
git add .
git commit -m "Add Render deployment config"
git push origin main
```

### 2. Deploy on Render

1. **Go to [Render.com](https://render.com)**
2. **Sign up/Login** with GitHub
3. **Click "New +" → "Web Service"**
4. **Connect your GitHub repository** (`HotelBooking`)
5. **Configure the service:**

   **Basic Settings:**
   - **Name**: `hotelbooking-api`
   - **Environment**: `Docker` (Render will auto-detect .NET)
   - **Region**: Choose closest to you
   - **Branch**: `main`

   **Build & Deploy:**
   - **Root Directory**: `backend/HotelBooking.API`
   - **Build Command**: `dotnet restore && dotnet publish -c Release -o out`
   - **Start Command**: `cd out && dotnet HotelBooking.API.dll`

### 3. Add MySQL Database

1. **In Render Dashboard** → "New +" → "MySQL" 
2. **Create database** (free tier available)
3. **Copy the DATABASE_URL** (internal connection string)

### 4. Set Environment Variables

In your Web Service → Environment tab, add:

```
DATABASE_URL=mysql://username:password@hostname:port/database_name
JWT_SECRET_KEY=your-super-secret-jwt-key-32-characters-minimum
JWT_ISSUER=HotelBookingIssuer
JWT_AUDIENCE=HotelBookingAudience
ASPNETCORE_ENVIRONMENT=Production
PORT=10000
```

### 5. Your API is Live! 🎉

Render will give you a URL like: `https://hotelbooking-api.onrender.com`

## 🔧 Files Created

- ✅ `render.yaml` - Render configuration (optional, can use web UI instead)
- ✅ Updated `Program.cs` - Supports Render environment variables

## 💡 What Render Does Automatically

- **Builds** your .NET application
- **Deploys** to global CDN  
- **Provides** managed PostgreSQL database
- **Generates** HTTPS domain with SSL
- **Auto-redeploys** on git push
- **Health checks** and auto-restart
- **Logs** and monitoring

## 💰 Pricing

- **Free Tier**: 750 hours/month (enough for hobby projects)
- **Starter Plan**: $7/month (always-on, no sleep)
- **MySQL**: Free 1GB, then $7/month for 10GB

## 🔄 Alternative: Infrastructure as Code

If you prefer config files over web UI, use the `render.yaml`:

```yaml
services:
  - type: web
    name: hotelbooking-api
    env: dotnet
    buildCommand: cd backend/HotelBooking.API && dotnet restore && dotnet publish -c Release -o out
    startCommand: cd backend/HotelBooking.API/out && dotnet HotelBooking.API.dll
    
databases:
  - name: hotelbooking-db
    databaseName: hotelbooking
```

## 🌐 Update Frontend

After deployment, update your frontend `apiClient.js`:

```javascript
const BASE_URL = 'https://hotelbooking-api.onrender.com/api';
```

## 🏥 Health Check Endpoint

Add to your `Program.cs` before `app.Run()`:

```csharp
app.MapGet("/health", () => "OK");
```

## 🐛 Debugging

### Check Logs
In Render dashboard → your service → "Logs" tab

### Common Issues
1. **Build Fails**: Check .NET version (should be 9.0)
2. **Database Issues**: Verify `DATABASE_URL` is set correctly
3. **CORS Errors**: Update allowed origins in `Program.cs`
4. **Port Issues**: Render uses `PORT` env var (automatically set)

## 🚀 Advanced Features

### Custom Domain
1. **Render dashboard** → your service → Settings → Custom Domains
2. **Add your domain** and configure DNS

### Auto-Deploy Branches
- **Settings** → Deploy → Auto-Deploy: Enable
- **Choose branch** (main, develop, etc.)

### Preview Deployments
- **Pull Request** deployments automatically created
- **Test changes** before merging

## 📊 Monitoring

Render provides:
- **Uptime monitoring**
- **Response time metrics**  
- **Error rate tracking**
- **Resource usage**

## ✅ Next Steps

1. **Deploy backend** following steps above
2. **Get your Render URL**
3. **Update frontend** `apiClient.js` with Render URL
4. **Deploy frontend** to S3 using the S3 guide
5. **Test** your full-stack app!

## 🔄 Database Migrations

Run migrations after deployment:

```bash
# Connect to your service and run
dotnet ef database update
```

Or add migration to startup in `Program.cs`:

```csharp
// After building the app
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
    context.Database.Migrate();
}
```

---

**Ready to deploy?** Push to GitHub and connect to Render! 🚀

**Render is perfect because:**
- ✅ No Docker configuration needed
- ✅ Native .NET support  
- ✅ Free tier available
- ✅ Automatic HTTPS
- ✅ Easy database integration
- ✅ Great for production
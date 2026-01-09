# How to Generate SonarQube Token

## Option 1: SonarCloud (Recommended)

### Step 1: Sign Up / Sign In
1. Go to **[https://sonarcloud.io](https://sonarcloud.io)**
2. Click **"Log in"** in the top right
3. Choose **"Log in with GitHub"**
4. Authorize SonarCloud to access your GitHub account

### Step 2: Import Your Repository
1. After logging in, click the **"+"** button in the top right
2. Select **"Analyze new project"**
3. Choose your GitHub organization/account
4. Select your repository: `proiect .NET` or `TasteTest/UniBytes`
5. Click **"Set Up"**

### Step 3: Generate Token
1. Once logged in, click on your **profile picture** (top right)
2. Go to **"My Account"**
3. Click on the **"Security"** tab
4. Under **"Generate Tokens"**:
   - **Name**: `GitHub Actions` (or any name you prefer)
   - **Type**: Select **"Global Analysis Token"** or **"Project Analysis Token"**
   - **Expires in**: Choose expiration (recommend: 90 days or No expiration)
5. Click **"Generate"**
6. **COPY THE TOKEN IMMEDIATELY** - You won't be able to see it again!
   - It will look like: `sqp_1234567890abcdef1234567890abcdef12345678`

### Step 4: Get Your Organization Key
1. In SonarCloud, go to **"My Account"** → **"Organizations"**
2. Copy your **Organization Key** (you'll need this for the project key)

### Step 5: Configure GitHub Secrets
1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. Add these two secrets:

   **Secret 1:**
   - Name: `SONAR_TOKEN`
   - Value: `sqp_1234...` (paste your token)
   
   **Secret 2:**
   - Name: `SONAR_HOST_URL`
   - Value: `https://sonarcloud.io`

### Step 6: Update Project Keys (if needed)
If your organization key is different, update these files:

**`.github/workflows/sonarqube.yml`** - Update project keys:
```yaml
/k:"your-org-key_unibytes-tasttest-backend"
```

---

## Option 2: Self-Hosted SonarQube

### Step 1: Start SonarQube Server

#### Quick Start with Docker:
```bash
docker run -d --name sonarqube \
  -p 9000:9000 \
  -e SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true \
  sonarqube:latest
```

Wait 2-3 minutes for SonarQube to start, then access: **http://localhost:9000**

### Step 2: Initial Login
1. Go to **http://localhost:9000**
2. Default credentials:
   - **Username**: `admin`
   - **Password**: `admin`
3. You'll be prompted to **change the password** - do this immediately

### Step 3: Create Projects
1. Click **"Create Project"** → **"Manually"**
2. Create first project:
   - **Project key**: `unibytes-tasttest-backend`
   - **Display name**: `UniBytes Backend`
   - Click **"Set Up"**
3. Repeat for frontend:
   - **Project key**: `unibytes-tasttest-frontend`
   - **Display name**: `UniBytes Frontend`

### Step 4: Generate Token
1. Click on your **profile icon** (top right, shows 'A' for admin)
2. Select **"My Account"**
3. Click the **"Security"** tab
4. Under **"Generate Tokens"**:
   - **Name**: `GitHub Actions`
   - **Type**: **"Global Analysis Token"** (recommended) or **"Project Analysis Token"**
   - **Expires in**: Choose expiration (or "No expiration" for testing)
5. Click **"Generate"**
6. **COPY THE TOKEN** - Save it somewhere safe!
   - Format: `sqp_abc123def456...`

### Step 5: Configure GitHub Secrets
1. Go to your GitHub repository
2. **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. Add these secrets:

   **Secret 1:**
   - Name: `SONAR_TOKEN`
   - Value: (paste your token)
   
   **Secret 2:**
   - Name: `SONAR_HOST_URL`
   - Value: `http://your-server-ip:9000` or `https://your-domain.com`
   
   ⚠️ **Important**: If using `http://localhost:9000`, GitHub Actions won't be able to reach it. You need:
   - A public IP address, or
   - A domain name, or
   - Use SonarCloud instead

---

## Visual Guide - SonarCloud Token Generation

```
1. https://sonarcloud.io → Log in with GitHub
                ↓
2. Profile Picture → My Account
                ↓
3. Security Tab
                ↓
4. Generate Tokens Section
                ↓
5. Fill in:
   - Name: GitHub Actions
   - Type: Global Analysis Token
   - Expires: 90 days (or No expiration)
                ↓
6. Click "Generate"
                ↓
7. COPY TOKEN (sqp_...)
                ↓
8. GitHub Repo → Settings → Secrets → Actions
                ↓
9. Add SONAR_TOKEN and SONAR_HOST_URL
```

---

## Testing Your Token

### Test Locally (Backend):
```bash
export SONAR_TOKEN="your-token-here"
export SONAR_HOST_URL="https://sonarcloud.io"  # or your server URL

cd backend

dotnet sonarscanner begin \
  /k:"unibytes-tasttest-backend" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.login="$SONAR_TOKEN"

dotnet build UniBytes.sln

dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"
```

If you see "Analysis report uploaded successfully", your token works! ✅

---

## Troubleshooting

### "Not authorized. Please check the user token"
- Token is incorrect or expired
- Regenerate a new token
- Make sure you copied the entire token

### "Project not found"
- Create the project in SonarQube first
- Verify project key matches exactly

### "Could not connect to SonarQube server"
- Check `SONAR_HOST_URL` is correct
- For self-hosted: ensure server is running and accessible
- For SonarCloud: use `https://sonarcloud.io`

### GitHub Actions can't reach self-hosted server
- Self-hosted SonarQube must be publicly accessible
- Use ngrok for testing: `ngrok http 9000`
- Or use SonarCloud instead

---

## Quick Reference

| Platform | Host URL | Token Format |
|----------|----------|--------------|
| **SonarCloud** | `https://sonarcloud.io` | `sqp_...` |
| **Self-Hosted** | `http://your-ip:9000` | `sqp_...` |

## Security Best Practices

✅ **DO:**
- Use tokens with expiration dates
- Rotate tokens regularly (every 90 days)
- Use different tokens for different purposes
- Store tokens in GitHub Secrets (never in code)

❌ **DON'T:**
- Commit tokens to git
- Share tokens publicly
- Use the same token everywhere
- Use tokens without expiration in production

---

## Next Steps

After generating your token:

1. ✅ Add `SONAR_TOKEN` to GitHub Secrets
2. ✅ Add `SONAR_HOST_URL` to GitHub Secrets
3. ✅ Push code to trigger the workflow
4. ✅ Check GitHub Actions tab for results
5. ✅ View analysis in SonarQube/SonarCloud dashboard

**Need help?** Check the [full setup guide](./SONARQUBE_SETUP.md)

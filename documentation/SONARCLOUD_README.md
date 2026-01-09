# SonarCloud Integration - Complete Setup

This repository is configured for **SonarCloud** code quality analysis. Follow these steps to activate it.

## 🚀 Quick Start (5 Minutes)

### 1. Sign Up for SonarCloud
- Go to [https://sonarcloud.io](https://sonarcloud.io)
- Click **"Log in with GitHub"**
- Authorize SonarCloud

### 2. Import Your Repository
- Click **"+"** → **"Analyze new project"**
- Select this repository
- Click **"Set Up"**

### 3. Get Your Keys
You'll need these values from SonarCloud:
- **Organization Key**: Found in My Account → Organizations
- **Project Key**: Shown when you create the project

### 4. Generate Token
- Go to **My Account** → **Security** → **Generate Tokens**
- Name: `GitHub Actions`
- Click **"Generate"**
- **Copy the token** (starts with `sqp_...`)

### 5. Add GitHub Secrets
In your GitHub repository:
- Go to **Settings** → **Secrets and variables** → **Actions**
- Add these secrets:

| Secret Name | Value |
|------------|-------|
| `SONAR_TOKEN` | Your token from step 4 |
| `SONAR_HOST_URL` | `https://sonarcloud.io` |

### 6. Update Workflow File
Edit `.github/workflows/sonarqube.yml`:

**Replace these placeholders:**
```yaml
# Line ~45 (Backend):
/k:"YOUR-ORG-KEY_YOUR-PROJECT-KEY-backend"
/o:"YOUR-ORG-KEY"

# Line ~103 (Frontend):
-Dsonar.projectKey=YOUR-ORG-KEY_YOUR-PROJECT-KEY-frontend
-Dsonar.organization=YOUR-ORG-KEY
```

**With your actual values:**
```yaml
# Example if your org is "theo":
/k:"theo_unibytes-backend"
/o:"theo"

-Dsonar.projectKey=theo_unibytes-frontend
-Dsonar.organization=theo
```

### 7. Install SonarCloud GitHub App (Optional)
- Go to [https://github.com/apps/sonarcloud](https://github.com/apps/sonarcloud)
- Click **"Install"**
- Select this repository

### 8. Test It!
- Push a commit or create a PR
- Check the **Actions** tab
- View results in SonarCloud

---

## 📚 Detailed Guides

| Guide | Description |
|-------|-------------|
| [**SONARCLOUD_SETUP.md**](./documentation/SONARCLOUD_SETUP.md) | Complete setup guide with screenshots |
| [**INSTALL_SONARCLOUD_BOT.md**](./documentation/INSTALL_SONARCLOUD_BOT.md) | How to install the GitHub App |
| [**GENERATE_SONAR_TOKEN.md**](./documentation/GENERATE_SONAR_TOKEN.md) | How to generate tokens |

---

## ✅ What You Get

Once configured, every push/PR automatically:
- ✅ Analyzes code quality and security
- ✅ Runs tests with coverage tracking
- ✅ Checks quality gates
- ✅ Posts results to PR (if bot installed)
- ✅ Blocks merge if quality gate fails

---

## 🔍 View Results

**SonarCloud Dashboard:**
- Go to [https://sonarcloud.io/projects](https://sonarcloud.io/projects)
- Click on your project

**GitHub PR:**
- Look for "SonarCloud Code Analysis" check
- Click "Details" to see full report

---

## 🆘 Troubleshooting

### Workflow fails with "Not authorized"
→ Check that `SONAR_TOKEN` is set correctly in GitHub Secrets

### "Project not found"
→ Make sure you created the project in SonarCloud first

### No SonarCloud check in PR
→ Verify the workflow file has correct project keys

### Need help?
→ Check the detailed guides in `documentation/` folder

---

## 📊 Quality Metrics Tracked

- **Bugs**: Potential runtime errors
- **Vulnerabilities**: Security issues
- **Code Smells**: Maintainability issues
- **Coverage**: Test coverage percentage
- **Duplications**: Duplicate code blocks
- **Security Hotspots**: Code requiring security review

---

## 🎯 Quality Gate (Default)

| Metric | Threshold |
|--------|-----------|
| Coverage | > 80% |
| Duplicated Lines | < 3% |
| Maintainability | Rating A |
| Reliability | Rating A |
| Security | Rating A |

---

## 📝 Files in This Setup

```
.github/workflows/
  └── sonarqube.yml          # GitHub Actions workflow

documentation/
  ├── SONARCLOUD_SETUP.md    # Complete setup guide
  ├── INSTALL_SONARCLOUD_BOT.md  # GitHub App installation
  └── GENERATE_SONAR_TOKEN.md    # Token generation guide

sonar-project.properties     # SonarCloud configuration
.gitignore                   # Excludes SonarCloud temp files
```

---

## 🔐 Security Notes

- ✅ Never commit `SONAR_TOKEN` to git
- ✅ Store tokens in GitHub Secrets only
- ✅ Use tokens with expiration dates
- ✅ Rotate tokens every 90 days

---

## 🎉 You're All Set!

Once configured, SonarCloud will automatically analyze your code on every push and PR.

**Questions?** Check the detailed guides in the `documentation/` folder.

**Ready to start?** Follow the Quick Start above! ⬆️

# SonarCloud Setup Guide - Step by Step

This guide will walk you through setting up SonarCloud for your GitHub repository in **5 minutes**.

## What is SonarCloud?

SonarCloud is a **free** cloud-based code quality and security service that:
- ✅ Analyzes your code automatically on every push/PR
- ✅ Detects bugs, vulnerabilities, and code smells
- ✅ Tracks code coverage
- ✅ Provides quality gates to prevent bad code from merging
- ✅ **100% Free for public repositories**

---

## Step-by-Step Setup

### Step 1: Sign Up for SonarCloud

1. **Go to**: [https://sonarcloud.io](https://sonarcloud.io)
2. Click **"Log in"** (top right)
3. Choose **"Log in with GitHub"**
4. Click **"Authorize SonarCloud"** when prompted
5. SonarCloud will ask for permissions - click **"Authorize SonarSource"**

✅ You're now logged into SonarCloud!

---

### Step 2: Import Your Repository

1. After logging in, click the **"+"** button in the top right corner
2. Select **"Analyze new project"**
3. You'll see a list of your GitHub organizations/accounts
4. Find and select your repository (look for `proiect .NET` or your repo name)
5. Click **"Set Up"** next to your repository

SonarCloud will create a project for you automatically.

---

### Step 3: Configure Analysis Method

After clicking "Set Up", SonarCloud will ask how you want to analyze your code:

1. Choose **"With GitHub Actions"** (recommended)
2. SonarCloud will show you a configuration page

**Important**: Keep this page open - we'll need information from it!

---

### Step 4: Get Your SonarCloud Token

On the configuration page, SonarCloud will show:

1. Your **Organization Key** (something like `your-github-username`)
2. Your **Project Key** (something like `your-github-username_proiect-net`)
3. A button to **"Generate a token"** or **"Create a token"**

**Generate the token:**
1. Click **"Generate a token"** or go to **My Account** → **Security** → **Generate Tokens**
2. Name it: `GitHub Actions`
3. Click **"Generate"**
4. **COPY THE TOKEN** - it looks like: `sqp_1234567890abcdef...`
   - ⚠️ **Save it somewhere safe - you won't see it again!**

---

### Step 5: Add GitHub Secrets

Now we'll add the token to your GitHub repository:

1. **Open your GitHub repository** in a browser
2. Go to **Settings** (top menu of your repo)
3. In the left sidebar, click **"Secrets and variables"** → **"Actions"**
4. Click **"New repository secret"** (green button)

**Add Secret #1:**
- Name: `SONAR_TOKEN`
- Secret: (paste the token you copied from SonarCloud)
- Click **"Add secret"**

**Add Secret #2:**
- Click **"New repository secret"** again
- Name: `SONAR_HOST_URL`
- Secret: `https://sonarcloud.io`
- Click **"Add secret"**

✅ Secrets configured!

---

### Step 6: Update Project Keys in Workflow

The workflow file needs your specific SonarCloud organization and project keys.

**Get your keys from SonarCloud:**
- **Organization Key**: Found in SonarCloud → My Account → Organizations
- **Project Key**: Found in SonarCloud → Your Project → Project Information

**Update the workflow file:**

Open `.github/workflows/sonarqube.yml` and update these lines:

**For Backend** (around line 38):
```yaml
/k:"YOUR-ORG-KEY_YOUR-PROJECT-KEY-backend"
```

**For Frontend** (around line 79):
```yaml
-Dsonar.projectKey=YOUR-ORG-KEY_YOUR-PROJECT-KEY-frontend
-Dsonar.organization=YOUR-ORG-KEY
```

**Example:**
If your GitHub username is `theo` and repo is `unibytes`:
```yaml
/k:"theo_unibytes-backend"
-Dsonar.organization=theo
```

---

### Step 7: Install SonarCloud GitHub App (Optional but Recommended)

This adds PR comments and checks directly in GitHub:

1. Go to: [https://github.com/apps/sonarcloud](https://github.com/apps/sonarcloud)
2. Click **"Install"** or **"Configure"**
3. Choose your GitHub account/organization
4. Select **"Only select repositories"**
5. Choose your repository
6. Click **"Install"** or **"Save"**

✅ SonarCloud bot is now installed!

**What this does:**
- Adds quality gate status checks to PRs
- Posts comments on PRs with code quality issues
- Shows analysis results directly in GitHub

---

### Step 8: Test It!

1. **Make a small change** to any file in your repo
2. **Commit and push** to `master` or create a PR
3. **Go to GitHub Actions** tab in your repo
4. You should see the **"SonarQube Analysis"** workflow running
5. **Wait for it to complete** (usually 2-5 minutes)
6. **Check SonarCloud**: Go to [https://sonarcloud.io/projects](https://sonarcloud.io/projects)
7. Click on your project to see the analysis results!

---

## Quick Reference

### Your SonarCloud URLs

After setup, you can view your projects at:
- **Dashboard**: `https://sonarcloud.io/organizations/YOUR-ORG-KEY/projects`
- **Backend Project**: `https://sonarcloud.io/project/overview?id=YOUR-PROJECT-KEY-backend`
- **Frontend Project**: `https://sonarcloud.io/project/overview?id=YOUR-PROJECT-KEY-frontend`

### GitHub Secrets Required

| Secret Name | Value | Where to Get It |
|------------|-------|-----------------|
| `SONAR_TOKEN` | `sqp_...` | SonarCloud → My Account → Security → Generate Tokens |
| `SONAR_HOST_URL` | `https://sonarcloud.io` | Always this value for SonarCloud |

---

## Troubleshooting

### "Project not found" error in GitHub Actions

**Solution**: 
1. Make sure you created the project in SonarCloud first
2. Verify the project key in the workflow matches SonarCloud exactly
3. Check that `SONAR_TOKEN` is set correctly in GitHub Secrets

### "Not authorized" error

**Solution**:
1. Regenerate the token in SonarCloud
2. Update `SONAR_TOKEN` in GitHub Secrets
3. Make sure you copied the entire token (starts with `sqp_`)

### Workflow runs but no results in SonarCloud

**Solution**:
1. Check the GitHub Actions logs for errors
2. Verify `SONAR_HOST_URL` is exactly `https://sonarcloud.io`
3. Make sure the organization key matches your SonarCloud organization

### Quality Gate fails

**Solution**:
1. This is expected! It means SonarCloud found issues
2. Click on the project in SonarCloud to see what failed
3. Fix the issues and push again
4. You can adjust quality gate settings in SonarCloud → Quality Gates

---

## What Happens Next?

Once set up, **every time you push code or create a PR**:

1. ✅ GitHub Actions automatically runs SonarQube analysis
2. ✅ Results are sent to SonarCloud
3. ✅ SonarCloud analyzes code quality, security, coverage
4. ✅ Quality gate check runs (pass/fail)
5. ✅ PR gets a status check showing if quality gate passed
6. ✅ SonarCloud bot comments on PR with issues (if installed)

---

## Viewing Results

### In GitHub
- Go to your PR
- Look for **"SonarCloud Code Analysis"** check
- Click **"Details"** to see the full report

### In SonarCloud
- Go to [https://sonarcloud.io](https://sonarcloud.io)
- Click on your project
- See:
  - **Overview**: Overall quality metrics
  - **Issues**: List of bugs, vulnerabilities, code smells
  - **Measures**: Detailed metrics and trends
  - **Code**: Browse code with inline issues
  - **Activity**: History of analyses

---

## Best Practices

1. **Fix issues before merging**: Don't merge PRs with failing quality gates
2. **Review security hotspots**: Always check security findings
3. **Maintain coverage**: Keep test coverage above 80%
4. **Monitor trends**: Watch for increasing technical debt
5. **Configure quality gates**: Adjust thresholds in SonarCloud settings

---

## Need Help?

- 📚 [SonarCloud Documentation](https://docs.sonarcloud.io/)
- 💬 [SonarCloud Community](https://community.sonarsource.com/)
- 🐛 [GitHub Actions Logs](https://github.com/YOUR-REPO/actions)

---

## Summary Checklist

- [ ] Signed up for SonarCloud with GitHub
- [ ] Imported repository in SonarCloud
- [ ] Generated SonarCloud token
- [ ] Added `SONAR_TOKEN` to GitHub Secrets
- [ ] Added `SONAR_HOST_URL` to GitHub Secrets
- [ ] Updated project keys in workflow file
- [ ] Installed SonarCloud GitHub App
- [ ] Pushed code to trigger first analysis
- [ ] Verified results in SonarCloud dashboard

**Once all checked, you're done! 🎉**

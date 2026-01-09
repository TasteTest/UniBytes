# Install SonarCloud GitHub App - Quick Guide

## What Does the SonarCloud GitHub App Do?

The SonarCloud GitHub App adds these features to your pull requests:
- ✅ **Quality Gate status checks** - Shows pass/fail directly in PR
- ✅ **Inline comments** - Comments on code with issues
- ✅ **PR decoration** - Shows metrics and issues summary
- ✅ **Automatic analysis** - Triggers on every PR

---

## Installation Steps

### Method 1: Install from SonarCloud (Recommended)

1. **Log into SonarCloud**: [https://sonarcloud.io](https://sonarcloud.io)

2. **Go to your project**:
   - Click on your project name
   - Or go to "My Projects"

3. **Open Administration**:
   - Click on "Administration" (bottom left)
   - Select "Analysis Method"

4. **GitHub Integration**:
   - You'll see a section about GitHub integration
   - Click **"Configure GitHub App"** or **"Install GitHub App"**

5. **Authorize**:
   - You'll be redirected to GitHub
   - Click **"Install"** or **"Configure"**
   - Choose your account/organization
   - Select **"Only select repositories"**
   - Choose your repository
   - Click **"Install"** or **"Save"**

✅ **Done!** The SonarCloud app is now installed.

---

### Method 2: Install Directly from GitHub Marketplace

1. **Go to**: [https://github.com/apps/sonarcloud](https://github.com/apps/sonarcloud)

2. **Click "Install"** (green button on the right)

3. **Choose where to install**:
   - Select your GitHub account or organization

4. **Choose repositories**:
   - Select **"Only select repositories"**
   - Find and select your repository (e.g., `proiect .NET`)
   - Click **"Install"**

5. **Authorize SonarCloud**:
   - You'll be redirected to SonarCloud
   - Click **"Authorize"** if prompted
   - Link it to your SonarCloud organization

✅ **Done!** The app is installed.

---

## Verify Installation

### Check in GitHub:

1. Go to your repository on GitHub
2. Click **"Settings"** → **"Integrations"** → **"GitHub Apps"**
3. You should see **"SonarCloud"** listed

### Check in SonarCloud:

1. Go to your project in SonarCloud
2. Click **"Administration"** → **"General Settings"**
3. Look for GitHub integration status - should show as connected

---

## What Happens After Installation?

### On Every Pull Request:

1. **Automatic Analysis**: SonarCloud analyzes your code automatically
2. **Status Check**: A check appears in the PR:
   ```
   ✅ SonarCloud Code Analysis — Quality Gate passed
   ```
   or
   ```
   ❌ SonarCloud Code Analysis — Quality Gate failed
   ```

3. **PR Decoration**: SonarCloud adds a comment showing:
   - Quality Gate status
   - Number of bugs, vulnerabilities, code smells
   - Code coverage
   - Link to full report

4. **Inline Comments**: Issues are commented directly on the code lines

### Example PR Check:

```
Checks
✅ Backend Tests
✅ Frontend Build
✅ SonarCloud Code Analysis
```

Click "Details" next to SonarCloud to see the full report.

---

## Configure PR Decoration

### Enable/Disable Features:

1. Go to **SonarCloud** → **Your Project**
2. Click **"Administration"** → **"General Settings"** → **"Pull Requests"**
3. Configure:
   - ✅ Decorate Pull Requests
   - ✅ Delete comments on resolved issues
   - ✅ Report Quality Gate status

---

## Permissions Required

The SonarCloud app needs these GitHub permissions:

| Permission | Why |
|-----------|-----|
| **Read access to code** | To analyze your code |
| **Read and write access to checks** | To add status checks to PRs |
| **Read and write access to pull requests** | To comment on PRs |
| **Read access to metadata** | To access repository information |

All permissions are safe and standard for code analysis tools.

---

## Troubleshooting

### "SonarCloud check not appearing in PR"

**Solutions:**
1. Make sure the GitHub App is installed
2. Verify the workflow is running (check Actions tab)
3. Check that `SONAR_TOKEN` is set in GitHub Secrets
4. Ensure the project exists in SonarCloud

### "Quality Gate check is pending forever"

**Solutions:**
1. Check GitHub Actions logs for errors
2. Verify the analysis completed in SonarCloud
3. Check that project keys match in workflow and SonarCloud

### "Cannot install app - permission denied"

**Solutions:**
1. You need admin access to the repository
2. Ask the repository owner to install it
3. Or ask for admin permissions

---

## Uninstall (if needed)

### From GitHub:
1. Go to repository **Settings** → **Integrations** → **GitHub Apps**
2. Find **SonarCloud**
3. Click **"Configure"**
4. Scroll down and click **"Uninstall"**

### From SonarCloud:
1. Go to **Administration** → **General Settings**
2. Find GitHub integration section
3. Click **"Disconnect"**

---

## Summary

**Quick Install:**
1. Go to [https://github.com/apps/sonarcloud](https://github.com/apps/sonarcloud)
2. Click **"Install"**
3. Select your repository
4. Click **"Install"**

**That's it!** 🎉

Now every PR will have:
- ✅ Quality gate status checks
- ✅ Code quality metrics
- ✅ Inline issue comments
- ✅ Direct link to SonarCloud report

---

## Next Steps

After installing the app:

1. ✅ Make sure `SONAR_TOKEN` is in GitHub Secrets
2. ✅ Update project keys in `.github/workflows/sonarqube.yml`
3. ✅ Create a test PR to verify everything works
4. ✅ Check that SonarCloud comments appear on the PR

**Need more help?** See [SONARCLOUD_SETUP.md](./SONARCLOUD_SETUP.md)

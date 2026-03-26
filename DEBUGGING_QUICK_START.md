# 🚀 Quick Start Guide - Debugging with Logs

## How to See the Debug Logs

### Step 1: Open Visual Studio
- Make sure your Pethub project is open
- Press `Ctrl+Shift+F5` to stop any running session

### Step 2: Start Debugging
- Press **F5** to start debugging
- The app will compile and launch

### Step 3: Open the Output Window
**Option A (Keyboard):**
```
Ctrl + Alt + O
```

**Option B (Menu):**
```
Debug → Windows → Output
```

### Step 4: Select Debug Output
In the Output window dropdown (top-left), select:
```
Debug
```

---

## What You Should See

### ✅ SUCCESS: Startup Logs

```
🚀 Application starting up...
✓ Database connection string found
Configuring DbContext with SQL Server...
✓ DbContext registered successfully
Configuring session services...
✓ Session configured successfully
Building application pipeline...
Environment: Development - Detailed error pages enabled
Setting up middleware pipeline...
✓ HTTPS redirection middleware added
✓ Routing middleware added
✓ Session middleware added
✓ Authorization middleware added
✓ Static assets mapped
✓ Razor Pages mapped
✓ Root redirect mapped to /Login
✅ Application pipeline configured. Starting to listen...
```

### ❌ ERROR: Session Not Configured

```
❌ Error configuring session
```

**What to do:**
- Check `Program.cs` has `builder.Services.AddSession()`
- Ensure `app.UseSession()` is present

---

## Testing the Login Flow

### 1. Navigate to App
1. Browser opens to `https://localhost:7034`
2. Should redirect to `/Login` (app shows this page)
3. **In Debug Output**, look for:
   ```
   LoginModel.OnGet() called
   ```

### 2. Try Admin Login
- Email: `Admin@pethub.com`
- Password: `admin123`

**Expected Output:**
```
LoginModel.OnPostAsync() started with Email=Admin@pethub.com
✓ Admin login successful
Admin session variables set successfully
```

### 3. Navigate to Protected Page
- Click "My Profile" or go to `/UserDashboard/Index`

**Expected Output:**
```
AuthenticatedPageModel: OnPageHandlerExecuting started for page /UserDashboard/Index
AuthenticatedPageModel: CurrentAccountId retrieved = 0
AuthenticatedPageModel: User authenticated with AccountId = 0
```

---

## Troubleshooting: App Crashes (Connection Refused)

### Check These Errors in Order:

1. **Database Connection**
   ```
   ❌ Connection string 'PethubContext' not found
   ```
   - Fix: Check `appsettings.json` has `PethubContext` connection string

2. **Session Configuration**
   ```
   ❌ Session middleware not configured or disabled
   ```
   - Fix: Ensure `Program.cs` has both:
     ```csharp
     builder.Services.AddSession()
     app.UseSession()
     ```

3. **Middleware Order**
   Look for this sequence in output:
   ```
   ✓ Session middleware added
   ✓ Authorization middleware added
   ```
   Session MUST come before Authorization

4. **Exception Details**
   ```
   ❌ FATAL: Application crashed
   ```
   - Look at the full exception message below this line
   - This tells you exactly what went wrong

---

## Troubleshooting: Login Fails

### Check These Messages:

1. **Session Null During Login**
   ```
   ❌ Session is null for user login - session middleware may not be configured
   ```
   - Session middleware is not running
   - Check: Does output show `✓ Session middleware added`?

2. **Database Connection Failed**
   ```
   ❌ Error configuring database context
   ```
   - Can't connect to SQL Server
   - Check: `appsettings.json` connection string
   - Check: SQL Server is running

3. **Wrong Credentials**
   ```
   Login failed: invalid email or password for Email=user@test.com
   ```
   - User doesn't exist or password is wrong
   - This is expected behavior

---

## Troubleshooting: Page Keeps Redirecting to Login

### Check These Messages:

1. **Session Lost**
   ```
   AuthenticatedPageModel: No valid session found, redirecting to Login
   ```
   - User session expired (30 minute idle timeout)
   - User needs to login again (expected)

2. **Session Null**
   ```
   AuthenticatedPageModel: HttpContext.Session is null
   ```
   - Session middleware not configured
   - Check: `app.UseSession()` in `Program.cs`

3. **Invalid Role**
   ```
   AdminPageModel: User does not have admin role (role=User), denying access
   ```
   - User is trying to access admin page
   - Only admin users (role=Admin) can access

---

## Copy-Paste Debugging Template

When debugging, use this template to gather information:

```
========== DEBUGGING INFO ==========
App Status: [Running / Crashed / Frozen]
Last URL Accessed: [e.g., https://localhost:7034/Posts/Create]
Expected Action: [e.g., Page should load]
Actual Result: [e.g., "Connection refused" or "Page loads blank"]

Key Log Messages Observed:
[Copy/paste relevant messages from Output window]

Errors Seen:
[Copy/paste any ❌ error messages]

========================================
```

Share this information when asking for help!

---

## Pro Tips

### Tip 1: Filter Log Output
In the Output window, type in the search box:
```
❌
```
This shows ONLY errors

### Tip 2: Pause on Break
Add a breakpoint to see log messages better:
1. Click in the leftmost column of a file
2. A red circle appears
3. Execution will pause there, letting you read logs

### Tip 3: Watch the Debug Output in Real-Time
Leave the Output window open while testing:
- Login → Watch messages appear
- Navigate → See authentication logs
- Click buttons → See action logs

---

## When to Check Logs

| Situation | What to Check |
|-----------|---------------|
| App won't start | Startup messages (🚀 to ✅) |
| Login fails | LoginModel.OnPostAsync() messages |
| Page redirects to login | AuthenticatedPageModel messages |
| Page loads blank | Landing/Page-specific messages |
| Database errors | DbContext configuration messages |

---

## Important Reminder

**The Output window is your friend!** 🎯

Every issue will have log messages explaining what's wrong. Before asking for help:

1. Open Output window
2. Reproduce the issue
3. Read ALL the messages
4. Look for ❌ errors
5. Check the messages ABOVE the error for context

This will solve 80% of issues immediately!

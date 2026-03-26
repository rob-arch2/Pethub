# 🔍 Debug Logging Guide

## What Was Added

I've added comprehensive null checking and debug logging throughout your codebase to help identify issues. Here's what to look for:

---

## 📋 Files Updated

### 1. **Program.cs** - Startup Diagnostics
**What it does:**
- Logs all startup steps with status indicators (🚀 ✓ ❌ ✅)
- Verifies database connection string exists
- Confirms Session middleware is configured correctly
- Shows middleware pipeline order (CRITICAL for authentication)
- Logs any startup errors with full stack traces

**Look for these messages in Output window:**
```
🚀 Application starting up...
✓ Database connection string found
✓ DbContext registered successfully
✓ Session configured successfully
✓ HTTPS redirection middleware added
✓ Session middleware added
✅ Application pipeline configured. Starting to listen...
```

**If you see ERRORS (❌):**
- Database connection issue
- Session not configured
- Middleware pipeline incorrect

---

### 2. **AuthenticatedPageModel.cs** - User Authentication
**What it does:**
- Logs every property access (CurrentAccountId, AccountUsername)
- Checks for null HttpContext, Session
- Catches exceptions with full details
- Verifies user session is valid

**Look for these messages:**
```
AuthenticatedPageModel: OnPageHandlerExecuting started for page /Posts/Create
AuthenticatedPageModel: CurrentAccountId retrieved = 5
AuthenticatedPageModel: User authenticated with AccountId = 5
AuthenticatedPageModel: No valid session found, redirecting to Login
❌ AuthenticatedPageModel: HttpContext is null
❌ AuthenticatedPageModel: Session middleware not configured
```

**If you see WARNINGS/ERRORS (⚠️ ❌):**
- Session is not being maintained
- HttpContext is becoming null unexpectedly
- Session middleware not running

---

### 3. **AdminPageModel.cs** - Admin Authorization
**What it does:**
- Logs admin role verification attempts
- Checks if session contains valid admin role
- Safe null checks for HttpContext and Session
- Denies non-admin access with logging

**Look for these messages:**
```
AdminPageModel: AccountRole from session = Admin
✓ AdminPageModel: User verified as admin, granting access
AdminPageModel: User does not have admin role (role=User), denying access
```

---

### 4. **Login.cshtml.cs** - Login Process
**What it does:**
- Logs login attempts with email
- Verifies session setup during login
- Logs successful authentication with user ID
- Catches and logs any auth errors

**Look for these messages:**
```
LoginModel.OnPostAsync() started with Email=user@example.com
✓ Admin login successful
Admin session variables set successfully
✓ User login successful - UserId=3, Username=JohnDoe
Session variables set for user 3
❌ Session is null for user login - session middleware may not be configured
```

**If you see ERRORS:**
- Session is null during login = middleware configuration issue
- Login failed = wrong credentials or database issue

---

### 5. **Landing.cshtml.cs** - Feed Loading
**What it does:**
- Logs page access with current user ID
- Logs report operations with details
- Catches exceptions in post loading

**Look for these messages:**
```
LandingModel.OnGetAsync() started - CurrentAccountId=5
✓ Loaded 12 posts
OnPostReportAsync() started - PostId=7, Reason=Inappropriate
✓ Post reported successfully - PostId=7, ReporterId=5
```

---

## 🎯 How to Use These Logs

### Step 1: Start the Application
Press **F5** to start debugging

### Step 2: Open the Output Window
**Visual Studio** → **View** → **Output** (or **Ctrl+Alt+O**)

Select dropdown: **Debug**

### Step 3: Watch for Startup Messages
Look for all the ✓ checkmarks during startup. You should see:
```
🚀 Application starting up...
✓ Database connection string found
✓ DbContext registered successfully
✓ Session configured successfully
...
✅ Application pipeline configured. Starting to listen...
```

### Step 4: Test Login Flow
1. Navigate to `https://localhost:7034`
2. Should redirect to `/Login` (message logged)
3. Enter credentials and submit
4. Watch for:
   - `LoginModel.OnPostAsync() started`
   - `✓ User login successful`
   - `Session variables set for user X`

### Step 5: Access Protected Pages
1. Click "Create Post" or navigate to `/Posts/Create`
2. Watch for:
   - `AuthenticatedPageModel: OnPageHandlerExecuting started`
   - `AuthenticatedPageModel: User authenticated with AccountId = X`
   - Page loads successfully

---

## 🚨 Debugging Failed Login

**Scenario:** Login fails, app stops responding

**Check these log messages:**
```
❌ Session is null for user login - session middleware may not be configured
```

**This means:** Session middleware is not properly set up in Program.cs

**Fix:** Verify these are in Program.cs:
```csharp
app.UseSession(); // MUST come before UseAuthorization
```

---

## 🚨 Debugging "Connection Refused"

**Scenario:** Browser shows "localhost refused to connect"

**Check Output window for:**
```
❌ FATAL: Connection string 'PethubContext' not found in configuration
❌ Error configuring database context
❌ Session middleware not configured
❌ Exception in AuthenticatedPageModel: Exception occurred
```

**Check each error message and fix accordingly**

---

## 📊 Debug Level Control

In **Program.cs**, you can control log verbosity:

```csharp
config.SetMinimumLevel(LogLevel.Debug); // Shows all messages
config.SetMinimumLevel(LogLevel.Information); // Shows info and above
config.SetMinimumLevel(LogLevel.Warning); // Only warnings and errors
```

---

## 💡 Common Debug Patterns

### Pattern 1: User Not Authenticated
**Log Messages:**
```
AuthenticatedPageModel: No valid session found, redirecting to Login
AuthenticatedPageModel: HttpContext.Session is null
```
**Cause:** Session middleware not running, or user session expired

### Pattern 2: Page Crashes Silently  
**Log Messages:**
```
❌ Exception in OnPageHandlerExecuting
```
**Check:** Click on the error message to see full exception details

### Pattern 3: Database Not Found
**Log Messages:**
```
❌ Error configuring database context
```
**Cause:** Connection string missing or database server not running

---

## 🔧 Next Steps if Still Debugging

1. **Copy all Debug output** from Visual Studio Output window
2. **Share the messages** with your team or in issue reports
3. **Look for ERRORS (❌)** first - those are the most critical
4. **Follow the log flow** to understand what's happening

---

## Summary

✅ **What this gives you:**
- Real-time visibility into what the app is doing
- Exact point where errors occur
- Session state tracking
- Authentication flow tracing
- Database operation logging

✅ **Next time something breaks:**
1. Open Output window
2. Reproduce the issue
3. Read the log messages - they'll tell you exactly what's wrong
4. No more guessing!

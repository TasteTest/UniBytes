# Google OAuth Setup Guide

Complete guide to set up Google OAuth authentication for the application.

## Overview

The application uses:
- **Frontend**: NextAuth.js (v4) for OAuth flow handling
- **Backend**: ASP.NET Core for user management and OAuth provider linking
- **Flow**: Google OAuth → NextAuth → Backend API → User Creation/Linking

## Architecture

```
┌─────────┐      ┌──────────┐      ┌─────────────┐      ┌──────────────┐
│ User    │─────>│ NextAuth │─────>│ Google OAuth│─────>│ Google       │
│ Browser │      │ (Frontend)│      │ Callback    │      │ Auth Server  │
└─────────┘      └──────────┘      └─────────────┘      └──────────────┘
     │                 │                    │
     │                 ▼                    │
     │           ┌──────────┐              │
     │           │ Backend  │◀─────────────┘
     │           │ API      │
     │           │ /auth/   │
     │           │ google   │
     │           └──────────┘
     │                 │
     │                 ▼
     │           ┌──────────┐
     └──────────>│ User     │
                 │ Created/ │
                 │ Linked   │
                 └──────────┘
```

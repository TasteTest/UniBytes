/**
 * API Endpoint Definitions
 * Centralized endpoint management
 */

export const endpoints = {
  // Auth endpoints
  auth: {
    google: '/auth/google',
    health: '/auth/health',
  },
  
  // User endpoints
  users: {
    base: '/users',
    byId: (id: string) => `/users/${id}`,
    byEmail: (email: string) => `/users/by-email/${encodeURIComponent(email)}`,
    active: '/users/active',
    admins: '/users/admins',
    lastLogin: (id: string) => `/users/${id}/last-login`,
  },
  
  // OAuth Provider endpoints
  oauthProviders: {
    base: '/oauthproviders',
    byId: (id: string) => `/oauthproviders/${id}`,
    byUser: (userId: string) => `/oauthproviders/user/${userId}`,
    byProvider: (provider: number, providerId: string) => 
      `/oauthproviders/provider/${provider}/${providerId}`,
  },
  
  // Analytics endpoints
  analytics: {
    base: '/useranalytics',
    byId: (id: string) => `/useranalytics/${id}`,
    byUser: (userId: string) => `/useranalytics/user/${userId}`,
    bySession: (sessionId: string) => `/useranalytics/session/${sessionId}`,
    byEvent: (eventType: string) => `/useranalytics/event/${eventType}`,
    dateRange: '/useranalytics/date-range',
  },
  
  // Loyalty endpoints
  loyalty: {
    base: '/loyaltyaccounts',
    byId: (id: string) => `/loyaltyaccounts/${id}`,
    byUserId: (userId: string) => `/loyaltyaccounts/user/${userId}`,
    details: (userId: string) => `/loyaltyaccounts/user/${userId}/details`,
    balance: (userId: string) => `/loyaltyaccounts/user/${userId}/balance`,
    addPoints: '/loyaltyaccounts/add-points',
    redeemPoints: '/loyaltyaccounts/redeem-points',
    active: '/loyaltyaccounts/active',
    byTier: (tier: number) => `/loyaltyaccounts/tier/${tier}`,
  },
} as const


import NextAuth from "next-auth"
import { JWT } from "next-auth/jwt"

declare module "next-auth" {
  interface Session {
    user: {
      id: string
      name?: string | null
      email?: string | null
      image?: string | null
      backendId?: string
      role?: number // 0=User, 1=Chef, 2=Admin
    }
    accessToken?: string
  }

  interface User {
    backendId?: string
    role?: number
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    backendId?: string
    role?: number
    accessToken?: string
    refreshToken?: string
  }
}



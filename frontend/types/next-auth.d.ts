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
    }
    accessToken?: string
  }

  interface User {
    backendId?: string
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    backendId?: string
    accessToken?: string
    refreshToken?: string
  }
}


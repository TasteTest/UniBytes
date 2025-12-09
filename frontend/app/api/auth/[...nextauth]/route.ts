import NextAuth from "next-auth"
import type { NextAuthOptions } from "next-auth"
import GoogleProvider from "next-auth/providers/google"

const API_URL = process.env.NEXT_PUBLIC_API_URL || ""

const authOptions: NextAuthOptions = {
  providers: [
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID || "",
      clientSecret: process.env.GOOGLE_CLIENT_SECRET || "",
      authorization: {
        params: {
          prompt: "consent",
          access_type: "offline",
          response_type: "code"
        }
      }
    }),
  ],
  pages: {
    signIn: '/auth/signin',
  },
  callbacks: {
    async signIn({ user, account, profile }) {
      if (account?.provider === "google") {
        try {
          // Extract name parts
          const nameParts = user.name?.split(' ') || []
          const firstName = (profile as any)?.given_name || nameParts[0] || ''
          const lastName = (profile as any)?.family_name || nameParts.slice(1).join(' ') || ''
          
          // Send OAuth data to backend for user creation/linking
          const response = await fetch(`${API_URL}/auth/google`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              email: user.email,
              firstName,
              lastName,
              avatarUrl: user.image,
              provider: 0, // Google = 0
              providerId: account.providerAccountId,
              providerEmail: user.email,
              accessToken: account.access_token,
              refreshToken: account.refresh_token,
              tokenExpiresAt: account.expires_at ? new Date(account.expires_at * 1000).toISOString() : null,
            }),
          })

          if (response.ok) {
            const data = await response.json()
            // Store backend user data in token for later use
            user.backendId = data.userId
            return true
          } else {
            console.error('Backend authentication failed:', await response.text())
            return false
          }
        } catch (error) {
          console.error('Error during sign in:', error)
          return false
        }
      }
      return true
    },
    async jwt({ token, user, account }) {
      // Initial sign in
      if (user) {
        token.backendId = user.backendId
      }
      // Store OAuth tokens
      if (account) {
        token.accessToken = account.access_token
        token.refreshToken = account.refresh_token
      }
      return token
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.id = token.sub as string
        session.user.backendId = token.backendId as string
        session.accessToken = token.accessToken as string
      }
      return session
    },
  },
  secret: process.env.NEXTAUTH_SECRET,
}

const handler = NextAuth(authOptions)

export { handler as GET, handler as POST }


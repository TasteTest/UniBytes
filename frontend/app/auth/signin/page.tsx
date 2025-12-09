"use client"

import { useEffect } from "react"
import { signIn, useSession } from "next-auth/react"
import { useRouter } from "next/navigation"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { UtensilsCrossed, Loader2 } from "lucide-react"

export default function SignIn() {
    const { data: session, status } = useSession()
    const router = useRouter()

    useEffect(() => {
        const loginToBackend = async () => {
            if (status === "authenticated" && session?.user?.email) {
                try {
                    const apiBase = process.env.NEXT_PUBLIC_API_URL
                    if (!apiBase) {
                        throw new Error("API base URL not configured")
                    }
                    const res = await fetch(`${apiBase}/users/by-email/${session.user.email}`);
                    if(res.ok) {
                        const userData = await res.json();
                        localStorage.setItem("token", JSON.stringify(userData));
                    }

                    router.push("/menu");

                } catch (error) {
                    console.error("Erorr communicating with backend:", error);
                }
            }
        };

        loginToBackend();
    }, [status, session, router]);

    return (
        <div className="container flex items-center justify-center min-h-[calc(100vh-4rem)] py-12">
            <Card className="w-full max-w-md card-glass border-none">
                <CardHeader className="space-y-4 text-center">
                    <div className="mx-auto w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                        <UtensilsCrossed className="h-6 w-6 text-primary" />
                    </div>
                    <div>
                        <CardTitle className="text-2xl">Welcome to CampusEats</CardTitle>
                        <CardDescription className="mt-2">
                            Sign in with your Google account to start ordering
                        </CardDescription>
                    </div>
                </CardHeader>
                <CardContent>
                    {status === "loading" ? (
                        <Button disabled className="w-full" size="lg">
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Connecting...
                        </Button>
                    ) : (
                        <Button
                            onClick={() => signIn("google", { redirect: false })}
                            className="w-full"
                            size="lg"
                        >
                            <svg className="mr-2 h-5 w-5" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
                                <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
                                <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/>
                                <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
                            </svg>
                            Sign in with Google
                        </Button>
                    )}
                    <p className="text-xs text-center text-muted-foreground mt-6">
                        By signing in, you agree to our Terms of Service and Privacy Policy
                    </p>
                </CardContent>
            </Card>
        </div>
    )
}
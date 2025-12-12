"use client"

import { Suspense, useEffect, useState } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { CheckCircle2, Loader2, XCircle } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { useCartStore } from "@/lib/store"
import { formatCurrency } from "@/lib/utils"
import { paymentService } from "@/lib/services/PaymentService"
import type { Payment } from "@/lib/types/payment.types"

export default function CheckoutSuccessPage() {
  return (
    <Suspense
      fallback={
        <div className="container py-16 max-w-2xl">
          <Card className="card-glass border-none text-center">
            <CardContent className="py-16">
              <Loader2 className="h-16 w-16 animate-spin mx-auto mb-4 text-primary" />
              <h2 className="text-2xl font-bold mb-2">Loading checkout details...</h2>
              <p className="text-muted-foreground">Retrieving your payment result.</p>
            </CardContent>
          </Card>
        </div>
      }
    >
      <CheckoutSuccessContent />
    </Suspense>
  )
}

function CheckoutSuccessContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const sessionId = searchParams.get("session_id")
  const { clearCart } = useCartStore()

  const [isVerifying, setIsVerifying] = useState(true)
  const [payment, setPayment] = useState<Payment | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const verifyPayment = async () => {
      if (!sessionId) {
        setError("No session ID provided")
        setIsVerifying(false)
        return
      }

      try {
        const result = await paymentService.verifyPayment(sessionId)

        if (result.isSuccess && result.data) {
          setPayment(result.data)
          // Clear cart on successful payment
          clearCart()
        } else {
          setError(result.error || "Failed to verify payment")
        }
      } catch (err) {
        setError("An unexpected error occurred")
      } finally {
        setIsVerifying(false)
      }
    }

    verifyPayment()
  }, [sessionId, clearCart])

  if (isVerifying) {
    return (
      <div className="container py-16 max-w-2xl">
        <Card className="card-glass border-none text-center">
          <CardContent className="py-16">
            <Loader2 className="h-16 w-16 animate-spin mx-auto mb-4 text-primary" />
            <h2 className="text-2xl font-bold mb-2">Verifying payment...</h2>
            <p className="text-muted-foreground">Please wait while we confirm your payment.</p>
          </CardContent>
        </Card>
      </div>
    )
  }

  if (error || !payment) {
    return (
      <div className="container py-16 max-w-2xl">
        <Card className="card-glass border-none text-center">
          <CardContent className="py-16">
            <XCircle className="h-16 w-16 mx-auto mb-4 text-destructive" />
            <h2 className="text-2xl font-bold mb-2">Payment Verification Failed</h2>
            <p className="text-muted-foreground mb-6">
              {error || "We couldn't verify your payment. Please contact support."}
            </p>
            <div className="flex gap-4 justify-center">
              <Button variant="outline" onClick={() => router.push("/")}>
                Go Home
              </Button>
              <Button onClick={() => router.push("/orders")}>
                View Orders
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container py-16 max-w-2xl">
      <Card className="card-glass border-none text-center">
        <CardHeader>
          <div className="mx-auto mb-4 rounded-full bg-green-100 dark:bg-green-900 p-3 w-fit">
            <CheckCircle2 className="h-16 w-16 text-green-600 dark:text-green-400" />
          </div>
          <CardTitle className="text-3xl">Payment Successful!</CardTitle>
          <CardDescription className="text-lg">
            Your order has been placed and payment confirmed
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-lg bg-muted p-4 space-y-2 text-left">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Amount:</span>
              <span className="font-semibold">
                {formatCurrency(payment.amount, payment.currency)}
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Status:</span>
              <span className="font-semibold text-green-600 dark:text-green-400">
                Succeeded
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Payment Provider:</span>
              <span className="font-semibold">Stripe</span>
            </div>
          </div>

          <p className="text-sm text-muted-foreground">
            You will receive an email confirmation shortly. Your order is being prepared and will be
            ready for pickup soon.
          </p>
        </CardContent>
        <CardFooter className="flex gap-4 justify-center">
          <Button variant="outline" onClick={() => router.push("/")}>
            Continue Shopping
          </Button>
          <Button onClick={() => router.push("/orders")}>
            View My Orders
          </Button>
        </CardFooter>
      </Card>
    </div>
  )
}


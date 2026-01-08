"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useSession } from "next-auth/react"
import { MapPin, Loader2, Gift, Percent } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Separator } from "@/components/ui/separator"
import { Badge } from "@/components/ui/badge"
import { useCartStore } from "@/lib/store"
import { formatCurrency } from "@/lib/utils"
import { useToast } from "@/hooks/use-toast"
import { paymentService } from "@/lib/services/PaymentService"
import type { CheckoutLineItem } from "@/lib/types/payment.types"

export default function CheckoutPage() {
  const router = useRouter()
  const { data: session } = useSession()
  const { items, getTotal, getSubtotal, getDiscount, clearCart, getFreeItems, getDiscountItems } = useCartStore()
  const { toast } = useToast()
  const [pickupTime, setPickupTime] = useState("asap")
  const [pickupLocation, setPickupLocation] = useState("")
  const [isProcessing, setIsProcessing] = useState(false)

  const freeItems = getFreeItems()
  const discountItems = getDiscountItems()
  const subtotal = getSubtotal()
  const discount = getDiscount()
  const afterDiscount = getTotal()
  const tax = afterDiscount * 0.08
  const total = afterDiscount + tax

  // Check if there are any items (regular or rewards)
  const hasItems = items.length > 0 || freeItems.length > 0

  const handleContinueToPayment = async () => {
    if (!session?.user) {
      toast({
        title: "Authentication required",
        description: "Please sign in to continue with payment.",
        variant: "destructive",
      })
      router.push("/auth/signin")
      return
    }

    if (!hasItems) {
      toast({
        title: "Cart is empty",
        description: "Please add items to your cart before checkout.",
        variant: "destructive",
      })
      return
    }

    if (!pickupLocation) {
      toast({
        title: "Pickup location required",
        description: "Please select a pickup location.",
        variant: "destructive",
      })
      return
    }

    setIsProcessing(true)

    try {
      // Prepare line items for Stripe (regular items)
      const lineItems: CheckoutLineItem[] = items.map((item) => ({
        menuItemId: item.menuItem.id,
        name: item.menuItem.name,
        description: item.menuItem.description || undefined,
        unitPrice: item.menuItem.price,
        quantity: item.quantity,
        currency: item.menuItem.currency || 'RON',
        imageUrl: item.menuItem.image || undefined,
        modifiers: item.modifiers?.length ? item.modifiers : [],
      }))

      // Add free reward items (MenuItem type) as items with price 0
      const freeRewardLineItems: CheckoutLineItem[] = freeItems.map((reward) => ({
        name: reward.rewardName,
        description: `${reward.rewardDescription} (Loyalty Reward - ${reward.pointsUsed} points)`,
        unitPrice: 0,
        quantity: 1,
        currency: 'RON',
        modifiers: [],
        isReward: true,
        rewardId: reward.rewardId,
        metadata: {
          rewardType: reward.rewardType,
          pointsUsed: reward.pointsUsed,
          ...reward.metadata,
        },
      }))

      const allLineItems = [...lineItems, ...freeRewardLineItems]

      // Generate a temporary order ID (in production, create order first)
      const orderId = crypto.randomUUID()

      // Get access token from session
      const accessToken = (session as any).accessToken || ''
      const userEmail = session.user.email || ''

      if (!accessToken || !userEmail) {
        toast({
          title: "Session error",
          description: "Please sign in again to continue.",
          variant: "destructive",
        })
        router.push("/auth/signin")
        return
      }

      // Create Stripe checkout session
      const result = await paymentService.createCheckoutSession({
        orderId,
        accessToken,
        userEmail,
        lineItems: allLineItems,
        successUrl: `${window.location.origin}/checkout/success?session_id={CHECKOUT_SESSION_ID}`,
        cancelUrl: `${window.location.origin}/checkout`,
        idempotencyKey: `checkout_${Date.now()}_${userEmail}`,
        metadata: {
          pickupLocation: pickupLocation,
          pickupTime: pickupTime === "asap" ? "ASAP (15-20 min)" : "Scheduled",
          hasFreeItems: freeItems.length > 0 ? 'true' : 'false',
          freeItemCount: freeItems.length.toString(),
          hasDiscount: discount > 0 ? 'true' : 'false',
          discountAmount: discount.toString(),
          discountItems: JSON.stringify(discountItems.map(d => ({
            rewardId: d.rewardId,
            rewardName: d.rewardName,
            discountType: d.metadata?.discountType,
            discountAmount: d.metadata?.discountAmount,
            discountPercent: d.metadata?.discountPercent,
          }))),
        }
      })

      if (result.isSuccess && result.data) {
        // Redirect to Stripe Checkout
        window.location.href = result.data.sessionUrl
      } else {
        toast({
          title: "Payment error",
          description: result.error || "Failed to create checkout session. Please try again.",
          variant: "destructive",
        })
        setIsProcessing(false)
      }
    } catch (error) {
      console.error("Error creating checkout session:", error)
      toast({
        title: "Payment error",
        description: "An unexpected error occurred. Please try again.",
        variant: "destructive",
      })
      setIsProcessing(false)
    }
  }

  return (
    <div className="container py-8 max-w-4xl">
      <h1 className="text-4xl font-bold mb-8">Checkout</h1>

      <div className="grid lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          {/* Pickup Details */}
          <Card className="card-glass border-none">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                Pickup Details
              </CardTitle>
              <CardDescription>Choose when and where to pick up your order</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="space-y-2">
                <Label>Pickup Location</Label>
                <Select value={pickupLocation} onValueChange={setPickupLocation}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a location" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="main-cafeteria">Main Cafeteria</SelectItem>
                    <SelectItem value="student-union">Student Union</SelectItem>
                    <SelectItem value="library-cafe">Library Café</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Pickup Time</Label>
                <RadioGroup value={pickupTime} onValueChange={setPickupTime}>
                  <div className="flex items-center space-x-2">
                    <RadioGroupItem value="asap" id="asap" />
                    <Label htmlFor="asap" className="font-normal cursor-pointer">
                      ASAP (15-20 min)
                    </Label>
                  </div>
                  <div className="flex items-center space-x-2">
                    <RadioGroupItem value="scheduled" id="scheduled" />
                    <Label htmlFor="scheduled" className="font-normal cursor-pointer">
                      Schedule for later
                    </Label>
                  </div>
                </RadioGroup>
              </div>

              {pickupTime === "scheduled" && (
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Date</Label>
                    <Input type="date" />
                  </div>
                  <div className="space-y-2">
                    <Label>Time</Label>
                    <Input type="time" />
                  </div>
                </div>
              )}
            </CardContent>
            <CardFooter>
              <Button
                onClick={handleContinueToPayment}
                disabled={!pickupLocation || isProcessing}
                className="w-full"
              >
                {isProcessing ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Processing...
                  </>
                ) : (
                  "Continue to Payment"
                )}
              </Button>
            </CardFooter>
          </Card>
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <Card className="card-glass border-none sticky top-20">
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex justify-between text-sm">
                <span>Subtotal</span>
                <span>{formatCurrency(subtotal)}</span>
              </div>
              {freeItems.length > 0 && (
                <div className="flex justify-between text-sm text-primary">
                  <span className="flex items-center gap-1">
                    <Gift className="h-4 w-4" />
                    Free Items ({freeItems.length})
                  </span>
                  <span>FREE</span>
                </div>
              )}
              {discount > 0 && (
                <div className="flex justify-between text-sm text-green-600">
                  <span className="flex items-center gap-1">
                    <Percent className="h-4 w-4" />
                    Loyalty Discount
                  </span>
                  <span>-{formatCurrency(discount)}</span>
                </div>
              )}
              <div className="flex justify-between text-sm">
                <span>Tax</span>
                <span>{formatCurrency(tax)}</span>
              </div>
              <Separator />
              <div className="flex justify-between font-bold">
                <span>Total</span>
                <span className="text-primary">{formatCurrency(total)}</span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}


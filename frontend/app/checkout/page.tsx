"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useSession } from "next-auth/react"
import { CreditCard, Clock, MapPin, Loader2, Gift, Percent } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Separator } from "@/components/ui/separator"
import { Progress } from "@/components/ui/progress"
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
  const [step, setStep] = useState(1)
  const [pickupTime, setPickupTime] = useState("asap")
  const [pickupLocation, setPickupLocation] = useState("")
  const [paymentMethod, setPaymentMethod] = useState("card")
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

  const progress = (step / 3) * 100

  return (
    <div className="container py-8 max-w-4xl">
      <h1 className="text-4xl font-bold mb-8">Checkout</h1>

      {/* Progress */}
      <div className="mb-8">
        <Progress value={progress} className="mb-2" />
        <div className="flex justify-between text-sm text-muted-foreground">
          <span className={step >= 1 ? "text-primary font-medium" : ""}>Pickup Details</span>
          <span className={step >= 2 ? "text-primary font-medium" : ""}>Payment</span>
          <span className={step >= 3 ? "text-primary font-medium" : ""}>Review</span>
        </div>
      </div>

      <div className="grid lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          {/* Step 1: Pickup Details */}
          {step === 1 && (
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
                  onClick={() => setStep(2)}
                  disabled={!pickupLocation}
                  className="w-full"
                >
                  Continue to Payment
                </Button>
              </CardFooter>
            </Card>
          )}

          {/* Step 2: Payment */}
          {step === 2 && (
            <Card className="card-glass border-none">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <CreditCard className="h-5 w-5" />
                  Payment Method
                </CardTitle>
                <CardDescription>Select your payment method</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <RadioGroup value={paymentMethod} onValueChange={setPaymentMethod}>
                  <div className="flex items-center space-x-2 p-4 border rounded-lg">
                    <RadioGroupItem value="card" id="card" />
                    <Label htmlFor="card" className="flex-1 cursor-pointer">
                      <div className="font-medium">Credit/Debit Card</div>
                      <div className="text-sm text-muted-foreground">Pay with card</div>
                    </Label>
                  </div>
                  <div className="flex items-center space-x-2 p-4 border rounded-lg">
                    <RadioGroupItem value="campus-card" id="campus-card" />
                    <Label htmlFor="campus-card" className="flex-1 cursor-pointer">
                      <div className="font-medium">Campus Card</div>
                      <div className="text-sm text-muted-foreground">Use campus meal plan</div>
                    </Label>
                  </div>
                </RadioGroup>

                {paymentMethod === "card" && (
                  <div className="space-y-4 pt-4">
                    <div className="space-y-2">
                      <Label>Card Number</Label>
                      <Input placeholder="1234 5678 9012 3456" />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>Expiry Date</Label>
                        <Input placeholder="MM/YY" />
                      </div>
                      <div className="space-y-2">
                        <Label>CVV</Label>
                        <Input placeholder="123" />
                      </div>
                    </div>
                  </div>
                )}
              </CardContent>
              <CardFooter className="flex gap-4">
                <Button variant="outline" onClick={() => setStep(1)} disabled={isProcessing}>
                  Back
                </Button>
                <Button
                  onClick={handleContinueToPayment}
                  className="flex-1"
                  disabled={isProcessing}
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
          )}

          {/* Step 3: Review */}
          {step === 3 && (
            <Card className="card-glass border-none">
              <CardHeader>
                <CardTitle>Review Your Order</CardTitle>
                <CardDescription>Please review before placing your order</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div>
                  <h3 className="font-semibold mb-2">Order Items</h3>
                  <div className="space-y-2">
                    {items.map((item) => (
                      <div key={item.id} className="flex justify-between text-sm">
                        <span>
                          {item.quantity}x {item.menuItem.name}
                        </span>
                        <span>
                          {formatCurrency(item.menuItem.price * item.quantity)}
                        </span>
                      </div>
                    ))}
                    {freeItems.map((reward) => (
                      <div key={reward.rewardId} className="flex justify-between text-sm text-primary">
                        <span className="flex items-center gap-1">
                          <Gift className="h-3 w-3" />
                          {reward.rewardName}
                          <Badge variant="outline" className="ml-1 text-xs">Free Item</Badge>
                        </span>
                        <span>FREE</span>
                      </div>
                    ))}
                    {discountItems.map((reward) => (
                      <div key={reward.rewardId} className="flex justify-between text-sm text-green-600">
                        <span className="flex items-center gap-1">
                          <Percent className="h-3 w-3" />
                          {reward.rewardName}
                          <Badge variant="outline" className="ml-1 text-xs border-green-500">Discount</Badge>
                        </span>
                        <span>
                          {reward.metadata?.discountType === 'percentage' 
                            ? `-${reward.metadata.discountPercent}%`
                            : `-${formatCurrency(reward.metadata?.discountAmount || 0)}`
                          }
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
                <Separator />
                <div>
                  <h3 className="font-semibold mb-2">Pickup Details</h3>
                  <p className="text-sm text-muted-foreground">
                    Location: {pickupLocation}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Time: {pickupTime === "asap" ? "ASAP (15-20 min)" : "Scheduled"}
                  </p>
                </div>
                <Separator />
                <div>
                  <h3 className="font-semibold mb-2">Payment Method</h3>
                  <p className="text-sm text-muted-foreground">
                    {paymentMethod === "card" ? "Credit/Debit Card" : "Campus Card"}
                  </p>
                </div>
              </CardContent>
              <CardFooter className="flex gap-4">
                <Button variant="outline" onClick={() => setStep(1)}>
                  Back
                </Button>
                <Button onClick={handleContinueToPayment} className="flex-1" disabled={isProcessing}>
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
          )}
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


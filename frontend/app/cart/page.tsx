"use client"

import { useState, useEffect } from "react"
import Link from "next/link"
import { Minus, Plus, Trash2, ShoppingBag, ShieldAlert, Gift, Percent, Loader2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import { Badge } from "@/components/ui/badge"
import { useCartStore } from "@/lib/store"
import { formatCurrency } from "@/lib/utils"
import { useToast } from "@/hooks/use-toast"
import { useRole } from "@/lib/hooks/useAdmin"

export default function CartPage() {
  // Hydration-safe: only render cart content after component mounts
  const [mounted, setMounted] = useState(false)
  useEffect(() => {
    setMounted(true)
  }, [])

  const {
    items,
    updateQuantity,
    removeItem,
    getTotal,
    getSubtotal,
    getDiscount,
    clearCart,
    getFreeItems,
    getDiscountItems,
    removeRewardItem
  } = useCartStore()
  const { toast } = useToast()
  const { canOrder, isChef, isAdmin } = useRole()

  // Show loading state until hydration is complete
  if (!mounted) {
    return (
      <div className="container py-16 flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  const freeItems = getFreeItems()
  const discountItems = getDiscountItems()

  // Helper to calculate category discount amount for display
  const getCategoryDiscountAmount = (reward: typeof discountItems[0]) => {
    if (reward.metadata?.discountType !== 'freeItem') return 0
    const targetCategory = reward.metadata?.targetCategory?.toLowerCase()
    if (!targetCategory) return 0

    const matchingItems = items.filter(
      item => item.menuItem.category?.toLowerCase() === targetCategory
    )
    if (matchingItems.length === 0) return 0

    // Find cheapest item
    const cheapestItem = matchingItems.reduce((min, item) => {
      const itemPrice = item.menuItem.price +
        item.modifiers.reduce((sum, mod) => sum + mod.price, 0)
      const minPrice = min.menuItem.price +
        min.modifiers.reduce((sum, mod) => sum + mod.price, 0)
      return itemPrice < minPrice ? item : min
    })
    return cheapestItem.menuItem.price +
      cheapestItem.modifiers.reduce((sum, mod) => sum + mod.price, 0)
  }

  const handleRemoveItem = (id: string) => {
    removeItem(id)
    toast({
      title: "Item removed",
      description: "Item has been removed from your cart",
    })
  }

  const handleRemoveReward = (rewardId: string) => {
    removeRewardItem(rewardId)
    toast({
      title: "Reward removed",
      description: "Reward has been removed from your cart",
    })
  }

  const subtotal = getSubtotal()
  const discount = getDiscount()
  const afterDiscount = getTotal()
  const tax = afterDiscount * 0.08 // 8% tax
  const total = afterDiscount + tax

  // Check if cart has any items (regular or rewards)
  const hasItems = items.length > 0 || freeItems.length > 0 || discountItems.length > 0

  // Block Chef/Admin from ordering
  if (!canOrder && (isChef || isAdmin)) {
    return (
      <div className="container py-16">
        <Card className="max-w-md mx-auto card-glass border-none text-center">
          <CardContent className="py-12">
            <ShieldAlert className="h-16 w-16 mx-auto text-orange-500 mb-4" />
            <h2 className="text-2xl font-bold mb-2">Ordering Not Available</h2>
            <p className="text-muted-foreground mb-6">
              {isChef ? "Chef" : "Admin"} accounts cannot place orders. Please use a regular user account to order.
            </p>
            <Button asChild variant="outline">
              <Link href="/">Return Home</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  if (!hasItems) {
    return (
      <div className="container py-16">
        <Card className="max-w-md mx-auto card-glass border-none text-center">
          <CardContent className="py-12">
            <ShoppingBag className="h-16 w-16 mx-auto text-muted-foreground mb-4" />
            <h2 className="text-2xl font-bold mb-2">Your cart is empty</h2>
            <p className="text-muted-foreground mb-6">
              Add some delicious items from our menu!
            </p>
            <Button asChild>
              <Link href="/menu">Browse Menu</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container py-8">
      <h1 className="text-4xl font-bold mb-8">Your Cart</h1>

      <div className="grid lg:grid-cols-3 gap-8">
        {/* Cart Items */}
        <div className="lg:col-span-2 space-y-4">
          {/* Regular Menu Items */}
          {items.map((item) => (
            <Card key={item.id} className="card-glass border-none">
              <CardContent className="p-6">
                <div className="flex gap-4">
                  <div className="flex-1">
                    <h3 className="font-semibold text-lg mb-1">
                      {item.menuItem.name}
                    </h3>
                    <p className="text-sm text-muted-foreground mb-2">
                      {item.menuItem.description}
                    </p>
                    {item.modifiers.length > 0 && (
                      <div className="text-sm text-muted-foreground">
                        Modifiers: {item.modifiers.map((m) => m.name).join(", ")}
                      </div>
                    )}
                    {item.specialInstructions && (
                      <div className="text-sm text-muted-foreground mt-1">
                        Note: {item.specialInstructions}
                      </div>
                    )}
                  </div>
                  <div className="flex flex-col items-end justify-between">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleRemoveItem(item.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="icon"
                        className="h-8 w-8"
                        onClick={() =>
                          updateQuantity(item.id, Math.max(1, item.quantity - 1))
                        }
                      >
                        <Minus className="h-3 w-3" />
                      </Button>
                      <span className="w-8 text-center font-medium">
                        {item.quantity}
                      </span>
                      <Button
                        variant="outline"
                        size="icon"
                        className="h-8 w-8"
                        onClick={() => updateQuantity(item.id, item.quantity + 1)}
                      >
                        <Plus className="h-3 w-3" />
                      </Button>
                    </div>
                    <span className="font-semibold text-lg">
                      {formatCurrency(
                        (item.menuItem.price +
                          item.modifiers.reduce((sum, m) => sum + m.price, 0)) *
                        item.quantity
                      )}
                    </span>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}

          {/* Free Reward Items (like Free Cookie, Free Drink) */}
          {freeItems.map((reward) => (
            <Card key={reward.rewardId} className="card-glass border-none ring-2 ring-primary/50 bg-primary/5">
              <CardContent className="p-6">
                <div className="flex gap-4">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <Gift className="h-5 w-5 text-primary" />
                      <h3 className="font-semibold text-lg">
                        {reward.rewardName}
                      </h3>
                      <Badge variant="success" className="ml-2">
                        Free Item
                      </Badge>
                    </div>
                    <p className="text-sm text-muted-foreground mb-2">
                      {reward.rewardDescription}
                    </p>
                    <p className="text-xs text-primary">
                      Redeemed for {reward.pointsUsed} points
                    </p>
                  </div>
                  <div className="flex flex-col items-end justify-center">
                    <span className="font-semibold text-lg text-primary">
                      FREE
                    </span>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}

          {/* Discount Reward Items (like 5 RON Off, 10% Off) */}
          {discountItems.map((reward) => (
            <Card key={reward.rewardId} className="card-glass border-none ring-2 ring-green-500/50 bg-green-500/5">
              <CardContent className="p-6">
                <div className="flex gap-4">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <Percent className="h-5 w-5 text-green-600" />
                      <h3 className="font-semibold text-lg">
                        {reward.rewardName}
                      </h3>
                      <Badge variant="outline" className="ml-2 border-green-500 text-green-600">
                        Discount
                      </Badge>
                    </div>
                    <p className="text-sm text-muted-foreground mb-2">
                      {reward.rewardDescription}
                    </p>
                    <p className="text-xs text-green-600">
                      Redeemed for {reward.pointsUsed} points
                    </p>
                  </div>
                  <div className="flex flex-col items-end justify-center">
                    <span className="font-semibold text-lg text-green-600">
                      {reward.metadata?.discountType === 'percentage'
                        ? `-${reward.metadata.discountPercent}%`
                        : reward.metadata?.discountType === 'freeItem'
                          ? getCategoryDiscountAmount(reward) > 0
                            ? `-${formatCurrency(getCategoryDiscountAmount(reward))}`
                            : 'No qualifying item'
                          : `-${formatCurrency(reward.metadata?.discountAmount || 0)}`
                      }
                    </span>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}

          <Button
            variant="outline"
            onClick={() => {
              clearCart()
              toast({
                title: "Cart cleared",
                description: "All items have been removed from your cart",
              })
            }}
          >
            Clear Cart
          </Button>
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <Card className="card-glass border-none sticky top-20">
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex justify-between">
                <span>Subtotal</span>
                <span>{formatCurrency(subtotal)}</span>
              </div>
              {freeItems.length > 0 && (
                <div className="flex justify-between text-primary">
                  <span className="flex items-center gap-1">
                    <Gift className="h-4 w-4" />
                    Free Items ({freeItems.length})
                  </span>
                  <span>FREE</span>
                </div>
              )}
              {discount > 0 && (
                <div className="flex justify-between text-green-600">
                  <span className="flex items-center gap-1">
                    <Percent className="h-4 w-4" />
                    Loyalty Discount
                  </span>
                  <span>-{formatCurrency(discount)}</span>
                </div>
              )}
              <div className="flex justify-between">
                <span>Tax (8%)</span>
                <span>{formatCurrency(tax)}</span>
              </div>
              <Separator />
              <div className="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span className="text-primary">{formatCurrency(total)}</span>
              </div>
            </CardContent>
            <CardFooter>
              <Button asChild className="w-full" size="lg">
                <Link href="/checkout">Proceed to Checkout</Link>
              </Button>
            </CardFooter>
          </Card>
        </div>
      </div>
    </div>
  )
}


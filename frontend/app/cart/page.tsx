"use client"

import Link from "next/link"
import { Minus, Plus, Trash2, ShoppingBag } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import { useCartStore } from "@/lib/store"
import { formatCurrency } from "@/lib/utils"
import { useToast } from "@/hooks/use-toast"

export default function CartPage() {
  const { items, updateQuantity, removeItem, getTotal, clearCart } = useCartStore()
  const { toast } = useToast()

  const handleRemoveItem = (id: string) => {
    removeItem(id)
    toast({
      title: "Item removed",
      description: "Item has been removed from your cart",
    })
  }

  const subtotal = getTotal()
  const tax = subtotal * 0.08 // 8% tax
  const total = subtotal + tax

  if (items.length === 0) {
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


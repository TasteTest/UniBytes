"use client"

import { useState } from "react"
import { Clock, CheckCircle2, ChefHat, Package } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Separator } from "@/components/ui/separator"
import { formatCurrency, formatDate } from "@/lib/utils"

interface Order {
  id: string
  items: Array<{ name: string; quantity: number; price: number }>
  status: "pending" | "preparing" | "ready" | "completed" | "cancelled"
  total: number
  pickupLocation: string
  pickupTime: string
  createdAt: string
}

// Mock data
const mockOrders: Order[] = [
  {
    id: "ORD-001",
    items: [
      { name: "Grilled Chicken Sandwich", quantity: 1, price: 8.99 },
      { name: "Caesar Salad", quantity: 1, price: 6.99 },
      { name: "Fresh Lemonade", quantity: 2, price: 3.49 },
    ],
    status: "ready",
    total: 24.12,
    pickupLocation: "Main Cafeteria",
    pickupTime: "ASAP",
    createdAt: new Date().toISOString(),
  },
  {
    id: "ORD-002",
    items: [
      { name: "Pepperoni Pizza", quantity: 1, price: 12.99 },
      { name: "Iced Coffee", quantity: 1, price: 4.49 },
    ],
    status: "preparing",
    total: 18.85,
    pickupLocation: "Student Union",
    pickupTime: "ASAP",
    createdAt: new Date(Date.now() - 600000).toISOString(),
  },
  {
    id: "ORD-003",
    items: [
      { name: "Bacon Burger", quantity: 2, price: 11.49 },
      { name: "Chocolate Chip Cookie", quantity: 3, price: 2.99 },
    ],
    status: "pending",
    total: 32.87,
    pickupLocation: "Library Café",
    pickupTime: "12:30 PM",
    createdAt: new Date(Date.now() - 300000).toISOString(),
  },
  {
    id: "ORD-004",
    items: [{ name: "Margherita Pizza", quantity: 1, price: 11.99 }],
    status: "completed",
    total: 12.95,
    pickupLocation: "Main Cafeteria",
    pickupTime: "ASAP",
    createdAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: "ORD-005",
    items: [
      { name: "Poke Bowl", quantity: 1, price: 12.99 },
      { name: "Green Juice", quantity: 1, price: 5.99 },
    ],
    status: "completed",
    total: 20.49,
    pickupLocation: "Student Union",
    pickupTime: "ASAP",
    createdAt: new Date(Date.now() - 172800000).toISOString(),
  },
  {
    id: "ORD-006",
    items: [
      { name: "Club Sandwich", quantity: 1, price: 10.49 },
      { name: "Greek Salad", quantity: 1, price: 7.49 },
      { name: "Brownie Sundae", quantity: 1, price: 5.99 },
    ],
    status: "completed",
    total: 25.85,
    pickupLocation: "Library Café",
    pickupTime: "1:00 PM",
    createdAt: new Date(Date.now() - 259200000).toISOString(),
  },
]

const statusConfig = {
  pending: { label: "Order Placed", icon: Clock, color: "bg-blue-500" },
  preparing: { label: "Preparing", icon: ChefHat, color: "bg-yellow-500" },
  ready: { label: "Ready for Pickup", icon: Package, color: "bg-green-500" },
  completed: { label: "Completed", icon: CheckCircle2, color: "bg-gray-500" },
  cancelled: { label: "Cancelled", icon: CheckCircle2, color: "bg-red-500" },
}

export default function OrdersPage() {
  const activeOrders = mockOrders.filter((o) => ["pending", "preparing", "ready"].includes(o.status))
  const pastOrders = mockOrders.filter((o) => ["completed", "cancelled"].includes(o.status))

  const OrderCard = ({ order }: { order: Order }) => {
    const config = statusConfig[order.status]
    const Icon = config.icon

    return (
      <Card className="card-glass border-none">
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                Order #{order.id}
                <Badge
                  variant={order.status === "ready" ? "success" : "secondary"}
                  className="ml-2"
                >
                  {config.label}
                </Badge>
              </CardTitle>
              <CardDescription className="mt-1">
                {formatDate(order.createdAt)}
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Timeline */}
          <div className="flex items-center gap-2">
            <div className="flex-1">
              <div className="relative">
                <div className="h-2 bg-muted rounded-full overflow-hidden">
                  <div
                    className={`h-full ${config.color} transition-all duration-500`}
                    style={{
                      width:
                        order.status === "pending"
                          ? "25%"
                          : order.status === "preparing"
                          ? "50%"
                          : order.status === "ready"
                          ? "75%"
                          : "100%",
                    }}
                  />
                </div>
              </div>
              <div className="flex justify-between mt-2 text-xs text-muted-foreground">
                <span>Placed</span>
                <span>Preparing</span>
                <span>Ready</span>
                <span>Complete</span>
              </div>
            </div>
          </div>

          <Separator />

          {/* Items */}
          <div>
            <h4 className="font-semibold mb-2 text-sm">Items</h4>
            {order.items.map((item, index) => (
              <div key={index} className="flex justify-between text-sm mb-1">
                <span className="text-muted-foreground">
                  {item.quantity}x {item.name}
                </span>
                <span>{formatCurrency(item.price * item.quantity)}</span>
              </div>
            ))}
          </div>

          <Separator />

          {/* Pickup Details */}
          <div className="space-y-1 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Pickup Location:</span>
              <span className="font-medium">{order.pickupLocation}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Pickup Time:</span>
              <span className="font-medium">{order.pickupTime}</span>
            </div>
          </div>

          <Separator />

          {/* Total */}
          <div className="flex justify-between items-center">
            <span className="font-semibold">Total</span>
            <span className="text-xl font-bold text-primary">
              {formatCurrency(order.total)}
            </span>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="container py-8 max-w-4xl">
      <h1 className="text-4xl font-bold mb-8">My Orders</h1>

      <Tabs defaultValue="active" className="space-y-6">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="active">
            Active Orders ({activeOrders.length})
          </TabsTrigger>
          <TabsTrigger value="past">
            Past Orders ({pastOrders.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="active" className="space-y-4">
          {activeOrders.length > 0 ? (
            activeOrders.map((order) => <OrderCard key={order.id} order={order} />)
          ) : (
            <Card className="card-glass border-none">
              <CardContent className="py-12 text-center">
                <Clock className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                <p className="text-muted-foreground">No active orders</p>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="past" className="space-y-4">
          {pastOrders.length > 0 ? (
            pastOrders.map((order) => <OrderCard key={order.id} order={order} />)
          ) : (
            <Card className="card-glass border-none">
              <CardContent className="py-12 text-center">
                <CheckCircle2 className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                <p className="text-muted-foreground">No past orders</p>
              </CardContent>
            </Card>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}


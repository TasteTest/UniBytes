"use client"

import { useState } from "react"
import { ChefHat, Clock, CheckCircle } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Separator } from "@/components/ui/separator"
import { useToast } from "@/hooks/use-toast"
import { formatTime } from "@/lib/utils"

interface KitchenOrder {
  id: string
  orderNumber: string
  items: Array<{ name: string; quantity: number; modifiers?: string[] }>
  station: "grill" | "salad" | "pizza" | "drinks"
  status: "pending" | "preparing" | "ready"
  placedAt: string
  estimatedTime: number
}

const mockOrders: KitchenOrder[] = [
  {
    id: "1",
    orderNumber: "ORD-001",
    items: [
      { name: "Grilled Chicken Sandwich", quantity: 1, modifiers: ["No mayo", "Extra lettuce"] },
      { name: "Caesar Salad", quantity: 1 },
    ],
    station: "grill",
    status: "pending",
    placedAt: new Date().toISOString(),
    estimatedTime: 10,
  },
  {
    id: "2",
    orderNumber: "ORD-002",
    items: [{ name: "Pepperoni Pizza", quantity: 1, modifiers: ["Extra cheese", "Well done"] }],
    station: "pizza",
    status: "preparing",
    placedAt: new Date(Date.now() - 300000).toISOString(),
    estimatedTime: 15,
  },
  {
    id: "3",
    orderNumber: "ORD-003",
    items: [
      { name: "Bacon Burger", quantity: 2, modifiers: ["Medium rare"] },
      { name: "Mushroom Swiss Burger", quantity: 1, modifiers: ["No onions"] },
    ],
    station: "grill",
    status: "preparing",
    placedAt: new Date(Date.now() - 480000).toISOString(),
    estimatedTime: 12,
  },
  {
    id: "4",
    orderNumber: "ORD-004",
    items: [
      { name: "Greek Salad", quantity: 1, modifiers: ["Extra feta"] },
      { name: "Cobb Salad", quantity: 1 },
    ],
    station: "salad",
    status: "pending",
    placedAt: new Date(Date.now() - 120000).toISOString(),
    estimatedTime: 6,
  },
  {
    id: "5",
    orderNumber: "ORD-005",
    items: [
      { name: "Margherita Pizza", quantity: 1 },
      { name: "Veggie Pizza", quantity: 1, modifiers: ["No olives"] },
    ],
    station: "pizza",
    status: "ready",
    placedAt: new Date(Date.now() - 900000).toISOString(),
    estimatedTime: 16,
  },
  {
    id: "6",
    orderNumber: "ORD-006",
    items: [
      { name: "Fresh Lemonade", quantity: 2 },
      { name: "Iced Coffee", quantity: 1, modifiers: ["Oat milk"] },
      { name: "Green Juice", quantity: 1 },
    ],
    station: "drinks",
    status: "pending",
    placedAt: new Date(Date.now() - 60000).toISOString(),
    estimatedTime: 3,
  },
  {
    id: "7",
    orderNumber: "ORD-007",
    items: [
      { name: "Cheeseburger", quantity: 1, modifiers: ["Well done", "Extra pickles"] },
    ],
    station: "grill",
    status: "ready",
    placedAt: new Date(Date.now() - 720000).toISOString(),
    estimatedTime: 12,
  },
]

export default function KitchenPage() {
  const [orders, setOrders] = useState(mockOrders)
  const { toast } = useToast()

  const updateOrderStatus = (
    orderId: string,
    newStatus: "pending" | "preparing" | "ready"
  ) => {
    setOrders((prev) =>
      prev.map((order) =>
        order.id === orderId ? { ...order, status: newStatus } : order
      )
    )
    toast({
      title: "Order updated",
      description: `Order status changed to ${newStatus}`,
      variant: "success",
    })
  }

  const OrderCard = ({ order }: { order: KitchenOrder }) => {
    const statusColors = {
      pending: "bg-yellow-500",
      preparing: "bg-blue-500",
      ready: "bg-green-500",
    }

    return (
      <Card className="card-glass border-none">
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                Order #{order.orderNumber}
                <Badge className={statusColors[order.status]}>
                  {order.status.charAt(0).toUpperCase() + order.status.slice(1)}
                </Badge>
              </CardTitle>
              <CardDescription className="mt-1">
                Placed at {formatTime(order.placedAt)} • Est. {order.estimatedTime} min
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Items */}
          <div>
            {order.items.map((item, index) => (
              <div key={index} className="mb-2">
                <div className="flex items-start gap-2">
                  <Badge variant="outline" className="mt-0.5">
                    {item.quantity}x
                  </Badge>
                  <div className="flex-1">
                    <p className="font-medium">{item.name}</p>
                    {item.modifiers && item.modifiers.length > 0 && (
                      <p className="text-sm text-muted-foreground">
                        {item.modifiers.join(", ")}
                      </p>
                    )}
                  </div>
                </div>
                {index < order.items.length - 1 && (
                  <Separator className="mt-2" />
                )}
              </div>
            ))}
          </div>

          {/* Actions */}
          <div className="flex gap-2">
            {order.status === "pending" && (
              <Button
                onClick={() => updateOrderStatus(order.id, "preparing")}
                className="flex-1"
              >
                <ChefHat className="h-4 w-4 mr-2" />
                Start Preparing
              </Button>
            )}
            {order.status === "preparing" && (
              <Button
                onClick={() => updateOrderStatus(order.id, "ready")}
                className="flex-1"
                variant="default"
              >
                <CheckCircle className="h-4 w-4 mr-2" />
                Mark as Ready
              </Button>
            )}
            {order.status === "ready" && (
              <div className="flex-1 text-center py-2 text-green-600 font-semibold">
                ✓ Ready for Pickup
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    )
  }

  const filterByStation = (station: string) => {
    if (station === "all") return orders
    return orders.filter((order) => order.station === station)
  }

  return (
    <div className="container py-8">
      {/* Demo Mode Banner */}
      <div className="mb-6 p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
        <div className="flex items-center gap-2">
          <span className="text-blue-600 dark:text-blue-400 font-semibold">🎭 Demo Mode</span>
          <span className="text-blue-600 dark:text-blue-400">- Kitchen staff view with mock orders</span>
        </div>
      </div>

      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2 flex items-center gap-3">
          <ChefHat className="h-10 w-10 text-primary" />
          Kitchen Dashboard
        </h1>
        <p className="text-muted-foreground">
          Manage incoming orders and update status
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-yellow-500">
                {orders.filter((o) => o.status === "pending").length}
              </p>
              <p className="text-sm text-muted-foreground mt-1">Pending</p>
            </div>
          </CardContent>
        </Card>
        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-blue-500">
                {orders.filter((o) => o.status === "preparing").length}
              </p>
              <p className="text-sm text-muted-foreground mt-1">Preparing</p>
            </div>
          </CardContent>
        </Card>
        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-green-500">
                {orders.filter((o) => o.status === "ready").length}
              </p>
              <p className="text-sm text-muted-foreground mt-1">Ready</p>
            </div>
          </CardContent>
        </Card>
        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-primary">
                {orders.length}
              </p>
              <p className="text-sm text-muted-foreground mt-1">Total Active</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Orders by Station */}
      <Tabs defaultValue="all">
        <TabsList className="mb-6">
          <TabsTrigger value="all">All Orders</TabsTrigger>
          <TabsTrigger value="grill">Grill</TabsTrigger>
          <TabsTrigger value="salad">Salad</TabsTrigger>
          <TabsTrigger value="pizza">Pizza</TabsTrigger>
          <TabsTrigger value="drinks">Drinks</TabsTrigger>
        </TabsList>

        {["all", "grill", "salad", "pizza", "drinks"].map((station) => (
          <TabsContent key={station} value={station} className="space-y-4">
            {filterByStation(station).length > 0 ? (
              filterByStation(station).map((order) => (
                <OrderCard key={order.id} order={order} />
              ))
            ) : (
              <Card className="card-glass border-none">
                <CardContent className="py-12 text-center">
                  <Clock className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                  <p className="text-muted-foreground">No orders for this station</p>
                </CardContent>
              </Card>
            )}
          </TabsContent>
        ))}
      </Tabs>
    </div>
  )
}


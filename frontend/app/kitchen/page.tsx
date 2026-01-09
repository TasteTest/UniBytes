"use client"

import { useState, useEffect } from "react"
import { useSession } from "next-auth/react"
import { ChefHat, Clock, CheckCircle, Loader2, ShieldAlert, Gift } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Separator } from "@/components/ui/separator"
import { useToast } from "@/hooks/use-toast"
import { formatTime, formatCurrency } from "@/lib/utils"
import { useAdmin } from "@/lib/hooks/useAdmin"
import Link from "next/link"

interface KitchenOrderItem {
  name: string
  unitPrice: number
  quantity: number
  totalPrice: number
  modifiers?: any
  isReward?: boolean
  rewardId?: string
}

interface KitchenOrder {
  id: string // Used for API calls only, not displayed
  orderIndex: number // Sequential number for display
  totalAmount: number
  currency: string
  paymentStatus: string
  orderStatus: string
  createdAt: string
  metadata?: any
  orderItems: KitchenOrderItem[]
}

const getKitchenStatus = (orderStatus: string): "pending" | "preparing" | "ready" | "completed" => {
  const status = (orderStatus || "").toString().toLowerCase()
  if (status === "pending" || status === "confirmed") return "pending"
  if (status === "preparing") return "preparing"
  if (status === "ready") return "ready"
  if (status === "completed") return "completed"
  return "pending"
}

const getBackendStatusCode = (kitchenStatus: string): number => {
  switch (kitchenStatus) {
    case "pending":
      return 0
    case "preparing":
      return 2
    case "ready":
      return 3
    case "completed":
      return 4
    default:
      return 0
  }
}

const getStation = (items: KitchenOrderItem[]): "grill" | "salad" | "pizza" | "drinks" | "general" => {
  const names = items.map((i) => (i.name || "").toLowerCase())
  if (names.some((n) => n.includes("burger") || n.includes("chicken") || n.includes("bacon"))) return "grill"
  if (names.some((n) => n.includes("salad"))) return "salad"
  if (names.some((n) => n.includes("pizza"))) return "pizza"
  if (names.some((n) => n.includes("drink") || n.includes("juice") || n.includes("coffee") || n.includes("lemonade"))) return "drinks"
  return "general"
}

// Generate a stable order number from timestamp (1-1000, resets after 1000)
const getStableOrderNumber = (createdAt: string): number => {
  const timestamp = new Date(createdAt).getTime()
  return (timestamp % 1000) + 1
}

export default function KitchenPage() {
  const { data: session } = useSession()
  const { canAccessKitchen, isLoading: roleLoading } = useAdmin()
  const [orders, setOrders] = useState<KitchenOrder[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { toast } = useToast()

  // Get auth info from session
  const token = (session as any)?.accessToken
  const userEmail = session?.user?.email

  const fetchOrders = async () => {
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL
      if (!apiBase) throw new Error("API base URL not configured")
      const res = await fetch(`${apiBase}/orders`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'X-User-Email': userEmail || '',
          'Content-Type': 'application/json'
        }
      })
      if (!res.ok) throw new Error("Failed to fetch orders")
      const data: KitchenOrder[] = await res.json()
      const active = data
        .filter((o: any) => {
          const s = (o.orderStatus || "").toString().toLowerCase()
          return s !== "cancelled" && s !== "failed" && s !== "completed"
        })
        .map((o: any) => ({
          ...o,
          orderIndex: getStableOrderNumber(o.createdAt),
        }))
      setOrders(active)
      setError(null)
    } catch (err) {
      console.error("Error fetching orders:", err)
      setError("Failed to load orders")
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (session && token && userEmail) {
      fetchOrders()
      const id = setInterval(fetchOrders, 10000)
      return () => clearInterval(id)
    }
  }, [session, token, userEmail])

  const updateOrderStatus = async (orderId: string, orderIndex: number, newStatus: "pending" | "preparing" | "ready" | "completed") => {
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL
      if (!apiBase) throw new Error("API base URL not configured")
      const code = getBackendStatusCode(newStatus)
      const res = await fetch(`${apiBase}/orders/${orderId}/status`, {
        method: "PUT",
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
          'X-User-Email': userEmail || ''
        },
        body: JSON.stringify({ orderStatus: code }),
      })
      if (!res.ok) throw new Error("Failed to update")

      setOrders((prev) => prev.map((o) => (o.id === orderId ? { ...o, orderStatus: newStatus.charAt(0).toUpperCase() + newStatus.slice(1) } : o)))
      toast({ title: "Order updated", description: `Order ${orderIndex} status changed to ${newStatus}` })
      fetchOrders()
    } catch (err) {
      console.error("Error updating order:", err)
      toast({ title: "Update failed", description: "Could not update order status", variant: "destructive" })
    }
  }

  const OrderCard = ({ order }: { order: KitchenOrder }) => {
    const kitchenStatus = getKitchenStatus(order.orderStatus)
    const station = getStation(order.orderItems)
    const estimatedTime = Math.max(5, order.orderItems.length * 5)

    const statusColors: Record<string, string> = {
      pending: "bg-yellow-500",
      preparing: "bg-blue-500",
      ready: "bg-green-500",
      completed: "bg-gray-500",
    }

    return (
      <Card className="card-glass border-none">
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                Order {order.orderIndex}
                <Badge className={statusColors[kitchenStatus]}>{kitchenStatus.charAt(0).toUpperCase() + kitchenStatus.slice(1)}</Badge>
                <Badge variant="outline">{station.toUpperCase()}</Badge>
              </CardTitle>
              <CardDescription className="mt-1">Placed at {formatTime(order.createdAt)} • Est. {estimatedTime} min</CardDescription>
            </div>
          </div>
        </CardHeader>

        <CardContent className="space-y-4">
          {/* Order Items - Full Details */}
          <div className="bg-muted/50 rounded-lg p-3">
            <h4 className="font-semibold mb-3 text-sm uppercase tracking-wide">Items to Prepare</h4>
            {(!order.orderItems || order.orderItems.length === 0) ? (
              <p className="text-muted-foreground text-sm">No items found</p>
            ) : (
              order.orderItems.map((item, index) => (
                <div key={index} className={`mb-3 last:mb-0 ${item.isReward ? 'bg-primary/10 rounded-lg p-2 -mx-2' : ''}`}>
                  <div className="flex items-start gap-3">
                    <Badge variant={item.isReward ? "secondary" : "default"} className="mt-0.5 min-w-[40px] justify-center">
                      {item.quantity}x
                    </Badge>
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        {item.isReward && <Gift className="h-4 w-4 text-primary" />}
                        <p className="font-bold text-lg">{item.name}</p>
                        {item.isReward && (
                          <Badge variant="outline" className="text-xs border-primary text-primary">
                            REWARD
                          </Badge>
                        )}
                      </div>
                      {item.modifiers && (
                        <div className="mt-1 p-2 bg-yellow-100 dark:bg-yellow-900/30 rounded text-sm">
                          <span className="font-medium">Special instructions: </span>
                          {typeof item.modifiers === 'object' ? JSON.stringify(item.modifiers) : item.modifiers}
                        </div>
                      )}
                      {item.isReward ? (
                        <p className="text-sm text-primary mt-1 font-medium">FREE (Loyalty Reward)</p>
                      ) : (
                        <p className="text-sm text-muted-foreground mt-1">{formatCurrency(item.unitPrice)} × {item.quantity} = {formatCurrency(item.totalPrice)}</p>
                      )}
                    </div>
                  </div>
                  {index < order.orderItems.length - 1 && <Separator className="mt-3" />}
                </div>
              ))
            )}
          </div>

          <Separator />

          <div className="flex justify-between items-center">
            <span className="font-semibold">Total</span>
            <span className="text-xl font-bold text-primary">{formatCurrency(order.totalAmount, order.currency)}</span>
          </div>

          <div className="flex gap-2">
            {kitchenStatus === "pending" && (
              <Button onClick={() => updateOrderStatus(order.id, order.orderIndex, "preparing")} className="flex-1">
                <ChefHat className="h-4 w-4 mr-2" /> Start Preparing
              </Button>
            )}

            {kitchenStatus === "preparing" && (
              <Button onClick={() => updateOrderStatus(order.id, order.orderIndex, "ready")} className="flex-1" variant="default">
                <CheckCircle className="h-4 w-4 mr-2" /> Mark as Ready
              </Button>
            )}

            {kitchenStatus === "ready" && (
              <>
                <div className="flex-1 text-center py-2 text-green-600 font-semibold">✓ Ready for Pickup</div>
                <Button onClick={() => updateOrderStatus(order.id, order.orderIndex, "completed")} variant="outline" size="sm">Complete</Button>
              </>
            )}
          </div>
        </CardContent>
      </Card>
    )
  }

  const filterByStation = (station: string) => {
    if (station === "all") return orders
    return orders.filter((o) => getStation(o.orderItems) === station)
  }

  // Role loading
  if (roleLoading) {
    return (
      <div className="container py-8 flex justify-center items-center h-[50vh]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  // Access denied for users without kitchen access
  if (!canAccessKitchen) {
    return (
      <div className="container py-8">
        <Card className="card-glass border-none max-w-md mx-auto">
          <CardContent className="pt-8 pb-8 text-center">
            <ShieldAlert className="h-16 w-16 mx-auto text-destructive mb-4" />
            <h2 className="text-2xl font-bold mb-2">Access Denied</h2>
            <p className="text-muted-foreground mb-6">
              Only Chef or Admin can access the Kitchen Dashboard.
            </p>
            <Link href="/">
              <Button variant="ghost" className="w-full">
                Return Home
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    )
  }

  if (loading) {
    return (
      <div className="container py-8 flex justify-center items-center h-[50vh]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="container py-8">
        <div className="text-center py-12">
          <p className="text-destructive mb-4">{error}</p>
          <Button onClick={fetchOrders}>Retry</Button>
        </div>
      </div>
    )
  }

  return (
    <div className="container py-8">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-4xl font-bold mb-2">Kitchen Dashboard</h1>
          <p className="text-muted-foreground">Manage and track active orders</p>
        </div>
        <ChefHat className="h-12 w-12 text-primary" />
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-yellow-500">{orders.filter((o) => getKitchenStatus(o.orderStatus) === "pending").length}</p>
              <p className="text-sm text-muted-foreground mt-1">Pending</p>
            </div>
          </CardContent>
        </Card>

        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-blue-500">{orders.filter((o) => getKitchenStatus(o.orderStatus) === "preparing").length}</p>
              <p className="text-sm text-muted-foreground mt-1">Preparing</p>
            </div>
          </CardContent>
        </Card>

        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-green-500">{orders.filter((o) => getKitchenStatus(o.orderStatus) === "ready").length}</p>
              <p className="text-sm text-muted-foreground mt-1">Ready</p>
            </div>
          </CardContent>
        </Card>

        <Card className="card-glass border-none">
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-3xl font-bold text-primary">{orders.length}</p>
              <p className="text-sm text-muted-foreground mt-1">Total Active</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Orders by Station */}
      <Tabs defaultValue="all">
        <TabsList className="mb-6">
          <TabsTrigger value="all">All Orders ({orders.length})</TabsTrigger>
          <TabsTrigger value="grill">Grill ({filterByStation("grill").length})</TabsTrigger>
          <TabsTrigger value="salad">Salad ({filterByStation("salad").length})</TabsTrigger>
          <TabsTrigger value="pizza">Pizza ({filterByStation("pizza").length})</TabsTrigger>
          <TabsTrigger value="drinks">Drinks ({filterByStation("drinks").length})</TabsTrigger>
          <TabsTrigger value="general">General ({filterByStation("general").length})</TabsTrigger>
        </TabsList>

        {["all", "grill", "salad", "pizza", "drinks", "general"].map((station) => (
          <TabsContent key={station} value={station} className="space-y-4">
            {filterByStation(station).length > 0 ? (
              filterByStation(station).map((order) => <OrderCard key={order.orderIndex} order={order} />)
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

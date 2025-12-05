"use client";

import { useSession } from "next-auth/react";
import { useEffect, useState } from "react";
import { Loader2, Clock, CheckCircle2 } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { formatCurrency, formatDate } from "@/lib/utils";

interface BackendOrderResponse {
    id: string;
    userId: string;
    externalUserRef?: string;
    totalAmount: number;
    currency: string;
    paymentStatus: string;
    orderStatus: string;    
    createdAt: string;
    metadata?: any;
}

interface Order {
    id: string;
    status: string;
    createdAt: string;
    total: number;
    currency: string;
    items: any[];
    pickupLocation?: string;
    pickupTime?: string;
}

const statusConfig: any = {
    pending: { label: "Pending", color: "bg-yellow-500", icon: Clock },
    confirmed: { label: "Confirmed", color: "bg-orange-500", icon: Clock },
    preparing: { label: "Preparing", color: "bg-blue-500", icon: Clock },
    ready: { label: "Ready", color: "bg-green-500", icon: CheckCircle2 },
    completed: { label: "Completed", color: "bg-gray-500", icon: CheckCircle2 },
    cancelled: { label: "Cancelled", color: "bg-red-500", icon: CheckCircle2 },
    failed: { label: "Failed", color: "bg-red-700", icon: CheckCircle2 },
};

export default function OrdersPage() {
    const { data: session, status } = useSession();
    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (status === "loading") return;

        const fetchData = async () => {
            try {
                const user = session?.user as any;
                const userId = user?.backendId || user?.id;
                const token = (session as any)?.accessToken;

                if (!userId) {
                    setLoading(false);
                    return;
                }

                const response = await fetch(`http://localhost:5267/api/orders/user/${user.backendId}`, {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) {
                    throw new Error("Failed to fetch orders");
                }

                const rawData: BackendOrderResponse[] = await response.json();
                const mappedOrders: Order[] = rawData.map(order => ({
                    id: order.id,
                    status: order.orderStatus.toLowerCase(),
                    createdAt: order.createdAt,
                    total: order.totalAmount,
                    currency: order.currency,
                    items: [],
                    pickupLocation: order.metadata?.pickupLocation || "Restaurant",
                    pickupTime: order.metadata?.pickupTime || null
                }));
                setOrders(mappedOrders);
            } catch (err: any) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [session, status]);

    const activeOrders = orders.filter((o) => ["pending", "preparing", "ready"].includes(o.status));
    const pastOrders = orders.filter((o) => ["completed", "cancelled"].includes(o.status));

    const OrderCard = ({ order }: { order: Order }) => {
        const config = statusConfig[order.status] || statusConfig.pending;

        return (
            <Card className="card-glass border-none mb-4">
                <CardHeader>
                    <div className="flex items-start justify-between">
                        <div>
                            <CardTitle className="flex items-center gap-2">
                                Order #{order.id.substring(0, 8)}...
                                <Badge
                                    variant={order.status === "ready" ? "default" : "secondary"}
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
                    <div className="flex items-center gap-2">
                        <div className="flex-1">
                            <div className="relative">
                                <div className="h-2 bg-muted rounded-full overflow-hidden">
                                    <div
                                        className={`h-full ${config.color} transition-all duration-500`}
                                        style={{
                                            width:
                                                order.status === "pending"
                                                    ? "15%"
                                                    : order.status === "confirmed"
                                                        ? "30%"
                                                        : order.status === "preparing"
                                                            ? "45%"
                                                            : order.status == "ready"
                                                                ? "60%"
                                                                : order.status === "completed"
                                                                    ? "100%"
                                                                    : "0%"
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

                    <div>
                        <h4 className="font-semibold mb-2 text-sm">Items</h4>
                        {order.items.map((item, index) => (
                            <div key={index} className="flex justify-between text-sm mb-1">
                                <span className="text-muted-foreground">
                                    {item.quantity}x {item.name}
                                </span>
                                <span>{formatCurrency(item.price * item.quantity, order.currency)}</span>
                            </div>
                        ))}
                    </div>

                    <Separator />

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

                    <div className="flex justify-between items-center">
                        <span className="font-semibold">Total</span>
                        <span className="text-xl font-bold text-primary">
                            {formatCurrency(order.total, order.currency)}
                        </span>
                    </div>
                </CardContent>
            </Card>
        );
    };

    if (loading) {
        return (
            <div className="container py-8 max-w-4xl flex justify-center items-center h-[50vh]">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (error) {
        return (
            <div className="container py-8 max-w-4xl text-center text-red-500">
                <p>{error}</p>
            </div>
        );
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
    );
}
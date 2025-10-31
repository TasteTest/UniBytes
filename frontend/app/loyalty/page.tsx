"use client"

import { Gift, Star, TrendingUp, Award } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { Separator } from "@/components/ui/separator"

interface Reward {
  id: string
  name: string
  description: string
  pointsRequired: number
  available: boolean
}

const mockRewards: Reward[] = [
  {
    id: "1",
    name: "Free Cookie",
    description: "Get a free chocolate chip cookie",
    pointsRequired: 50,
    available: true,
  },
  {
    id: "2",
    name: "Free Drink",
    description: "Any size fountain drink",
    pointsRequired: 100,
    available: true,
  },
  {
    id: "3",
    name: "$5 Off",
    description: "$5 off your next order",
    pointsRequired: 200,
    available: true,
  },
  {
    id: "4",
    name: "Free Meal",
    description: "Any menu item up to $15",
    pointsRequired: 500,
    available: true,
  },
]

export default function LoyaltyPage() {
  const currentPoints = 175
  const nextReward = mockRewards.find((r) => r.pointsRequired > currentPoints)
  const pointsToNext = nextReward ? nextReward.pointsRequired - currentPoints : 0
  const progressToNext = nextReward
    ? (currentPoints / nextReward.pointsRequired) * 100
    : 100

  return (
    <div className="container py-8 max-w-4xl">
      <h1 className="text-4xl font-bold mb-8">Rewards & Loyalty</h1>

      {/* Points Overview */}
      <Card className="card-glass border-none mb-8">
        <CardContent className="pt-6">
          <div className="text-center space-y-4">
            <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-saffron-400 to-orange-500">
              <Star className="h-10 w-10 text-white fill-white" />
            </div>
            <div>
              <h2 className="text-5xl font-bold text-gradient mb-2">{currentPoints}</h2>
              <p className="text-muted-foreground">Total Points</p>
            </div>
            {nextReward && (
              <div className="max-w-md mx-auto space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    {pointsToNext} points to {nextReward.name}
                  </span>
                  <span className="font-medium">{nextReward.pointsRequired} pts</span>
                </div>
                <Progress value={progressToNext} />
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* How It Works */}
      <Card className="card-glass border-none mb-8">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5" />
            How It Works
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid md:grid-cols-3 gap-6">
            <div className="text-center">
              <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-3">
                <span className="text-2xl font-bold text-primary">1</span>
              </div>
              <h3 className="font-semibold mb-1">Order Food</h3>
              <p className="text-sm text-muted-foreground">
                Earn 10 points for every $1 spent
              </p>
            </div>
            <div className="text-center">
              <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-3">
                <span className="text-2xl font-bold text-primary">2</span>
              </div>
              <h3 className="font-semibold mb-1">Collect Points</h3>
              <p className="text-sm text-muted-foreground">
                Points add up with each order
              </p>
            </div>
            <div className="text-center">
              <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center mx-auto mb-3">
                <span className="text-2xl font-bold text-primary">3</span>
              </div>
              <h3 className="font-semibold mb-1">Redeem Rewards</h3>
              <p className="text-sm text-muted-foreground">
                Use points for free items and discounts
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Available Rewards */}
      <div className="space-y-4">
        <h2 className="text-2xl font-bold flex items-center gap-2">
          <Gift className="h-6 w-6" />
          Available Rewards
        </h2>
        <div className="grid md:grid-cols-2 gap-4">
          {mockRewards.map((reward) => {
            const canRedeem = currentPoints >= reward.pointsRequired
            return (
              <Card
                key={reward.id}
                className={`card-glass border-none ${
                  canRedeem ? "ring-2 ring-primary" : ""
                }`}
              >
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="flex items-center gap-2">
                        {reward.name}
                        {canRedeem && (
                          <Badge variant="success" className="ml-2">
                            <Award className="h-3 w-3 mr-1" />
                            Available
                          </Badge>
                        )}
                      </CardTitle>
                      <CardDescription className="mt-1">
                        {reward.description}
                      </CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Star className="h-4 w-4 text-primary" />
                      <span className="font-bold">{reward.pointsRequired}</span>
                      <span className="text-sm text-muted-foreground">points</span>
                    </div>
                    <Button disabled={!canRedeem} size="sm">
                      {canRedeem ? "Redeem" : "Not Available"}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            )
          })}
        </div>
      </div>

      {/* Recent Activity */}
      <Card className="card-glass border-none mt-8">
        <CardHeader>
          <CardTitle>Recent Activity</CardTitle>
          <CardDescription>Your latest points earned</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {[
              { date: "Today", description: "Order #ORD-001", points: "+150" },
              { date: "Yesterday", description: "Order #ORD-002", points: "+120" },
              { date: "3 days ago", description: "Order #ORD-003", points: "+89" },
            ].map((activity, index) => (
              <div key={index}>
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">{activity.description}</p>
                    <p className="text-sm text-muted-foreground">{activity.date}</p>
                  </div>
                  <div className="text-lg font-bold text-primary">
                    {activity.points}
                  </div>
                </div>
                {index < 2 && <Separator className="mt-4" />}
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}


"use client"

import { useEffect, useState } from "react"
import { Gift, Star, TrendingUp, Award, Loader2 } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { Separator } from "@/components/ui/separator"
import { useToast } from "@/hooks/use-toast"
import { loyaltyService } from "@/lib/api/loyalty"
import { AVAILABLE_REWARDS } from "@/lib/config/rewards"
import type { LoyaltyAccountDetails } from "@/lib/types/loyalty.types"

export default function LoyaltyPage() {
  const [accountDetails, setAccountDetails] = useState<LoyaltyAccountDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [redeeming, setRedeeming] = useState<string | null>(null)
  const { toast } = useToast()


  // For now, using a demo user ID
  const userId = "3fa85f64-5717-4562-b3fc-2c963f66afa7"

  useEffect(() => {
    loadLoyaltyData()
  }, [])

  const loadLoyaltyData = async () => {
    try {
      setLoading(true)
      const response = await loyaltyService.getOrCreateAccount(userId)
      
      if (!response.isSuccess) {
        throw new Error(response.error || "Failed to load account")
      }

      // Get full details
      const detailsResponse = await loyaltyService.getAccountDetails(userId)
      
      if (detailsResponse.isSuccess && detailsResponse.data) {
        setAccountDetails(detailsResponse.data)
      }
    } catch (error) {
      console.error("Error loading loyalty data:", error)
      toast({
        title: "Error",
        description: "Failed to load loyalty data. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const handleRedeem = async (rewardId: string, points: number, rewardName: string) => {
    if (!accountDetails) return

    try {
      setRedeeming(rewardId)
      
      const reward = AVAILABLE_REWARDS.find(r => r.id === rewardId)
      if (!reward) return

      const response = await loyaltyService.redeemPoints({
        userId,
        points,
        rewardType: reward.rewardType,
        rewardMetadata: {
          rewardId,
          rewardName,
          rewardDescription: reward.description,
          ...reward.metadata,
        },
      })

      if (!response.isSuccess) {
        throw new Error(response.error || "Failed to redeem points")
      }

      toast({
        title: "Success! 🎉",
        description: `You've redeemed ${rewardName}`,
      })

      // Reload data
      await loadLoyaltyData()
    } catch (error: any) {
      toast({
        title: "Redemption Failed",
        description: error.message || "Failed to redeem reward",
        variant: "destructive",
      })
    } finally {
      setRedeeming(null)
    }
  }

  if (loading) {
    return (
      <div className="container py-8 max-w-4xl flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  if (!accountDetails) {
    return (
      <div className="container py-8 max-w-4xl">
        <Card className="card-glass border-none">
          <CardContent className="pt-6 text-center">
            <p className="text-muted-foreground mb-4">Failed to load loyalty account</p>
            <Button onClick={loadLoyaltyData}>
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  const currentPoints = accountDetails.account.pointsBalance
  const tierName = accountDetails.account.tierName
  const nextReward = AVAILABLE_REWARDS.find((r) => r.pointsRequired > currentPoints)
  const pointsToNext = nextReward ? nextReward.pointsRequired - currentPoints : 0
  const progressToNext = nextReward
    ? (currentPoints / nextReward.pointsRequired) * 100
    : 100

  // Get recent transactions (earned points)
  const recentActivity = accountDetails.recentTransactions
    .filter(t => t.changeAmount > 0)
    .slice(0, 5)
    .map(t => ({
      date: new Date(t.createdAt).toLocaleDateString(),
      description: t.reason,
      points: `+${t.changeAmount}`,
    }))

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
              <Badge variant="outline" className="mt-2">
                {tierName}
              </Badge>
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
          {AVAILABLE_REWARDS.map((reward) => {
            const canRedeem = currentPoints >= reward.pointsRequired
            const isRedeeming = redeeming === reward.id
            
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
                    <Button 
                      disabled={!canRedeem || isRedeeming} 
                      size="sm"
                      onClick={() => handleRedeem(reward.id, reward.pointsRequired, reward.name)}
                    >
                      {isRedeeming && <Loader2 className="h-4 w-4 animate-spin mr-2" />}
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
          {recentActivity.length === 0 ? (
            <p className="text-center text-muted-foreground py-4">
              No activity yet. Start ordering to earn points!
            </p>
          ) : (
            <div className="space-y-4">
              {recentActivity.map((activity, index) => (
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
                  {index < recentActivity.length - 1 && <Separator className="mt-4" />}
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Redemption History */}
      {accountDetails.recentRedemptions.length > 0 && (
        <Card className="card-glass border-none mt-8">
          <CardHeader>
            <CardTitle>Redemption History</CardTitle>
            <CardDescription>Your redeemed rewards</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {accountDetails.recentRedemptions.slice(0, 5).map((redemption, index) => {
                const metadata = JSON.parse(redemption.rewardMetadata || '{}')
                return (
                  <div key={redemption.id}>
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-medium">
                          {metadata.rewardName || redemption.rewardType}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {new Date(redemption.createdAt).toLocaleDateString()}
                        </p>
                      </div>
                      <div className="text-lg font-bold text-destructive">
                        -{redemption.pointsUsed}
                      </div>
                    </div>
                    {index < accountDetails.recentRedemptions.length - 1 && <Separator className="mt-4" />}
                  </div>
                )
              })}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}


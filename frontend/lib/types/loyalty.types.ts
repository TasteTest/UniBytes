/**
 * Loyalty System Type Definitions
 */

export enum LoyaltyTier {
  Bronze = 0,
  Silver = 1,
  Gold = 2,
  Platinum = 3,
}

export interface LoyaltyAccount {
  id: string
  userId: string
  pointsBalance: number
  tier: LoyaltyTier
  tierName: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface LoyaltyTransaction {
  id: string
  loyaltyAccountId: string
  changeAmount: number
  reason: string
  referenceId: string | null
  metadata: string
  createdAt: string
}

export interface LoyaltyRedemption {
  id: string
  loyaltyAccountId: string
  pointsUsed: number
  rewardType: string
  rewardMetadata: string
  createdAt: string
}

export interface LoyaltyAccountDetails {
  account: LoyaltyAccount
  recentTransactions: LoyaltyTransaction[]
  recentRedemptions: LoyaltyRedemption[]
  totalPointsEarned: number
  totalPointsRedeemed: number
}

// Request types
export interface CreateLoyaltyAccountRequest {
  userId: string
  pointsBalance?: number
  tier?: number
  isActive?: boolean
}

export interface UpdateLoyaltyAccountRequest {
  pointsBalance?: number
  tier?: number
  isActive?: boolean
}

export interface AddPointsRequest {
  userId: string
  points: number
  reason: string
  referenceId?: string
  metadata?: string
}

export interface RedeemPointsRequest {
  userId: string
  points: number
  rewardType: string
  rewardMetadata?: Record<string, any>
}

// Frontend-specific types
export interface Reward {
  id: string
  name: string
  description: string
  pointsRequired: number
  rewardType: string
  available: boolean
  imageUrl?: string
  metadata?: Record<string, any>
}

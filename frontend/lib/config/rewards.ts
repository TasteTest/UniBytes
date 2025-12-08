/**
 * Rewards Configuration
 * Define available rewards and their point requirements
 */

import { Reward } from '../types/loyalty.types'

export const AVAILABLE_REWARDS: Reward[] = [
  {
    id: 'reward-free-cookie',
    name: 'Free Cookie',
    description: 'Get a free chocolate chip cookie',
    pointsRequired: 50,
    rewardType: 'MenuItem',
    available: true,
  },
  {
    id: 'reward-free-drink',
    name: 'Free Drink',
    description: 'Any size fountain drink',
    pointsRequired: 100,
    rewardType: 'MenuItem',
    available: true,
  },
  {
    id: 'reward-5-off',
    name: '$5 Off',
    description: '$5 off your next order',
    pointsRequired: 200,
    rewardType: 'Discount',
    available: true,
    metadata: {
      discountAmount: 5,
      discountType: 'fixed',
    },
  },
  {
    id: 'reward-free-meal',
    name: 'Free Meal',
    description: 'Any menu item up to $15',
    pointsRequired: 500,
    rewardType: 'MenuItem',
    available: true,
    metadata: {
      maxValue: 15,
    },
  },
  {
    id: 'reward-10-percent-off',
    name: '10% Off',
    description: '10% off your entire order',
    pointsRequired: 300,
    rewardType: 'Discount',
    available: true,
    metadata: {
      discountPercent: 10,
      discountType: 'percentage',
    },
  },
  {
    id: 'reward-free-dessert',
    name: 'Free Dessert',
    description: 'Any dessert item',
    pointsRequired: 150,
    rewardType: 'MenuItem',
    available: true,
  },
]

/**
 * Points earning rates
 */
export const POINTS_EARNING_RATE = {
  pointsPerDollar: 10,
  bonusMultipliers: {
    Bronze: 1,
    Silver: 1.25,
    Gold: 1.5,
    Platinum: 2,
  },
}

/**
 * Tier thresholds (total lifetime points earned)
 */
export const TIER_THRESHOLDS = {
  Bronze: 0,
  Silver: 1000,
  Gold: 5000,
  Platinum: 10000,
}

/**
 * Calculate points earned for a purchase
 */
export function calculatePointsEarned(amount: number, tierMultiplier: number = 1): number {
  return Math.floor(amount * POINTS_EARNING_RATE.pointsPerDollar * tierMultiplier)
}

/**
 * Get next tier threshold
 */
export function getNextTierThreshold(currentPoints: number): { tier: string; threshold: number } | null {
  const tiers = Object.entries(TIER_THRESHOLDS).sort((a, b) => a[1] - b[1])
  
  for (const [tier, threshold] of tiers) {
    if (currentPoints < threshold) {
      return { tier, threshold }
    }
  }
  
  return null // Already at max tier
}

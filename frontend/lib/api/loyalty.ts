/**
 * Loyalty API Service
 */

import { ApiClient, apiClient } from './client'
import { endpoints } from './endpoints'
import { ApiResponse } from '../types/common.types'
import {
  LoyaltyAccount,
  LoyaltyAccountDetails,
  CreateLoyaltyAccountRequest,
  UpdateLoyaltyAccountRequest,
  AddPointsRequest,
  RedeemPointsRequest,
  LoyaltyRedemption,
  LoyaltyTier,
} from '../types/loyalty.types'

// Use the shared API client (which can have auth headers set)
const loyaltyClient = apiClient

export class LoyaltyService {
  /**
   * Get loyalty account by user ID
   */
  async getByUserId(userId: string): Promise<ApiResponse<LoyaltyAccount>> {
    return loyaltyClient.get<LoyaltyAccount>(endpoints.loyalty.byUserId(userId))
  }

  /**
   * Get complete account details with history
   */
  async getAccountDetails(userId: string): Promise<ApiResponse<LoyaltyAccountDetails>> {
    return loyaltyClient.get<LoyaltyAccountDetails>(endpoints.loyalty.details(userId))
  }

  /**
   * Get user's points balance
   */
  async getBalance(userId: string): Promise<ApiResponse<number>> {
    return loyaltyClient.get<number>(endpoints.loyalty.balance(userId))
  }

  /**
   * Get all loyalty accounts
   */
  async getAll(): Promise<ApiResponse<LoyaltyAccount[]>> {
    return loyaltyClient.get<LoyaltyAccount[]>(endpoints.loyalty.base)
  }

  /**
   * Get active loyalty accounts
   */
  async getActive(): Promise<ApiResponse<LoyaltyAccount[]>> {
    return loyaltyClient.get<LoyaltyAccount[]>(endpoints.loyalty.active)
  }

  /**
   * Get accounts by tier
   */
  async getByTier(tier: LoyaltyTier): Promise<ApiResponse<LoyaltyAccount[]>> {
    return loyaltyClient.get<LoyaltyAccount[]>(endpoints.loyalty.byTier(tier))
  }

  /**
   * Create a new loyalty account
   */
  async create(request: CreateLoyaltyAccountRequest): Promise<ApiResponse<LoyaltyAccount>> {
    return loyaltyClient.post<LoyaltyAccount>(endpoints.loyalty.base, request)
  }

  /**
   * Update loyalty account
   */
  async update(id: string, request: UpdateLoyaltyAccountRequest): Promise<ApiResponse<LoyaltyAccount>> {
    return loyaltyClient.put<LoyaltyAccount>(endpoints.loyalty.byId(id), request)
  }

  /**
   * Delete loyalty account
   */
  async delete(id: string): Promise<ApiResponse<void>> {
    return loyaltyClient.delete<void>(endpoints.loyalty.byId(id))
  }

  /**
   * Add points to user's account
   */
  async addPoints(request: AddPointsRequest): Promise<ApiResponse<LoyaltyAccount>> {
    return loyaltyClient.post<LoyaltyAccount>(endpoints.loyalty.addPoints, request)
  }

  /**
   * Redeem points for a reward
   */
  async redeemPoints(request: RedeemPointsRequest): Promise<ApiResponse<LoyaltyRedemption>> {
    // Convert metadata object to JSON string if present
    const body = {
      ...request,
      rewardMetadata: request.rewardMetadata
        ? JSON.stringify(request.rewardMetadata)
        : '{}'
    }
    return loyaltyClient.post<LoyaltyRedemption>(endpoints.loyalty.redeemPoints, body)
  }

  /**
   * Get or create loyalty account for user
   */
  async getOrCreateAccount(userId: string): Promise<ApiResponse<LoyaltyAccount>> {
    const existing = await this.getByUserId(userId)

    if (existing.isSuccess) {
      return existing
    }

    // Account doesn't exist, create it
    return this.create({ userId })
  }

  /**
   * Get tier name from tier enum
   */
  getTierName(tier: LoyaltyTier): string {
    const tierNames: Record<LoyaltyTier, string> = {
      [LoyaltyTier.Bronze]: 'Bronze',
      [LoyaltyTier.Silver]: 'Silver',
      [LoyaltyTier.Gold]: 'Gold',
      [LoyaltyTier.Platinum]: 'Platinum',
    }
    return tierNames[tier] || 'Bronze'
  }

  /**
   * Get tier color for UI
   */
  getTierColor(tier: LoyaltyTier): string {
    const colors: Record<LoyaltyTier, string> = {
      [LoyaltyTier.Bronze]: 'from-orange-600 to-orange-800',
      [LoyaltyTier.Silver]: 'from-gray-400 to-gray-600',
      [LoyaltyTier.Gold]: 'from-yellow-500 to-yellow-700',
      [LoyaltyTier.Platinum]: 'from-purple-500 to-purple-700',
    }
    return colors[tier] || colors[LoyaltyTier.Bronze]
  }
}

// Export singleton instance
export const loyaltyService = new LoyaltyService()

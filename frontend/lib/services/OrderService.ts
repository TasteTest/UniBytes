/**
 * Order Service Implementation
 * Handles order creation and management
 */

import { Result } from '../types/common.types'
import { getSession } from "next-auth/react"

const API_URL = process.env.NEXT_PUBLIC_API_URL || ''

export interface CreateOrderItemRequest {
    menuItemId?: string
    name: string
    unitPrice: number
    quantity: number
    modifiers?: any
    isReward?: boolean
    rewardId?: string
}

export interface CreateOrderRequest {
    userId: string
    orderItems: CreateOrderItemRequest[]
    currency?: string
    metadata?: Record<string, any>
}

export interface OrderResponse {
    id: string
    userId: string
    status: number
    totalAmount: number
    currency: string
    createdAt: string
}

export class OrderService {
    async createFreeOrder(request: CreateOrderRequest): Promise<Result<OrderResponse>> {
        try {
            if (!API_URL) {
                return { isSuccess: false, error: 'API base URL not configured' }
            }

            const session = await getSession()
            if (!session?.accessToken) {
                return { isSuccess: false, error: 'User not authenticated' }
            }

            const response = await fetch(`${API_URL}/orders`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${session.accessToken}`,
                    'X-User-Email': session.user?.email || ''
                },
                body: JSON.stringify(request),
            })

            const responseText = await response.text()

            if (!response.ok) {
                console.error("Order API Error:", responseText)
                try {
                    const errorJson = JSON.parse(responseText)
                    return { isSuccess: false, error: errorJson.error || errorJson.detail || 'Failed to create order' }
                } catch {
                    return { isSuccess: false, error: responseText }
                }
            }

            const data: OrderResponse = JSON.parse(responseText)
            return {
                isSuccess: true,
                data
            }
        } catch (error) {
            return {
                isSuccess: false,
                error: error instanceof Error ? error.message : 'Failed to create order'
            }
        }
    }
}

// Singleton instance
export const orderService = new OrderService()

/**
 * Payment Service Implementation
 * Contains business logic for payment operations
 */

import { Result } from '../types/common.types'
import { CreateCheckoutSessionRequest, CheckoutSessionResponse, Payment } from '../types/payment.types'

const PAYMENT_API_URL = process.env.NEXT_PUBLIC_PAYMENT_API_URL || 'http://localhost:5001/api'

export class PaymentService {
  async createCheckoutSession(request: CreateCheckoutSessionRequest): Promise<Result<CheckoutSessionResponse>> {
    try {
      const response = await fetch(`${PAYMENT_API_URL}/payments/checkout-session`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      })

      if (!response.ok) {
        const error = await response.json()
        return {
          isSuccess: false,
          error: error.error || 'Failed to create checkout session'
        }
      }

      const data: CheckoutSessionResponse = await response.json()
      return {
        isSuccess: true,
        data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to create checkout session'
      }
    }
  }

  async verifyPayment(sessionId: string): Promise<Result<Payment>> {
    try {
      const response = await fetch(`${PAYMENT_API_URL}/payments/verify/${sessionId}`)

      if (!response.ok) {
        const error = await response.json()
        return {
          isSuccess: false,
          error: error.error || 'Failed to verify payment'
        }
      }

      const data: Payment = await response.json()
      return {
        isSuccess: true,
        data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to verify payment'
      }
    }
  }

  async getPaymentByOrderId(orderId: string): Promise<Result<Payment>> {
    try {
      const response = await fetch(`${PAYMENT_API_URL}/payments/order/${orderId}`)

      if (!response.ok) {
        const error = await response.json()
        return {
          isSuccess: false,
          error: error.error || 'Failed to get payment'
        }
      }

      const data: Payment = await response.json()
      return {
        isSuccess: true,
        data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to get payment'
      }
    }
  }
}

// Singleton instance
export const paymentService = new PaymentService()


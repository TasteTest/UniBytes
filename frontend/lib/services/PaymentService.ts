/**
 * Payment Service Implementation
 * Contains business logic for payment operations
 */

import { Result } from '../types/common.types'
import { CreateCheckoutSessionRequest, CheckoutSessionResponse, Payment } from '../types/payment.types'
import { getSession } from "next-auth/react";

const PAYMENT_API_URL = process.env.NEXT_PUBLIC_PAYMENT_API_URL || process.env.NEXT_PUBLIC_API_URL || ''

export class PaymentService {
  async createCheckoutSession(request: CreateCheckoutSessionRequest): Promise<Result<CheckoutSessionResponse>> {
    try {
      if (!PAYMENT_API_URL) {
        return { isSuccess: false, error: 'Payment API base URL not configured' }
      }
      const session = await getSession();
      if (!session?.accessToken) {
        return { isSuccess: false, error: 'User not authenticated' };
      }

      console.log(`Connecting to: ${PAYMENT_API_URL}/payments/checkout-session`);

      const response = await fetch(`${PAYMENT_API_URL}/payments/checkout-session`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${session.accessToken}`,
          'X-User-Email': session.user?.email || ''
        },
        body: JSON.stringify(request),
      })

      const responseText = await response.text();

      if (!response.ok) {
        console.error("Payment API Error:", responseText);
        try {
          const errorJson = JSON.parse(responseText);
          return { isSuccess: false, error: errorJson.detail || 'Payment failed' };
        } catch {
          return { isSuccess: false, error: responseText };
        }
      }

      const data: CheckoutSessionResponse = JSON.parse(responseText);
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
      if (!PAYMENT_API_URL) {
        return { isSuccess: false, error: 'Payment API base URL not configured' }
      }
      const session = await getSession();
      const response = await fetch(`${PAYMENT_API_URL}/payments/verify/${sessionId}`, {
        headers: {
          'Authorization': `Bearer ${session?.accessToken || ''}`,
          'X-User-Email': session?.user?.email || ''
        }
      })

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
      if (!PAYMENT_API_URL) {
        return { isSuccess: false, error: 'Payment API base URL not configured' }
      }
      const session = await getSession();
      const response = await fetch(`${PAYMENT_API_URL}/payments/order/${orderId}`, {
        headers: {
          'Authorization': `Bearer ${session?.accessToken || ''}`,
          'X-User-Email': session?.user?.email || ''
        }
      })

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


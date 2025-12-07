/**
 * Payment-related types
 */

export interface CheckoutLineItem {
  menuItemId?: string
  name: string
  description?: string
  unitPrice: number
  quantity: number
  imageUrl?: string
  modifiers?: any
}

export interface CreateCheckoutSessionRequest {
  orderId: string
  accessToken: string
  userEmail: string
  lineItems: CheckoutLineItem[]
  successUrl: string
  cancelUrl: string
  idempotencyKey?: string
  metadata?: any
}

export interface CheckoutSessionResponse {
  sessionId: string
  sessionUrl: string
  paymentId: string
  message: string
}

export enum PaymentStatus {
  Processing = 0,
  Succeeded = 1,
  Failed = 2,
  Refunded = 3,
  Cancelled = 4
}

export enum PaymentProvider {
  Stripe = 0,
  Mock = 1
}

export interface Payment {
  id: string
  orderId?: string
  userId?: string
  amount: number
  currency: string
  provider: PaymentProvider
  providerPaymentId?: string
  status: PaymentStatus
  failureMessage?: string
  createdAt: string
  updatedAt: string
}


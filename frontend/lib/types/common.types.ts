/**
 * Common types used across the application
 */

export interface ApiResponse<T> {
  data?: T
  error?: string
  errors?: string[]
  isSuccess: boolean
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  hasMore: boolean
}

export interface RequestConfig {
  headers?: Record<string, string>
  params?: Record<string, string | number>
}

export enum OAuthProviderType {
  Google = 0,
  GitHub = 1,
  LinkedIn = 2,
  Facebook = 3
}

export interface Result<T> {
  isSuccess: boolean
  data?: T
  error?: string
  errors?: string[]
}


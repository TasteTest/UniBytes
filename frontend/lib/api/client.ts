/**
 * HTTP Client Configuration
 */

import { ApiResponse, RequestConfig } from '../types/common.types'

export class ApiClient {
  private readonly baseUrl: string
  private readonly defaultHeaders: HeadersInit

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || process.env.NEXT_PUBLIC_API_URL || ''
    this.defaultHeaders = {
      'Content-Type': 'application/json',
    }
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {},
    config?: RequestConfig
  ): Promise<ApiResponse<T>> {
    try {
      if (!this.baseUrl) {
        return {
          isSuccess: false,
          error: 'API base URL not configured (set NEXT_PUBLIC_API_URL)'
        }
      }
      const url = new URL(`${this.baseUrl}${endpoint}`)
      
      // Add query parameters
      if (config?.params) {
        Object.entries(config.params).forEach(([key, value]) => {
          url.searchParams.append(key, String(value))
        })
      }

      const headers = {
        ...this.defaultHeaders,
        ...config?.headers,
        ...options.headers,
      }

      const response = await fetch(url.toString(), {
        ...options,
        headers,
      })

      // Handle different response types
      const contentType = response.headers.get('content-type')
      let data: any

      if (contentType?.includes('application/json')) {
        data = await response.json()
      } else if (response.status === 204) {
        // No content
        data = null
      } else {
        data = await response.text()
      }

      if (!response.ok) {
        return {
          isSuccess: false,
          error: typeof data === 'string' ? data : data?.error || data?.message || 'Request failed',
          errors: data?.errors
        }
      }

      return {
        isSuccess: true,
        data: data as T
      }
    } catch (error) {
      console.error('API Request Error:', error)
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Network error occurred'
      }
    }
  }

  async get<T>(endpoint: string, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'GET' }, config)
  }

  async post<T>(endpoint: string, body?: any, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>(
      endpoint,
      {
        method: 'POST',
        body: body ? JSON.stringify(body) : undefined,
      },
      config
    )
  }

  async put<T>(endpoint: string, body?: any, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>(
      endpoint,
      {
        method: 'PUT',
        body: body ? JSON.stringify(body) : undefined,
      },
      config
    )
  }

  async delete<T>(endpoint: string, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.request<T>(endpoint, { method: 'DELETE' }, config)
  }

  setAuthToken(token: string): void {
    (this.defaultHeaders as Record<string, string>)['Authorization'] = `Bearer ${token}`
  }

  removeAuthToken(): void {
    delete (this.defaultHeaders as Record<string, string>)['Authorization']
  }
}

// Singleton instance
export const apiClient = new ApiClient()


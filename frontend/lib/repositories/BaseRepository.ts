/**
 * Base Repository Implementation
 * Provides common CRUD operations
 */

import { ApiClient } from '../api/client'
import { ApiResponse, RequestConfig } from '../types/common.types'
import { IRepository } from './interfaces/IRepository'

export abstract class BaseRepository<T, TCreate, TUpdate> implements IRepository<T, TCreate, TUpdate> {
  protected readonly client: ApiClient
  protected readonly baseEndpoint: string

  constructor(client: ApiClient, baseEndpoint: string) {
    this.client = client
    this.baseEndpoint = baseEndpoint
  }

  async getAll(config?: RequestConfig): Promise<ApiResponse<T[]>> {
    return this.client.get<T[]>(this.baseEndpoint, config)
  }

  async getById(id: string, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.client.get<T>(`${this.baseEndpoint}/${id}`, config)
  }

  async create(data: TCreate, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.client.post<T>(this.baseEndpoint, data, config)
  }

  async update(id: string, data: TUpdate, config?: RequestConfig): Promise<ApiResponse<T>> {
    return this.client.put<T>(`${this.baseEndpoint}/${id}`, data, config)
  }

  async delete(id: string, config?: RequestConfig): Promise<ApiResponse<void>> {
    return this.client.delete<void>(`${this.baseEndpoint}/${id}`, config)
  }
}


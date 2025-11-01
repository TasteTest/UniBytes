/**
 * Generic Repository Interface
 * Following Interface Segregation Principle
 */

import { ApiResponse, RequestConfig } from '../../types/common.types'

export interface IRepository<T, TCreate, TUpdate> {
  getAll(config?: RequestConfig): Promise<ApiResponse<T[]>>
  getById(id: string, config?: RequestConfig): Promise<ApiResponse<T>>
  create(data: TCreate, config?: RequestConfig): Promise<ApiResponse<T>>
  update(id: string, data: TUpdate, config?: RequestConfig): Promise<ApiResponse<T>>
  delete(id: string, config?: RequestConfig): Promise<ApiResponse<void>>
}


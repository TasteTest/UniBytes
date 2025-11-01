/**
 * User Service Interface
 * Defines business logic contract for user operations
 */

import { Result } from '../../types/common.types'
import { User, CreateUserRequest, UpdateUserRequest } from '../../types/user.types'

export interface IUserService {
  getCurrentUser(email: string): Promise<Result<User>>
  getAllUsers(): Promise<Result<User[]>>
  getActiveUsers(): Promise<Result<User[]>>
  createUser(data: CreateUserRequest): Promise<Result<User>>
  updateUser(id: string, data: UpdateUserRequest): Promise<Result<User>>
  deleteUser(id: string): Promise<Result<void>>
  updateLastLogin(id: string): Promise<Result<void>>
}


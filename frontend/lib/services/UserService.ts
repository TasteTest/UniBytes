/**
 * User Service Implementation
 * Contains business logic for user operations
 */

import { Result } from '../types/common.types'
import { User, CreateUserRequest, UpdateUserRequest } from '../types/user.types'
import { IUserRepository } from '../repositories/interfaces/IUserRepository'
import { IUserService } from './interfaces/IUserService'

export class UserService implements IUserService {
  private readonly repository: IUserRepository

  constructor(repository: IUserRepository) {
    this.repository = repository
  }

  async getCurrentUser(email: string): Promise<Result<User>> {
    try {
      const response = await this.repository.getByEmail(email)
      
      if (!response.isSuccess || !response.data) {
        return {
          isSuccess: false,
          error: response.error || 'User not found'
        }
      }

      return {
        isSuccess: true,
        data: response.data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to get current user'
      }
    }
  }

  async getAllUsers(): Promise<Result<User[]>> {
    try {
      const response = await this.repository.getAll()
      
      if (!response.isSuccess) {
        return {
          isSuccess: false,
          error: response.error || 'Failed to fetch users'
        }
      }

      return {
        isSuccess: true,
        data: response.data || []
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to get all users'
      }
    }
  }

  async getActiveUsers(): Promise<Result<User[]>> {
    try {
      const response = await this.repository.getActiveUsers()
      
      if (!response.isSuccess) {
        return {
          isSuccess: false,
          error: response.error || 'Failed to fetch active users'
        }
      }

      return {
        isSuccess: true,
        data: response.data || []
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to get active users'
      }
    }
  }

  async createUser(data: CreateUserRequest): Promise<Result<User>> {
    try {
      // Validation
      if (!data.email || !this.isValidEmail(data.email)) {
        return {
          isSuccess: false,
          error: 'Invalid email address'
        }
      }

      const response = await this.repository.create(data)
      
      if (!response.isSuccess || !response.data) {
        return {
          isSuccess: false,
          error: response.error || 'Failed to create user'
        }
      }

      return {
        isSuccess: true,
        data: response.data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to create user'
      }
    }
  }

  async updateUser(id: string, data: UpdateUserRequest): Promise<Result<User>> {
    try {
      // Validation
      if (data.email && !this.isValidEmail(data.email)) {
        return {
          isSuccess: false,
          error: 'Invalid email address'
        }
      }

      const response = await this.repository.update(id, data)
      
      if (!response.isSuccess || !response.data) {
        return {
          isSuccess: false,
          error: response.error || 'Failed to update user'
        }
      }

      return {
        isSuccess: true,
        data: response.data
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to update user'
      }
    }
  }

  async deleteUser(id: string): Promise<Result<void>> {
    try {
      const response = await this.repository.delete(id)
      
      if (!response.isSuccess) {
        return {
          isSuccess: false,
          error: response.error || 'Failed to delete user'
        }
      }

      return {
        isSuccess: true
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to delete user'
      }
    }
  }

  async updateLastLogin(id: string): Promise<Result<void>> {
    try {
      const response = await this.repository.updateLastLogin(id)
      
      if (!response.isSuccess) {
        return {
          isSuccess: false,
          error: response.error || 'Failed to update last login'
        }
      }

      return {
        isSuccess: true
      }
    } catch (error) {
      return {
        isSuccess: false,
        error: error instanceof Error ? error.message : 'Failed to update last login'
      }
    }
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  }
}


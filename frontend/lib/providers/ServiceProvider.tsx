/**
 * Service Provider Context
 * Implements Dependency Injection for services
 * Following Dependency Inversion Principle
 */

'use client'

import React, { createContext, useContext, useMemo } from 'react'
import { apiClient } from '../api/client'
import { UserRepository } from '../repositories/UserRepository'
import { AuthRepository } from '../repositories/AuthRepository'
import { UserService } from '../services/UserService'
import { AuthService } from '../services/AuthService'
import { IUserService } from '../services/interfaces/IUserService'
import { IAuthService } from '../services/interfaces/IAuthService'

interface ServiceContextValue {
  userService: IUserService
  authService: IAuthService
}

const ServiceContext = createContext<ServiceContextValue | null>(null)

interface ServiceProviderProps {
  children: React.ReactNode
}

export const ServiceProvider: React.FC<ServiceProviderProps> = ({ children }) => {
  const services = useMemo(() => {
    // Initialize repositories
    const userRepository = new UserRepository(apiClient)
    const authRepository = new AuthRepository(apiClient)

    // Initialize services with their dependencies
    const userService = new UserService(userRepository)
    const authService = new AuthService(authRepository)

    return {
      userService,
      authService,
    }
  }, [])

  return (
    <ServiceContext.Provider value={services}>
      {children}
    </ServiceContext.Provider>
  )
}

/**
 * Hook to access services
 * @throws Error if used outside ServiceProvider
 */
export const useServices = (): ServiceContextValue => {
  const context = useContext(ServiceContext)
  
  if (!context) {
    throw new Error('useServices must be used within a ServiceProvider')
  }
  
  return context
}

/**
 * Individual service hooks for convenience
 */
export const useUserService = (): IUserService => {
  const { userService } = useServices()
  return userService
}

export const useAuthService = (): IAuthService => {
  const { authService } = useServices()
  return authService
}


import { authApi, getErrorMessage } from './api'
import type {
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  ApiResponse,
  User,
} from '@/types/auth'

class AuthService {
  async register(data: RegisterRequest): Promise<LoginResponse> {
    try {
      const response = await authApi.post<ApiResponse<LoginResponse>>(
        '/api/auth/register',
        data
      )

      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.error || 'Registration failed')
      }

      const authData = response.data.data
      this.setAuthData(authData)
      return authData
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  async login(data: LoginRequest): Promise<LoginResponse> {
    try {
      const response = await authApi.post<ApiResponse<LoginResponse>>(
        '/api/auth/login',
        data
      )

      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.error || 'Login failed')
      }

      const authData = response.data.data
      this.setAuthData(authData)
      return authData
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  async logout(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem('refreshToken')
      if (refreshToken) {
        await authApi.post('/api/auth/logout', { refreshToken })
      }
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      this.clearAuthData()
    }
  }

  async refreshToken(): Promise<LoginResponse> {
    try {
      const refreshToken = localStorage.getItem('refreshToken')
      if (!refreshToken) {
        throw new Error('No refresh token available')
      }

      const response = await authApi.post<ApiResponse<LoginResponse>>(
        '/api/auth/refresh',
        { refreshToken }
      )

      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.error || 'Token refresh failed')
      }

      const authData = response.data.data
      this.setAuthData(authData)
      return authData
    } catch (error) {
      this.clearAuthData()
      throw new Error(getErrorMessage(error))
    }
  }

  async getCurrentUser(): Promise<User> {
    try {
      const response = await authApi.get<ApiResponse<User>>('/api/auth/me')

      if (!response.data.success || !response.data.data) {
        throw new Error(response.data.error || 'Failed to get user data')
      }

      return response.data.data
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    try {
      const response = await authApi.post<ApiResponse<void>>(
        '/api/auth/change-password',
        { currentPassword, newPassword }
      )

      if (!response.data.success) {
        throw new Error(response.data.error || 'Password change failed')
      }
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  async forgotPassword(email: string): Promise<void> {
    try {
      const response = await authApi.post<ApiResponse<void>>(
        '/api/auth/forgot-password',
        { email }
      )

      if (!response.data.success) {
        throw new Error(response.data.error || 'Password reset request failed')
      }
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  async resetPassword(token: string, newPassword: string): Promise<void> {
    try {
      const response = await authApi.post<ApiResponse<void>>(
        '/api/auth/reset-password',
        { token, newPassword }
      )

      if (!response.data.success) {
        throw new Error(response.data.error || 'Password reset failed')
      }
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  async verifyEmail(token: string): Promise<void> {
    try {
      const response = await authApi.post<ApiResponse<void>>(
        '/api/auth/verify-email',
        { token }
      )

      if (!response.data.success) {
        throw new Error(response.data.error || 'Email verification failed')
      }
    } catch (error) {
      throw new Error(getErrorMessage(error))
    }
  }

  private setAuthData(data: LoginResponse): void {
    localStorage.setItem('accessToken', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    localStorage.setItem('user', JSON.stringify({
      userId: data.userId,
      username: data.username,
      email: data.email,
      roles: data.roles,
    }))
  }

  private clearAuthData(): void {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  getStoredUser(): User | null {
    if (typeof window === 'undefined') return null
    const userStr = localStorage.getItem('user')
    if (!userStr) return null
    try {
      return JSON.parse(userStr)
    } catch {
      return null
    }
  }

  getStoredToken(): string | null {
    if (typeof window === 'undefined') return null
    return localStorage.getItem('accessToken')
  }

  isAuthenticated(): boolean {
    return !!this.getStoredToken()
  }
}

export const authService = new AuthService()

// Auth Types matching backend API responses

export interface LoginRequest {
  emailOrUsername: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  phoneNumber?: string
}

export interface LoginResponse {
  userId: string
  username: string
  email: string
  accessToken: string
  refreshToken: string
  expiresAt: string
  roles: string[]
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: string
  timestamp: string
}

export interface User {
  userId: string
  username: string
  email: string
  phoneNumber?: string
  roles: string[]
  isEmailVerified: boolean
  isPhoneVerified: boolean
  isLocked: boolean
  lastLoginAt?: string
  createdAt: string
}

export interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

import axios, { AxiosInstance, AxiosError } from 'axios'
import type { ApiResponse } from '@/types/auth'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'
const AUTH_API_URL = process.env.NEXT_PUBLIC_AUTH_API || 'http://localhost:5001'

// Create axios instance for general API
export const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Create axios instance for auth API
export const authApi: AxiosInstance = axios.create({
  baseURL: AUTH_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor to add token
const requestInterceptor = (config: any) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('accessToken')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
  }
  return config
}

api.interceptors.request.use(requestInterceptor)
authApi.interceptors.request.use(requestInterceptor)

// Response interceptor to handle token refresh
const responseInterceptor = async (error: AxiosError) => {
  const originalRequest = error.config as any

  if (error.response?.status === 401 && !originalRequest._retry) {
    originalRequest._retry = true

    try {
      const refreshToken = localStorage.getItem('refreshToken')
      if (!refreshToken) {
        throw new Error('No refresh token available')
      }

      const response = await authApi.post<ApiResponse<{ accessToken: string; refreshToken: string }>>(
        '/api/auth/refresh',
        { refreshToken }
      )

      if (response.data.success && response.data.data) {
        const { accessToken, refreshToken: newRefreshToken } = response.data.data
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', newRefreshToken)

        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return axios(originalRequest)
      }
    } catch (refreshError) {
      // Refresh failed, logout user
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
      if (typeof window !== 'undefined') {
        window.location.href = '/auth/signin'
      }
      return Promise.reject(refreshError)
    }
  }

  return Promise.reject(error)
}

api.interceptors.response.use((response) => response, responseInterceptor)
authApi.interceptors.response.use((response) => response, responseInterceptor)

// Helper to handle API errors
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const apiError = error.response?.data as ApiResponse<any>
    return apiError?.error || error.message || 'An error occurred'
  }
  if (error instanceof Error) {
    return error.message
  }
  return 'An unknown error occurred'
}

'use client'

import { Provider } from 'react-redux'
import { store } from './index'
import { useEffect } from 'react'
import { initializeAuth } from './authSlice'

export function ReduxProvider({ children }: { children: React.ReactNode }) {
  useEffect(() => {
    // Initialize auth state from localStorage on mount
    store.dispatch(initializeAuth())
  }, [])

  return <Provider store={store}>{children}</Provider>
}

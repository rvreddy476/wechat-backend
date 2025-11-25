// Type definitions for the home feed

export interface User {
  id: string
  name: string
  username: string
  avatar: string
  bio?: string
}

export interface Post {
  id: string
  author: User
  content: string
  image?: string
  timestamp: Date
  likes: number
  comments: number
  shares: number
  isLiked: boolean
}

export interface Friend {
  id: string
  name: string
  username: string
  avatar: string
  isOnline: boolean
  lastSeen?: Date
}

export interface Message {
  id: string
  senderId: string
  text: string
  timestamp: Date
}

export interface ChatWindow {
  friendId: string
  isMinimized: boolean
  messages: Message[]
}

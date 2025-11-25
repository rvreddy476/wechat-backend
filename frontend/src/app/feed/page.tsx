'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import toast from 'react-hot-toast'
import { LeftSidebar } from '@/components/home/LeftSidebar'
import { FeedColumn } from '@/components/home/FeedColumn'
import { RightSidebar } from '@/components/home/RightSidebar'
import { ChatWindowsBar } from '@/components/home/ChatWindowsBar'
import { currentUser, mockPosts, mockFriends, getMockMessages } from '@/lib/mockData'
import type { Post, ChatWindow } from '@/types/feed'
import { useAppSelector, useAppDispatch } from '@/store'
import { logout } from '@/store/authSlice'

export default function HomePage() {
  const router = useRouter()
  const dispatch = useAppDispatch()
  const { user: authUser, isAuthenticated } = useAppSelector((state) => state.auth)

  // State
  const [posts, setPosts] = useState<Post[]>(mockPosts)
  const [chatWindows, setChatWindows] = useState<ChatWindow[]>([])

  // Handlers
  const handleLogout = async () => {
    try {
      await dispatch(logout()).unwrap()
      toast.success('Logged out successfully')
      router.push('/auth/signin')
    } catch (error) {
      toast.error('Failed to logout')
    }
  }

  const handlePost = (content: string) => {
    const newPost: Post = {
      id: Date.now().toString(),
      author: currentUser,
      content,
      timestamp: new Date(),
      likes: 0,
      comments: 0,
      shares: 0,
      isLiked: false,
    }
    setPosts([newPost, ...posts])
    toast.success('Post created!')
  }

  const handleLike = (postId: string) => {
    setPosts(
      posts.map((post) =>
        post.id === postId
          ? {
              ...post,
              likes: post.isLiked ? post.likes - 1 : post.likes + 1,
              isLiked: !post.isLiked,
            }
          : post
      )
    )
  }

  const handleComment = (postId: string) => {
    toast('Comments coming soon! 💬')
  }

  const handleShare = (postId: string) => {
    toast('Share functionality coming soon! 📤')
  }

  const handleFriendClick = (friendId: string) => {
    // Check if chat window already exists
    const existingChat = chatWindows.find((chat) => chat.friendId === friendId)

    if (existingChat) {
      // If minimized, maximize it
      if (existingChat.isMinimized) {
        setChatWindows(
          chatWindows.map((chat) =>
            chat.friendId === friendId ? { ...chat, isMinimized: false } : chat
          )
        )
      }
      return
    }

    // Create new chat window (max 3 windows)
    if (chatWindows.length >= 3) {
      toast.error('Maximum 3 chat windows allowed')
      return
    }

    const newChat: ChatWindow = {
      friendId,
      isMinimized: false,
      messages: getMockMessages(friendId, currentUser.id),
    }

    setChatWindows([...chatWindows, newChat])
  }

  const handleCloseChat = (friendId: string) => {
    setChatWindows(chatWindows.filter((chat) => chat.friendId !== friendId))
  }

  const handleMinimizeChat = (friendId: string) => {
    setChatWindows(
      chatWindows.map((chat) =>
        chat.friendId === friendId ? { ...chat, isMinimized: !chat.isMinimized } : chat
      )
    )
  }

  const handleSendMessage = (friendId: string, text: string) => {
    setChatWindows(
      chatWindows.map((chat) => {
        if (chat.friendId === friendId) {
          return {
            ...chat,
            messages: [
              ...chat.messages,
              {
                id: Date.now().toString(),
                senderId: currentUser.id,
                text,
                timestamp: new Date(),
              },
            ],
          }
        }
        return chat
      })
    )

    // Simulate friend reply after 2 seconds
    setTimeout(() => {
      setChatWindows((prev) =>
        prev.map((chat) => {
          if (chat.friendId === friendId) {
            return {
              ...chat,
              messages: [
                ...chat.messages,
                {
                  id: (Date.now() + 1).toString(),
                  senderId: friendId,
                  text: 'Thanks for your message! 😊',
                  timestamp: new Date(),
                },
              ],
            }
          }
          return chat
        })
      )
    }, 2000)
  }

  return (
    <div className="min-h-screen bg-background">
      <div className="max-w-[1400px] mx-auto">
        <div className="grid grid-cols-12 gap-6">
          {/* Left Sidebar - 2 columns */}
          <div className="col-span-12 lg:col-span-2 xl:col-span-2">
            <LeftSidebar user={currentUser} onLogout={handleLogout} />
          </div>

          {/* Feed Column - 6 columns */}
          <div className="col-span-12 lg:col-span-6 xl:col-span-6 py-6">
            <FeedColumn
              user={currentUser}
              posts={posts}
              onPost={handlePost}
              onLike={handleLike}
              onComment={handleComment}
              onShare={handleShare}
            />
          </div>

          {/* Right Sidebar - 4 columns */}
          <div className="col-span-12 lg:col-span-4 xl:col-span-4">
            <RightSidebar friends={mockFriends} onFriendClick={handleFriendClick} />
          </div>
        </div>
      </div>

      {/* Chat Windows */}
      <ChatWindowsBar
        chatWindows={chatWindows}
        friends={mockFriends}
        currentUserId={currentUser.id}
        onClose={handleCloseChat}
        onMinimize={handleMinimizeChat}
        onSendMessage={handleSendMessage}
      />
    </div>
  )
}

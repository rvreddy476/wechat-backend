'use client'

import { useState, useRef, useEffect } from 'react'
import Image from 'next/image'
import { Minus, X, Send } from 'lucide-react'
import type { Friend, Message } from '@/types/feed'

interface ChatWindowProps {
  friend: Friend
  messages: Message[]
  isMinimized: boolean
  currentUserId: string
  onClose: () => void
  onMinimize: () => void
  onSendMessage: (text: string) => void
}

export function ChatWindow({
  friend,
  messages,
  isMinimized,
  currentUserId,
  onClose,
  onMinimize,
  onSendMessage,
}: ChatWindowProps) {
  const [inputText, setInputText] = useState('')
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const handleSend = () => {
    if (inputText.trim()) {
      onSendMessage(inputText)
      setInputText('')
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  return (
    <div className="w-80 bg-white rounded-t-lg shadow-2xl border border-gray-200 flex flex-col">
      {/* Header */}
      <div
        className="flex items-center justify-between p-3 border-b border-gray-200 bg-gradient-to-r from-primary/5 to-purple-50 rounded-t-lg cursor-pointer"
        onClick={onMinimize}
      >
        <div className="flex items-center gap-2 flex-1 min-w-0">
          <div className="relative">
            <Image
              src={friend.avatar}
              alt={friend.name}
              width={32}
              height={32}
              className="rounded-full"
            />
            {friend.isOnline && (
              <div className="absolute bottom-0 right-0 w-2.5 h-2.5 bg-green-500 border-2 border-white rounded-full" />
            )}
          </div>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-sm truncate">{friend.name}</p>
            <p className="text-xs text-gray-500">
              {friend.isOnline ? 'Online' : 'Offline'}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-1">
          <button
            onClick={(e) => {
              e.stopPropagation()
              onMinimize()
            }}
            className="p-1 hover:bg-white/50 rounded transition-colors"
          >
            <Minus className="w-4 h-4 text-gray-600" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation()
              onClose()
            }}
            className="p-1 hover:bg-white/50 rounded transition-colors"
          >
            <X className="w-4 h-4 text-gray-600" />
          </button>
        </div>
      </div>

      {/* Messages */}
      {!isMinimized && (
        <>
          <div className="flex-1 overflow-y-auto p-4 space-y-3 h-96 bg-gray-50">
            {messages.map((message) => {
              const isMe = message.senderId === currentUserId
              return (
                <div
                  key={message.id}
                  className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}
                >
                  <div
                    className={`max-w-[70%] rounded-lg px-4 py-2 ${
                      isMe
                        ? 'bg-primary text-white'
                        : 'bg-white border border-gray-200 text-gray-800'
                    }`}
                  >
                    <p className="text-sm">{message.text}</p>
                    <p
                      className={`text-xs mt-1 ${
                        isMe ? 'text-white/70' : 'text-gray-500'
                      }`}
                    >
                      {message.timestamp.toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </p>
                  </div>
                </div>
              )
            })}
            <div ref={messagesEndRef} />
          </div>

          {/* Input */}
          <div className="p-3 border-t border-gray-200 bg-white">
            <div className="flex items-center gap-2">
              <input
                type="text"
                value={inputText}
                onChange={(e) => setInputText(e.target.value)}
                onKeyPress={handleKeyPress}
                placeholder="Type a message..."
                className="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
              />
              <button
                onClick={handleSend}
                disabled={!inputText.trim()}
                className="p-2 bg-primary text-white rounded-lg hover:bg-primary-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                <Send className="w-4 h-4" />
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}

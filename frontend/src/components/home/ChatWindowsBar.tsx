'use client'

import type { Friend, Message, ChatWindow as ChatWindowType } from '@/types/feed'
import { ChatWindow } from './ChatWindow'

interface ChatWindowsBarProps {
  chatWindows: ChatWindowType[]
  friends: Friend[]
  currentUserId: string
  onClose: (friendId: string) => void
  onMinimize: (friendId: string) => void
  onSendMessage: (friendId: string, text: string) => void
}

export function ChatWindowsBar({
  chatWindows,
  friends,
  currentUserId,
  onClose,
  onMinimize,
  onSendMessage,
}: ChatWindowsBarProps) {
  if (chatWindows.length === 0) return null

  return (
    <div className="fixed bottom-0 right-4 z-50 flex gap-2 items-end">
      {chatWindows.map((chat) => {
        const friend = friends.find((f) => f.id === chat.friendId)
        if (!friend) return null

        return (
          <ChatWindow
            key={chat.friendId}
            friend={friend}
            messages={chat.messages}
            isMinimized={chat.isMinimized}
            currentUserId={currentUserId}
            onClose={() => onClose(chat.friendId)}
            onMinimize={() => onMinimize(chat.friendId)}
            onSendMessage={(text) => onSendMessage(chat.friendId, text)}
          />
        )
      })}
    </div>
  )
}

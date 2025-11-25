'use client'

import Image from 'next/image'
import type { Friend } from '@/types/feed'
import { formatRelativeTime } from '@/lib/utils'

interface RightSidebarProps {
  friends: Friend[]
  onFriendClick?: (friendId: string) => void
}

export function RightSidebar({ friends, onFriendClick }: RightSidebarProps) {
  const onlineFriends = friends.filter((f) => f.isOnline)
  const offlineFriends = friends.filter((f) => !f.isOnline)

  return (
    <div className="sticky top-0 h-screen overflow-y-auto px-4 py-6">
      {/* Ads Banner */}
      <div className="mb-6 bg-gradient-to-br from-primary/10 to-purple-100 rounded-lg p-6 text-center">
        <h3 className="font-bold text-lg mb-2">WeChat Premium</h3>
        <p className="text-sm text-gray-600 mb-4">
          Unlock exclusive features and ad-free experience
        </p>
        <button className="bg-primary text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">
          Upgrade Now
        </button>
      </div>

      {/* Online Friends */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
        <h3 className="font-semibold text-sm mb-3 text-gray-800">
          Friends Online ({onlineFriends.length})
        </h3>

        <div className="space-y-2">
          {onlineFriends.map((friend) => (
            <button
              key={friend.id}
              onClick={() => onFriendClick?.(friend.id)}
              className="flex items-center gap-3 w-full p-2 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <div className="relative">
                <Image
                  src={friend.avatar}
                  alt={friend.name}
                  width={36}
                  height={36}
                  className="rounded-full"
                />
                <div className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 border-2 border-white rounded-full" />
              </div>
              <div className="flex-1 min-w-0 text-left">
                <p className="font-medium text-sm truncate">{friend.name}</p>
                <p className="text-xs text-green-600">Online</p>
              </div>
            </button>
          ))}
        </div>

        {offlineFriends.length > 0 && (
          <>
            <div className="my-3 border-t border-gray-100" />
            <h3 className="font-semibold text-sm mb-3 text-gray-500">
              Offline ({offlineFriends.length})
            </h3>
            <div className="space-y-2">
              {offlineFriends.map((friend) => (
                <button
                  key={friend.id}
                  onClick={() => onFriendClick?.(friend.id)}
                  className="flex items-center gap-3 w-full p-2 rounded-lg hover:bg-gray-50 transition-colors opacity-60"
                >
                  <div className="relative">
                    <Image
                      src={friend.avatar}
                      alt={friend.name}
                      width={36}
                      height={36}
                      className="rounded-full grayscale"
                    />
                  </div>
                  <div className="flex-1 min-w-0 text-left">
                    <p className="font-medium text-sm truncate">{friend.name}</p>
                    <p className="text-xs text-gray-500">
                      {friend.lastSeen
                        ? formatRelativeTime(friend.lastSeen)
                        : 'Offline'}
                    </p>
                  </div>
                </button>
              ))}
            </div>
          </>
        )}
      </div>

      {/* Suggestions */}
      <div className="mt-4 bg-white rounded-lg shadow-sm border border-gray-200 p-4">
        <h3 className="font-semibold text-sm mb-3 text-gray-800">
          Suggested for you
        </h3>
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex items-center gap-3">
              <Image
                src={`https://ui-avatars.com/api/?name=User${i}&background=random`}
                alt={`User ${i}`}
                width={32}
                height={32}
                className="rounded-full"
              />
              <div className="flex-1 min-w-0">
                <p className="font-medium text-sm truncate">User {i}</p>
                <p className="text-xs text-gray-500">Followed by 2 friends</p>
              </div>
              <button className="text-primary text-xs font-medium hover:underline">
                Follow
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

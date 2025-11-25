'use client'

import Image from 'next/image'
import Link from 'next/link'
import {
  Home,
  Compass,
  Video,
  MessageCircle,
  Bell,
  Bookmark,
  Settings,
  LogOut
} from 'lucide-react'
import type { User } from '@/types/feed'

interface LeftSidebarProps {
  user: User
  onLogout?: () => void
}

const menuItems = [
  { icon: Home, label: 'Home', href: '/feed', active: true },
  { icon: Compass, label: 'Explore', href: '/explore', active: false },
  { icon: Video, label: 'Reels', href: '/reels', active: false },
  { icon: MessageCircle, label: 'Messages', href: '/messages', active: false },
  { icon: Bell, label: 'Notifications', href: '/notifications', active: false },
  { icon: Bookmark, label: 'Bookmarks', href: '/bookmarks', active: false },
  { icon: Settings, label: 'Settings', href: '/settings', active: false },
]

export function LeftSidebar({ user, onLogout }: LeftSidebarProps) {
  return (
    <div className="sticky top-0 h-screen overflow-y-auto px-4 py-6">
      {/* User Profile */}
      <Link
        href="/profile"
        className="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-100 transition-colors mb-6"
      >
        <div className="relative">
          <Image
            src={user.avatar}
            alt={user.name}
            width={48}
            height={48}
            className="rounded-full"
          />
          <div className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 border-2 border-white rounded-full" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="font-semibold text-sm truncate">{user.name}</p>
          <p className="text-xs text-gray-500 truncate">@{user.username}</p>
        </div>
      </Link>

      {/* Navigation Menu */}
      <nav className="space-y-1">
        {menuItems.map((item) => (
          <Link
            key={item.label}
            href={item.href}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              item.active
                ? 'bg-primary/10 text-primary font-semibold'
                : 'hover:bg-gray-100 text-gray-700'
            }`}
          >
            <item.icon className="w-5 h-5" />
            <span className="text-sm">{item.label}</span>
          </Link>
        ))}
      </nav>

      {/* Logout Button */}
      <button
        onClick={onLogout}
        className="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-red-50 text-red-600 transition-colors w-full mt-6"
      >
        <LogOut className="w-5 h-5" />
        <span className="text-sm font-medium">Logout</span>
      </button>

      {/* User Bio */}
      {user.bio && (
        <div className="mt-6 p-4 bg-gray-50 rounded-lg">
          <p className="text-xs text-gray-600">{user.bio}</p>
        </div>
      )}
    </div>
  )
}

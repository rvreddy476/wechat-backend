'use client'

import { useState } from 'react'
import Image from 'next/image'
import { Heart, MessageCircle, Share2, Bookmark } from 'lucide-react'
import type { Post } from '@/types/feed'
import { formatRelativeTime } from '@/lib/utils'

interface FeedCardProps {
  post: Post
  onLike?: (postId: string) => void
  onComment?: (postId: string) => void
  onShare?: (postId: string) => void
}

export function FeedCard({ post, onLike, onComment, onShare }: FeedCardProps) {
  const [isLiked, setIsLiked] = useState(post.isLiked)
  const [likes, setLikes] = useState(post.likes)

  const handleLike = () => {
    setIsLiked(!isLiked)
    setLikes(isLiked ? likes - 1 : likes + 1)
    onLike?.(post.id)
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 mb-4 overflow-hidden">
      {/* Header */}
      <div className="p-4 flex items-center gap-3">
        <Image
          src={post.author.avatar}
          alt={post.author.name}
          width={40}
          height={40}
          className="rounded-full"
        />
        <div className="flex-1 min-w-0">
          <p className="font-semibold text-sm">{post.author.name}</p>
          <p className="text-xs text-gray-500">
            @{post.author.username} · {formatRelativeTime(post.timestamp)}
          </p>
        </div>
        <button className="text-gray-400 hover:text-gray-600">
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <circle cx="12" cy="5" r="2" />
            <circle cx="12" cy="12" r="2" />
            <circle cx="12" cy="19" r="2" />
          </svg>
        </button>
      </div>

      {/* Content */}
      <div className="px-4 pb-3">
        <p className="text-sm text-gray-800 whitespace-pre-wrap">{post.content}</p>
      </div>

      {/* Image */}
      {post.image && (
        <div className="relative w-full aspect-video bg-gray-100">
          <Image
            src={post.image}
            alt="Post image"
            fill
            className="object-cover"
          />
        </div>
      )}

      {/* Actions */}
      <div className="p-4 flex items-center justify-between border-t border-gray-100">
        <button
          onClick={handleLike}
          className={`flex items-center gap-2 px-3 py-2 rounded-lg transition-colors ${
            isLiked
              ? 'text-primary bg-primary/5'
              : 'text-gray-600 hover:bg-gray-100'
          }`}
        >
          <Heart
            className={`w-5 h-5 ${isLiked ? 'fill-current' : ''}`}
          />
          <span className="text-sm font-medium">{likes}</span>
        </button>

        <button
          onClick={() => onComment?.(post.id)}
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors"
        >
          <MessageCircle className="w-5 h-5" />
          <span className="text-sm font-medium">{post.comments}</span>
        </button>

        <button
          onClick={() => onShare?.(post.id)}
          className="flex items-center gap-2 px-3 py-2 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors"
        >
          <Share2 className="w-5 h-5" />
          <span className="text-sm font-medium">{post.shares}</span>
        </button>

        <button className="p-2 rounded-lg text-gray-600 hover:bg-gray-100 transition-colors">
          <Bookmark className="w-5 h-5" />
        </button>
      </div>
    </div>
  )
}

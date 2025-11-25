'use client'

import { useState } from 'react'
import Image from 'next/image'
import { ImageIcon, Smile, Send } from 'lucide-react'
import type { User } from '@/types/feed'
import { Button } from '@/components/ui/button'

interface PostComposerProps {
  user: User
  onPost?: (content: string) => void
}

export function PostComposer({ user, onPost }: PostComposerProps) {
  const [content, setContent] = useState('')

  const handlePost = () => {
    if (content.trim()) {
      onPost?.(content)
      setContent('')
    }
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-4">
      <div className="flex gap-3">
        <Image
          src={user.avatar}
          alt={user.name}
          width={40}
          height={40}
          className="rounded-full"
        />
        <div className="flex-1">
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="What's on your mind?"
            className="w-full resize-none border-none outline-none text-sm placeholder:text-gray-400 min-h-[60px]"
            rows={2}
          />

          <div className="flex items-center justify-between mt-3 pt-3 border-t border-gray-100">
            <div className="flex items-center gap-2">
              <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors text-gray-600">
                <ImageIcon className="w-5 h-5" />
              </button>
              <button className="p-2 hover:bg-gray-100 rounded-lg transition-colors text-gray-600">
                <Smile className="w-5 h-5" />
              </button>
            </div>

            <Button
              onClick={handlePost}
              disabled={!content.trim()}
              size="sm"
              className="gap-2"
            >
              <Send className="w-4 h-4" />
              Post
            </Button>
          </div>
        </div>
      </div>
    </div>
  )
}

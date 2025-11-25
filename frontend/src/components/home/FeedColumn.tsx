'use client'

import type { Post, User } from '@/types/feed'
import { PostComposer } from './PostComposer'
import { FeedCard } from './FeedCard'

interface FeedColumnProps {
  user: User
  posts: Post[]
  onPost?: (content: string) => void
  onLike?: (postId: string) => void
  onComment?: (postId: string) => void
  onShare?: (postId: string) => void
}

export function FeedColumn({
  user,
  posts,
  onPost,
  onLike,
  onComment,
  onShare,
}: FeedColumnProps) {
  return (
    <div className="max-w-2xl mx-auto">
      <PostComposer user={user} onPost={onPost} />

      <div className="space-y-0">
        {posts.map((post) => (
          <FeedCard
            key={post.id}
            post={post}
            onLike={onLike}
            onComment={onComment}
            onShare={onShare}
          />
        ))}
      </div>
    </div>
  )
}

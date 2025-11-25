import type { Post, Friend, User } from '@/types/feed'

// Current user
export const currentUser: User = {
  id: '1',
  name: 'John Doe',
  username: 'johndoe',
  avatar: 'https://ui-avatars.com/api/?name=John+Doe&background=E30C4D&color=fff',
  bio: 'Software Developer | Tech Enthusiast',
}

// Mock posts
export const mockPosts: Post[] = [
  {
    id: '1',
    author: {
      id: '2',
      name: 'Sarah Wilson',
      username: 'sarahw',
      avatar: 'https://ui-avatars.com/api/?name=Sarah+Wilson&background=0066FF&color=fff',
    },
    content: 'Just launched my new portfolio website! 🚀 Check it out and let me know what you think. Built with Next.js and Tailwind CSS.',
    image: 'https://images.unsplash.com/photo-1498050108023-c5249f4df085?w=800&h=400&fit=crop',
    timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
    likes: 124,
    comments: 18,
    shares: 7,
    isLiked: false,
  },
  {
    id: '2',
    author: {
      id: '3',
      name: 'Mike Chen',
      username: 'mikechen',
      avatar: 'https://ui-avatars.com/api/?name=Mike+Chen&background=10B981&color=fff',
    },
    content: 'Beautiful sunset at the beach today 🌅 Nature never fails to amaze me!',
    image: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800&h=400&fit=crop',
    timestamp: new Date(Date.now() - 1000 * 60 * 120), // 2 hours ago
    likes: 342,
    comments: 45,
    shares: 12,
    isLiked: true,
  },
  {
    id: '3',
    author: {
      id: '4',
      name: 'Emma Davis',
      username: 'emmad',
      avatar: 'https://ui-avatars.com/api/?name=Emma+Davis&background=7C3AED&color=fff',
    },
    content: 'Pro tip for developers: Take breaks! Your code will thank you later. 💻☕',
    timestamp: new Date(Date.now() - 1000 * 60 * 240), // 4 hours ago
    likes: 89,
    comments: 12,
    shares: 5,
    isLiked: false,
  },
  {
    id: '4',
    author: {
      id: '5',
      name: 'Alex Johnson',
      username: 'alexj',
      avatar: 'https://ui-avatars.com/api/?name=Alex+Johnson&background=F59E0B&color=fff',
    },
    content: 'Excited to announce that I\'m starting a new position as Senior Frontend Developer at TechCorp! 🎉',
    timestamp: new Date(Date.now() - 1000 * 60 * 480), // 8 hours ago
    likes: 567,
    comments: 89,
    shares: 23,
    isLiked: true,
  },
  {
    id: '5',
    author: {
      id: '6',
      name: 'Lisa Anderson',
      username: 'lisaa',
      avatar: 'https://ui-avatars.com/api/?name=Lisa+Anderson&background=EC4899&color=fff',
    },
    content: 'Working on an exciting new project with an amazing team. Can\'t wait to share more soon! 🔥',
    image: 'https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800&h=400&fit=crop',
    timestamp: new Date(Date.now() - 1000 * 60 * 720), // 12 hours ago
    likes: 234,
    comments: 34,
    shares: 15,
    isLiked: false,
  },
]

// Mock online friends
export const mockFriends: Friend[] = [
  {
    id: '2',
    name: 'Sarah Wilson',
    username: 'sarahw',
    avatar: 'https://ui-avatars.com/api/?name=Sarah+Wilson&background=0066FF&color=fff',
    isOnline: true,
  },
  {
    id: '3',
    name: 'Mike Chen',
    username: 'mikechen',
    avatar: 'https://ui-avatars.com/api/?name=Mike+Chen&background=10B981&color=fff',
    isOnline: true,
  },
  {
    id: '4',
    name: 'Emma Davis',
    username: 'emmad',
    avatar: 'https://ui-avatars.com/api/?name=Emma+Davis&background=7C3AED&color=fff',
    isOnline: false,
    lastSeen: new Date(Date.now() - 1000 * 60 * 15), // 15 minutes ago
  },
  {
    id: '5',
    name: 'Alex Johnson',
    username: 'alexj',
    avatar: 'https://ui-avatars.com/api/?name=Alex+Johnson&background=F59E0B&color=fff',
    isOnline: true,
  },
  {
    id: '6',
    name: 'Lisa Anderson',
    username: 'lisaa',
    avatar: 'https://ui-avatars.com/api/?name=Lisa+Anderson&background=EC4899&color=fff',
    isOnline: false,
    lastSeen: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
  },
  {
    id: '7',
    name: 'David Brown',
    username: 'davidb',
    avatar: 'https://ui-avatars.com/api/?name=David+Brown&background=EF4444&color=fff',
    isOnline: true,
  },
  {
    id: '8',
    name: 'Sophie Taylor',
    username: 'sophiet',
    avatar: 'https://ui-avatars.com/api/?name=Sophie+Taylor&background=06B6D4&color=fff',
    isOnline: false,
    lastSeen: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
  },
]

// Mock messages for chat
export const getMockMessages = (friendId: string, currentUserId: string) => [
  {
    id: '1',
    senderId: friendId,
    text: 'Hey! How are you?',
    timestamp: new Date(Date.now() - 1000 * 60 * 10),
  },
  {
    id: '2',
    senderId: currentUserId,
    text: 'Hi! I\'m doing great, thanks! How about you?',
    timestamp: new Date(Date.now() - 1000 * 60 * 9),
  },
  {
    id: '3',
    senderId: friendId,
    text: 'Pretty good! Working on something exciting 🚀',
    timestamp: new Date(Date.now() - 1000 * 60 * 8),
  },
  {
    id: '4',
    senderId: currentUserId,
    text: 'That sounds awesome! Tell me more about it',
    timestamp: new Date(Date.now() - 1000 * 60 * 7),
  },
]

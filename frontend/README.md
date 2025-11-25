# WeChat Frontend

Modern, elegant social media frontend built with Next.js 14, React, TypeScript, and Tailwind CSS.

## 🎨 Design System

### Brand Colors
- **Primary Button Color**: `#E30C4D` (rgb(227,12,77))
- **Background Color**: `#EAE5DE` (rgb(234,229,222)) - Light mode
- **Dark Background**: `#0F172A` - Dark mode

### Tech Stack
- **Framework**: Next.js 14 (App Router)
- **UI Library**: React 18 + TypeScript
- **Styling**: Tailwind CSS + shadcn/ui
- **State Management**: Redux Toolkit
- **Real-time**: SignalR Client
- **Forms**: React Hook Form + Zod
- **Animations**: Framer Motion
- **Icons**: Lucide React
- **Notifications**: React Hot Toast

## 📁 Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── auth/
│   │   │   ├── signin/page.tsx       # Sign In page
│   │   │   ├── signup/page.tsx       # Sign Up page
│   │   │   └── layout.tsx            # Auth layout
│   │   ├── globals.css               # Global styles
│   │   ├── layout.tsx                # Root layout
│   │   └── page.tsx                  # Home page (redirects)
│   ├── components/
│   │   └── ui/                       # shadcn/ui components
│   │       ├── button.tsx
│   │       ├── input.tsx
│   │       ├── label.tsx
│   │       ├── card.tsx
│   │       └── spinner.tsx
│   ├── lib/
│   │   └── utils.ts                  # Utility functions
│   ├── services/
│   │   ├── api.ts                    # Axios instances
│   │   └── authService.ts            # Auth API service
│   ├── store/
│   │   ├── index.ts                  # Redux store
│   │   ├── authSlice.ts              # Auth state
│   │   └── Provider.tsx              # Redux provider
│   ├── types/
│   │   └── auth.ts                   # TypeScript types
│   └── hooks/                        # Custom hooks (future)
├── public/                           # Static assets
├── .env.example                      # Environment variables template
├── components.json                   # shadcn/ui config
├── next.config.js                    # Next.js config
├── tailwind.config.ts                # Tailwind config
├── tsconfig.json                     # TypeScript config
└── package.json                      # Dependencies
```

## 🚀 Getting Started

### Prerequisites
- Node.js 18+ and npm/yarn/pnpm
- Backend services running (see backend README)

### Installation

1. **Install dependencies**:
```bash
cd frontend
npm install
# or
yarn install
# or
pnpm install
```

2. **Configure environment variables**:
```bash
cp .env.example .env
```

Edit `.env`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_AUTH_API=http://localhost:5001
```

3. **Run development server**:
```bash
npm run dev
# or
yarn dev
# or
pnpm dev
```

4. **Open browser**:
Navigate to [http://localhost:3000](http://localhost:3000)

## 🎯 Features

### ✅ Implemented
- **Authentication**
  - Beautiful Sign Up page with validation
  - Sign In page with "Remember Me"
  - Full form validation (React Hook Form + Zod)
  - Password strength requirements
  - Eye icon to show/hide passwords
  - Animated transitions (Framer Motion)

- **State Management**
  - Redux Toolkit for global state
  - Auth state persistence (localStorage)
  - Automatic token refresh
  - Auto-login on page reload

- **API Integration**
  - Axios interceptors for auth
  - Automatic token refresh on 401
  - Error handling
  - Type-safe API calls

- **UI Components** (shadcn/ui)
  - Button (with brand color #E30C4D)
  - Input (with icons)
  - Label
  - Card
  - Spinner/Loader

- **Design Features**
  - Responsive design (mobile-first)
  - Custom brand colors
  - Smooth animations
  - Toast notifications
  - Dark mode support (CSS variables ready)

### 🔜 Coming Next
- Home/Feed page
- User Profile
- Post creation
- Chat interface
- Video feed
- Notifications panel
- Settings page

## 🔐 Authentication Flow

```
1. User fills Sign Up/Sign In form
   ↓
2. Form validation (Zod schema)
   ↓
3. Submit to backend API
   ↓
4. Receive JWT tokens (access + refresh)
   ↓
5. Store in localStorage + Redux
   ↓
6. Redirect to home page
   ↓
7. Auto token refresh on expiry
```

## 🎨 UI Showcase

### Sign Up Page
- Username (3-20 chars, alphanumeric + underscore)
- Email (valid email format)
- Phone Number (optional)
- Password (min 8 chars, uppercase, lowercase, number, special char)
- Confirm Password (must match)
- Animated form fields (slide-in)
- Real-time validation errors
- Loading state with spinner

### Sign In Page
- Email or Username
- Password with show/hide toggle
- "Remember Me" checkbox
- "Forgot Password?" link
- Social login buttons (Google, GitHub) - UI ready
- Smooth animations
- Error toast notifications

## 🛠️ Development

### Available Scripts

```bash
# Development server
npm run dev

# Build for production
npm run build

# Start production server
npm run start

# Run linter
npm run lint
```

### Adding New Components

Use shadcn/ui CLI (when needed):
```bash
npx shadcn-ui@latest add [component-name]
```

### Code Style
- TypeScript strict mode
- Functional components with hooks
- Server components by default (use 'use client' when needed)
- Tailwind CSS for styling
- Custom brand colors in tailwind.config.ts

## 📡 API Endpoints Used

### Auth Service (http://localhost:5001)
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh access token
- `GET /api/auth/me` - Get current user
- `POST /api/auth/change-password` - Change password
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token
- `POST /api/auth/verify-email` - Verify email with token

## 🎭 Animation Patterns

Using Framer Motion for smooth transitions:

```tsx
// Fade in
initial={{ opacity: 0 }}
animate={{ opacity: 1 }}

// Slide in from left
initial={{ x: -20, opacity: 0 }}
animate={{ x: 0, opacity: 1 }}

// Scale in (for titles)
initial={{ scale: 0.5 }}
animate={{ scale: 1 }}
transition={{ type: 'spring', stiffness: 200 }}
```

## 🔒 Security Features

- JWT token storage in localStorage
- Automatic token refresh
- HTTP-only cookies (future)
- CSRF protection (future)
- XSS prevention (React default)
- Form validation on client & server
- Password strength enforcement
- Rate limiting (backend)

## 🌈 Theming

The app supports light and dark modes via CSS variables:

```css
/* Light mode */
--background: 234 229 222 (#EAE5DE)
--primary: 227 12 77 (#E30C4D)

/* Dark mode */
--background: 15 23 42 (#0F172A)
--primary: 227 12 77 (same)
```

## 📱 Responsive Breakpoints

```
sm:  640px   (Mobile landscape)
md:  768px   (Tablet)
lg:  1024px  (Desktop)
xl:  1280px  (Large desktop)
2xl: 1536px  (Extra large)
```

## 🐛 Troubleshooting

### Port already in use
```bash
# Kill process on port 3000
lsof -ti:3000 | xargs kill -9
```

### Module not found
```bash
# Clear cache and reinstall
rm -rf node_modules .next
npm install
```

### TypeScript errors
```bash
# Regenerate types
rm -rf .next
npm run dev
```

## 🚢 Deployment

### Vercel (Recommended)
```bash
npm install -g vercel
vercel
```

### Docker
```bash
# Build
docker build -t wechat-frontend .

# Run
docker run -p 3000:3000 wechat-frontend
```

### Environment Variables for Production
```env
NEXT_PUBLIC_API_URL=https://api.yourdo main.com
NEXT_PUBLIC_AUTH_API=https://auth.yourdomain.com
```

## 📄 License

MIT

## 🤝 Contributing

1. Create feature branch
2. Make changes
3. Test thoroughly
4. Submit pull request

---

**Built with ❤️ using Next.js 14 and the brand color #E30C4D**

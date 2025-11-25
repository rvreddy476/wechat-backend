/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    domains: ['localhost', 'storage.googleapis.com'], // Add your GCP storage domain
  },
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
    NEXT_PUBLIC_AUTH_API: process.env.NEXT_PUBLIC_AUTH_API || 'http://localhost:5001',
  },
}

module.exports = nextConfig

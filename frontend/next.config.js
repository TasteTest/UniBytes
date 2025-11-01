/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  images: {
    domains: ['lh3.googleusercontent.com', 'api.dicebear.com'],
  },
}

module.exports = nextConfig


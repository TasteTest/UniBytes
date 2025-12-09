/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  images: {
    domains: ['lh3.googleusercontent.com', 'api.dicebear.com', 'unibytesstorage.blob.core.windows.net'],
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'unibytesstorage.blob.core.windows.net',
      },
    ],
  },
}

module.exports = nextConfig


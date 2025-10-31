# CampusEats Frontend

A modern, accessible, and responsive cafeteria ordering platform built with Next.js, shadcn/ui, and Tailwind CSS.

## ✨ Features

- 🎨 **Modern Design**: Glassmorphism, subtle gradients, and smooth animations
- 🌓 **Dark Mode**: Full light/dark mode support
- 📱 **Responsive**: Mobile-first design that works on all devices
- ♿ **Accessible**: WCAG AA compliant with keyboard navigation and ARIA labels
- 🔐 **Secure Auth**: Google sign-in only via NextAuth
- 🛒 **Smart Cart**: Persistent cart with modifiers and special instructions
- 🏆 **Loyalty Program**: Points-based rewards system
- 👨‍🍳 **Kitchen Dashboard**: Real-time order management for kitchen staff
- 👑 **Admin Panel**: Complete menu management interface

## 🚀 Tech Stack

- **Framework**: Next.js 14 (App Router, React Server Components)
- **UI Library**: shadcn/ui components
- **Styling**: Tailwind CSS with custom design tokens
- **Authentication**: NextAuth.js (Google OAuth)
- **State Management**: Zustand for cart state
- **Icons**: Lucide React
- **Animations**: Tailwind CSS animations + Framer Motion
- **Type Safety**: TypeScript

## 📦 Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd frontend
```

2. Install dependencies:
```bash
npm install
# or
yarn install
# or
pnpm install
```

3. Set up environment variables:
```bash
cp .env.example .env
```

4. Configure your `.env` file:
```env
GOOGLE_CLIENT_ID=your_google_client_id_here
GOOGLE_CLIENT_SECRET=your_google_client_secret_here
NEXTAUTH_SECRET=your_nextauth_secret_here
NEXTAUTH_URL=http://localhost:3000
NEXT_PUBLIC_API_URL=http://localhost:8000/api
```

5. Run the development server:
```bash
npm run dev
# or
yarn dev
# or
pnpm dev
```

6. Open [http://localhost:3000](http://localhost:3000) in your browser.

## 🎨 Design System

### Color Palette

- **Primary**: Saffron/Orange (`#f97316`) - Warm, inviting accent color
- **Background**: Light slate for light mode, deep slate for dark mode
- **Text**: High contrast for readability (WCAG AA compliant)

### Design Tokens

All design tokens are defined in `tailwind.config.ts` and can be customized:

- Colors: HSL-based for easy theming
- Spacing: Consistent scale for padding/margins
- Radii: Rounded corners (`--radius` variable)
- Shadows: Elevation system for depth

### Typography

- Font: Inter (system font fallback)
- Scale: Based on Tailwind's default scale
- Weights: 400 (normal), 500 (medium), 600 (semibold), 700 (bold)

## 📱 Pages & Routes

### Public Pages
- `/` - Landing page with features overview
- `/auth/signin` - Google sign-in page
- `/menu` - Browse menu items with search and filtering

### User Pages (Authenticated)
- `/cart` - Shopping cart with quantity management
- `/checkout` - Multi-step checkout flow
- `/orders` - Active and past orders with status tracking
- `/loyalty` - Rewards program and points tracking
- `/profile` - User profile and preferences

### Staff Pages
- `/kitchen` - Kitchen dashboard for order management
- `/admin` - Admin panel for menu CRUD operations

## 🔌 API Integration

The frontend expects the following API endpoints:

### Authentication
```
POST /api/auth/signin
POST /api/auth/signout
GET  /api/auth/session
```

### Menu
```
GET    /api/menu/items
GET    /api/menu/items/:id
POST   /api/menu/items (admin)
PUT    /api/menu/items/:id (admin)
DELETE /api/menu/items/:id (admin)
GET    /api/menu/categories
```

### Orders
```
POST   /api/orders
GET    /api/orders
GET    /api/orders/:id
PATCH  /api/orders/:id/status
```

### Loyalty
```
GET    /api/loyalty/points
GET    /api/loyalty/rewards
POST   /api/loyalty/redeem
GET    /api/loyalty/history
```

### Users
```
GET    /api/users/profile
PATCH  /api/users/profile
GET    /api/users/payment-methods
```

### Sample API Response Formats

#### Menu Item
```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "price": 0.00,
  "category": "string",
  "image": "string (optional)",
  "available": true,
  "preparationTime": 0
}
```

#### Order
```json
{
  "id": "string",
  "userId": "string",
  "items": [
    {
      "menuItemId": "string",
      "quantity": 0,
      "modifiers": ["string"],
      "specialInstructions": "string (optional)"
    }
  ],
  "status": "pending|preparing|ready|completed|cancelled",
  "total": 0.00,
  "pickupLocation": "string",
  "pickupTime": "ISO 8601",
  "createdAt": "ISO 8601"
}
```

## 🧩 Component Library

All components follow shadcn/ui conventions and are located in `components/ui/`:

- `button` - Primary interaction element
- `card` - Container with header, content, footer
- `dialog` - Modal dialogs
- `sheet` - Side panels (cart, mobile menu)
- `toast` - Notifications
- `tabs` - Tabbed interfaces
- `badge` - Labels and status indicators
- `avatar` - User profile images
- `skeleton` - Loading states
- `progress` - Progress indicators
- `input`, `select`, `checkbox`, `radio-group` - Form elements

### Custom Utilities

#### Glassmorphism
```tsx
<Card className="card-glass">
  {/* Content with frosted glass effect */}
</Card>
```

#### Hover Effects
```tsx
<Card className="hover-lift">
  {/* Card lifts on hover */}
</Card>
```

#### Gradient Text
```tsx
<h1 className="text-gradient">
  Gradient Text
</h1>
```

## 🎭 Theming

### Switching Themes
```tsx
import { useTheme } from "next-themes"

const { theme, setTheme } = useTheme()
setTheme("dark") // or "light" or "system"
```

### Custom Theme Colors
Edit `app/globals.css` to customize theme colors:

```css
:root {
  --primary: 24 95% 53%; /* Saffron */
  --background: 0 0% 100%;
  /* ... */
}

.dark {
  --primary: 24 95% 53%;
  --background: 222.2 84% 4.9%;
  /* ... */
}
```

## 🔒 Authentication Setup

### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URIs:
   - `http://localhost:3000/api/auth/callback/google` (development)
   - `https://yourdomain.com/api/auth/callback/google` (production)
6. Copy Client ID and Secret to `.env`

### Generating NextAuth Secret
```bash
openssl rand -base64 32
```

## 🏗️ Project Structure

```
frontend/
├── app/                    # Next.js app directory
│   ├── (routes)/          # Page routes
│   ├── api/               # API routes
│   ├── globals.css        # Global styles
│   └── layout.tsx         # Root layout
├── components/
│   ├── ui/                # shadcn/ui components
│   ├── layout/            # Layout components
│   └── providers/         # Context providers
├── lib/
│   ├── utils.ts           # Utility functions
│   └── store.ts           # Zustand store
├── hooks/
│   └── use-toast.ts       # Toast hook
├── types/
│   └── next-auth.d.ts     # Type definitions
├── public/                # Static assets
└── tailwind.config.ts     # Tailwind configuration
```

## ⚡ Performance

- **Server Components**: Default for better performance
- **Client Components**: Only where interactivity is needed
- **Image Optimization**: Next.js Image component
- **Code Splitting**: Automatic route-based splitting
- **Lazy Loading**: Dynamic imports for heavy components

## ♿ Accessibility

- Semantic HTML elements
- ARIA labels and roles
- Keyboard navigation support
- Focus management
- Screen reader friendly
- Color contrast WCAG AA compliant
- Skip to content link

## 🧪 Testing

```bash
# Run linting
npm run lint

# Build for production
npm run build

# Start production server
npm start
```

## 📄 License

MIT

## 👥 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 🎯 Roadmap

- [ ] Mobile app (React Native)
- [ ] Real-time order updates (WebSocket)
- [ ] Push notifications
- [ ] Menu item customization builder
- [ ] Nutritional information
- [ ] Dietary filters (vegan, gluten-free, etc.)
- [ ] Group ordering
- [ ] Scheduled orders
- [ ] Analytics dashboard

## 📞 Support

For issues and questions, please open an issue on GitHub or contact support@campuseats.com

---

Built with ❤️ by the CampusEats Team


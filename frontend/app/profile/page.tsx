"use client"

import { User, Mail, Bell, CreditCard, MapPin } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Separator } from "@/components/ui/separator"

// Mock user data for demo
const mockUser = {
  name: "Alex Johnson",
  email: "alex.johnson@university.edu",
  image: "https://api.dicebear.com/7.x/avataaars/svg?seed=Alex",
}

export default function ProfilePage() {
  const session = { user: mockUser }

  return (
    <div className="container py-8 max-w-4xl">
      {/* Demo Mode Banner */}
      <div className="mb-6 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg">
        <div className="flex items-center gap-2">
          <span className="text-green-600 dark:text-green-400 font-semibold">👤 Demo Mode</span>
          <span className="text-green-600 dark:text-green-400">- Showing mock user profile data</span>
        </div>
      </div>

      <h1 className="text-4xl font-bold mb-8">My Profile</h1>

      {/* Profile Header */}
      <Card className="card-glass border-none mb-6">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage src={session.user?.image || ""} alt={session.user?.name || ""} />
              <AvatarFallback className="text-2xl">
                {session.user?.name?.charAt(0).toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h2 className="text-2xl font-bold mb-1">{session.user?.name}</h2>
              <p className="text-muted-foreground">{session.user?.email}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Account Information */}
      <Card className="card-glass border-none mb-6">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <User className="h-5 w-5" />
            Account Information
          </CardTitle>
          <CardDescription>Update your account details</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-2">
            <Label htmlFor="name">Full Name</Label>
            <Input id="name" defaultValue={session.user?.name || ""} />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="email">Email Address</Label>
            <Input
              id="email"
              type="email"
              defaultValue={session.user?.email || ""}
              disabled
            />
            <p className="text-xs text-muted-foreground">
              Email is managed through your Google account
            </p>
          </div>
          <Button>Save Changes</Button>
        </CardContent>
      </Card>

      {/* Preferences */}
      <Card className="card-glass border-none mb-6">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bell className="h-5 w-5" />
            Notifications
          </CardTitle>
          <CardDescription>Manage your notification preferences</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Order Updates</Label>
              <p className="text-sm text-muted-foreground">
                Get notified about your order status
              </p>
            </div>
            <Switch defaultChecked />
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Promotional Emails</Label>
              <p className="text-sm text-muted-foreground">
                Receive special offers and promotions
              </p>
            </div>
            <Switch />
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Loyalty Rewards</Label>
              <p className="text-sm text-muted-foreground">
                Get notified when you earn rewards
              </p>
            </div>
            <Switch defaultChecked />
          </div>
        </CardContent>
      </Card>

      {/* Favorite Locations */}
      <Card className="card-glass border-none mb-6">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MapPin className="h-5 w-5" />
            Favorite Pickup Locations
          </CardTitle>
          <CardDescription>Quick access to your preferred locations</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {["Main Cafeteria", "Student Union", "Library Café"].map((location) => (
            <div
              key={location}
              className="flex items-center justify-between p-3 border rounded-lg"
            >
              <span>{location}</span>
              <Button variant="ghost" size="sm">
                Remove
              </Button>
            </div>
          ))}
          <Button variant="outline" className="w-full">
            Add Location
          </Button>
        </CardContent>
      </Card>

      {/* Payment Methods */}
      <Card className="card-glass border-none">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="h-5 w-5" />
            Payment Methods
          </CardTitle>
          <CardDescription>Manage your saved payment methods</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex items-center justify-between p-4 border rounded-lg">
            <div className="flex items-center gap-3">
              <div className="w-12 h-8 bg-gradient-to-r from-blue-500 to-blue-600 rounded flex items-center justify-center text-white text-xs font-bold">
                VISA
              </div>
              <div>
                <p className="font-medium">•••• 4242</p>
                <p className="text-sm text-muted-foreground">Expires 12/24</p>
              </div>
            </div>
            <Button variant="ghost" size="sm">
              Remove
            </Button>
          </div>
          <div className="flex items-center justify-between p-4 border rounded-lg">
            <div className="flex items-center gap-3">
              <div className="w-12 h-8 bg-gradient-to-r from-orange-400 to-orange-500 rounded flex items-center justify-center text-white text-xs font-bold">
                CARD
              </div>
              <div>
                <p className="font-medium">Campus Card</p>
                <p className="text-sm text-muted-foreground">Balance: $125.50</p>
              </div>
            </div>
            <Button variant="ghost" size="sm">
              Reload
            </Button>
          </div>
          <Button variant="outline" className="w-full">
            Add Payment Method
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}


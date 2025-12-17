/**
 * Profile Page
 * Uses SOLID architecture with real backend integration
 * No more mock data!
 */

"use client"

import { useState, useEffect } from "react"
import { useSession } from "next-auth/react"
import { User, Mail, Bell, CreditCard, MapPin, Loader2, Shield } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Separator } from "@/components/ui/separator"
import { useToast } from "@/hooks/use-toast"
import { useUser } from "@/lib/hooks/useUser"

export default function ProfilePage() {
  const { data: session } = useSession()
  const { user, loading, error, updateProfile } = useUser()
  const { toast } = useToast()

  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    location: "",
    bio: ""
  })
  const [isSaving, setIsSaving] = useState(false)

  // Initialize form when user data loads
  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName || "",
        lastName: user.lastName || "",
        location: user.location || "",
        bio: user.bio || ""
      })
    }
  }, [user])

  const handleSave = async () => {
    setIsSaving(true)
    try {
      const success = await updateProfile(formData)

      if (success) {
        toast({
          title: "Profile Updated",
          description: "Your profile has been updated successfully",
        })
      } else {
        toast({
          title: "Update Failed",
          description: "Failed to update your profile. Please try again.",
          variant: "destructive",
        })
      }
    } catch (err) {
      toast({
        title: "Error",
        description: "An unexpected error occurred",
        variant: "destructive",
      })
    } finally {
      setIsSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="container py-8 max-w-4xl">
        <div className="flex items-center justify-center min-h-[400px]">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="container py-8 max-w-4xl">
        <Card className="card-glass border-none">
          <CardContent className="py-12 text-center">
            <p className="text-destructive mb-4">{error}</p>
            <p className="text-muted-foreground">
              Please sign in to view your profile
            </p>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container py-8 max-w-4xl">
      <h1 className="text-4xl font-bold mb-8">My Profile</h1>

      {/* Profile Header */}
      <Card className="card-glass border-none mb-6">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage
                src={session?.user?.image || user?.avatarUrl || ""}
                alt={`${user?.firstName} ${user?.lastName}` || session?.user?.email || ""}
              />
              <AvatarFallback className="text-2xl">
                {user?.firstName?.charAt(0).toUpperCase() || session?.user?.email?.charAt(0).toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h2 className="text-2xl font-bold mb-1">
                {user?.firstName && user?.lastName
                  ? `${user.firstName} ${user.lastName}`
                  : session?.user?.name || "User"}
              </h2>
              <p className="text-muted-foreground">{user?.email || session?.user?.email}</p>
              {user?.lastLoginAt && (
                <p className="text-sm text-muted-foreground mt-1">
                  Last login: {new Date(user.lastLoginAt).toLocaleString()}
                </p>
              )}
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
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label htmlFor="firstName">First Name</Label>
              <Input
                id="firstName"
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                placeholder="Enter your first name"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="lastName">Last Name</Label>
              <Input
                id="lastName"
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                placeholder="Enter your last name"
              />
            </div>
          </div>
          <div className="grid gap-2">
            <Label htmlFor="email">Email Address</Label>
            <Input
              id="email"
              type="email"
              value={user?.email || session?.user?.email || ""}
              disabled
            />
            <p className="text-xs text-muted-foreground">
              Email is managed through your Google account
            </p>
          </div>
          <div className="grid gap-2">
            <Label htmlFor="location">Location</Label>
            <Input
              id="location"
              value={formData.location}
              onChange={(e) => setFormData({ ...formData, location: e.target.value })}
              placeholder="e.g., Main Campus"
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="bio">Bio</Label>
            <Input
              id="bio"
              value={formData.bio}
              onChange={(e) => setFormData({ ...formData, bio: e.target.value })}
              placeholder="Tell us a bit about yourself"
            />
          </div>
          <Button onClick={handleSave} disabled={isSaving}>
            {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Save Changes
          </Button>
        </CardContent>
      </Card>

      {/* Account Status */}
      <Card className="card-glass border-none mb-6">
        <CardHeader>
          <CardTitle>Account Status</CardTitle>
          <CardDescription>Your account details</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm">Account Status</span>
            <span className={`text-sm font-medium ${user?.isActive ? 'text-green-600' : 'text-red-600'}`}>
              {user?.isActive ? 'Active' : 'Inactive'}
            </span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm">Role</span>
            <span className={`text-sm font-medium ${user?.role === 2 ? 'text-purple-600' :
                user?.role === 1 ? 'text-orange-500' : 'text-blue-600'
              }`}>
              {user?.role === 2 ? 'Admin' : user?.role === 1 ? 'Chef' : 'User'}
            </span>
          </div>
          <Separator />
          <div className="flex items-center justify-between">
            <span className="text-sm">Account Created</span>
            <span className="text-sm text-muted-foreground">
              {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A'}
            </span>
          </div>
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
    </div>
  )
}


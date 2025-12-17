"use client"

import { useState, useEffect } from "react"
import { Plus, Pencil, Trash2, Search, Users, UtensilsCrossed, ShieldAlert, Loader2 } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { useToast } from "@/hooks/use-toast"
import { formatCurrency } from "@/lib/utils"
import { useRole } from "@/lib/hooks/useAdmin"
import { useSession } from "next-auth/react"
import Link from "next/link"
import type { MenuItem } from "@/lib/store"

// User type for admin management
interface UserData {
  id: string
  email: string
  firstName?: string
  lastName?: string
  role: number
  isActive: boolean
  createdAt: string
}

const ROLES = [
  { value: 0, label: "User", color: "bg-blue-500" },
  { value: 1, label: "Chef", color: "bg-orange-500" },
  { value: 2, label: "Admin", color: "bg-purple-500" },
]

const initialMenuItems: MenuItem[] = [
  {
    id: "1",
    name: "Grilled Chicken Sandwich",
    description: "Juicy grilled chicken with fresh vegetables",
    price: 8.99,
    currency: "RON",
    category: "Sandwiches",
    available: true,
    preparationTime: 10,
  },
  {
    id: "2",
    name: "Caesar Salad",
    description: "Fresh romaine lettuce with parmesan",
    price: 6.99,
    currency: "RON",
    category: "Salads",
    available: true,
    preparationTime: 5,
  },
]

export default function AdminPage() {
  const { isAdmin, isLoading: roleLoading } = useRole()
  const { data: session } = useSession()
  const [activeTab, setActiveTab] = useState("menu")

  // Menu state
  const [menuItems, setMenuItems] = useState<MenuItem[]>(initialMenuItems)
  const [searchQuery, setSearchQuery] = useState("")
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<MenuItem | null>(null)

  // Users state
  const [users, setUsers] = useState<UserData[]>([])
  const [usersLoading, setUsersLoading] = useState(false)
  const [userSearch, setUserSearch] = useState("")

  const { toast } = useToast()

  const [formData, setFormData] = useState<Partial<MenuItem>>({
    name: "",
    description: "",
    price: 0,
    currency: "RON",
    category: "",
    available: true,
    preparationTime: 0,
  })

  const categories = ["Sandwiches", "Salads", "Pizza", "Burgers", "Bowls", "Desserts", "Drinks"]

  // Fetch users when Users tab is active
  useEffect(() => {
    if (activeTab === "users" && isAdmin) {
      fetchUsers()
    }
  }, [activeTab, isAdmin])

  const fetchUsers = async () => {
    setUsersLoading(true)
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL
      const response = await fetch(`${apiBase}/users`, {
        headers: {
          "X-User-Email": session?.user?.email || ""
        }
      })
      if (response.ok) {
        const data = await response.json()
        setUsers(data)
      }
    } catch (err) {
      console.error("Error fetching users:", err)
    } finally {
      setUsersLoading(false)
    }
  }

  const handleRoleChange = async (userId: string, newRole: string) => {
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL
      const response = await fetch(`${apiBase}/users/${userId}/role`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          "X-User-Email": session?.user?.email || ""
        },
        body: JSON.stringify({ role: newRole })
      })

      if (response.ok) {
        const updatedUser = await response.json()
        setUsers(prev => prev.map(u => u.id === userId ? { ...u, role: updatedUser.role } : u))
        toast({
          title: "Role updated",
          description: `User role changed to ${newRole}`,
        })
      } else {
        toast({
          title: "Error",
          description: "Failed to update role",
          variant: "destructive"
        })
      }
    } catch (err) {
      toast({
        title: "Error",
        description: "Failed to connect to server",
        variant: "destructive"
      })
    }
  }

  const handleOpenDialog = (item?: MenuItem) => {
    if (item) {
      setEditingItem(item)
      setFormData(item)
    } else {
      setEditingItem(null)
      setFormData({
        name: "",
        description: "",
        price: 0,
        currency: "RON",
        category: "",
        available: true,
        preparationTime: 0,
      })
    }
    setDialogOpen(true)
  }

  const handleSave = () => {
    if (editingItem) {
      setMenuItems((prev) =>
        prev.map((item) =>
          item.id === editingItem.id ? { ...formData, id: item.id } as MenuItem : item
        )
      )
      toast({
        title: "Item updated",
        description: "Menu item has been updated successfully",
      })
    } else {
      const newItem: MenuItem = {
        ...formData,
        id: Date.now().toString(),
      } as MenuItem
      setMenuItems((prev) => [...prev, newItem])
      toast({
        title: "Item added",
        description: "New menu item has been added successfully",
      })
    }
    setDialogOpen(false)
  }

  const handleDelete = (id: string) => {
    setMenuItems((prev) => prev.filter((item) => item.id !== id))
    toast({
      title: "Item deleted",
      description: "Menu item has been deleted",
    })
  }

  const toggleAvailability = (id: string) => {
    setMenuItems((prev) =>
      prev.map((item) =>
        item.id === id ? { ...item, available: !item.available } : item
      )
    )
  }

  const filteredItems = menuItems.filter(
    (item) =>
      item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.description.toLowerCase().includes(searchQuery.toLowerCase())
  )

  const filteredUsers = users.filter(
    (user) =>
      user.email.toLowerCase().includes(userSearch.toLowerCase()) ||
      (user.firstName?.toLowerCase() || "").includes(userSearch.toLowerCase()) ||
      (user.lastName?.toLowerCase() || "").includes(userSearch.toLowerCase())
  )

  // Loading state
  if (roleLoading) {
    return (
      <div className="container py-8 flex justify-center items-center h-[50vh]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    )
  }

  // Access denied for non-admins
  if (!isAdmin) {
    return (
      <div className="container py-8">
        <Card className="card-glass border-none max-w-md mx-auto">
          <CardContent className="pt-8 pb-8 text-center">
            <ShieldAlert className="h-16 w-16 mx-auto text-destructive mb-4" />
            <h2 className="text-2xl font-bold mb-2">Access Denied</h2>
            <p className="text-muted-foreground mb-6">
              Only Admin can access the Admin Dashboard.
            </p>
            <Link href="/">
              <Button variant="ghost" className="w-full">
                Return Home
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="container py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Admin Dashboard</h1>
        <p className="text-muted-foreground">Manage menu items and users</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full max-w-md grid-cols-2">
          <TabsTrigger value="menu" className="flex items-center gap-2">
            <UtensilsCrossed className="h-4 w-4" />
            Menu Items
          </TabsTrigger>
          <TabsTrigger value="users" className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            Users
          </TabsTrigger>
        </TabsList>

        {/* Menu Tab */}
        <TabsContent value="menu" className="space-y-6">
          {/* Header Actions */}
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search menu items..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Button onClick={() => handleOpenDialog()}>
              <Plus className="h-4 w-4 mr-2" />
              Add Menu Item
            </Button>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-primary">{menuItems.length}</p>
                  <p className="text-sm text-muted-foreground mt-1">Total Items</p>
                </div>
              </CardContent>
            </Card>
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-green-500">
                    {menuItems.filter((i) => i.available).length}
                  </p>
                  <p className="text-sm text-muted-foreground mt-1">Available</p>
                </div>
              </CardContent>
            </Card>
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-red-500">
                    {menuItems.filter((i) => !i.available).length}
                  </p>
                  <p className="text-sm text-muted-foreground mt-1">Unavailable</p>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Menu Items Table */}
          <Card className="card-glass border-none">
            <CardHeader>
              <CardTitle>Menu Items</CardTitle>
              <CardDescription>View and manage all menu items</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {filteredItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between p-4 border rounded-lg hover:bg-accent/50 transition-colors"
                  >
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-1">
                        <h3 className="font-semibold">{item.name}</h3>
                        <Badge variant={item.available ? "default" : "destructive"}>
                          {item.available ? "Available" : "Unavailable"}
                        </Badge>
                        <Badge variant="secondary">{item.category}</Badge>
                      </div>
                      <p className="text-sm text-muted-foreground mb-1">
                        {item.description}
                      </p>
                      <div className="flex items-center gap-4 text-sm">
                        <span className="font-bold text-primary">
                          {formatCurrency(item.price)}
                        </span>
                        <span className="text-muted-foreground">
                          ⏱️ {item.preparationTime} min
                        </span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => toggleAvailability(item.id)}
                      >
                        {item.available ? "Disable" : "Enable"}
                      </Button>
                      <Button
                        variant="outline"
                        size="icon"
                        onClick={() => handleOpenDialog(item)}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="outline"
                        size="icon"
                        onClick={() => handleDelete(item.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Users Tab */}
        <TabsContent value="users" className="space-y-6">
          {/* Search */}
          <div className="relative max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search users by name or email..."
              value={userSearch}
              onChange={(e) => setUserSearch(e.target.value)}
              className="pl-10"
            />
          </div>

          {/* Stats */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-primary">{users.length}</p>
                  <p className="text-sm text-muted-foreground mt-1">Total Users</p>
                </div>
              </CardContent>
            </Card>
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-blue-500">
                    {users.filter(u => u.role === 0).length}
                  </p>
                  <p className="text-sm text-muted-foreground mt-1">Users</p>
                </div>
              </CardContent>
            </Card>
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-orange-500">
                    {users.filter(u => u.role === 1).length}
                  </p>
                  <p className="text-sm text-muted-foreground mt-1">Chefs</p>
                </div>
              </CardContent>
            </Card>
            <Card className="card-glass border-none">
              <CardContent className="pt-6">
                <div className="text-center">
                  <p className="text-3xl font-bold text-purple-500">
                    {users.filter(u => u.role === 2).length}
                  </p>
                  <p className="text-sm text-muted-foreground mt-1">Admins</p>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Users List */}
          <Card className="card-glass border-none">
            <CardHeader>
              <CardTitle>All Users</CardTitle>
              <CardDescription>Manage user roles</CardDescription>
            </CardHeader>
            <CardContent>
              {usersLoading ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
              ) : (
                <div className="space-y-4">
                  {filteredUsers.map((user) => (
                    <div
                      key={user.id}
                      className="flex items-center justify-between p-4 border rounded-lg hover:bg-accent/50 transition-colors"
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-1">
                          <h3 className="font-semibold">
                            {user.firstName && user.lastName
                              ? `${user.firstName} ${user.lastName}`
                              : user.email}
                          </h3>
                          <Badge className={ROLES[user.role]?.color || "bg-gray-500"}>
                            {ROLES[user.role]?.label || "Unknown"}
                          </Badge>
                        </div>
                        <p className="text-sm text-muted-foreground">{user.email}</p>
                      </div>
                      <div className="w-32">
                        <Select
                          value={user.role.toString()}
                          onValueChange={(value) => handleRoleChange(user.id, ROLES[parseInt(value)]?.label || "User")}
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            {ROLES.map((role) => (
                              <SelectItem key={role.value} value={role.value.toString()}>
                                {role.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    </div>
                  ))}
                  {filteredUsers.length === 0 && !usersLoading && (
                    <p className="text-center text-muted-foreground py-8">
                      No users found
                    </p>
                  )}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Add/Edit Menu Item Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingItem ? "Edit Menu Item" : "Add Menu Item"}
            </DialogTitle>
            <DialogDescription>
              {editingItem
                ? "Update the menu item details"
                : "Add a new item to the menu"}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description</Label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="price">Price</Label>
                <Input
                  id="price"
                  type="number"
                  step="0.01"
                  value={formData.price}
                  onChange={(e) =>
                    setFormData({ ...formData, price: parseFloat(e.target.value) })
                  }
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="prepTime">Prep Time (min)</Label>
                <Input
                  id="prepTime"
                  type="number"
                  value={formData.preparationTime}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      preparationTime: parseInt(e.target.value),
                    })
                  }
                />
              </div>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="category">Category</Label>
              <Select
                value={formData.category}
                onValueChange={(value) =>
                  setFormData({ ...formData, category: value })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select category" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((cat) => (
                    <SelectItem key={cat} value={cat}>
                      {cat}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center space-x-2">
              <Checkbox
                id="available"
                checked={formData.available}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, available: checked as boolean })
                }
              />
              <Label htmlFor="available" className="cursor-pointer">
                Available for ordering
              </Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              {editingItem ? "Update" : "Add"} Item
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

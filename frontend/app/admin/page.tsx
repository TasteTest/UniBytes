"use client"

import { useState } from "react"
import { Plus, Pencil, Trash2, Search } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
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
import type { MenuItem } from "@/lib/store"

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
  const [menuItems, setMenuItems] = useState<MenuItem[]>(initialMenuItems)
  const [searchQuery, setSearchQuery] = useState("")
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<MenuItem | null>(null)
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
        variant: "success",
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
        variant: "success",
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

  return (
    <div className="container py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Admin Dashboard</h1>
        <p className="text-muted-foreground">Manage menu items and settings</p>
      </div>

      {/* Header Actions */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
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
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
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
                    <Badge variant={item.available ? "success" : "destructive"}>
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

      {/* Add/Edit Dialog */}
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


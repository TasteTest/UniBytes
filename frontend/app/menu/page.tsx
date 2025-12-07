"use client"

import { useState, useEffect } from "react"
import { Search, Plus, Loader2 } from "lucide-react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { useCartStore, type MenuItem } from "@/lib/store"
import { useToast } from "@/hooks/use-toast"
import { formatCurrency } from "@/lib/utils"

interface BackendMenuItem {
  id: string
  categoryId: string
  name: string
  description: string | null
  price: number
  currency: string
  available: boolean
  imageUrl: string | null
}

interface Category {
  id: string
  name: string
  description: string | null
  displayOrder: number
  isActive: boolean
}

export default function MenuPage() {
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedCategory, setSelectedCategory] = useState("All")
  const [selectedItem, setSelectedItem] = useState<MenuItem | null>(null)
  const [menuItems, setMenuItems] = useState<MenuItem[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const addItem = useCartStore((state) => state.addItem)
  const { toast } = useToast()

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true)
        
        // Fetch categories and menu items in parallel
        const [categoriesRes, menuItemsRes] = await Promise.all([
          fetch('http://localhost:5267/api/categories'),
          fetch('http://localhost:5267/api/menuitems')
        ])

        if (!categoriesRes.ok || !menuItemsRes.ok) {
          throw new Error('Failed to fetch menu data')
        }

        const categoriesData: Category[] = await categoriesRes.json()
        const menuItemsData: BackendMenuItem[] = await menuItemsRes.json()

        // Create category map for lookup
        const categoryMap = new Map(categoriesData.map(cat => [cat.id, cat.name]))

        // Map backend items to frontend MenuItem format
        const mappedItems: MenuItem[] = menuItemsData
          .filter(item => item.available)
          .map(item => ({
            id: item.id,
            name: item.name,
            description: item.description || '',
            price: item.price,
            category: categoryMap.get(item.categoryId) || 'Other',
            available: item.available,
            image: item.imageUrl || undefined,
            preparationTime: 10, // Default prep time
          }))

        setCategories(categoriesData.filter(cat => cat.isActive))
        setMenuItems(mappedItems)
        setError(null)
      } catch (err) {
        console.error('Error fetching menu:', err)
        setError('Failed to load menu. Please try again later.')
      } finally {
        setLoading(false)
      }
    }

    fetchData()
  }, [])

  const filteredItems = menuItems.filter((item) => {
    const matchesSearch = item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      item.description.toLowerCase().includes(searchQuery.toLowerCase())
    const matchesCategory = selectedCategory === "All" || item.category === selectedCategory
    return matchesSearch && matchesCategory && item.available
  })

  const handleAddToCart = (menuItem: MenuItem) => {
    addItem({
      id: `cart-${Date.now()}`,
      menuItem,
      quantity: 1,
      modifiers: [],
    })
    toast({
      title: "Added to cart",
      description: `${menuItem.name} has been added to your cart`,
      variant: "success",
    })
  }

  if (loading) {
    return (
      <div className="container py-8">
        <div className="mb-8">
          <Skeleton className="h-10 w-64 mb-4" />
          <Skeleton className="h-6 w-96" />
        </div>
        <Skeleton className="h-12 w-full mb-6" />
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[...Array(6)].map((_, i) => (
            <Card key={i} className="card-glass border-none">
              <CardHeader>
                <Skeleton className="h-6 w-3/4 mb-2" />
                <Skeleton className="h-4 w-full" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-4 w-24" />
              </CardContent>
              <CardFooter>
                <Skeleton className="h-8 w-24" />
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="container py-8">
        <div className="text-center py-12">
          <p className="text-destructive mb-4">{error}</p>
          <Button onClick={() => window.location.reload()}>Retry</Button>
        </div>
      </div>
    )
  }

  const categoryNames = ["All", ...categories.map(cat => cat.name)]

  return (
    <div className="container py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-4">Our Menu</h1>
        <p className="text-muted-foreground">
          Browse our selection of delicious meals and add them to your cart
        </p>
      </div>

      {/* Search */}
      <div className="mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search menu items..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10"
          />
        </div>
      </div>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-4">Our Menu</h1>
        <p className="text-muted-foreground">
          Browse our selection of delicious meals and add them to your cart
        </p>
      </div>

      {/* Search */}
      <div className="mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search menu items..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10"
          />
        </div>
      </div>

      {/* Categories */}
      <Tabs value={selectedCategory} onValueChange={setSelectedCategory} className="mb-8">
        <TabsList className="w-full justify-start overflow-x-auto">
          {categoryNames.map((category) => (
            <TabsTrigger key={category} value={category}>
              {category}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      {/* Menu Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredItems.map((item) => (
          <Card
            key={item.id}
            className="card-glass border-none cursor-pointer group"
            onClick={() => setSelectedItem(item)}
          >
            <CardHeader>
              <div className="flex items-start justify-between">
                <div>
                  <CardTitle className="group-hover:text-primary transition-colors">
                    {item.name}
                  </CardTitle>
                  <CardDescription className="mt-2">{item.description}</CardDescription>
                </div>
                <Badge variant="secondary">{item.category}</Badge>
              </div>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <span>⏱️ {item.preparationTime} min</span>
              </div>
            </CardContent>
            <CardFooter className="flex items-center justify-between">
              <span className="text-2xl font-bold text-primary">
                {formatCurrency(item.price)}
              </span>
              <Button
                size="sm"
                onClick={(e) => {
                  e.stopPropagation()
                  handleAddToCart(item)
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Add
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>

      {filteredItems.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted-foreground">No items found matching your search</p>
        </div>
      )}

      {/* Item Detail Dialog */}
      <Dialog open={!!selectedItem} onOpenChange={() => setSelectedItem(null)}>
        <DialogContent className="max-w-2xl">
          {selectedItem && (
            <>
              <DialogHeader>
                <DialogTitle className="text-2xl">{selectedItem.name}</DialogTitle>
                <DialogDescription>{selectedItem.description}</DialogDescription>
              </DialogHeader>
              <div className="space-y-6">
                <div className="flex items-center justify-between">
                  <Badge>{selectedItem.category}</Badge>
                  <span className="text-sm text-muted-foreground">
                    ⏱️ {selectedItem.preparationTime} min prep time
                  </span>
                </div>
                <div className="flex items-center justify-between pt-4 border-t">
                  <span className="text-3xl font-bold text-primary">
                    {formatCurrency(selectedItem.price)}
                  </span>
                  <Button
                    size="lg"
                    onClick={() => {
                      handleAddToCart(selectedItem)
                      setSelectedItem(null)
                    }}
                  >
                    <Plus className="h-4 w-4 mr-2" />
                    Add to Cart
                  </Button>
                </div>
              </div>
            </>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}


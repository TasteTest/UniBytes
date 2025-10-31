"use client"

import { useState } from "react"
import { Search, Plus } from "lucide-react"
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

// Mock data - replace with actual API call
const mockMenuItems: MenuItem[] = [
  {
    id: "1",
    name: "Grilled Chicken Sandwich",
    description: "Juicy grilled chicken with fresh vegetables and house sauce",
    price: 8.99,
    category: "Sandwiches",
    available: true,
    preparationTime: 10,
  },
  {
    id: "2",
    name: "Caesar Salad",
    description: "Fresh romaine lettuce with parmesan and homemade dressing",
    price: 6.99,
    category: "Salads",
    available: true,
    preparationTime: 5,
  },
  {
    id: "3",
    name: "Margherita Pizza",
    description: "Classic pizza with fresh mozzarella and basil",
    price: 11.99,
    category: "Pizza",
    available: true,
    preparationTime: 15,
  },
  {
    id: "4",
    name: "Cheeseburger",
    description: "Angus beef patty with cheese, lettuce, tomato",
    price: 9.99,
    category: "Burgers",
    available: true,
    preparationTime: 12,
  },
  {
    id: "5",
    name: "Veggie Bowl",
    description: "Quinoa with roasted vegetables and tahini dressing",
    price: 7.99,
    category: "Bowls",
    available: true,
    preparationTime: 8,
  },
  {
    id: "6",
    name: "Chocolate Chip Cookie",
    description: "Freshly baked with premium chocolate chips",
    price: 2.99,
    category: "Desserts",
    available: true,
    preparationTime: 2,
  },
  {
    id: "7",
    name: "Club Sandwich",
    description: "Triple-decker with turkey, bacon, lettuce and tomato",
    price: 10.49,
    category: "Sandwiches",
    available: true,
    preparationTime: 12,
  },
  {
    id: "8",
    name: "Greek Salad",
    description: "Feta cheese, olives, cucumber, tomatoes with olive oil",
    price: 7.49,
    category: "Salads",
    available: true,
    preparationTime: 6,
  },
  {
    id: "9",
    name: "Pepperoni Pizza",
    description: "Classic pepperoni with mozzarella cheese",
    price: 12.99,
    category: "Pizza",
    available: true,
    preparationTime: 18,
  },
  {
    id: "10",
    name: "Bacon Burger",
    description: "Beef patty with crispy bacon and BBQ sauce",
    price: 11.49,
    category: "Burgers",
    available: true,
    preparationTime: 14,
  },
  {
    id: "11",
    name: "Teriyaki Bowl",
    description: "Chicken teriyaki over rice with steamed vegetables",
    price: 9.99,
    category: "Bowls",
    available: true,
    preparationTime: 10,
  },
  {
    id: "12",
    name: "Brownie Sundae",
    description: "Warm brownie with vanilla ice cream and chocolate sauce",
    price: 5.99,
    category: "Desserts",
    available: true,
    preparationTime: 5,
  },
  {
    id: "13",
    name: "BLT Sandwich",
    description: "Crispy bacon, lettuce, tomato on toasted bread",
    price: 7.99,
    category: "Sandwiches",
    available: true,
    preparationTime: 8,
  },
  {
    id: "14",
    name: "Cobb Salad",
    description: "Mixed greens with chicken, egg, bacon, avocado",
    price: 9.49,
    category: "Salads",
    available: true,
    preparationTime: 8,
  },
  {
    id: "15",
    name: "Veggie Pizza",
    description: "Bell peppers, mushrooms, onions, olives",
    price: 11.49,
    category: "Pizza",
    available: false,
    preparationTime: 16,
  },
  {
    id: "16",
    name: "Mushroom Swiss Burger",
    description: "Sautéed mushrooms and swiss cheese",
    price: 10.99,
    category: "Burgers",
    available: true,
    preparationTime: 13,
  },
  {
    id: "17",
    name: "Poke Bowl",
    description: "Fresh ahi tuna with rice, edamame, and wasabi mayo",
    price: 12.99,
    category: "Bowls",
    available: true,
    preparationTime: 12,
  },
  {
    id: "18",
    name: "Cheesecake Slice",
    description: "New York style cheesecake with berry compote",
    price: 4.99,
    category: "Desserts",
    available: true,
    preparationTime: 3,
  },
  {
    id: "19",
    name: "Fresh Lemonade",
    description: "Freshly squeezed lemons with a hint of mint",
    price: 3.49,
    category: "Drinks",
    available: true,
    preparationTime: 3,
  },
  {
    id: "20",
    name: "Iced Coffee",
    description: "Cold brew coffee with your choice of milk",
    price: 4.49,
    category: "Drinks",
    available: true,
    preparationTime: 2,
  },
  {
    id: "21",
    name: "Smoothie Bowl",
    description: "Acai berry smoothie topped with granola and fresh fruit",
    price: 8.49,
    category: "Bowls",
    available: true,
    preparationTime: 6,
  },
  {
    id: "22",
    name: "Green Juice",
    description: "Kale, spinach, apple, cucumber, lemon",
    price: 5.99,
    category: "Drinks",
    available: true,
    preparationTime: 4,
  },
]

const categories = ["All", "Sandwiches", "Salads", "Pizza", "Burgers", "Bowls", "Desserts", "Drinks"]

export default function MenuPage() {
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedCategory, setSelectedCategory] = useState("All")
  const [selectedItem, setSelectedItem] = useState<MenuItem | null>(null)
  const addItem = useCartStore((state) => state.addItem)
  const { toast } = useToast()

  const filteredItems = mockMenuItems.filter((item) => {
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

      {/* Categories */}
      <Tabs value={selectedCategory} onValueChange={setSelectedCategory} className="mb-8">
        <TabsList className="w-full justify-start overflow-x-auto">
          {categories.map((category) => (
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


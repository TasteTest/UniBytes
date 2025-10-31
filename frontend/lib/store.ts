import { create } from 'zustand'

export interface MenuItem {
  id: string
  name: string
  description: string
  price: number
  category: string
  image?: string
  available: boolean
  preparationTime?: number
}

export interface CartItemModifier {
  id: string
  name: string
  price: number
}

export interface CartItem {
  id: string
  menuItem: MenuItem
  quantity: number
  modifiers: CartItemModifier[]
  specialInstructions?: string
}

interface CartStore {
  items: CartItem[]
  addItem: (item: CartItem) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  clearCart: () => void
  getTotal: () => number
  getItemCount: () => number
}

export const useCartStore = create<CartStore>()((set, get) => ({
  items: [],
  addItem: (item) =>
    set((state) => {
      const existingItem = state.items.find(
        (i) => i.id === item.id && 
        JSON.stringify(i.modifiers) === JSON.stringify(item.modifiers)
      )
      if (existingItem) {
        return {
          items: state.items.map((i) =>
            i.id === item.id ? { ...i, quantity: i.quantity + item.quantity } : i
          ),
        }
      }
      return { items: [...state.items, item] }
    }),
  removeItem: (id) =>
    set((state) => ({
      items: state.items.filter((i) => i.id !== id),
    })),
  updateQuantity: (id, quantity) =>
    set((state) => ({
      items: state.items.map((i) =>
        i.id === id ? { ...i, quantity } : i
      ),
    })),
  clearCart: () => set({ items: [] }),
  getTotal: () => {
    const items = get().items
    return items.reduce((total, item) => {
      const itemTotal = item.menuItem.price + 
        item.modifiers.reduce((sum, mod) => sum + mod.price, 0)
      return total + itemTotal * item.quantity
    }, 0)
  },
  getItemCount: () => {
    const items = get().items
    return items.reduce((count, item) => count + item.quantity, 0)
  },
}))


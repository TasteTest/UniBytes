import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export interface MenuItem {
  id: string
  name: string
  description: string
  price: number
  currency: string
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

export interface RewardCartItem {
  rewardId: string
  rewardName: string
  rewardDescription: string
  pointsUsed: number
  rewardType: string
  metadata?: Record<string, any>
}

interface CartStore {
  items: CartItem[]
  rewardItems: RewardCartItem[]
  addItem: (item: CartItem) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  clearCart: () => void
  getTotal: () => number
  getSubtotal: () => number
  getDiscount: () => number
  getItemCount: () => number
  addRewardItem: (rewardItem: RewardCartItem) => void
  removeRewardItem: (rewardId: string) => void
  getRewardItems: () => RewardCartItem[]
  getFreeItems: () => RewardCartItem[]
  getDiscountItems: () => RewardCartItem[]
  clearRewardItems: () => void
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      items: [],
      rewardItems: [],
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
      clearCart: () => set({ items: [], rewardItems: [] }),
      getSubtotal: () => {
        const items = get().items
        return items.reduce((total, item) => {
          const itemTotal = item.menuItem.price +
            item.modifiers.reduce((sum, mod) => sum + mod.price, 0)
          return total + itemTotal * item.quantity
        }, 0)
      },
      getDiscount: () => {
        const subtotal = get().getSubtotal()
        const items = get().items
        const rewardItems = get().rewardItems
        let discount = 0

        rewardItems.forEach(reward => {
          if (reward.rewardType === 'Discount') {
            if (reward.metadata?.discountType === 'fixed') {
              discount += reward.metadata.discountAmount || 0
            } else if (reward.metadata?.discountType === 'percentage') {
              discount += subtotal * ((reward.metadata.discountPercent || 0) / 100)
            }
          } else if (reward.rewardType === 'CategoryDiscount') {
            // Find cheapest item in target category
            const targetCategory = reward.metadata?.targetCategory?.toLowerCase()
            if (targetCategory) {
              const matchingItems = items.filter(
                item => item.menuItem.category?.toLowerCase() === targetCategory
              )
              if (matchingItems.length > 0) {
                // Find cheapest item (base price + modifiers)
                const cheapestItem = matchingItems.reduce((min, item) => {
                  const itemPrice = item.menuItem.price +
                    item.modifiers.reduce((sum, mod) => sum + mod.price, 0)
                  const minPrice = min.menuItem.price +
                    min.modifiers.reduce((sum, mod) => sum + mod.price, 0)
                  return itemPrice < minPrice ? item : min
                })
                const cheapestPrice = cheapestItem.menuItem.price +
                  cheapestItem.modifiers.reduce((sum, mod) => sum + mod.price, 0)
                discount += cheapestPrice
              }
            }
          }
        })

        // Discount cannot exceed subtotal
        return Math.min(discount, subtotal)
      },
      getTotal: () => {
        const subtotal = get().getSubtotal()
        const discount = get().getDiscount()
        return Math.max(0, subtotal - discount)
      },
      getItemCount: () => {
        const items = get().items
        const rewardItems = get().rewardItems
        return items.reduce((count, item) => count + item.quantity, 0) + rewardItems.length
      },
      addRewardItem: (rewardItem) =>
        set((state) => {
          // Prevent adding duplicate reward items
          const exists = state.rewardItems.some(r => r.rewardId === rewardItem.rewardId)
          if (exists) {
            return state
          }
          return { rewardItems: [...state.rewardItems, rewardItem] }
        }),
      removeRewardItem: (rewardId) =>
        set((state) => ({
          rewardItems: state.rewardItems.filter((r) => r.rewardId !== rewardId),
        })),
      getRewardItems: () => get().rewardItems,
      getFreeItems: () => get().rewardItems.filter(r => r.rewardType === 'MenuItem'),
      getDiscountItems: () => get().rewardItems.filter(r => r.rewardType === 'Discount' || r.rewardType === 'CategoryDiscount'),
      clearRewardItems: () => set({ rewardItems: [] }),
    }),
    {
      name: 'unibytes-cart', // localStorage key
    }
  )
)

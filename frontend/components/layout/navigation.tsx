"use client"

import Link from "next/link"
import { useSession, signOut } from "next-auth/react"
import { useTheme } from "next-themes"
import { ShoppingCart, Menu, Moon, Sun, User, Home, UtensilsCrossed, History, Award, LogOut, ChefHat, Shield } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Badge } from "@/components/ui/badge"
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet"
import { useCartStore } from "@/lib/store"
import { useState, useEffect } from "react"

export function Navigation() {
  const { data: session } = useSession()
  const { theme, setTheme } = useTheme()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  // Hydration-safe cart count: render 0 on server, update on client after mount
  const [itemCount, setItemCount] = useState(0)
  useEffect(() => {
    // Subscribe to cart changes after hydration
    const unsubscribe = useCartStore.subscribe((state) => {
      setItemCount(state.getItemCount())
    })
    // Set initial value
    setItemCount(useCartStore.getState().getItemCount())
    return unsubscribe
  }, [])

  // Role: 0=User, 1=Chef, 2=Admin
  const role = session?.user?.role ?? 0
  const isChef = role === 1
  const isAdmin = role === 2

  const navLinks = [
    { href: "/", label: "Home", icon: Home },
    { href: "/menu", label: "Menu", icon: UtensilsCrossed },
    { href: "/orders", label: "Orders", icon: History },
    { href: "/loyalty", label: "Rewards", icon: Award },
  ]

  const handleSignOut = async () => {
    await signOut({ callbackUrl: '/' })
  }

  return (
    <nav className="sticky top-0 z-40 w-full border-b card-glass">
      <div className="container flex h-16 items-center justify-between">
        {/* Logo */}
        <Link href="/" className="flex items-center space-x-2">
          <UtensilsCrossed className="h-6 w-6 text-primary" />
          <span className="text-xl font-bold text-gradient">CampusEats</span>
        </Link>

        {/* Desktop Navigation */}
        <div className="hidden md:flex items-center space-x-6">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="text-sm font-medium transition-colors hover:text-primary flex items-center gap-2"
            >
              <link.icon className="h-4 w-4" />
              {link.label}
            </Link>
          ))}
          {/* Kitchen link - visible for Chef only (Chef manages orders) */}
          {session && isChef && (
            <Link
              href="/kitchen"
              className="text-sm font-medium transition-colors hover:text-primary flex items-center gap-2 text-orange-500"
            >
              <ChefHat className="h-4 w-4" />
              Kitchen
            </Link>
          )}
          {/* Admin link - visible only for Admin */}
          {session && isAdmin && (
            <Link
              href="/admin"
              className="text-sm font-medium transition-colors hover:text-primary flex items-center gap-2 text-purple-500"
            >
              <Shield className="h-4 w-4" />
              Admin
            </Link>
          )}
        </div>

        {/* Right Actions */}
        <div className="flex items-center space-x-4">
          {/* Theme Toggle */}
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
          >
            <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
            <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
            <span className="sr-only">Toggle theme</span>
          </Button>

          {/* Cart */}
          <Link href="/cart">
            <Button variant="ghost" size="icon" className="relative">
              <ShoppingCart className="h-5 w-5" />
              {itemCount > 0 && (
                <Badge className="absolute -top-1 -right-1 h-5 w-5 flex items-center justify-center p-0 text-xs">
                  {itemCount}
                </Badge>
              )}
            </Button>
          </Link>

          {/* User Menu */}
          {session ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                  <Avatar>
                    <AvatarImage src={session.user?.image || ""} alt={session.user?.name || ""} />
                    <AvatarFallback>
                      {session.user?.name?.charAt(0).toUpperCase() || session.user?.email?.charAt(0).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent className="w-56" align="end">
                <DropdownMenuLabel>
                  <div className="flex flex-col space-y-1">
                    <p className="text-sm font-medium leading-none">{session.user?.name}</p>
                    <p className="text-xs leading-none text-muted-foreground">
                      {session.user?.email}
                    </p>
                    {isAdmin && (
                      <Badge variant="secondary" className="w-fit mt-1 text-xs">Admin</Badge>
                    )}
                  </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                  <Link href="/profile">
                    <User className="mr-2 h-4 w-4" />
                    Profile
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/orders">
                    <History className="mr-2 h-4 w-4" />
                    Order History
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/loyalty">
                    <Award className="mr-2 h-4 w-4" />
                    Rewards
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleSignOut}>
                  <LogOut className="mr-2 h-4 w-4" />
                  Sign out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Button asChild>
              <Link href="/auth/signin">Sign In</Link>
            </Button>
          )}

          {/* Mobile Menu */}
          <Sheet open={mobileMenuOpen} onOpenChange={setMobileMenuOpen}>
            <SheetTrigger asChild className="md:hidden">
              <Button variant="ghost" size="icon">
                <Menu className="h-5 w-5" />
              </Button>
            </SheetTrigger>
            <SheetContent>
              <div className="flex flex-col space-y-4 mt-8">
                {navLinks.map((link) => (
                  <Link
                    key={link.href}
                    href={link.href}
                    onClick={() => setMobileMenuOpen(false)}
                    className="flex items-center gap-3 text-lg font-medium transition-colors hover:text-primary"
                  >
                    <link.icon className="h-5 w-5" />
                    {link.label}
                  </Link>
                ))}
                {session && isChef && (
                  <div className="pt-4 border-t">
                    <Link
                      href="/kitchen"
                      onClick={() => setMobileMenuOpen(false)}
                      className="flex items-center gap-3 text-lg font-medium transition-colors hover:text-primary text-orange-500"
                    >
                      <ChefHat className="h-5 w-5" />
                      Kitchen
                    </Link>
                  </div>
                )}
                {session && (
                  <div className="pt-4 border-t">
                    <Button
                      variant="ghost"
                      className="w-full justify-start text-lg font-medium"
                      onClick={() => {
                        setMobileMenuOpen(false)
                        handleSignOut()
                      }}
                    >
                      <LogOut className="mr-3 h-5 w-5" />
                      Sign out
                    </Button>
                  </div>
                )}
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </nav>
  )
}

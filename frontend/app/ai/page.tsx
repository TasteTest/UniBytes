"use client"

import { useState } from "react"
import { Sparkles, Loader2, Utensils, Heart, AlertCircle, Cookie, Plus } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { Badge } from "@/components/ui/badge"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { AIService, AIRequest, MenuItemDto } from "@/lib/services/AIService"
import { useToast } from "@/hooks/use-toast"
import { useCartStore, type MenuItem } from "@/lib/store"
import { formatCurrency } from "@/lib/utils"
import Image from "next/image"
import { useSession } from "next-auth/react"
import { useRole } from "@/lib/hooks/useAdmin"

const DIETARY_GOALS = [
    "Weight Loss",
    "Weight Gain",
    "Muscle Building",
    "General Health",
    "No Specific Goal"
]

const DIETARY_RESTRICTIONS = [
    "Vegetarian",
    "Vegan",
    "Gluten-Free",
    "Dairy-Free"
]

const MEAL_TYPES = [
    "Breakfast",
    "Lunch",
    "Dinner",
    "Snack",
    "Any"
]

const CALORIE_PREFERENCES = [
    "Low (<500)",
    "Medium (500-800)",
    "High (>800)",
    "No Preference"
]

const MENU_TYPES = [
    "Single Item",
    "Light Snack (2 items)",
    "Light Meal (3-4 items)",
    "Full Meal (5-6 items)",
    "Complete Menu (7+ items)"
]

export default function AIPage() {
    const { data: session } = useSession()
    const { canOrder } = useRole()
    const addItem = useCartStore((state) => state.addItem)
    const [formData, setFormData] = useState({
        dietaryGoal: "General Health",
        dietaryRestrictions: [] as string[],
        allergies: "",
        dislikes: "",
        preferredMealType: "Any",
        caloriePreference: "No Preference",
        menuType: "Light Meal (3-4 items)"
    })

    const [response, setResponse] = useState<{ text: string; reasoning?: string; products: MenuItemDto[] } | null>(null)
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const { toast } = useToast()

    const handleRestrictionToggle = (restriction: string) => {
        setFormData(prev => ({
            ...prev,
            dietaryRestrictions: prev.dietaryRestrictions.includes(restriction)
                ? prev.dietaryRestrictions.filter(r => r !== restriction)
                : [...prev.dietaryRestrictions, restriction]
        }))
    }

    const handleSubmit = async () => {
        setIsLoading(true)
        setError(null)
        setResponse(null)

        try {
            const aiService = new AIService()
            const request: AIRequest = {
                dietaryGoal: formData.dietaryGoal,
                dietaryRestrictions: formData.dietaryRestrictions,
                allergies: formData.allergies || undefined,
                dislikes: formData.dislikes || undefined,
                preferredMealType: formData.preferredMealType,
                caloriePreference: formData.caloriePreference,
                menuType: formData.menuType
            }

            const result = await aiService.getMenuRecommendations(request)

            setResponse({
                text: result.response,
                reasoning: result.reasoning,
                products: result.recommendedProducts || []
            })
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : "Failed to get menu recommendations"
            setError(errorMessage)
            toast({
                title: "Error",
                description: errorMessage,
                variant: "destructive"
            })
        } finally {
            setIsLoading(false)
        }
    }

    const handleAddToCart = (product: MenuItemDto) => {
        if (!session?.user) {
            toast({
                title: "Sign in required",
                description: "Please sign in to add items to your cart",
                variant: "destructive"
            })
            return
        }

        const menuItem: MenuItem = {
            id: product.id,
            name: product.name,
            description: product.description || '',
            price: product.price,
            currency: product.currency || 'RON',
            category: 'Recommended',
            available: product.available,
            image: product.imageUrl || undefined,
            preparationTime: 10
        }

        addItem({
            id: `cart-${Date.now()}`,
            menuItem,
            quantity: 1,
            modifiers: [],
        })

        toast({
            title: "Added to cart",
            description: `${product.name} has been added to your cart`,
            variant: "success",
        })
    }

    return (
        <div className="container py-8 max-w-5xl">
            <div className="flex flex-col gap-6">
                {/* Header */}
                <div className="text-center space-y-2">
                    <div className="flex items-center justify-center gap-2">
                        <Utensils className="h-8 w-8 text-primary" />
                        <h1 className="text-4xl font-bold text-gradient">AI Menu Recommendations</h1>
                    </div>
                    <p className="text-muted-foreground">
                        Tell us your preferences and we'll create a personalized menu just for you!
                    </p>
                </div>

                {/* Preference Form */}
                <Card className="card-glass">
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <Heart className="h-5 w-5 text-primary" />
                            Your Preferences
                        </CardTitle>
                        <CardDescription>
                            Fill out your dietary preferences below to get personalized recommendations
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-6">
                        {/* Dietary Goal */}
                        <div className="space-y-2">
                            <Label htmlFor="dietaryGoal">Dietary Goal *</Label>
                            <Select
                                value={formData.dietaryGoal}
                                onValueChange={(value) => setFormData(prev => ({ ...prev, dietaryGoal: value }))}
                            >
                                <SelectTrigger id="dietaryGoal">
                                    <SelectValue placeholder="Select your dietary goal" />
                                </SelectTrigger>
                                <SelectContent>
                                    {DIETARY_GOALS.map(goal => (
                                        <SelectItem key={goal} value={goal}>{goal}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {/* Dietary Restrictions */}
                        <div className="space-y-3">
                            <Label>Dietary Restrictions</Label>
                            <div className="grid grid-cols-2 gap-4">
                                {DIETARY_RESTRICTIONS.map(restriction => (
                                    <div key={restriction} className="flex items-center space-x-2">
                                        <Checkbox
                                            id={restriction}
                                            checked={formData.dietaryRestrictions.includes(restriction)}
                                            onCheckedChange={() => handleRestrictionToggle(restriction)}
                                        />
                                        <label
                                            htmlFor={restriction}
                                            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                                        >
                                            {restriction}
                                        </label>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* Allergies */}
                        <div className="space-y-2">
                            <Label htmlFor="allergies">Allergies (Optional)</Label>
                            <Input
                                id="allergies"
                                placeholder="E.g., peanuts, shellfish, etc."
                                value={formData.allergies}
                                onChange={(e) => setFormData(prev => ({ ...prev, allergies: e.target.value }))}
                                maxLength={500}
                            />
                        </div>

                        {/* Dislikes */}
                        <div className="space-y-2">
                            <Label htmlFor="dislikes">Foods You Dislike (Optional)</Label>
                            <Input
                                id="dislikes"
                                placeholder="E.g., mushrooms, olives, etc."
                                value={formData.dislikes}
                                onChange={(e) => setFormData(prev => ({ ...prev, dislikes: e.target.value }))}
                                maxLength={500}
                            />
                        </div>

                        {/* Row for Meal Type and Calorie Preference */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Preferred Meal Type */}
                            <div className="space-y-2">
                                <Label htmlFor="mealType">Preferred Meal Type *</Label>
                                <Select
                                    value={formData.preferredMealType}
                                    onValueChange={(value) => setFormData(prev => ({ ...prev, preferredMealType: value }))}
                                >
                                    <SelectTrigger id="mealType">
                                        <SelectValue placeholder="Select meal type" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {MEAL_TYPES.map(type => (
                                            <SelectItem key={type} value={type}>{type}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            {/* Calorie Preference */}
                            <div className="space-y-2">
                                <Label htmlFor="caloriePreference">Calorie Preference *</Label>
                                <Select
                                    value={formData.caloriePreference}
                                    onValueChange={(value) => setFormData(prev => ({ ...prev, caloriePreference: value }))}
                                >
                                    <SelectTrigger id="caloriePreference">
                                        <SelectValue placeholder="Select calorie preference" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {CALORIE_PREFERENCES.map(pref => (
                                            <SelectItem key={pref} value={pref}>{pref}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        {/* Menu Type */}
                        <div className="space-y-2">
                            <Label htmlFor="menuType">Menu Type *</Label>
                            <Select
                                value={formData.menuType}
                                onValueChange={(value) => setFormData(prev => ({ ...prev, menuType: value }))}
                            >
                                <SelectTrigger id="menuType">
                                    <SelectValue placeholder="Select menu type" />
                                </SelectTrigger>
                                <SelectContent>
                                    {MENU_TYPES.map(type => (
                                        <SelectItem key={type} value={type}>{type}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {/* Submit Button */}
                        <Button
                            onClick={handleSubmit}
                            disabled={isLoading}
                            className="w-full"
                            size="lg"
                        >
                            {isLoading ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Generating Recommendations...
                                </>
                            ) : (
                                <>
                                    <Sparkles className="mr-2 h-4 w-4" />
                                    Get Menu Recommendations
                                </>
                            )}
                        </Button>
                    </CardContent>
                </Card>

                {/* Response Section */}
                {(response || error) && (
                    <>
                        <Card className="card-glass">
                            <CardHeader>
                                <CardTitle className="flex items-center gap-2">
                                    <Cookie className="h-5 w-5 text-primary" />
                                    Your Personalized Menu
                                </CardTitle>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                {error && (
                                    <Alert variant="destructive">
                                        <AlertCircle className="h-4 w-4" />
                                        <AlertDescription>{error}</AlertDescription>
                                    </Alert>
                                )}

                                {response && (
                                    <div className="space-y-4">
                                        <div className="prose dark:prose-invert max-w-none">
                                            <div className="text-base leading-relaxed whitespace-pre-wrap bg-muted/30 p-4 rounded-lg">
                                                {response.text}
                                            </div>
                                        </div>

                                        {response.reasoning && (
                                            <div className="border-t pt-4">
                                                <h4 className="text-sm font-semibold mb-2 flex items-center gap-2">
                                                    <Sparkles className="h-4 w-4 text-muted-foreground" />
                                                    AI Reasoning
                                                </h4>
                                                <p className="text-sm text-muted-foreground whitespace-pre-wrap bg-muted/20 p-3 rounded-md">
                                                    {response.reasoning}
                                                </p>
                                            </div>
                                        )}
                                    </div>
                                )}
                            </CardContent>
                        </Card>

                        {/* Product Grid */}
                        {response && response.products && response.products.length > 0 && (
                            <Card className="card-glass">
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2">
                                        <Utensils className="h-5 w-5 text-primary" />
                                        Recommended Products
                                    </CardTitle>
                                    <CardDescription>
                                        Click on any item to add it to your cart
                                    </CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                                        {response.products.map((product) => (
                                            <Card
                                                key={product.id}
                                                className="card-glass border-none group"
                                            >
                                                <div className="flex flex-col h-full">
                                                    <div className="relative w-full h-44 overflow-hidden rounded-t-xl bg-muted">
                                                        {product.imageUrl ? (
                                                            <Image
                                                                src={product.imageUrl}
                                                                alt={product.name}
                                                                fill
                                                                sizes="(min-width: 1024px) 33vw, (min-width: 768px) 50vw, 100vw"
                                                                className="object-cover transition-transform duration-300 group-hover:scale-105"
                                                                priority={false}
                                                            />
                                                        ) : (
                                                            <div className="absolute inset-0 flex items-center justify-center text-muted-foreground text-sm">
                                                                No image
                                                            </div>
                                                        )}
                                                        <Badge variant="secondary" className="absolute top-3 right-3">
                                                            AI Recommended
                                                        </Badge>
                                                    </div>
                                                    <CardHeader className="pb-0">
                                                        <CardTitle className="group-hover:text-primary transition-colors">
                                                            {product.name}
                                                        </CardTitle>
                                                        <CardDescription className="mt-1 line-clamp-2">
                                                            {product.description || 'No description available'}
                                                        </CardDescription>
                                                    </CardHeader>
                                                    <CardFooter className="mt-auto flex items-center justify-between pt-4">
                                                        <span className="text-2xl font-bold text-primary">
                                                            {formatCurrency(product.price)}
                                                        </span>
                                                        {canOrder && (
                                                            <Button
                                                                size="sm"
                                                                onClick={() => handleAddToCart(product)}
                                                            >
                                                                <Plus className="h-4 w-4 mr-1" />
                                                                Add
                                                            </Button>
                                                        )}
                                                    </CardFooter>
                                                </div>
                                            </Card>
                                        ))}
                                    </div>
                                </CardContent>
                            </Card>
                        )}
                    </>
                )}

                {/* Loading State */}
                {isLoading && !response && !error && (
                    <Card className="card-glass">
                        <CardContent className="py-12">
                            <div className="flex flex-col items-center justify-center gap-4">
                                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                                <p className="text-muted-foreground">Creating your personalized menu...</p>
                            </div>
                        </CardContent>
                    </Card>
                )}
            </div>
        </div>
    )
}

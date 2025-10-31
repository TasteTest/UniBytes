import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { UtensilsCrossed, Zap, Award, Clock } from "lucide-react"

export default function Home() {
  const features = [
    {
      icon: UtensilsCrossed,
      title: "Diverse Menu",
      description: "Choose from a wide variety of delicious meals and snacks",
    },
    {
      icon: Zap,
      title: "Quick Ordering",
      description: "Order in seconds and skip the line",
    },
    {
      icon: Award,
      title: "Earn Rewards",
      description: "Get points with every order and redeem exciting rewards",
    },
    {
      icon: Clock,
      title: "Real-time Tracking",
      description: "Know exactly when your order will be ready",
    },
  ]

  return (
    <div className="flex flex-col">
      {/* Hero Section */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-saffron-100 via-orange-50 to-background dark:from-saffron-900/20 dark:via-orange-900/10 dark:to-background" />
        <div className="container relative py-24 md:py-32">
          <div className="mx-auto max-w-3xl text-center">
            <h1 className="text-4xl font-bold tracking-tight sm:text-6xl mb-6">
              Delicious Campus Meals,{" "}
              <span className="text-gradient">Delivered Fast</span>
            </h1>
            <p className="text-lg text-muted-foreground mb-8">
              Skip the line and order your favorite cafeteria meals with CampusEats.
              Quick, easy, and rewarding.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Button asChild size="lg" className="text-lg px-8">
                <Link href="/menu">Browse Menu</Link>
              </Button>
              <Button asChild variant="outline" size="lg" className="text-lg px-8">
                <Link href="/auth/signin">Sign In with Google</Link>
              </Button>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="container py-24">
        <h2 className="text-3xl font-bold text-center mb-12">
          Why Choose CampusEats?
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {features.map((feature, index) => (
            <Card key={index} className="card-glass border-none">
              <CardHeader>
                <feature.icon className="h-12 w-12 text-primary mb-4" />
                <CardTitle>{feature.title}</CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription>{feature.description}</CardDescription>
              </CardContent>
            </Card>
          ))}
        </div>
      </section>

      {/* CTA Section */}
      <section className="container py-24">
        <Card className="card-glass border-none">
          <CardContent className="py-12 px-6 text-center">
            <h2 className="text-3xl font-bold mb-4">Ready to Get Started?</h2>
            <p className="text-lg text-muted-foreground mb-8 max-w-2xl mx-auto">
              Sign in with your Google account and start enjoying the convenience
              of CampusEats today.
            </p>
            <Button asChild size="lg" className="text-lg px-8">
              <Link href="/auth/signin">Get Started</Link>
            </Button>
          </CardContent>
        </Card>
      </section>
    </div>
  )
}


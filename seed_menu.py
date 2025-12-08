#!/usr/bin/env python3
"""
Script to seed menu data into the UniBytes backend API
"""

import requests
import json
from typing import Dict, List

# API base URL
BASE_URL = "http://localhost:5267/api"

# Menu categories
CATEGORIES = [
    {"name": "Sandwiches", "description": "Fresh sandwiches made to order", "displayOrder": 1, "isActive": True},
    {"name": "Salads", "description": "Healthy salads with fresh ingredients", "displayOrder": 2, "isActive": True},
    {"name": "Pizza", "description": "Hand-tossed pizzas with premium toppings", "displayOrder": 3, "isActive": True},
    {"name": "Burgers", "description": "Juicy burgers with quality beef", "displayOrder": 4, "isActive": True},
    {"name": "Bowls", "description": "Nutritious bowl meals", "displayOrder": 5, "isActive": True},
    {"name": "Desserts", "description": "Sweet treats and desserts", "displayOrder": 6, "isActive": True},
    {"name": "Drinks", "description": "Refreshing beverages", "displayOrder": 7, "isActive": True},
]

# Menu items mapped to category names
MENU_ITEMS = {
    "Sandwiches": [
        {"name": "Grilled Chicken Sandwich", "description": "Juicy grilled chicken with fresh vegetables and house sauce", "price": 8.99},
        {"name": "Club Sandwich", "description": "Triple-decker with turkey, bacon, lettuce and tomato", "price": 10.49},
        {"name": "BLT Sandwich", "description": "Crispy bacon, lettuce, tomato on toasted bread", "price": 7.99},
    ],
    "Salads": [
        {"name": "Caesar Salad", "description": "Fresh romaine lettuce with parmesan and homemade dressing", "price": 6.99},
        {"name": "Greek Salad", "description": "Feta cheese, olives, cucumber, tomatoes with olive oil", "price": 7.49},
        {"name": "Cobb Salad", "description": "Mixed greens with chicken, egg, bacon, avocado", "price": 9.49},
    ],
    "Pizza": [
        {"name": "Margherita Pizza", "description": "Classic pizza with fresh mozzarella and basil", "price": 11.99},
        {"name": "Pepperoni Pizza", "description": "Classic pepperoni with mozzarella cheese", "price": 12.99},
        {"name": "Veggie Pizza", "description": "Bell peppers, mushrooms, onions, olives", "price": 11.49},
    ],
    "Burgers": [
        {"name": "Cheeseburger", "description": "Angus beef patty with cheese, lettuce, tomato", "price": 9.99},
        {"name": "Bacon Burger", "description": "Beef patty with crispy bacon and BBQ sauce", "price": 11.49},
        {"name": "Mushroom Swiss Burger", "description": "Sautéed mushrooms and swiss cheese", "price": 10.99},
    ],
    "Bowls": [
        {"name": "Veggie Bowl", "description": "Quinoa with roasted vegetables and tahini dressing", "price": 7.99},
        {"name": "Teriyaki Bowl", "description": "Chicken teriyaki over rice with steamed vegetables", "price": 9.99},
        {"name": "Poke Bowl", "description": "Fresh ahi tuna with rice, edamame, and wasabi mayo", "price": 12.99},
        {"name": "Smoothie Bowl", "description": "Acai berry smoothie topped with granola and fresh fruit", "price": 8.49},
    ],
    "Desserts": [
        {"name": "Chocolate Chip Cookie", "description": "Freshly baked with premium chocolate chips", "price": 2.99},
        {"name": "Brownie Sundae", "description": "Warm brownie with vanilla ice cream and chocolate sauce", "price": 5.99},
        {"name": "Cheesecake Slice", "description": "New York style cheesecake with berry compote", "price": 4.99},
    ],
    "Drinks": [
        {"name": "Fresh Lemonade", "description": "Freshly squeezed lemons with a hint of mint", "price": 3.49},
        {"name": "Iced Coffee", "description": "Cold brew coffee with your choice of milk", "price": 4.49},
        {"name": "Green Juice", "description": "Kale, spinach, apple, cucumber, lemon", "price": 5.99},
    ],
}


def create_categories() -> Dict[str, str]:
    """Create menu categories and return mapping of category name to ID"""
    print("Creating categories...")
    category_map = {}
    
    # First, try to get existing categories
    try:
        response = requests.get(f"{BASE_URL}/categories")
        if response.status_code == 200:
            existing_categories = response.json()
            for cat in existing_categories:
                category_map[cat["name"]] = cat["id"]
                print(f"✓ Found existing category: {cat['name']} (ID: {cat['id']})")
    except Exception as e:
        print(f"Warning: Could not fetch existing categories: {str(e)}")
    
    # Create categories that don't exist yet
    for category in CATEGORIES:
        if category["name"] in category_map:
            continue  # Skip if already exists
            
        try:
            response = requests.post(
                f"{BASE_URL}/categories",
                json=category,
                headers={"Content-Type": "application/json"}
            )
            
            if response.status_code == 201:
                created = response.json()
                category_map[category["name"]] = created["id"]
                print(f"✓ Created category: {category['name']} (ID: {created['id']})")
            elif response.status_code == 500 and "duplicate key" in response.text:
                print(f"⚠ Category {category['name']} already exists")
            else:
                print(f"✗ Failed to create category {category['name']}: {response.status_code}")
        except Exception as e:
            print(f"✗ Error creating category {category['name']}: {str(e)}")
    
    return category_map


def create_menu_items(category_map: Dict[str, str]):
    """Create menu items using the category mapping"""
    print("\nCreating menu items...")
    
    for category_name, items in MENU_ITEMS.items():
        category_id = category_map.get(category_name)
        
        if not category_id:
            print(f"✗ Category {category_name} not found, skipping items")
            continue
        
        for item in items:
            menu_item = {
                "categoryId": category_id,
                "name": item["name"],
                "description": item["description"],
                "price": item["price"],
                "currency": "USD",
                "available": True,
                "components": None
            }
            
            try:
                response = requests.post(
                    f"{BASE_URL}/menuitems",
                    json=menu_item,
                    headers={"Content-Type": "application/json"}
                )
                
                if response.status_code == 201:
                    created = response.json()
                    print(f"✓ Created menu item: {item['name']} (ID: {created['id']})")
                else:
                    print(f"✗ Failed to create menu item {item['name']}: {response.status_code}")
                    print(f"  Response: {response.text[:500]}")  # Limit output
            except Exception as e:
                print(f"✗ Error creating menu item {item['name']}: {str(e)}")


def main():
    print("=" * 60)
    print("UniBytes Menu Seeding Script")
    print("=" * 60)
    print(f"Target API: {BASE_URL}")
    print()
    
    # Check if API is reachable
    try:
        response = requests.get(f"{BASE_URL}/categories", timeout=5)
        print(f"✓ API is reachable (Status: {response.status_code})")
    except Exception as e:
        print(f"✗ Cannot reach API: {str(e)}")
        print("Please ensure the backend is running on http://localhost:5267")
        return
    
    print()
    
    # Create categories first
    category_map = create_categories()
    
    if not category_map:
        print("\n✗ No categories were created. Aborting.")
        return
    
    # Create menu items
    create_menu_items(category_map)
    
    print()
    print("=" * 60)
    print("Seeding complete!")
    print("=" * 60)


if __name__ == "__main__":
    main()

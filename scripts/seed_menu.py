#!/usr/bin/env python3
"""
Script to seed menu data into the UniBytes backend API
"""

import os
import random
import requests
import json
import mimetypes
from typing import Dict, List, Iterator, Optional, Tuple

# API base URL (required). Provide via BACKEND_URL env var, e.g.
# export BACKEND_URL="https://your-backend.azurecontainerapps.io/api"
BACKEND_URL = os.getenv("BACKEND_URL", "").rstrip("/")
if not BACKEND_URL:
    raise SystemExit("Please set BACKEND_URL env var pointing to the backend /api base (e.g. https://.../api)")

# Image directory containing menu item images (default: scripts/images next to this script)
DEFAULT_IMAGE_DIR = os.path.join(os.path.dirname(__file__), "images")
IMAGE_DIR = os.getenv("IMAGE_DIR", DEFAULT_IMAGE_DIR)


def list_image_files(directory: str) -> List[str]:
    """Return sorted list of image file paths in the directory"""
    exts = {".png", ".jpg", ".jpeg", ".webp"}
    files = []
    for name in sorted(os.listdir(directory)):
        if os.path.splitext(name.lower())[1] in exts:
            files.append(os.path.join(directory, name))
    return files


def upload_image(menu_item_id: str, image_path: str):
    """Upload image for a given menu item"""
    with open(image_path, "rb") as f:
        mime_type, _ = mimetypes.guess_type(image_path)
        content_type = mime_type or "application/octet-stream"
        files = {"image": (os.path.basename(image_path), f, content_type)}
        try:
            resp = requests.post(
                f"{BACKEND_URL}/menuitems/{menu_item_id}/image",
                files=files,
                timeout=30,
            )
            if resp.status_code == 200:
                data = resp.json()
                print(f"  ✓ Uploaded image -> {data.get('imageUrl')}")
            else:
                print(f"  ✗ Image upload failed ({resp.status_code}) for {image_path}: {resp.text[:200]}")
        except Exception as e:
            print(f"  ✗ Error uploading image {image_path}: {e}")

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
        response = requests.get(f"{BACKEND_URL}/categories")
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
                f"{BACKEND_URL}/categories",
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
    # Ensure IMAGE_DIR exists and list images
    images = []
    try:
        images = list_image_files(IMAGE_DIR)
    except FileNotFoundError:
        images = []

    if not images:
        print(f"⚠ No images found in '{IMAGE_DIR}'; items will be created without images.")

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
                "currency": "ron",
                "available": True,
                "components": None
            }
            
            try:
                response = requests.post(
                    f"{BACKEND_URL}/menuitems",
                    json=menu_item,
                    headers={"Content-Type": "application/json"}
                )
                
                if response.status_code == 201:
                    # API may or may not include the created entity's ID in the JSON body.
                    menu_id = None
                    try:
                        created = response.json()
                        menu_id = created.get("id") if isinstance(created, dict) else None
                    except Exception:
                        created = None

                    # Fallback: parse Location header for the new resource ID
                    if not menu_id:
                        location = response.headers.get("Location") or response.headers.get("location")
                        if location:
                            menu_id = location.rstrip("/").split("/")[-1]

                    if menu_id:
                        print(f"✓ Created menu item: {item['name']} (ID: {menu_id})")
                        # Try to upload an image if available
                        if images:
                            image_path = random.choice(images)
                            upload_image(menu_id, image_path)
                        else:
                            print(f"  ⚠ No image available for {item['name']}")
                    else:
                        print(f"✓ Created menu item: {item['name']} (ID: unknown) - missing id in response")
                else:
                    print(f"✗ Failed to create menu item {item['name']}: {response.status_code}")
                    print(f"  Response: {response.text[:500]}")  # Limit output
            except Exception as e:
                print(f"✗ Error creating menu item {item['name']}: {str(e)}")


def main():
    print("=" * 60)
    print("UniBytes Menu Seeding Script")
    print("=" * 60)
    print(f"Target API: {BACKEND_URL}")
    print()
    
    # Check if API is reachable
    try:
        response = requests.get(f"{BACKEND_URL}/categories", timeout=5)
        print(f"✓ API is reachable (Status: {response.status_code})")
    except Exception as e:
        print(f"✗ Cannot reach API: {str(e)}")
        print("Please ensure the backend is reachable and BACKEND_URL is correct.")
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

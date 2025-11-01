#!/usr/bin/env bash

# CampusEats - Complete Sprint Automation
# This script uses gh CLI to manage project iterations and items

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Configuration
ORG="TasteTest"
PROJECT_NUMBER="1"

GH_CMD="/opt/homebrew/bin/gh"
if [ ! -f "$GH_CMD" ]; then
    GH_CMD="/usr/local/bin/gh"
fi

if [ ! -f "$GH_CMD" ]; then
    echo -e "${RED}Error: GitHub CLI not installed${NC}"
    exit 1
fi

if ! $GH_CMD auth status &> /dev/null; then
    echo -e "${RED}Error: Not authenticated${NC}"
    exit 1
fi

echo -e "${GREEN}╔═══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   CampusEats Complete Sprint Automation              ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════════════════════╝${NC}"
echo ""

echo -e "${YELLOW}Note: GitHub Projects v2 iterations must be created via web UI${NC}"
echo -e "${YELLOW}This script will help you organize items efficiently${NC}"
echo ""

# Instructions
cat << 'INSTRUCTIONS'
═══════════════════════════════════════════════════════
STEP 1: Create Iterations in GitHub Project UI
═══════════════════════════════════════════════════════

1. Go to: https://github.com/orgs/TasteTest/projects/1
2. Click on "⚙️" (Settings) in the top right
3. Scroll down to "Iterations"
4. Click "Add Iteration Field"
5. Configure:
   - Field name: "Sprint"
   - Duration: 1 week
   - Start day: Monday
   
6. Add 12 iterations:
   Sprint 1 - Start: Nov 3, 2025 (1 week)
   Sprint 2 - Start: Nov 10, 2025 (1 week)
   Sprint 3 - Start: Nov 17, 2025 (1 week)
   Sprint 4 - Start: Nov 24, 2025 (1 week)
   Sprint 5 - Start: Dec 1, 2025 (1 week)
   Sprint 6 - Start: Dec 8, 2025 (1 week)
   Sprint 7 - Start: Dec 15, 2025 (1 week)
   Sprint 8 - Start: Dec 22, 2025 (1 week)
   Sprint 9 - Start: Dec 29, 2025 (1 week)
   Sprint 10 - Start: Jan 5, 2026 (1 week)
   Sprint 11 - Start: Jan 12, 2026 (1 week)
   Sprint 12 - Start: Jan 19, 2026 (1 week)

Press Enter when iterations are created...
INSTRUCTIONS

read -p ""

echo ""
echo -e "${GREEN}✓ Iterations should now be created${NC}"
echo ""

cat << 'ORGANIZING'
═══════════════════════════════════════════════════════
STEP 2: Bulk Assign Items to Sprints
═══════════════════════════════════════════════════════

Follow these steps to quickly assign items:

1. In your project, click "View" → "New view" → "Table"
2. Add filters for each sprint using the guidance below
3. Select all filtered items (shift+click)
4. Bulk edit → Set "Sprint" field to the sprint number

ORGANIZING

echo ""
echo -e "${CYAN}═══ Sprint 1: Foundation & Infrastructure ═══${NC}"
cat << 'SPRINT1'
Filter: title contains any of:
  - "DbContext"
  - "MediatR Pipeline"
  - "Repository Pattern"
  - "Event Listeners"

Expected items: ~15 items (infrastructure setup)
SPRINT1

echo ""
echo -e "${CYAN}═══ Sprint 2: Authentication & User Management ═══${NC}"
cat << 'SPRINT2'
Filter: title contains any of:
  - "OAuth"
  - "Profile Viewing"
  - "Profile Editing"
  - "Domain Events"

Expected items: ~5 items
SPRINT2

echo ""
echo -e "${CYAN}═══ Sprint 3: Menu Management ═══${NC}"
cat << 'SPRINT3'
Filter: title contains any of:
  - "Menu Category"
  - "Menu Item"
  - "Unavailable"

Exclude: "Caching", "Search"

Expected items: ~8 items
SPRINT3

echo ""
echo -e "${CYAN}═══ Sprint 4: Order Creation & Management ═══${NC}"
cat << 'SPRINT4'
Filter: title contains any of:
  - "Create New Order"
  - "Add Items to Order"
  - "View Order Details"
  - "List My Orders"
  - "Update Order Status"

And service is "Orders"

Expected items: ~6 items
SPRINT4

echo ""
echo -e "${CYAN}═══ Sprint 5: Payment Integration ═══${NC}"
cat << 'SPRINT5'
Filter: title contains any of:
  - "Stripe"
  - "Payment Intent"
  - "Payment Success"
  - "Payment Failure"
  - "Idempotency" (Payments service)
  - "Secret Management"
  - "Background Jobs" (Payments)

Expected items: ~9 items
SPRINT5

echo ""
echo -e "${CYAN}═══ Sprint 6: Kitchen Queue & Order Processing ═══${NC}"
cat << 'SPRINT6'
Filter: title contains any of:
  - "Kitchen Order from Order Service"
  - "View Kitchen Queue"
  - "Claim"
  - "Start Order Preparation"
  - "Mark Item as Ready"
  - "Mark Order as Complete"

Expected items: ~5 items
SPRINT6

echo ""
echo -e "${CYAN}═══ Sprint 7: Kitchen Operations & Real-time ═══${NC}"
cat << 'SPRINT7'
Filter: title contains any of:
  - "Filter Queue"
  - "Real-Time Updates"
  - "SignalR"
  - "Special Request Notes"
  - "Cancel Order"
  - "Cancel Kitchen"

Expected items: ~5 items
SPRINT7

echo ""
echo -e "${CYAN}═══ Sprint 8: Loyalty System Foundation ═══${NC}"
cat << 'SPRINT8'
Filter: title contains any of:
  - "Create Loyalty Account"
  - "View My Loyalty Account"
  - "Earn Points"
  - "Points Transaction History"
  - "Upgrade Tier"
  - "Concurrency Control"

Expected items: ~6 items
SPRINT8

echo ""
echo -e "${CYAN}═══ Sprint 9: Loyalty Rewards & Menu Enhancements ═══${NC}"
cat << 'SPRINT9'
Filter: title contains any of:
  - "Available Rewards"
  - "Redeem Points"
  - "Tier Benefits"
  - "Redemption History"
  - "Caching Strategy"
  - "Search Menu Items"

Expected items: ~6 items
SPRINT9

echo ""
echo -e "${CYAN}═══ Sprint 10: Admin User & Order Tools ═══${NC}"
cat << 'SPRINT10'
Filter: title contains any of:
  - "List All Users"
  - "User Activity Dashboard"
  - "Track User Activity Events"
  - "User Deactivation"
  - "Promote User to Admin"
  - "View All Orders (Admin)"

Expected items: ~6 items
SPRINT10

echo ""
echo -e "${CYAN}═══ Sprint 11: Analytics & Reporting ═══${NC}"
cat << 'SPRINT11'
Filter: title contains any of:
  - "Order Statistics"
  - "Payment History"
  - "Payment Statistics"
  - "Kitchen Performance Metrics"
  - "Order Preparation History"
  - "Loyalty Statistics"

Expected items: ~6 items
SPRINT11

echo ""
echo -e "${CYAN}═══ Sprint 12: Polish & Advanced Features ═══${NC}"
cat << 'SPRINT12'
Filter: title contains any of:
  - "Reorder Previous Order"
  - "Filter Menu Items by Price"
  - "Item Preparation Details"
  - "Delete Menu"
  - "Manual Points"
  - "Manually Update Order"
  - "Deactivate Loyalty"
  - "Cancel Redemption"
  - "Mock Payments"
  - "Simulate Payment"

Expected items: ~11 items
SPRINT12

echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}All Done!${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}Additional Setup:${NC}"
echo ""
echo "1. Set Sprint 1 as 'Current Iteration'"
echo "2. Create these project views:"
echo "   • Current Sprint (filter: iteration = @current)"
echo "   • Roadmap (group by: iteration)"
echo "   • By Service (group by: service)"
echo "   • Backlog (filter: iteration is empty)"
echo ""
echo "3. Add custom fields if not present:"
echo "   • Priority (Single select: P0, P1, P2, P3)"
echo "   • Story Points (Number: 1, 2, 3, 5, 8)"
echo "   • Service (Single select: Users, Menu, Orders, Kitchen, Payments, Loyalty)"
echo ""
echo -e "${GREEN}Your project is now fully organized! 🎉${NC}"
echo ""

#!/bin/bash

# CampusEats - Sprint Organization Script
# This script organizes project items into iterations based on priority and dependencies

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
ORG="TasteTest"
PROJECT_NUMBER="1"

# Find gh CLI path
GH_CMD="/opt/homebrew/bin/gh"
if [ ! -f "$GH_CMD" ]; then
    GH_CMD="/usr/local/bin/gh"
fi

# Check if gh CLI is installed
if [ ! -f "$GH_CMD" ]; then
    echo -e "${RED}Error: GitHub CLI (gh) is not installed${NC}"
    exit 1
fi

# Check if authenticated
if ! $GH_CMD auth status &> /dev/null; then
    echo -e "${RED}Error: Not authenticated with GitHub CLI${NC}"
    exit 1
fi

echo -e "${GREEN}Starting Sprint Organization${NC}"
echo "Organization: $ORG"
echo "Project: $PROJECT_NUMBER"
echo ""

# Get project ID
echo -e "${YELLOW}Fetching project details...${NC}"
PROJECT_DATA=$($GH_CMD project view $PROJECT_NUMBER --owner $ORG --format json)
PROJECT_ID=$(echo $PROJECT_DATA | jq -r '.id')

echo "Project ID: $PROJECT_ID"
echo ""

# Sprint 1 (Current Iteration) - Foundation & Core Auth (P0 items + critical P1)
# Focus: Set up infrastructure, OAuth, basic user profiles, menu display, order creation
echo -e "${BLUE}=== Sprint 1: Foundation & Core Authentication ===${NC}"
echo "Items to include:"
echo "  • All P0 infrastructure tasks (EF Core, MediatR, DbContext setup)"
echo "  • Google OAuth Sign-In"
echo "  • User Profile Viewing"
echo "  • List Menu Categories & Items (public view)"
echo "  • View Menu Item Details"
echo "  • Create New Order"
echo "  • Add Items to Order"
echo ""

# Sprint 2 - Payment & Order Processing (P0 + critical P1)
# Focus: Complete order flow with payments
echo -e "${BLUE}=== Sprint 2: Payment & Order Processing ===${NC}"
echo "Items to include:"
echo "  • Stripe SDK Integration"
echo "  • Create Payment Intent"
echo "  • Confirm Payment Success"
echo "  • Handle Payment Failure"
echo "  • Prevent Duplicate Payments (Idempotency)"
echo "  • View Order Details"
echo "  • List My Orders"
echo "  • Update Order Status"
echo "  • Create Kitchen Order from Order Service"
echo "  • View Kitchen Queue"
echo ""

# Sprint 3 - Kitchen Operations & Order Management (P1)
# Focus: Kitchen workflow and order tracking
echo -e "${BLUE}=== Sprint 3: Kitchen Operations ===${NC}"
echo "Items to include:"
echo "  • Claim/Start Order Preparation"
echo "  • Mark Item as Ready"
echo "  • Mark Order as Complete"
echo "  • Filter Kitchen Queue by Station"
echo "  • Real-Time Updates (SignalR)"
echo "  • Cancel Order"
echo "  • User Profile Editing"
echo "  • Update Menu Category"
echo "  • Update Menu Item"
echo "  • Mark Item as Unavailable"
echo ""

# Sprint 4 - Loyalty & Enhanced Features (P1 + P2)
# Focus: Loyalty system and admin features
echo -e "${BLUE}=== Sprint 4: Loyalty System ===${NC}"
echo "Items to include:"
echo "  • Create Loyalty Account"
echo "  • View My Loyalty Account"
echo "  • Earn Points on Order Completion"
echo "  • View Points Transaction History"
echo "  • View Available Rewards"
echo "  • Redeem Points for Reward"
echo "  • Auto-Upgrade Tier Based on Points"
echo "  • Implement Caching Strategy (Menu)"
echo "  • Search Menu Items"
echo "  • Add Special Request Notes to Items"
echo ""

# Sprint 5 - Admin Tools & Analytics (P2 + P3)
# Focus: Admin dashboards and reporting
echo -e "${BLUE}=== Sprint 5: Admin & Analytics ===${NC}"
echo "Items to include:"
echo "  • List All Users (Admin)"
echo "  • View All Orders (Admin)"
echo "  • View Payment History"
echo "  • View Kitchen Performance Metrics"
echo "  • View Order Statistics (Admin)"
echo "  • View Payment Statistics (Admin)"
echo "  • View Loyalty Statistics (Admin)"
echo "  • View User Activity Dashboard"
echo "  • Track User Activity Events"
echo "  • User Deactivation"
echo ""

# Sprint 6 - Polish & Advanced Features (P2 + P3)
# Focus: Advanced features and edge cases
echo -e "${BLUE}=== Sprint 6: Polish & Advanced Features ===${NC}"
echo "Items to include:"
echo "  • Reorder Previous Order"
echo "  • Filter Menu Items by Price Range"
echo "  • View Redemption History"
echo "  • View Tier Benefits"
echo "  • Cancel Kitchen Order"
echo "  • View Order Preparation History"
echo "  • View Item Preparation Details"
echo "  • Delete Menu Category (Admin)"
echo "  • Delete Menu Item (Admin)"
echo "  • Manual Points Adjustment (Admin)"
echo "  • Manually Update Order (Admin)"
echo "  • Promote User to Admin"
echo "  • Deactivate Loyalty Account (Admin)"
echo "  • Cancel Redemption (Admin)"
echo "  • Support Mock Payments (Dev/Test)"
echo "  • Simulate Payment Scenarios"
echo ""

echo -e "${GREEN}Sprint organization complete!${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Manually assign items to iterations in your GitHub Project"
echo "2. Set iteration dates (2-week sprints recommended)"
echo "3. Sprint 1 should contain ~20-25 story points (foundation work)"
echo "4. Subsequent sprints: ~30-35 story points each"
echo ""
echo "View your project at: https://github.com/orgs/$ORG/projects/$PROJECT_NUMBER"

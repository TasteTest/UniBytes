#!/usr/bin/env bash

# CampusEats - GitHub Project Sprint & Roadmap Setup
# This script creates iterations and organizes items into sprints with a roadmap

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
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

echo -e "${GREEN}╔═══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   CampusEats Sprint & Roadmap Setup                  ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════════════════════╝${NC}"
echo ""
echo "Organization: $ORG"
echo "Project: $PROJECT_NUMBER"
echo ""

# Get current date
CURRENT_DATE=$(date +%Y-%m-%d)

# Calculate sprint dates (1 week each, starting next Monday)
get_next_monday() {
    local current_day=$(date +%u)  # 1=Monday, 7=Sunday
    if [ $current_day -eq 1 ]; then
        # If today is Monday, use today
        date +%Y-%m-%d
    else
        # Calculate days until next Monday
        local days_until_monday=$((8 - current_day))
        date -v+${days_until_monday}d +%Y-%m-%d
    fi
}

add_weeks() {
    local start_date=$1
    local weeks=$2
    date -j -v+${weeks}w -f "%Y-%m-%d" "$start_date" +%Y-%m-%d
}

SPRINT_1_START=$(get_next_monday)
SPRINT_1_END=$(add_weeks "$SPRINT_1_START" 1)

echo -e "${CYAN}Sprint Schedule:${NC}"
echo "Sprint 1 starts: $SPRINT_1_START"
echo ""

# Create roadmap markdown
cat > ROADMAP.md << 'EOF'
# CampusEats Development Roadmap

## 🎯 Project Timeline
**Duration:** 12 weeks (12 × 1-week sprints)  
**Start Date:** TBD (Set in GitHub Project)  
**Target Completion:** 12 weeks from start

---

## 📅 Sprint Overview

### 🏗️ Phase 1: Foundation (Weeks 1-2)
**Goal:** Set up infrastructure and authentication

#### Sprint 1: Foundation & Infrastructure
- Set up all microservices with EF Core, MediatR, and Repository Pattern
- Configure event listeners for Kitchen and Loyalty services
- **Deliverable:** Complete backend infrastructure ready for feature development

#### Sprint 2: Authentication & User Management
- Implement Google OAuth with NextAuth
- User profile viewing and editing
- Domain events for cross-service communication
- **Deliverable:** Users can sign in and manage their profiles

---

### 🍽️ Phase 2: Core Features (Weeks 3-5)
**Goal:** Build menu, ordering, and payment capabilities

#### Sprint 3: Menu Management
- Complete menu category and item CRUD operations
- Public menu viewing
- Admin menu management tools
- **Deliverable:** Functional menu system with admin controls

#### Sprint 4: Order Creation & Management
- Order placement and item management
- Order history and status tracking
- Idempotency for reliable order processing
- **Deliverable:** Students can create and track orders

#### Sprint 5: Payment Integration
- Full Stripe integration with test mode
- Payment intent creation and confirmation
- Idempotency and secure secret management
- **Deliverable:** Complete order-to-payment flow

---

### 👨‍🍳 Phase 3: Kitchen Operations (Weeks 6-7)
**Goal:** Enable kitchen staff workflow

#### Sprint 6: Kitchen Queue & Order Processing
- Kitchen order queue from Orders service
- Order preparation workflow (claim, ready, complete)
- **Deliverable:** Kitchen staff can manage incoming orders

#### Sprint 7: Kitchen Operations & Real-time
- Real-time updates using SignalR
- Queue filtering by station
- Special request notes
- Order cancellation
- **Deliverable:** Live kitchen dashboard with real-time updates

---

### 🎁 Phase 4: Loyalty System (Weeks 8-9)
**Goal:** Implement rewards and loyalty program

#### Sprint 8: Loyalty System Foundation
- Loyalty account creation and management
- Points earning on order completion
- Automatic tier upgrades (Bronze, Silver, Gold)
- Concurrency control for point transactions
- **Deliverable:** Working loyalty program with tiers

#### Sprint 9: Loyalty Rewards & Menu Enhancements
- Rewards redemption system
- Tier benefits display
- Menu caching for performance
- Menu search functionality
- **Deliverable:** Complete loyalty redemption flow

---

### 🛠️ Phase 5: Admin & Analytics (Weeks 10-11)
**Goal:** Build admin tools and reporting

#### Sprint 10: Admin User & Order Tools
- User management dashboard
- User activity tracking
- Admin role management
- Order administration tools
- **Deliverable:** Admin can manage users and orders

#### Sprint 11: Analytics & Reporting
- Order statistics and analytics
- Payment history and reports
- Kitchen performance metrics
- Loyalty program analytics
- **Deliverable:** Complete analytics dashboards

---

### ✨ Phase 6: Polish (Week 12)
**Goal:** Complete remaining features and testing tools

#### Sprint 12: Polish & Advanced Features
- Reorder functionality
- Advanced filtering
- Admin cleanup tools
- Mock payment modes for testing
- **Deliverable:** Production-ready application

---

## 🎯 Key Milestones

| Milestone | Sprint | Description |
|-----------|--------|-------------|
| 🏗️ Infrastructure Complete | Sprint 1 | All services set up with databases |
| 🔐 Authentication Live | Sprint 2 | Users can sign in with Google |
| 🍽️ Menu System Live | Sprint 3 | Public can view menu, admins can manage |
| 📦 Orders Working | Sprint 4 | Students can place orders |
| 💳 Payments Integrated | Sprint 5 | Complete checkout flow with Stripe |
| 👨‍🍳 Kitchen Dashboard | Sprint 7 | Real-time kitchen operations |
| 🎁 Loyalty Program | Sprint 9 | Full rewards system operational |
| 📊 Admin Tools Complete | Sprint 11 | Full admin and analytics capabilities |
| 🚀 Production Ready | Sprint 12 | Application ready for deployment |

---

## 📊 Progress Tracking

### Velocity & Capacity
- **Average Sprint Capacity:** ~20-30 story points/week
- **Total Story Points:** ~277 points
- **Team Size:** Adjust based on your team

### Success Metrics
- [ ] All P0 features completed by Sprint 7
- [ ] All P1 features completed by Sprint 9
- [ ] Complete test coverage for payment flows
- [ ] Real-time updates performing under 100ms latency
- [ ] All admin dashboards functional

---

## 🔄 Iteration Process

### Weekly Sprint Cycle
1. **Monday:** Sprint planning & kickoff
2. **Tuesday-Thursday:** Development & daily standups
3. **Friday:** Sprint review, retrospective, and planning for next week

### Definition of Done
- [ ] Code reviewed and approved
- [ ] Unit tests written and passing
- [ ] Integration tests for cross-service features
- [ ] Documentation updated
- [ ] Deployed to staging environment
- [ ] Acceptance criteria met

---

## 🚀 Deployment Strategy

### Environments
- **Development:** Continuous deployment from feature branches
- **Staging:** Deploy at end of each sprint
- **Production:** Deploy after Sprint 5 (MVP), then bi-weekly

### MVP Launch (End of Sprint 5)
Includes:
- Google OAuth authentication
- Menu browsing
- Order placement
- Payment processing
- Basic kitchen queue

### Full Launch (End of Sprint 12)
Complete feature set with:
- All loyalty features
- Admin tools
- Analytics dashboards
- Advanced features

---

## 📝 Notes

- Sprint dates will be set in GitHub Project iterations
- Story point estimates may be adjusted based on actual velocity
- P0 items take priority; P3 items may be deferred if needed
- Cross-service integration points require extra coordination

---

**Last Updated:** Generated automatically  
**Project:** CampusEats - UniBytes  
**Organization:** TasteTest
EOF

echo -e "${GREEN}✓ Created ROADMAP.md${NC}"
echo ""

echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}Sprint Assignment Guide${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
echo ""

echo -e "${CYAN}Sprint 1: Foundation & Infrastructure${NC}"
echo "  • Set Up EF Core DbContext (all 6 services)"
echo "  • Set Up MediatR Pipeline (all 6 services)"
echo "  • Implement Repository Pattern (Users Service)"
echo "  • Set Up Event Listeners (Kitchen, Loyalty)"
echo ""

echo -e "${CYAN}Sprint 2: Authentication & User Management${NC}"
echo "  • Google OAuth Sign-In"
echo "  • User Profile Viewing"
echo "  • User Profile Editing"
echo "  • Implement Domain Events (Orders, Loyalty)"
echo ""

echo -e "${CYAN}Sprint 3: Menu Management${NC}"
echo "  • Create/List Menu Categories & Items"
echo "  • Update Menu Items and Categories"
echo "  • Mark items as unavailable"
echo ""

echo -e "${CYAN}Sprint 4: Order Creation & Management${NC}"
echo "  • Create New Order & Add Items"
echo "  • View Order Details & History"
echo "  • Update Order Status"
echo "  • Implement Idempotency"
echo ""

echo -e "${CYAN}Sprint 5: Payment Integration${NC}"
echo "  • Stripe SDK Integration"
echo "  • Payment Intent Creation & Confirmation"
echo "  • Payment Failure Handling"
echo "  • Idempotency & Secret Management"
echo ""

echo -e "${CYAN}Sprint 6: Kitchen Queue & Order Processing${NC}"
echo "  • Create Kitchen Orders"
echo "  • View Kitchen Queue"
echo "  • Order Preparation Workflow"
echo ""

echo -e "${CYAN}Sprint 7: Kitchen Operations & Real-time${NC}"
echo "  • Real-Time Updates (SignalR)"
echo "  • Queue Filtering"
echo "  • Order Cancellation"
echo ""

echo -e "${CYAN}Sprint 8: Loyalty System Foundation${NC}"
echo "  • Loyalty Account Creation"
echo "  • Points Earning & Tracking"
echo "  • Automatic Tier Upgrades"
echo ""

echo -e "${CYAN}Sprint 9: Loyalty Rewards & Menu${NC}"
echo "  • Rewards Redemption"
echo "  • Menu Caching & Search"
echo ""

echo -e "${CYAN}Sprint 10: Admin User & Order Tools${NC}"
echo "  • User Management Dashboard"
echo "  • Activity Tracking"
echo "  • Admin Order Views"
echo ""

echo -e "${CYAN}Sprint 11: Analytics & Reporting${NC}"
echo "  • Order, Payment, Kitchen, Loyalty Analytics"
echo ""

echo -e "${CYAN}Sprint 12: Polish & Advanced Features${NC}"
echo "  • Reorder, Filtering, Admin Cleanup"
echo "  • Mock Payment Support"
echo ""

echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}Setup Complete!${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo ""
echo "1. Review ROADMAP.md for the complete project timeline"
echo ""
echo "2. In GitHub Project (https://github.com/orgs/$ORG/projects/$PROJECT_NUMBER):"
echo "   a. Go to Settings → Iterations"
echo "   b. Create 12 iterations (1 week each)"
echo "   c. Name them: 'Sprint 1', 'Sprint 2', ... 'Sprint 12'"
echo "   d. Set start date to next Monday: $SPRINT_1_START"
echo ""
echo "3. Manually assign items to sprints:"
echo "   - Use the summary above to match item titles"
echo "   - Drag items to their respective iteration"
echo "   - Start with Sprint 1 as 'Current Iteration'"
echo ""
echo "4. Create custom fields in Project Settings:"
echo "   - Sprint (Iteration field) - Already exists if you created iterations"
echo "   - Status (Todo, In Progress, In Review, Done)"
echo "   - Priority (P0, P1, P2, P3)"
echo "   - Story Points (1, 2, 3, 5, 8)"
echo "   - Service (Users, Menu, Orders, Kitchen, Payments, Loyalty)"
echo ""
echo "5. Set up Project views:"
echo "   - 'Current Sprint' - Filter by current iteration"
echo "   - 'Roadmap' - Group by iteration, sort by priority"
echo "   - 'By Service' - Group by service field"
echo "   - 'By Priority' - Group by priority field"
echo ""
echo -e "${GREEN}Your project roadmap is ready! 🚀${NC}"
echo ""

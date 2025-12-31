# Expense Tracker SaaS - Full Stack Application Plan

## Overview
Build a complete Expense Tracker SaaS application with:
- **Backend**: .NET 9 REST API with PostgreSQL (Docker)
- **Frontend**: React + TypeScript with Vite
- **Deployment**: AWS (Backend on ECS/App Runner, Frontend on S3+CloudFront)
- **Features**: Multi-user, expense tracking, categories, budgets, reports, receipts

## Application Features

### Core Features (MVP)
1. **User Authentication**
   - Register, Login, JWT tokens
   - Email verification (optional for v1)

2. **Expense Management**
   - Create, read, update, delete expenses
   - Attach receipts (images to S3)
   - Categorize expenses
   - Add tags
   - Date and amount tracking

3. **Categories**
   - Predefined categories (Food, Transport, Entertainment, etc.)
   - Custom user categories
   - Category icons/colors

4. **Budget Management**
   - Set monthly budgets per category
   - Budget alerts (80%, 100% spent)
   - Budget vs actual spending

5. **Reports & Analytics**
   - Monthly spending summary
   - Category breakdown (pie charts)
   - Spending trends over time
   - Export to CSV/PDF

6. **Dashboard**
   - Recent expenses
   - Monthly overview
   - Budget status
   - Quick add expense

### Future Features (v2)
- Recurring expenses
- Multi-currency support
- Shared expenses/households
- Bank integration
- Mobile app

## Database Schema

### Users Table
```sql
- Id (UUID, PK)
- Email (string, unique)
- PasswordHash (string)
- FirstName (string)
- LastName (string)
- CreatedAt (datetime)
- UpdatedAt (datetime)
```

### Expenses Table
```sql
- Id (UUID, PK)
- UserId (UUID, FK)
- Amount (decimal)
- Currency (string, default: USD)
- Description (string)
- CategoryId (UUID, FK)
- Date (datetime)
- ReceiptUrl (string, nullable)
- Tags (string[], nullable)
- CreatedAt (datetime)
- UpdatedAt (datetime)
```

### Categories Table
```sql
- Id (UUID, PK)
- UserId (UUID, FK, nullable) - null for system categories
- Name (string)
- Icon (string)
- Color (string)
- IsSystem (bool) - predefined vs custom
- CreatedAt (datetime)
```

### Budgets Table
```sql
- Id (UUID, PK)
- UserId (UUID, FK)
- CategoryId (UUID, FK)
- Amount (decimal)
- Period (enum: Monthly, Yearly)
- StartDate (datetime)
- EndDate (datetime, nullable)
- CreatedAt (datetime)
- UpdatedAt (datetime)
```

## Backend API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login (returns JWT)
- `POST /api/auth/refresh` - Refresh token
- `GET /api/auth/me` - Get current user info

### Expenses
- `GET /api/expenses` - List user's expenses (pagination, filters)
- `GET /api/expenses/{id}` - Get expense details
- `POST /api/expenses` - Create expense
- `PUT /api/expenses/{id}` - Update expense
- `DELETE /api/expenses/{id}` - Delete expense
- `POST /api/expenses/{id}/receipt` - Upload receipt image

### Categories
- `GET /api/categories` - List all categories (system + user's)
- `POST /api/categories` - Create custom category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete custom category

### Budgets
- `GET /api/budgets` - List user's budgets
- `GET /api/budgets/{id}` - Get budget details with spending
- `POST /api/budgets` - Create budget
- `PUT /api/budgets/{id}` - Update budget
- `DELETE /api/budgets/{id}` - Delete budget
- `GET /api/budgets/{id}/status` - Current spending vs budget

### Reports
- `GET /api/reports/summary?month=2025-01` - Monthly summary
- `GET /api/reports/category-breakdown?startDate=&endDate=` - Spending by category
- `GET /api/reports/trends?months=6` - Spending trends
- `GET /api/reports/export?format=csv` - Export data

### Health
- `GET /health` - Health check

## Phase 1: Backend Development (Docker-based)

### 1.1 Project Structure Setup
```bash
# Create project directory
mkdir ExpenseTrackerAPI
cd ExpenseTrackerAPI

# Use Docker to create .NET project
docker run --rm -v ${PWD}:/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet new webapi -n ExpenseTrackerAPI
```

**Project Structure:**
```
ExpenseTrackerAPI/
├── Controllers/
│   ├── AuthController.cs
│   ├── ExpensesController.cs
│   ├── CategoriesController.cs
│   ├── BudgetsController.cs
│   └── ReportsController.cs
├── Models/
│   ├── User.cs
│   ├── Expense.cs
│   ├── Category.cs
│   └── Budget.cs
├── DTOs/
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   ├── ExpenseDto.cs
│   └── BudgetDto.cs
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IExpenseService.cs
│   ├── ExpenseService.cs
│   └── IS3Service.cs (for receipts)
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Middleware/
│   └── JwtMiddleware.cs
├── Helpers/
│   └── JwtHelper.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

### 1.2 Docker Compose Setup (Backend + Database)
```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.dev
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=db;Database=expensetracker;Username=postgres;Password=postgres
      - JwtSettings__Secret=your-super-secret-key-change-in-production
      - AWS__Region=us-east-1
      - AWS__BucketName=expensetracker-receipts
    volumes:
      - ./ExpenseTrackerAPI:/app
    depends_on:
      - db
    command: dotnet watch run --urls "http://0.0.0.0:8080"

  db:
    image: postgres:16
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=expensetracker
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data

  adminer:
    image: adminer
    ports:
      - "8080:8080"
    depends_on:
      - db

volumes:
  postgres_data:
```

### 1.3 NuGet Packages (Install via Docker)
```bash
# Run from project root
docker run --rm -v ${PWD}/ExpenseTrackerAPI:/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 bash -c "
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
  dotnet add package Microsoft.EntityFrameworkCore.Design
  dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
  dotnet add package BCrypt.Net-Next
  dotnet add package Serilog.AspNetCore
  dotnet add package Swashbuckle.AspNetCore
  dotnet add package AWSSDK.S3
"
```

### 1.4 Key Backend Features to Implement
1. **JWT Authentication** - Secure token-based auth
2. **Entity Framework Core** - ORM with PostgreSQL
3. **Repository Pattern** - Clean architecture
4. **Validation** - FluentValidation for DTOs
5. **Logging** - Serilog with structured logging
6. **Exception Handling** - Global error middleware
7. **CORS** - Configure for React frontend
8. **Swagger** - API documentation
9. **S3 Integration** - Receipt image uploads

## Phase 2: Frontend Development (React + TypeScript)

### 2.1 Create React Project
```bash
# Create frontend directory
mkdir expense-tracker-web
cd expense-tracker-web

# Use Node Docker image to create Vite project
docker run --rm -it -v ${PWD}:/app -w /app node:20 npx create-vite@latest . --template react-ts
```

### 2.2 Project Structure
```
expense-tracker-web/
├── public/
├── src/
│   ├── components/
│   │   ├── Auth/
│   │   │   ├── Login.tsx
│   │   │   └── Register.tsx
│   │   ├── Dashboard/
│   │   │   ├── Dashboard.tsx
│   │   │   ├── SummaryCards.tsx
│   │   │   └── RecentExpenses.tsx
│   │   ├── Expenses/
│   │   │   ├── ExpenseList.tsx
│   │   │   ├── ExpenseForm.tsx
│   │   │   ├── ExpenseItem.tsx
│   │   │   └── ReceiptUpload.tsx
│   │   ├── Categories/
│   │   │   ├── CategoryList.tsx
│   │   │   └── CategoryForm.tsx
│   │   ├── Budgets/
│   │   │   ├── BudgetList.tsx
│   │   │   ├── BudgetForm.tsx
│   │   │   └── BudgetProgress.tsx
│   │   ├── Reports/
│   │   │   ├── MonthlyReport.tsx
│   │   │   ├── CategoryChart.tsx
│   │   │   └── TrendChart.tsx
│   │   ├── Layout/
│   │   │   ├── Navbar.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Layout.tsx
│   │   └── Common/
│   │       ├── Button.tsx
│   │       ├── Input.tsx
│   │       ├── Modal.tsx
│   │       └── Loading.tsx
│   ├── services/
│   │   ├── api.ts
│   │   ├── authService.ts
│   │   ├── expenseService.ts
│   │   ├── categoryService.ts
│   │   └── budgetService.ts
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   ├── useExpenses.ts
│   │   └── useBudgets.ts
│   ├── context/
│   │   └── AuthContext.tsx
│   ├── types/
│   │   ├── user.ts
│   │   ├── expense.ts
│   │   ├── category.ts
│   │   └── budget.ts
│   ├── utils/
│   │   ├── formatters.ts
│   │   └── validators.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── package.json
├── tsconfig.json
├── vite.config.ts
└── Dockerfile
```

### 2.3 Key Frontend Libraries
```json
{
  "dependencies": {
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "react-router-dom": "^6.21.0",
    "axios": "^1.6.0",
    "react-query": "^5.0.0",
    "zustand": "^4.4.0",
    "react-hook-form": "^7.49.0",
    "zod": "^3.22.0",
    "date-fns": "^3.0.0",
    "recharts": "^2.10.0",
    "lucide-react": "^0.300.0",
    "tailwindcss": "^3.4.0",
    "shadcn/ui": "latest"
  }
}
```

### 2.4 Docker Setup for Frontend Development
```dockerfile
# Dockerfile.dev
FROM node:20
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
```

```yaml
# Add to docker-compose.yml
  web:
    build:
      context: ./expense-tracker-web
      dockerfile: Dockerfile.dev
    ports:
      - "5173:5173"
    volumes:
      - ./expense-tracker-web:/app
      - /app/node_modules
    environment:
      - VITE_API_URL=http://localhost:5000
```

### 2.5 Key Frontend Features
1. **Authentication Flow** - Login/Register, protected routes
2. **Dashboard** - Summary cards, quick actions, charts
3. **Expense Management** - CRUD with image upload
4. **Category Management** - Visual category selector
5. **Budget Tracking** - Progress bars, alerts
6. **Reports/Analytics** - Charts with Recharts
7. **Responsive Design** - Mobile-first with Tailwind
8. **Dark Mode** - Theme toggle
9. **Form Validation** - React Hook Form + Zod
10. **State Management** - Zustand or React Query

## Phase 3: Production Docker Images

### 3.1 Backend Dockerfile (Multi-stage)
```dockerfile
# Backend: ExpenseTrackerAPI/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ExpenseTrackerAPI.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ExpenseTrackerAPI.dll"]
```

### 3.2 Frontend Dockerfile (Multi-stage)
```dockerfile
# Frontend: expense-tracker-web/Dockerfile
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 3.3 Build Production Images
```bash
# Build backend
docker build -t expensetracker-api:latest ./ExpenseTrackerAPI

# Build frontend
docker build -t expensetracker-web:latest ./expense-tracker-web

# Test locally
docker-compose -f docker-compose.prod.yml up
```

## Phase 4: AWS Deployment Strategy

### Backend Deployment (Choose One)

#### Option A: AWS App Runner (Recommended - Easiest)
**Best for:** Quick deployment, auto-scaling, minimal config

**Architecture:**
```
[Users] → [App Runner (API)] → [RDS PostgreSQL]
                ↓
              [S3 (Receipts)]
```

**Steps:**
1. Create RDS PostgreSQL database
2. Create S3 bucket for receipts
3. Push API image to ECR
4. Create App Runner service
5. Configure environment variables
6. Done!

**Cost:** ~$25-40/month

#### Option B: AWS ECS + Fargate (Production-Ready)
**Best for:** More control, VPC isolation, scalability

**Architecture:**
```
[Route53] → [ALB] → [ECS Fargate (API)] → [RDS PostgreSQL]
                           ↓
                      [S3 (Receipts)]
```

**Components:**
- ECR for Docker images
- ECS Cluster with Fargate
- Application Load Balancer
- VPC with public/private subnets
- NAT Gateway
- Securit5 Groups
- RDS in private subnet
- S3 bucket

**Cost:** ~$60-100/month

### Frontend Deployment

#### Option A: S3 + CloudFront (Recommended)
**Best for:** Static site hosting, global CDN, cost-effective

**Architecture:**
```
[Users] →6[CloudFront CDN] → [S3 Bucket (Static Files)]
                ↓
           [App RunWorkflow

**Backend Pipeline:**
```yaml
name: Backend Deploy
on:
  push:
    branches: [main]
    paths: ['ExpenseTrackerAPI/**']

jobs:
  deploy:
    - Checkout code
    - Login to AWS ECR
    - Bui7d Docker image
    - Push to ECR
    - Deploy to App Runner/ECS
    - Run database migrations
```

**Frontend Pipeline:**
```yaml
name: Frontend Deploy
on:
  push:
    branches: [main]
   Complete Tech Stack

### Backend
- **.NET 9** (or .NET 8 LTS)
- **ASP.NET Core Web API**
- **Entity Framework Core** with PostgreSQL
- **JWT Authentication**
- **Serilog** for logging
- **FluentValidation**
- **AutoMapper**
- **Swashbuckle** (Swagger)
- **AWS SDK** for S3

### Frontend
- **React 18** with TypeScript
- **Vite** (build tool)
- **React Router** (routing)
- **React Query / TanStack Query** (data fetching)
- **Zustand** (state management)
- **React Hook Form** (forms)
- **Zod** (validation)
- **Tailwind CSS** (styling)
- **shadcn/ui** (components)
- **Recharts** (charts)
- **Lucide React** (icons)
- **Axios** (HTTP client)

### Database
- **PostgreSQL 16** (via RDS or Docker)

### File Storage
- **Amazon S3** (receipt images)

### Infrastructure
- **Docker & Docker Compose**
- **AWS App Runner** or **ECS Fargate**
- **Amazon RDS PostgreSQL**
- **Amazon S3**
- **Amazon CloudFront**
- **AWS Secrets Manager** (secrets)
- **AWS CloudWatch** (monitoring)
**Cost:** ~$0.01 per build minute + hosting

## Phase 4: Infrastructure as Code (Optional but Recommended)

### Option 1: AWS CDK (C#)
- Define infrastructure in C#
- Type-safe, familiar language

### Option 2: Terraform
- Popular, cloud-agnostic
- Large community

### Option 3: AWS CloudFormation
- Native AWS, YAML/JSON templates

## Phase 5: CI/CD Pipeline

###Complete Project Structure
```
expense-tracker/
├── ExpenseTrackerAPI/              # Backend (.NET)
│   ├── Controllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Services/
│   ├── Data/
│   ├── Middleware/
│   ├── Helpers/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── Dockerfile.dev
├── expense-tracker-web/            # Frontend (React)
│   ├── public/
│   ├── src/
│   │   ├── components/
│   │   ├── services/
│   │   ├── hooks/
│   │   ├── context/
│   │   ├── types/
│   │   ├── utils/
│   │   └── App.tsx
│   ├── package.json
│   ├── vite.config.ts
│   ├── Dockerfile
│   └── nginx.conf
├── infrastructure/                 # IaC (optional)
│   └── terraform/ or aws-cdk/
├── .github/
│   └── workflows/
│       ├── backend-deploy.yml
│       └── frontend-deploy.yml
├── docker-compose.yml              # Local development
├── docker-compose.prod.yml         # Production testing
├── plan.md, metrics, alarms
- **X-Ray**: Distributed tracing
- **CloudWatch Insights**: Log analysis

### Application Setup
- Structured logging (Serilog)
- Health checks
- Metrics endpoints

## Recommended Tech Stack

### Core
- **.NET 9** (latest stable) or **.NET 8** (LTS - Long Term Support until Nov 2026)
- **ASP.NET Core Web API**
- **Entity Framework Core** (if using database)

**Note:** .NET 9 is the latest but .NET 8 is LTS. For production, LTS is often preferred.

### Database Options
- **Amazon RDS** (PostgreSQL/MySQL/SQL Server)
- **Amazon Aurora Serverless**
- **DynamoDB** (NoSQL)

### Caching (Optional)
- **Amazon ElastiCache** (Redis)

### Storage (if needed)
- **Amazon S3**

## Project Structure
```Breakdown

### Development (Local Docker)
- **$0** - Everything runs locally

### AWS Production (Minimal - App Runner Stack)
- **App Runner**: ~$25/month (1 vCPU, 2GB RAM)
- **RDS db.t4g.micro**: ~$15/month
- **S3**: ~$1/month (receipt storage)
- **CloudFront**: ~$1-5/month
- **Route53**: ~$0.50/month
- **Total: ~$42-47/month**

### AWS Production (Scalable - ECS Stack)
- **ECS Fargate**: ~$30/month (0.25 vCPU, 0.5GB)
- **Application Load Balancer**: ~$20/month
- **RDS db.t4g.small**: ~$30/month
- **S3**: ~$1-5/month
- **CloudFront**: ~$1-5/month
- **NAT Gateway**: ~$35/month (optional, for private subnets)
- **Route53**: ~$0.50/month
- **Total: ~$82-126/month**

### Optimization Tips
- Use RDS Serverless v2 for variable traffic
- Enable S3 Intelligent-Tiering for old receipts
- Use CloudFront for API caching
- Implement API response caching
- Start with App Runner, migrate to ECS if needed
│   └── workflows/
│       └── deploy.yml
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Cost Considerations

### Development (Free Tier Eligible)
- App Runner: $0 (minimal usage)
- ECS Fargate: ~$5-20/month
- RDS db.t3.micro: ~$15/month
- Or use SQLite/in-memory DB initially

### Production Estimate
- ECS Fargate (1 task): ~$20-30/month
- ALB: ~$20/month
- RDS: ~$15-100/month depending on size
- **Total: ~$55-150/month**

## Development Workflow

### Phase 1: Backend Foundation (Week 1-2)
1. ✅ Setup Docker environment
2. ✅ Create .NET API project structure
3. ✅ Implement database models & migrations
4. ✅ Setup PostgreSQL in Docker
5. ✅ Build Authentication (JWT)
6. ✅ Create Expenses CRUD endpoints
7. ✅ Implement Categories & Budgets
8. ✅ Add S3 integration for receipts
9. ✅ Setup Swagger documentation

### Phase 2: Frontend Foundation (Week 2-3)
1. ✅ Setup React + TypeScript + Vite
2. ✅ Configure Tailwind CSS & shadcn/ui
3. ✅ Build authentication flow
4. ✅ Create layout & navigation
5. ✅ Implement Dashboard
6. ✅ Build Expense management UI
7. ✅ Create Category & Budget UIs
8. ✅ Add charts & reports
9. ✅ Implement receipt upload

### Phase 3: Integration & Testing (Week 3-4)
1. ✅ Connect frontend to backend API
2. ✅ Test all CRUD operations
3. ✅ Implement error handling
4. ✅ Add loading states
5. ✅ Test authentication flow
6. ✅ Mobile responsive testing
7. ✅ Fix bugs and polish UI

### Phase 4: AWS Deployment (Week 4)
1. ✅ Create AWS account & configure
2. ✅ Setup RDS PostgreSQL
3. ✅ Create S3 bucket
4. ✅ Push images to ECR
5. ✅ Deploy backend to App Runner
6. ✅ Deploy frontend to S3 + CloudFront
7. ✅ Configure custom domain
8. ✅ Setup SSL certificates

### Phase 5: CI/CD & Monitoring (Week 5)
1. ✅ Setup GitHub Actions
2. ✅ Configure automated deployments
3. ✅ Setup CloudWatch monitoring
4. ✅ Configure alerts
5. ✅ Add database backups

## Next Immediate Steps

1. **Initialize Backend** - Create .NET project with Docker
2. **Setup Database** - PostgreSQL with Docker Compose
3. **Build Auth System** - JWT authentication
4. **Create Expense Endpoints** - Core CRUD operations
5. **Initialize Frontend** - React + TypeScript setup
6. **Connect & Test** - Full stack integration

**Ready to start? Let's begin with Phase 1.1 - Backend Setup!**

## Quick Start Commands (100% Docker - No Local Installation!)

```bash
# 1. Create backend project
docker run --rm -v ${PWD}:/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet new webapi -n ExpenseTrackerAPI

# 2. Create frontend project
docker run --rm -it -v ${PWD}:/app -w /app node:20 npx create-vite@latest expense-tracker-web --template react-ts

# 3. Start full stack locally
docker-compose up

# Backend will be at: http://localhost:5000
# Frontend will be at: http://localhost:5173
# Database admin at: http://localhost:8080 (Adminer)
# API docs at: http://localhost:5000/swagger

# 4. View logs
docker-compose logs -f api
docker-compose logs -f web

# 5. Run migrations (after creating them)
docker-compose exec api dotnet ef database update

# 6. Stop all services
docker-compose down

# 7. Build production images
docker build -t expensetracker-api:latest ./ExpenseTrackerAPI
docker build -t expensetracker-web:latest ./expense-tracker-web

# 8. Push to AWS ECR (when ready)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com
docker tag expensetracker-api:latest YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/expensetracker-api:latest
docker push YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/expensetracker-api:latest
```

**Note:** Everything runs in Docker! You only need Docker installed on your machine.

## Resources

### Backend
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT Authentication in .NET](https://jwt.io)
- [Serilog Documentation](https://serilog.net)

### Frontend
- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs)
- [Vite Guide](https://vitejs.dev/guide)
- [TanStack Query](https://tanstack.com/query)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [shadcn/ui](https://ui.shadcn.com)
- [Recharts](https://recharts.org)

### DevOps
- [Docker Documentation](https://docs.docker.com)
- [AWS App Runner](https://docs.aws.amazon.com/apprunner)
- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs)
- [AWS S3 + CloudFront](https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/GettingStarted.html)
- [GitHub Actions](https://docs.github.com/en/actions)

## Ready to Build? 🚀

This is a complete roadmap for building a production-ready Expense Tracker SaaS application. We'll build it step by step using Docker for everything!

**Let me know when you're ready to start, and we'll begin with setting up the backend!**

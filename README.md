# SaveSense AI

An AI-powered shopping assistant that helps users save money before buying
anything online — coupon discovery, price history, price-drop detection,
cross-store comparison, cashback discovery, and purchase-timing
recommendations.

Built as a full-stack, production-quality portfolio project demonstrating
enterprise architecture patterns (Clean Architecture, CQRS) across a
Next.js/React frontend, an ASP.NET Core 9 backend, and a Chrome extension.

See [PROJECT_RULES.md](./PROJECT_RULES.md) for engineering standards and
[docs/adr/](./docs/adr/) for architecture decision records.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js, React, TypeScript, TailwindCSS, React Query, Zustand |
| Backend | ASP.NET Core 9, Clean Architecture, CQRS/MediatR, FluentValidation, EF Core |
| Data | PostgreSQL, Redis |
| Auth | JWT, OAuth, Google Login |
| Extension | Chrome Manifest V3 |
| AI | OpenAI API |
| Infra | Docker, Azure, GitHub Actions, Terraform |

## Repository Layout

```
/frontend           Next.js application
/backend            ASP.NET Core 9 solution (Clean Architecture)
/chrome-extension    Manifest V3 extension
/shared             Cross-cutting contracts shared across apps
/docs                Architecture Decision Records, diagrams
/infrastructure      Docker, Terraform
/scripts             Dev/CI helper scripts
/.github/workflows   CI/CD pipelines
```

## Build Phases

| Phase | Scope | Status |
|---|---|---|
| 0 | Repository setup | ✅ Done |
| 1 | Architecture | Not started |
| 2 | Authentication | Not started |
| 3 | Database | Not started |
| 4 | Coupon service | Not started |
| 5 | Price tracking | Not started |
| 6 | Cashback integrations | Not started |
| 7 | Chrome Extension | Not started |
| 8 | AI Shopping Assistant | Not started |
| 9 | Deployment | Not started |
| 10 | CI/CD | Not started |
| 11 | Performance optimization | Not started |
| 12 | Security audit | Not started |

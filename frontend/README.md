# Frontend

Next.js (App Router) + React + TypeScript + TailwindCSS. Server state via
React Query, client UI state via Zustand, dark mode via `next-themes`.

## Structure

```
src/
  app/            Next.js routes, layouts, pages
  components/     Shared, reusable UI primitives (no business logic)
  features/       Feature-first modules (features/health, features/coupons, ...)
  hooks/          Cross-feature hooks
  lib/            Framework-agnostic utilities
  services/       Typed API client layer (talks to the backend)
  context/        React context providers
  types/          Shared TypeScript types not owned by one feature
  styles/         Tailwind entry point, theme tokens
```

See [docs/adr/0001-monorepo-structure.md](../docs/adr/0001-monorepo-structure.md)
for why the repo is organized this way, and `PROJECT_RULES.md` at the repo
root for engineering standards.

## Local development

```bash
npm install
npm run dev
```

Requires `NEXT_PUBLIC_API_URL` pointing at the backend (see `.env.example`).

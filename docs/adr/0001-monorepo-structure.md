# ADR 0001: Use a Monorepo with Feature-First Top-Level Folders

## Status
Accepted

## Context
SaveSense AI consists of several deployable units that change together far more
often than they change independently:

- A Next.js/React frontend
- An ASP.NET Core 9 backend (Clean Architecture, CQRS)
- A Chrome Extension (Manifest V3)
- Shared contracts/types consumed by more than one of the above

A change to the API contract (e.g. adding a field to a `PriceHistory` response)
typically requires coordinated edits in the backend, the frontend, and possibly
the extension in the same change set. We need to decide whether these live in
one repository or several.

## Decision
Use a single repository ("monorepo") with top-level folders separating each
deployable unit:

```
/frontend           Next.js application
/backend            ASP.NET Core 9 solution (Clean Architecture)
/chrome-extension    Manifest V3 extension
/shared             Cross-cutting contracts/types (e.g. OpenAPI-generated clients)
/docs               Architecture Decision Records, diagrams, runbooks
/infrastructure     Docker, Terraform (Phase 9+)
/scripts            Local dev / CI helper scripts
/.github/workflows  CI/CD pipelines (Phase 10)
```

## Alternatives Considered

**Polyrepo (separate repo per app)**
- Pro: independent CI, independent access control, smaller individual repos
- Con: a single API contract change requires PRs across 2-3 repos kept in sync
  manually; harder to demo as one coherent portfolio project

**Monorepo with a build tool (Nx / Turborepo)**
- Pro: task orchestration, remote caching, dependency graph-aware builds
- Con: added tooling complexity before there is enough scale (2-3 apps) to
  justify it. Revisit if CI times or cross-package dependency management
  become painful.

## Consequences
- One `git clone` gives a reviewer the entire system.
- CI (Phase 10) must use path filters so a frontend-only change doesn't
  trigger a backend build/test run, and vice versa.
- No cross-repo version pinning is needed — the backend and frontend are
  always tested against each other at the same commit.
- If the extension or backend later needs genuinely independent release
  cadences or access control, this decision should be revisited.

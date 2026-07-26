# .github

GitHub Actions workflows live in `workflows/`. Populated in Phase 10 (CI/CD)
— lint/build/test pipelines with path filters so a frontend-only change
doesn't trigger backend builds, and vice versa (see
[docs/adr/0001-monorepo-structure.md](../docs/adr/0001-monorepo-structure.md)).

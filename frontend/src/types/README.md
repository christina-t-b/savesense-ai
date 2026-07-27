Shared TypeScript types not owned by one feature. Once `/shared` has
OpenAPI-generated types (Phase 2+), most of what would go here should be
imported from `@savesense-ai/shared` instead. Feature-specific types live in
`features/{feature}/types/` or alongside the code that uses them.

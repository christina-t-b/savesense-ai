# Infrastructure

`docker-compose.yml` runs Postgres for local development — pulled forward
into Phase 2 (Authentication) since auth needs persistent user storage, ahead
of the originally-planned Phase 3. Redis isn't here yet; it gets added
whenever a real caching workload exists to justify it (likely Phase 5, price
tracking), rather than running unused infrastructure. Terraform is introduced
in Phase 9 (Deployment) for the same reason — no speculative infra.

```bash
cp .env.example .env   # first time only
docker compose up -d
```

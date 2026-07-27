"use client";

import { useState } from "react";
import { Button } from "@/components/Button";
import { useGetPing } from "@/features/health/hooks/useGetPing";

/**
 * Proves the frontend can reach the backend (fetch, CORS, React Query, and
 * rendering) end-to-end, mirroring the backend's GetPing vertical slice.
 * Intentionally trivial — a real feature replaces this in Phase 4.
 */
export function HealthCheckCard() {
  const [name, setName] = useState("");
  const [submittedName, setSubmittedName] = useState<string | null>(null);

  const { data, error, isFetching } = useGetPing(submittedName ?? "", submittedName !== null);

  return (
    <div className="rounded-lg border border-foreground/10 p-6 max-w-md w-full">
      <h2 className="text-lg font-semibold mb-2">Backend connectivity check</h2>
      <p className="text-sm text-foreground/70 mb-4">
        Calls the backend&apos;s <code>/api/health/ping</code> endpoint.
      </p>

      <form
        className="flex gap-2 mb-4"
        onSubmit={(e) => {
          e.preventDefault();
          setSubmittedName(name);
        }}
      >
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Your name (optional)"
          className="flex-1 rounded-md border border-foreground/20 bg-transparent px-3 py-2 text-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-blue-500"
        />
        <Button type="submit" disabled={isFetching}>
          {isFetching ? "Pinging…" : "Ping backend"}
        </Button>
      </form>

      {data && (
        <p className="text-sm" role="status">
          {data.message} — server time {new Date(data.serverTimeUtc).toLocaleString()}
        </p>
      )}
      {error && (
        <p className="text-sm text-red-600" role="alert">
          {error instanceof Error ? error.message : "Something went wrong."}
        </p>
      )}
    </div>
  );
}

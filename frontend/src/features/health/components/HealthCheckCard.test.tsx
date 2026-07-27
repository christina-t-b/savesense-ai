import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { HealthCheckCard } from "./HealthCheckCard";

function renderWithQueryClient(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

describe("HealthCheckCard", () => {
  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        new Response(
          JSON.stringify({ message: "pong, Christina", serverTimeUtc: "2026-01-01T00:00:00Z" }),
          { status: 200, headers: { "Content-Type": "application/json" } },
        ),
      ),
    );
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("calls the ping endpoint with the entered name and renders the response", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<HealthCheckCard />);

    await user.type(screen.getByPlaceholderText(/your name/i), "Christina");
    await user.click(screen.getByRole("button", { name: /ping backend/i }));

    await waitFor(() => {
      expect(screen.getByRole("status")).toHaveTextContent("pong, Christina");
    });

    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/health/ping?name=Christina"),
      expect.anything(),
    );
  });
});

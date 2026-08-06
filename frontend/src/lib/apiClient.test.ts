import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { apiFetch, ApiError, logout, refreshAccessToken } from "@/lib/apiClient";
import { useAuthStore } from "@/lib/auth/authStore";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("apiClient", () => {
  beforeEach(() => {
    useAuthStore.setState({ accessToken: null });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("apiFetch attaches the Authorization header when a token is present", async () => {
    useAuthStore.setState({ accessToken: "token-123" });
    const fetchMock = vi.fn(async () => jsonResponse({ ok: true }));
    vi.stubGlobal("fetch", fetchMock);

    await apiFetch("/api/whatever");

    const [, init] = fetchMock.mock.calls[0];
    expect((init?.headers as Record<string, string>).Authorization).toBe("Bearer token-123");
  });

  it("apiFetch omits the Authorization header when there is no token", async () => {
    const fetchMock = vi.fn(async () => jsonResponse({ ok: true }));
    vi.stubGlobal("fetch", fetchMock);

    await apiFetch("/api/whatever");

    const [, init] = fetchMock.mock.calls[0];
    expect((init?.headers as Record<string, string>).Authorization).toBeUndefined();
  });

  it("apiFetch retries once with a fresh token after a 401, and succeeds", async () => {
    useAuthStore.setState({ accessToken: "expired-token" });
    const fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
      if (url.endsWith("/api/auth/refresh")) {
        return jsonResponse({ accessToken: "new-token" });
      }
      const authHeader = (init?.headers as Record<string, string>)?.Authorization;
      return authHeader === "Bearer new-token" ? jsonResponse({ data: "success" }) : jsonResponse({}, 401);
    });
    vi.stubGlobal("fetch", fetchMock);

    const result = await apiFetch<{ data: string }>("/api/whatever");

    expect(result).toEqual({ data: "success" });
    expect(useAuthStore.getState().accessToken).toBe("new-token");
    // original call (old token, 401), refresh call, retried call (new token, succeeds)
    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  it("apiFetch does not retry a second time if the retried request also 401s", async () => {
    useAuthStore.setState({ accessToken: "expired-token" });
    const fetchMock = vi.fn(async (url: string) => {
      if (url.endsWith("/api/auth/refresh")) {
        return jsonResponse({ accessToken: "new-token" });
      }
      return jsonResponse({}, 401);
    });
    vi.stubGlobal("fetch", fetchMock);

    await expect(apiFetch("/api/whatever")).rejects.toThrow(ApiError);
    // original call, refresh call, one retry — never a second refresh
    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  it("apiFetch propagates the 401 without retrying when refresh itself fails", async () => {
    useAuthStore.setState({ accessToken: "expired-token" });
    const fetchMock = vi.fn(async (url: string) => {
      if (url.endsWith("/api/auth/refresh")) {
        return jsonResponse({}, 401);
      }
      return jsonResponse({}, 401);
    });
    vi.stubGlobal("fetch", fetchMock);

    await expect(apiFetch("/api/whatever")).rejects.toThrow(ApiError);
    expect(useAuthStore.getState().accessToken).toBeNull();
    expect(fetchMock).toHaveBeenCalledTimes(2); // original call + failed refresh, no retry
  });

  it("refreshAccessToken sends credentials and stores the new access token on success", async () => {
    const fetchMock = vi.fn(async () => jsonResponse({ accessToken: "fresh-token" }));
    vi.stubGlobal("fetch", fetchMock);

    const success = await refreshAccessToken();

    expect(success).toBe(true);
    expect(useAuthStore.getState().accessToken).toBe("fresh-token");
    const [, init] = fetchMock.mock.calls[0];
    expect(init?.credentials).toBe("include");
  });

  it("refreshAccessToken clears the store and returns false on failure", async () => {
    useAuthStore.setState({ accessToken: "stale-token" });
    vi.stubGlobal("fetch", vi.fn(async () => jsonResponse({}, 401)));

    const success = await refreshAccessToken();

    expect(success).toBe(false);
    expect(useAuthStore.getState().accessToken).toBeNull();
  });

  it("logout calls the logout endpoint with credentials and clears the store", async () => {
    useAuthStore.setState({ accessToken: "token-123" });
    const fetchMock = vi.fn(async () => new Response(null, { status: 204 }));
    vi.stubGlobal("fetch", fetchMock);

    await logout();

    expect(useAuthStore.getState().accessToken).toBeNull();
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toContain("/api/auth/logout");
    expect(init?.credentials).toBe("include");
  });
});

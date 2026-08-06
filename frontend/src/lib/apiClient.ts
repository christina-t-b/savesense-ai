import { useAuthStore } from "@/lib/auth/authStore";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly details?: unknown,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

function requireBaseUrl(): string {
  if (!API_BASE_URL) {
    throw new Error(
      "NEXT_PUBLIC_API_URL is not set. Copy .env.example to .env.local and point it at the backend.",
    );
  }
  return API_BASE_URL;
}

/**
 * Reads the refresh token from its httpOnly cookie (never JS-visible —
 * credentials: 'include' is what sends it) and exchanges it for a new
 * access token + rotated refresh cookie. Called both on app load (to
 * restore a session after a page reload) and by apiFetch on a 401.
 */
export async function refreshAccessToken(): Promise<boolean> {
  try {
    const response = await fetch(`${requireBaseUrl()}/api/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });

    if (!response.ok) {
      useAuthStore.getState().clear();
      return false;
    }

    const data = (await response.json()) as { accessToken: string };
    useAuthStore.getState().setAccessToken(data.accessToken);
    return true;
  } catch {
    useAuthStore.getState().clear();
    return false;
  }
}

export async function logout(): Promise<void> {
  await fetch(`${requireBaseUrl()}/api/auth/logout`, {
    method: "POST",
    credentials: "include",
  }).catch(() => undefined);
  useAuthStore.getState().clear();
}

async function fetchWithAuth(path: string, init: RequestInit | undefined, isRetry: boolean): Promise<Response> {
  const token = useAuthStore.getState().accessToken;

  const response = await fetch(`${requireBaseUrl()}${path}`, {
    ...init,
    headers: {
      Accept: "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  });

  if (response.status === 401 && !isRetry && token) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      return fetchWithAuth(path, init, true);
    }
  }

  return response;
}

/**
 * Thin fetch wrapper every feature's service layer goes through, so error
 * handling, auth headers, and the backend base URL are defined once instead
 * of per call site.
 */
export async function apiFetch<TResponse>(
  path: string,
  init?: RequestInit,
): Promise<TResponse> {
  const response = await fetchWithAuth(path, init, false);

  if (!response.ok) {
    const body = await response.json().catch(() => undefined);
    throw new ApiError(
      `Request to ${path} failed with status ${response.status}`,
      response.status,
      body,
    );
  }

  return (await response.json()) as TResponse;
}

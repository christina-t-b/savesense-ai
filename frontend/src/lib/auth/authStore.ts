import { create } from "zustand";

interface AuthState {
  accessToken: string | null;
  setAccessToken: (token: string) => void;
  clear: () => void;
}

/**
 * Deliberately not persisted (no localStorage/sessionStorage) — the access
 * token only ever lives in memory, so a page reload always starts from
 * zero and re-derives a fresh token from the httpOnly refresh cookie
 * (see lib/apiClient's refreshAccessToken, called on app load).
 */
export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  setAccessToken: (token) => set({ accessToken: token }),
  clear: () => set({ accessToken: null }),
}));

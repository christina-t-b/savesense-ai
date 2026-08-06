import { apiFetch } from "@/lib/apiClient";

export interface CurrentUser {
  id: string;
  email: string;
  displayName: string;
}

export function getCurrentUser(): Promise<CurrentUser> {
  return apiFetch<CurrentUser>("/api/auth/me");
}

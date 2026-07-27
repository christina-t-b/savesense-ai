import { apiFetch } from "@/lib/apiClient";

export interface PingResult {
  message: string;
  serverTimeUtc: string;
}

export function getPing(name?: string): Promise<PingResult> {
  const query = name ? `?name=${encodeURIComponent(name)}` : "";
  return apiFetch<PingResult>(`/api/health/ping${query}`);
}

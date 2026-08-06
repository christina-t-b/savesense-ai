"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { refreshAccessToken } from "@/lib/apiClient";

/**
 * The backend redirects here right after Google login, having already set
 * the httpOnly refresh cookie. This page's only job is to exchange that
 * cookie for an in-memory access token before sending the user onward —
 * the access token itself never appears in this URL.
 */
export default function AuthCallbackPage() {
  const router = useRouter();

  useEffect(() => {
    refreshAccessToken().then((success) => {
      router.replace(success ? "/" : "/?authError=1");
    });
  }, [router]);

  return <p className="p-8 text-sm text-foreground/60">Signing you in…</p>;
}

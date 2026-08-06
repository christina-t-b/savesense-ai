"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "next-themes";
import { useEffect, useState } from "react";
import { refreshAccessToken } from "@/lib/apiClient";

export function AppProviders({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient());

  useEffect(() => {
    // Runs once for the app's whole lifetime (this layout isn't remounted
    // by client-side navigation). Skipped on /auth/callback, which already
    // does its own refresh — running both would race, each consuming the
    // same one-time-use refresh cookie.
    if (window.location.pathname !== "/auth/callback") {
      refreshAccessToken();
    }
  }, []);

  return (
    <ThemeProvider attribute="class" defaultTheme="system" enableSystem>
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </ThemeProvider>
  );
}

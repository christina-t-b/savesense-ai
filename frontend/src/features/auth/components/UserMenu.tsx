"use client";

import { Button } from "@/components/Button";
import { useAuthStore } from "@/lib/auth/authStore";
import { logout } from "@/lib/apiClient";
import { useCurrentUser } from "@/features/auth/hooks/useCurrentUser";
import { SignInButton } from "@/features/auth/components/SignInButton";
import { useQueryClient } from "@tanstack/react-query";

export function UserMenu() {
  const accessToken = useAuthStore((state) => state.accessToken);
  const { data: user, isLoading } = useCurrentUser();
  const queryClient = useQueryClient();

  if (!accessToken) {
    return <SignInButton />;
  }

  if (isLoading) {
    return <p className="text-sm text-foreground/60">Loading…</p>;
  }

  return (
    <div className="flex items-center gap-3">
      <span className="text-sm">{user?.displayName}</span>
      <Button
        variant="secondary"
        onClick={async () => {
          await logout();
          queryClient.removeQueries({ queryKey: ["auth", "me"] });
        }}
      >
        Sign out
      </Button>
    </div>
  );
}

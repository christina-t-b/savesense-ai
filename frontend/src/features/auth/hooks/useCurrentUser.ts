import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/lib/auth/authStore";
import { getCurrentUser } from "@/features/auth/services/getCurrentUser";

export function useCurrentUser() {
  const accessToken = useAuthStore((state) => state.accessToken);

  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: getCurrentUser,
    enabled: accessToken !== null,
  });
}

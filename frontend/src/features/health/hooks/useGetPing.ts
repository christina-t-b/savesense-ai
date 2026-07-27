import { useQuery } from "@tanstack/react-query";
import { getPing } from "@/features/health/services/getPing";

export function useGetPing(name: string, enabled: boolean) {
  return useQuery({
    queryKey: ["health", "ping", name],
    queryFn: () => getPing(name),
    enabled,
  });
}

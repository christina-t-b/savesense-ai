import { ThemeToggle } from "@/components/ThemeToggle";
import { HealthCheckCard } from "@/features/health/components/HealthCheckCard";
import { UserMenu } from "@/features/auth/components/UserMenu";

export default function Home() {
  return (
    <div className="flex flex-col flex-1 items-center gap-8 px-6 py-16">
      <div className="flex w-full max-w-md items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">SaveSense AI</h1>
        <div className="flex items-center gap-3">
          <UserMenu />
          <ThemeToggle />
        </div>
      </div>
      <HealthCheckCard />
    </div>
  );
}

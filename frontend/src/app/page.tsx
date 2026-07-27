import { ThemeToggle } from "@/components/ThemeToggle";
import { HealthCheckCard } from "@/features/health/components/HealthCheckCard";

export default function Home() {
  return (
    <div className="flex flex-col flex-1 items-center gap-8 px-6 py-16">
      <div className="flex w-full max-w-md items-center justify-between">
        <h1 className="text-2xl font-semibold">SaveSense AI</h1>
        <ThemeToggle />
      </div>
      <HealthCheckCard />
    </div>
  );
}

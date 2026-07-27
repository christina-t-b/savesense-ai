"use client";

import { useTheme } from "next-themes";
import { Button } from "@/components/Button";

export function ThemeToggle() {
  // resolvedTheme is undefined until next-themes reads localStorage/system
  // preference on the client, so it doubles as our "hydrated yet?" signal —
  // no need for a separate mounted state + effect to get the same guarantee.
  const { resolvedTheme, setTheme } = useTheme();

  return (
    <Button
      variant="secondary"
      onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
      aria-label="Toggle dark mode"
    >
      {resolvedTheme === undefined ? "Toggle theme" : resolvedTheme === "dark" ? "Light mode" : "Dark mode"}
    </Button>
  );
}

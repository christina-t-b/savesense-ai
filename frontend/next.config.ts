import type { NextConfig } from "next";
import path from "node:path";

const nextConfig: NextConfig = {
  turbopack: {
    // Pin the workspace root to this npm workspace (repo root, where our
    // package-lock.json lives) — otherwise Turbopack can pick up an
    // unrelated lockfile elsewhere on the machine.
    root: path.join(__dirname, ".."),
  },
};

export default nextConfig;

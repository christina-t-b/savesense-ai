import { buttonClassName } from "@/components/Button";

/**
 * A real <a>, not a fetch — signing in has to be a full-page navigation so
 * the browser follows the redirect chain to Google and back.
 */
export function SignInButton() {
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL;

  return (
    <a href={`${apiBaseUrl}/api/auth/google/login`} className={buttonClassName("primary")}>
      Sign in with Google
    </a>
  );
}

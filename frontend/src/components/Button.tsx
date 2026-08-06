import { type ButtonHTMLAttributes } from "react";

export type ButtonVariant = "primary" | "secondary";

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    "bg-foreground text-background hover:opacity-90 disabled:opacity-50",
  secondary:
    "border border-foreground/20 hover:bg-foreground/5 disabled:opacity-50",
};

/** Shared with anchor-styled-as-button links (e.g. SignInButton) that can't
 * be a real <button> because they need to be a full-page navigation. */
export function buttonClassName(variant: ButtonVariant = "primary", className = ""): string {
  return `rounded-md px-4 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500 disabled:cursor-not-allowed inline-block text-center ${variantClasses[variant]} ${className}`;
}

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
}

export function Button({ variant = "primary", className = "", ...props }: ButtonProps) {
  return <button className={buttonClassName(variant, className)} {...props} />;
}

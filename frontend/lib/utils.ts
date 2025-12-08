import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatCurrency(amount: number, currency?: string): string {
  // Use an explicit NEXT_PUBLIC_LOCALE when available so server and client match.
  // Falling back to a stable default ensures deterministic SSR output.
  const envLocale = typeof process !== 'undefined' && process.env?.NEXT_PUBLIC_LOCALE
    ? String(process.env.NEXT_PUBLIC_LOCALE)
    : undefined

  const locale = envLocale ?? 'en-US'

  // Prefer explicitly passed currency, then environment variable, otherwise use default 'RON'
  const envCurrency = typeof process !== 'undefined' && process.env?.NEXT_PUBLIC_CURRENCY
    ? String(process.env.NEXT_PUBLIC_CURRENCY)
    : undefined

  const currencyCode = currency ?? envCurrency ?? 'RON'

  if (currencyCode) {
    try {
      return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency: currencyCode,
      }).format(amount)
    } catch (e) {
      // If an invalid currency code was provided, fall through to decimal formatting below
    }
  }

  // Fallback: format as a decimal and append the currency code if available
  const formatted = new Intl.NumberFormat(locale, {
    style: 'decimal',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)

  return currencyCode ? `${formatted} ${currencyCode}` : formatted
}

export function formatDate(date: Date | string): string {
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(date))
}

export function formatTime(date: Date | string): string {
  return new Intl.DateTimeFormat('en-US', {
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(date))
}


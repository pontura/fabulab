// The app saved timestamps in two formats: ISO ("2026-11-18T20:46:13.8420000Z") and the device culture
// ("11/18/2026 20:44:36"). Milliseconds for sorting; 0 when it cannot be read
export function timeValue(timestamp: string): number {
  if (!timestamp) return 0
  const iso = Date.parse(timestamp)
  if (!Number.isNaN(iso)) return iso
  const m = /^(\d{1,2})\/(\d{1,2})\/(\d{4})\s+(\d{1,2}):(\d{2})(?::(\d{2}))?/.exec(timestamp)
  if (!m) return 0
  const [, month, day, year, hours, minutes, seconds] = m
  return Date.UTC(+year, +month - 1, +day, +hours, +minutes, +(seconds ?? 0))
}

// Newest first
export function byNewest<T extends { timestamp: string }>(a: T, b: T): number {
  return timeValue(b.timestamp) - timeValue(a.timestamp)
}

export function startOfLocalHour(date = new Date()) {
  return new Date(
    date.getFullYear(),
    date.getMonth(),
    date.getDate(),
    date.getHours(),
  )
}

export function combineLocalDateAndHour(date: Date, hour: number) {
  const value = new Date(
    date.getFullYear(),
    date.getMonth(),
    date.getDate(),
    hour,
  )

  const isValidLocalHour =
    value.getFullYear() === date.getFullYear() &&
    value.getMonth() === date.getMonth() &&
    value.getDate() === date.getDate() &&
    value.getHours() === hour

  return isValidLocalHour ? value : null
}

export function formatLocalTime(date: Date) {
  return new Intl.DateTimeFormat(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  }).format(date)
}

export function formatUtcTime(date: Date | string) {
  return new Intl.DateTimeFormat(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZone: "UTC",
    timeZoneName: "short",
  }).format(new Date(date))
}

export function browserTimeZone() {
  return Intl.DateTimeFormat().resolvedOptions().timeZone
}

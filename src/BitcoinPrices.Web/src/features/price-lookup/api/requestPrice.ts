import { instrumentParts } from "../instrument"
import type { AggregatedPrice, PriceRequest } from "../priceTypes"
import { readApiError } from "./readApiError"

// For a more complex client, consider TanStack Query for request caching, retries, and request-state orchestration.
export async function requestPrice(
  request: PriceRequest,
  signal?: AbortSignal,
): Promise<AggregatedPrice> {
  const instrument = instrumentParts[request.instrument]
  const query = new URLSearchParams({
    baseCurrency: instrument.baseCurrency,
    quoteCurrency: instrument.quoteCurrency,
    timeUtc: request.timeUtc,
    aggregationMethod: request.aggregationMethod,
  })

  const url = new URL("/api/prices", window.location.origin)
  url.search = query.toString()

  const response = await fetch(url, {
    headers: { Accept: "application/json" },
    signal,
  })

  if (!response.ok) {
    throw await readApiError(response)
  }

  return (await response.json()) as AggregatedPrice
}

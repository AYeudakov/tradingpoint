import type { SourcePrice } from "../priceTypes"
import { formatPrice } from "../formatPrice"

export function SourcePrices({
  prices,
  currency,
}: {
  prices: SourcePrice[]
  currency: string
}) {
  return (
    <div>
      <div className="mb-2 text-[0.68rem] font-semibold tracking-[0.14em] text-muted-foreground uppercase">
        Source prices
      </div>
      <div className="divide-y rounded-lg border">
        {prices.map((source) => (
          <div
            key={source.provider}
            className="flex items-center justify-between gap-4 px-3 py-2.5"
          >
            <span className="font-medium">{source.provider}</span>
            <span className="font-mono tabular-nums">
              {formatPrice(source.price, currency)}
            </span>
          </div>
        ))}
      </div>
    </div>
  )
}

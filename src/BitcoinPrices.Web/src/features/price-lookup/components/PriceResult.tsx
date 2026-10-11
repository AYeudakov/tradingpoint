import { Database, Radio } from "lucide-react"

import { Badge } from "@/shared/ui/badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/ui/card"

import { formatPrice } from "../formatPrice"
import { formatUtcTime } from "../dateTime"
import type { AggregatedPrice } from "../priceTypes"
import { SourcePrices } from "./SourcePrices"

export function PriceResult({ result }: { result: AggregatedPrice }) {
  const fromDatastore = result.resultSource === "datastore"

  return (
    <Card className="animate-result-in border-0 shadow-[0_20px_50px_-32px_rgba(15,23,42,0.45)] ring-1 ring-slate-900/10">
      <CardHeader className="border-b pb-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <div className="text-[0.68rem] font-semibold tracking-[0.14em] text-muted-foreground uppercase">
              {result.instrument} price
            </div>
            <CardTitle className="mt-1 font-mono text-xs font-normal text-muted-foreground">
              {formatUtcTime(result.timeUtc)}
            </CardTitle>
          </div>
          <Badge
            variant="outline"
            className={
              fromDatastore
                ? "border-amber-300 bg-amber-50 text-amber-900"
                : "border-emerald-300 bg-emerald-50 text-emerald-900"
            }
          >
            {fromDatastore ? <Database /> : <Radio />}
            {fromDatastore ? "Cache / datastore" : "External APIs"}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-5">
        <div>
          <div className="text-[0.68rem] font-semibold tracking-[0.14em] text-muted-foreground uppercase">
            Aggregated price
          </div>
          <div className="mt-1 font-mono text-3xl font-semibold tracking-tight tabular-nums sm:text-4xl">
            {formatPrice(result.aggregatedPrice, result.currency)}
          </div>
          <div className="mt-1 text-xs text-muted-foreground">
            Arithmetic mean · version {result.aggregationVersion}
          </div>
        </div>
        <SourcePrices prices={result.sourcePrices} currency={result.currency} />
      </CardContent>
    </Card>
  )
}

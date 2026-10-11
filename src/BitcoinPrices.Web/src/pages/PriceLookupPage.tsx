import {
  AggregationMethod,
  aggregationMethodLabels,
} from "@/features/price-lookup/aggregationMethod"
import { PriceRequestForm } from "@/features/price-lookup/components/PriceRequestForm"
import { Instrument } from "@/features/price-lookup/instrument"

import { PageContent, PageFooter, PageHeader, PageHero } from "./ui"

const instruments = [
  { value: Instrument.BtcUsd, label: "BTC / USD" },
]

const aggregationMethods = [
  {
    value: AggregationMethod.ArithmeticMean,
    label: aggregationMethodLabels[AggregationMethod.ArithmeticMean],
  },
]

export function PriceLookupPage() {
  return (
    <PageContent>
      <PageHeader />
      <PageHero />
      <PriceRequestForm
        instruments={instruments}
        defaultInstrument={Instrument.BtcUsd}
        instrumentAvailabilityNote="Other instruments — coming soon"
        aggregationMethods={aggregationMethods}
        defaultAggregationMethod={AggregationMethod.ArithmeticMean}
        aggregationMethodAvailabilityNote="Other methods — coming soon"
      />
      <PageFooter />
    </PageContent>
  )
}

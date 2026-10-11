import type { AggregationMethod } from "./aggregationMethod"
import type { Instrument } from "./instrument"

export type PriceRequest = {
  instrument: Instrument
  aggregationMethod: AggregationMethod
  timeUtc: string
}

export type ResultSource = "datastore" | "externalApis"

export type SourcePrice = {
  provider: string
  price: number
}

export type AggregatedPrice = {
  instrument: string
  timeUtc: string
  currency: string
  aggregatedPrice: number
  resultSource: ResultSource
  aggregationMethod: "arithmeticMean"
  aggregationVersion: number
  sourcePrices: SourcePrice[]
}

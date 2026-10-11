// In a real-world application, this type could be generated from or shared with the backend contract.
export enum Instrument {
  BtcUsd = "BTC/USD",
}

export const instrumentParts: Record<
  Instrument,
  { baseCurrency: string; quoteCurrency: string }
> = {
  [Instrument.BtcUsd]: { baseCurrency: "BTC", quoteCurrency: "USD" },
}

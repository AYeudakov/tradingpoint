import { render, screen } from "@testing-library/react"
import userEvent from "@testing-library/user-event"
import { delay, http, HttpResponse } from "msw"
import { describe, expect, it } from "vitest"

import { server } from "@/testSetup"

import {
  AggregationMethod,
  aggregationMethodLabels,
} from "../aggregationMethod"
import { Instrument } from "../instrument"
import { PriceRequestForm } from "./PriceRequestForm"

const formProps = {
  instruments: [{ value: Instrument.BtcUsd, label: "BTC / USD" }],
  defaultInstrument: Instrument.BtcUsd,
  instrumentAvailabilityNote: "Other instruments — coming soon",
  aggregationMethods: [
    {
      value: AggregationMethod.ArithmeticMean,
      label: aggregationMethodLabels[AggregationMethod.ArithmeticMean],
    },
  ],
  defaultAggregationMethod: AggregationMethod.ArithmeticMean,
  aggregationMethodAvailabilityNote: "Other methods — coming soon",
} as const

const responseBody = {
  instrument: "BTC/USD",
  timeUtc: "2023-01-01T01:00:00+00:00",
  currency: "USD",
  aggregatedPrice: 16530.25,
  resultSource: "externalApis" as const,
  aggregationMethod: "arithmeticMean" as const,
  aggregationVersion: 1,
  sourcePrices: [
    { provider: "Bitstamp", price: 16531 },
    { provider: "Bitfinex", price: 16529.5 },
  ],
}

describe("PriceRequestForm", () => {
  it("shows loading state, aggregate, source prices, and API provenance", async () => {
    // Arrange
    const user = userEvent.setup()
    server.use(
      http.get("*/api/prices", async () => {
        await delay(80)
        return HttpResponse.json(responseBody)
      }),
    )
    render(<PriceRequestForm {...formProps} />)

    // Act
    await user.click(screen.getByRole("button", { name: "Get aggregated price" }))

    // Assert
    expect(screen.getByRole("button", { name: "Loading price…" })).toBeDisabled()
    expect(await screen.findByText("$16,530.25")).toBeInTheDocument()
    expect(screen.getByText("External APIs")).toBeInTheDocument()
    expect(screen.getByText("Bitstamp")).toBeInTheDocument()
    expect(screen.getByText("$16,531.00")).toBeInTheDocument()
    expect(screen.getByText("Bitfinex")).toBeInTheDocument()
    expect(screen.getByText("$16,529.50")).toBeInTheDocument()
  })

  it("shows a useful API error in an alert", async () => {
    // Arrange
    const user = userEvent.setup()
    server.use(
      http.get("*/api/prices", () =>
        HttpResponse.json(
          {
            title: "External prices unavailable",
            status: 503,
            detail: "Not enough providers returned a price.",
          },
          { status: 503 },
        ),
      ),
    )
    render(<PriceRequestForm {...formProps} />)

    // Act
    await user.click(screen.getByRole("button", { name: "Get aggregated price" }))

    // Assert
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Not enough providers returned a price.",
    )
  })
})

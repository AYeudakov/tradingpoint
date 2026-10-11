import { http, HttpResponse } from "msw"
import { describe, expect, it } from "vitest"

import { server } from "@/testSetup"

import { AggregationMethod } from "../aggregationMethod"
import { Instrument } from "../instrument"
import { ApiError } from "./readApiError"
import { requestPrice } from "./requestPrice"

const responseBody = {
  instrument: "BTC/USD",
  timeUtc: "2023-01-01T01:00:00+00:00",
  currency: "USD",
  aggregatedPrice: 16538.5,
  resultSource: "externalApis" as const,
  aggregationMethod: "arithmeticMean" as const,
  aggregationVersion: 1,
  sourcePrices: [
    { provider: "Bitfinex", price: 16556 },
    { provider: "Bitstamp", price: 16521 },
  ],
}

describe("requestPrice", () => {
  it("sends the typed request as the backend query contract", async () => {
    // Arrange
    let requestedUrl: URL | undefined
    server.use(
      http.get("*/api/prices", ({ request }) => {
        requestedUrl = new URL(request.url)
        return HttpResponse.json(responseBody)
      }),
    )

    // Act
    const result = await requestPrice({
      instrument: Instrument.BtcUsd,
      aggregationMethod: AggregationMethod.ArithmeticMean,
      timeUtc: "2023-01-01T01:00:00.000Z",
    })

    // Assert
    expect(requestedUrl?.searchParams.get("baseCurrency")).toBe("BTC")
    expect(requestedUrl?.searchParams.get("quoteCurrency")).toBe("USD")
    expect(requestedUrl?.searchParams.get("timeUtc")).toBe(
      "2023-01-01T01:00:00.000Z",
    )
    expect(requestedUrl?.searchParams.get("aggregationMethod")).toBe(
      "ArithmeticMean",
    )
    expect(result).toEqual(responseBody)
  })

  it("surfaces the detail from a Problem Details response", async () => {
    // Arrange
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

    // Act
    const action = requestPrice({
      instrument: Instrument.BtcUsd,
      aggregationMethod: AggregationMethod.ArithmeticMean,
      timeUtc: "2023-01-01T01:00:00.000Z",
    })

    // Assert
    await expect(action).rejects.toEqual(
      expect.objectContaining<ApiError>({
        name: "ApiError",
        message: "Not enough providers returned a price.",
        status: 503,
      }),
    )
  })
})

import { z } from "zod"

import { AggregationMethod } from "./aggregationMethod"
import { combineLocalDateAndHour } from "./dateTime"
import { Instrument } from "./instrument"

export const priceRequestSchema = z
  .object({
    instrument: z.enum(Instrument),
    aggregationMethod: z.enum(AggregationMethod),
    date: z.date({ error: "Choose a date." }),
    hour: z.number().int().min(0).max(23),
  })
  .superRefine(({ date, hour }, context) => {
    const localDateTime = combineLocalDateAndHour(date, hour)

    if (!localDateTime) {
      context.addIssue({
        code: "custom",
        path: ["hour"],
        message: "This hour does not exist in your time zone due to daylight saving time.",
      })
      return
    }

    if (localDateTime.getTime() > Date.now()) {
      context.addIssue({
        code: "custom",
        path: ["hour"],
        message: "Choose an hour that is not in the future.",
      })
    }
  })

export type PriceRequestFormValues = z.infer<typeof priceRequestSchema>

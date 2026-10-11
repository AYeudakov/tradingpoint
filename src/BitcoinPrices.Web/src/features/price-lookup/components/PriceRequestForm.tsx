import { zodResolver } from "@hookform/resolvers/zod"
import { AlertCircle, ArrowRight } from "lucide-react"
import { useEffect, useState } from "react"
import { Controller, useForm } from "react-hook-form"

import { Alert, AlertDescription, AlertTitle } from "@/shared/ui/alert"
import { Button } from "@/shared/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/ui/card"
import { Field, FieldGroup, FieldLabel } from "@/shared/ui/field"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectSeparator,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select"
import { Spinner } from "@/shared/ui/spinner"

import { requestPrice } from "../api/requestPrice"
import {
  type AggregationMethod,
} from "../aggregationMethod"
import { combineLocalDateAndHour, startOfLocalHour } from "../dateTime"
import type { Instrument } from "../instrument"
import {
  priceRequestSchema,
  type PriceRequestFormValues,
} from "../priceRequestSchema"
import type { AggregatedPrice } from "../priceTypes"
import { DateTimePicker } from "./DateTimePicker"
import { PriceResult } from "./PriceResult"

type SelectOption<TValue extends string> = {
  value: TValue
  label: string
}

type PriceRequestFormProps = {
  instruments: readonly SelectOption<Instrument>[]
  defaultInstrument: Instrument
  instrumentAvailabilityNote?: string
  aggregationMethods: readonly SelectOption<AggregationMethod>[]
  defaultAggregationMethod: AggregationMethod
  aggregationMethodAvailabilityNote?: string
}

export function PriceRequestForm({
  instruments,
  defaultInstrument,
  instrumentAvailabilityNote,
  aggregationMethods,
  defaultAggregationMethod,
  aggregationMethodAvailabilityNote,
}: PriceRequestFormProps) {
  const defaultTime = startOfLocalHour()
  const [result, setResult] = useState<AggregatedPrice>()
  const [requestError, setRequestError] = useState<string>()
  const [activeRequest, setActiveRequest] = useState<AbortController>()
  const [instrumentSelectOpen, setInstrumentSelectOpen] = useState(false)
  const [aggregationSelectOpen, setAggregationSelectOpen] = useState(false)

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<PriceRequestFormValues>({
    resolver: zodResolver(priceRequestSchema),
    defaultValues: {
      instrument: defaultInstrument,
      aggregationMethod: defaultAggregationMethod,
      date: defaultTime,
      hour: defaultTime.getHours(),
    },
  })

  useEffect(() => () => activeRequest?.abort(), [activeRequest])

  const submit = handleSubmit(async (values) => {
    const localDateTime = combineLocalDateAndHour(values.date, values.hour)
    if (!localDateTime) return

    const requestController = new AbortController()
    setActiveRequest(requestController)
    setRequestError(undefined)

    try {
      setResult(
        await requestPrice(
          {
            instrument: values.instrument,
            aggregationMethod: values.aggregationMethod,
            timeUtc: localDateTime.toISOString(),
          },
          requestController.signal,
        ),
      )
    } catch (error) {
      if (error instanceof DOMException && error.name === "AbortError") return
      setRequestError(
        error instanceof Error
          ? error.message
          : "The price could not be loaded. Please try again.",
      )
    } finally {
      setActiveRequest((current) =>
        current === requestController ? undefined : current,
      )
    }
  })

  return (
    <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,0.92fr)_minmax(22rem,1.08fr)]">
      <Card className="border-0 shadow-[0_20px_50px_-32px_rgba(15,23,42,0.45)] ring-1 ring-slate-900/10">
        <CardHeader className="border-b pb-4">
          <div className="text-[0.68rem] font-semibold tracking-[0.16em] text-primary uppercase">
            Price lookup
          </div>
          <CardTitle className="text-xl">Choose a market hour</CardTitle>
          <CardDescription>
            We aggregate the closing price reported by each configured provider.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={submit} className="space-y-5" noValidate>
            <FieldGroup>
              <Controller
                control={control}
                name="instrument"
                render={({ field }) => (
                  <Field
                    className={
                      instrumentSelectOpen
                        ? "pb-20 transition-[padding]"
                        : "transition-[padding]"
                    }
                  >
                    <FieldLabel htmlFor="instrument">Instrument</FieldLabel>
                    <Select
                      value={field.value}
                      onValueChange={field.onChange}
                      onOpenChange={setInstrumentSelectOpen}
                      disabled={isSubmitting}
                    >
                      <SelectTrigger id="instrument" className="h-10 w-full bg-background">
                        <SelectValue>
                          {instruments.find(({ value }) => value === field.value)?.label}
                        </SelectValue>
                      </SelectTrigger>
                      <SelectContent align="start" alignItemWithTrigger={false}>
                        {instruments.map(({ value, label }) => (
                          <SelectItem key={value} value={value}>
                            {label}
                          </SelectItem>
                        ))}
                        {instrumentAvailabilityNote && (
                          <>
                            <SelectSeparator />
                            <SelectItem value="instrument-info" disabled>
                              {instrumentAvailabilityNote}
                            </SelectItem>
                          </>
                        )}
                      </SelectContent>
                    </Select>
                  </Field>
                )}
              />

              <Controller
                control={control}
                name="aggregationMethod"
                render={({ field }) => (
                  <Field
                    className={
                      aggregationSelectOpen
                        ? "pb-20 transition-[padding]"
                        : "transition-[padding]"
                    }
                  >
                    <FieldLabel htmlFor="aggregation-method">
                      Aggregation method
                    </FieldLabel>
                    <Select
                      value={field.value}
                      onValueChange={field.onChange}
                      onOpenChange={setAggregationSelectOpen}
                      disabled={isSubmitting}
                    >
                      <SelectTrigger
                        id="aggregation-method"
                        className="h-10 w-full bg-background"
                      >
                        <SelectValue>
                          {
                            aggregationMethods.find(
                              ({ value }) => value === field.value,
                            )?.label
                          }
                        </SelectValue>
                      </SelectTrigger>
                      <SelectContent align="start" alignItemWithTrigger={false}>
                        {aggregationMethods.map(({ value, label }) => (
                          <SelectItem key={value} value={value}>
                            {label}
                          </SelectItem>
                        ))}
                        {aggregationMethodAvailabilityNote && (
                          <>
                            <SelectSeparator />
                            <SelectItem value="method-info" disabled>
                              {aggregationMethodAvailabilityNote}
                            </SelectItem>
                          </>
                        )}
                      </SelectContent>
                    </Select>
                  </Field>
                )}
              />

              <DateTimePicker
                control={control}
                disabled={isSubmitting}
                dateError={errors.date?.message}
                hourError={errors.hour?.message}
              />
            </FieldGroup>

            {requestError && (
              <Alert variant="destructive">
                <AlertCircle />
                <AlertTitle>Could not load the price</AlertTitle>
                <AlertDescription>{requestError}</AlertDescription>
              </Alert>
            )}

            <Button type="submit" size="lg" className="w-full" disabled={isSubmitting}>
              {isSubmitting ? (
                <>
                  <Spinner aria-hidden="true" />
                  Loading price…
                </>
              ) : (
                <>
                  Get aggregated price
                  <ArrowRight />
                </>
              )}
            </Button>
          </form>
        </CardContent>
      </Card>

      {result ? (
        <PriceResult result={result} />
      ) : (
        <div className="hidden min-h-80 items-center justify-center rounded-xl border border-dashed border-slate-300 bg-white/35 p-8 text-center text-sm text-muted-foreground lg:flex">
          <div className="max-w-56">
            <div className="mx-auto mb-3 size-2 rounded-full bg-primary shadow-[0_0_0_6px_rgba(37,99,235,0.1)]" />
            Your aggregated price and provider breakdown will appear here.
          </div>
        </div>
      )}
    </div>
  )
}

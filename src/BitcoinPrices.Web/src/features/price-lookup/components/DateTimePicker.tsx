import { format } from "date-fns"
import { CalendarDays, Clock3 } from "lucide-react"
import { useState } from "react"
import { Controller, type Control, useWatch } from "react-hook-form"

import { Button } from "@/shared/ui/button"
import { Calendar } from "@/shared/ui/calendar"
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/shared/ui/field"
import { Popover, PopoverContent, PopoverTrigger } from "@/shared/ui/popover"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui/select"

import {
  browserTimeZone,
  combineLocalDateAndHour,
  formatLocalTime,
  formatUtcTime,
} from "../dateTime"
import type { PriceRequestFormValues } from "../priceRequestSchema"

type DateTimePickerProps = {
  control: Control<PriceRequestFormValues>
  disabled?: boolean
  dateError?: string
  hourError?: string
}

const hours = Array.from({ length: 24 }, (_, hour) => hour)

export function DateTimePicker({
  control,
  disabled,
  dateError,
  hourError,
}: DateTimePickerProps) {
  const date = useWatch({ control, name: "date" })
  const hour = useWatch({ control, name: "hour" })
  const [now] = useState(() => new Date())
  const localDateTime = date ? combineLocalDateAndHour(date, hour) : null

  return (
    <Field data-invalid={Boolean(dateError || hourError)}>
      <FieldLabel>Requested time</FieldLabel>
      <div className="grid grid-cols-[minmax(0,1fr)_7.5rem] gap-2">
        <Controller
          control={control}
          name="date"
          render={({ field }) => (
            <Popover>
              <PopoverTrigger
                render={
                  <Button
                    type="button"
                    variant="outline"
                    aria-label="Choose date"
                    disabled={disabled}
                    className="h-10 w-full justify-start bg-background px-3 font-normal"
                  />
                }
              >
                <CalendarDays className="text-muted-foreground" />
                {field.value ? format(field.value, "PPP") : "Choose date"}
              </PopoverTrigger>
              <PopoverContent align="start" className="w-auto p-0">
                <Calendar
                  mode="single"
                  selected={field.value}
                  onSelect={(value) => value && field.onChange(value)}
                  disabled={{ after: now }}
                  timeZone={browserTimeZone()}
                  autoFocus
                />
              </PopoverContent>
            </Popover>
          )}
        />

        <Controller
          control={control}
          name="hour"
          render={({ field }) => (
            <Select
              value={String(field.value)}
              onValueChange={(value) => field.onChange(Number(value))}
              disabled={disabled}
            >
              <SelectTrigger aria-label="Choose hour" className="h-10 w-full bg-background">
                <Clock3 className="text-muted-foreground" />
                <SelectValue>{String(field.value).padStart(2, "0")}:00</SelectValue>
              </SelectTrigger>
              <SelectContent align="start" alignItemWithTrigger={false}>
                {hours.map((value) => (
                  <SelectItem
                    key={value}
                    value={String(value)}
                    disabled={
                      date
                        ? (combineLocalDateAndHour(date, value)?.getTime() ?? Infinity) >
                          now.getTime()
                        : true
                    }
                  >
                    {String(value).padStart(2, "0")}:00
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
      </div>
      <FieldDescription>
        Choose an hour in your local time zone. Future hours are unavailable.
      </FieldDescription>
      <FieldError>{dateError ?? hourError}</FieldError>

      {localDateTime && (
        <div className="mt-1 grid overflow-hidden rounded-lg border bg-muted/35 sm:grid-cols-2">
          <TimeValue label="Your local time" value={formatLocalTime(localDateTime)} />
          <TimeValue
            label="Start time in UTC"
            value={formatUtcTime(localDateTime)}
            className="border-t sm:border-t-0 sm:border-l"
          />
        </div>
      )}
    </Field>
  )
}

function TimeValue({
  label,
  value,
  className = "",
}: {
  label: string
  value: string
  className?: string
}) {
  return (
    <div className={`px-3 py-2.5 ${className}`}>
      <div className="text-[0.68rem] font-semibold tracking-[0.12em] text-muted-foreground uppercase">
        {label}
      </div>
      <div className="mt-1 font-mono text-xs font-medium">{value}</div>
    </div>
  )
}

import { Activity } from "lucide-react"
import type { ReactNode } from "react"

export function PageContent({ children }: { children: ReactNode }) {
  return (
    <main className="relative min-h-screen overflow-hidden px-4 py-6 sm:px-6 sm:py-10">
      <div className="pointer-events-none absolute inset-0 market-grid opacity-45" />
      <div className="relative mx-auto max-w-5xl">{children}</div>
    </main>
  )
}

export function PageHeader() {
  return (
    <header className="mb-8 flex items-center justify-between border-b border-slate-900/10 pb-4">
      <div className="flex items-center gap-3">
        <div className="grid size-9 place-items-center rounded-lg bg-slate-950 text-white shadow-sm">
          <Activity className="size-5" />
        </div>
        <div>
          <div className="text-sm font-semibold tracking-tight">Price Ledger</div>
          <div className="text-[0.67rem] tracking-[0.16em] text-muted-foreground uppercase">
            Hourly market aggregate
          </div>
        </div>
      </div>
    </header>
  )
}

export function PageHero() {
  return (
    <section className="mb-8 max-w-2xl">
      <h1 className="max-w-xl text-3xl leading-tight font-semibold tracking-[-0.035em] text-balance sm:text-5xl">
        One hourly price,
        <span className="text-slate-500"> verified across providers.</span>
      </h1>
      <p className="mt-4 max-w-xl text-sm leading-6 text-muted-foreground sm:text-base">
        Select a local market hour. We translate it to UTC, retrieve or reuse
        provider observations, and show exactly where the result came from.
      </p>
    </section>
  )
}

export function PageFooter() {
  return (
    <footer className="mt-8 border-t border-slate-900/10 pt-4 text-xs text-muted-foreground">
      Prices are informational and reflect the selected one-hour candle close.
    </footer>
  )
}

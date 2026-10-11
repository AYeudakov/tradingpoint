import type { ProblemDetails } from "./problemDetails"

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status?: number,
  ) {
    super(message)
    this.name = "ApiError"
  }
}

export async function readApiError(response: Response) {
  let problem: ProblemDetails | undefined

  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
    // A non-JSON proxy or network response still gets a useful fallback below.
  }

  return new ApiError(
    problem?.detail ?? problem?.title ?? "The price could not be loaded. Please try again.",
    problem?.status ?? response.status,
  )
}

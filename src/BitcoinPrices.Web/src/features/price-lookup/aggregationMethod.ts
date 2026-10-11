// In a real-world application, this type could be generated from or shared with the backend contract.
export enum AggregationMethod {
  ArithmeticMean = "ArithmeticMean",
}

export const aggregationMethodLabels: Record<AggregationMethod, string> = {
  [AggregationMethod.ArithmeticMean]: "Arithmetic mean",
}

// All API calls — never expose DB credentials here, backend handles auth
const BASE_URL = import.meta.env.DEV
  ? ''  // proxied via vite dev server to localhost:5297
  : 'https://gasbackend.onrender.com'

export const API = {
  columnNames: `${BASE_URL}/api/Read/ActualValuesDescriptions`,
  forecast: `${BASE_URL}/api/algorithm/forecast`,
  prediction: `${BASE_URL}/api/algorithm/prediction`,
  uploadColumnNames: `${BASE_URL}/api/upload/csvColumnNames`,
  uploadActualValues: `${BASE_URL}/api/upload/csvActualValues`,
}

export interface ColumnData {
  id: number
  columnName: string
  totalCount: number
  actualValues: number[]
  dateOfEntry: string
}

export interface ForecastResult {
  algoType: string
  columnName: string
  alphaSes: number
  alphaHwes: number
  beta: number
  gamma: number
  forecastValues: number[]
  predictionValues: number[]
  predictionValues2?: number[]
  preditionValuesAverage?: number[]
  mse: number
  mae: number
  mape: number
  rmse: number
  mse2?: number
  mae2?: number
  mape2?: number
  rmse2?: number
  mse3?: number
  mae3?: number
  mape3?: number
  rmse3?: number
  timeComputed: string
}

export async function fetchColumns(): Promise<ColumnData[]> {
  const res = await fetch(API.columnNames)
  if (!res.ok) throw new Error(`Failed to load columns (${res.status})`)
  return res.json()
}

export async function runForecast(payload: object): Promise<ForecastResult> {
  const res = await fetch(API.forecast, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) {
    const err = await res.text()
    throw new Error(err || `Forecast failed (${res.status})`)
  }
  return res.json()
}

export async function runPrediction(payload: object, horizon: number): Promise<ForecastResult> {
  const res = await fetch(`${API.prediction}?forecastHorizon=${horizon}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!res.ok) {
    const err = await res.text()
    throw new Error(err || `Prediction failed (${res.status})`)
  }
  return res.json()
}

export async function uploadCsvForColumns(file: File): Promise<string[]> {
  const form = new FormData()
  form.append('File', file)
  const res = await fetch(API.uploadColumnNames, { method: 'POST', body: form })
  if (!res.ok) throw new Error(`Upload failed (${res.status})`)
  const data = await res.json()
  return data.columnNames ?? data
}

export async function uploadCsvActualValues(file: File, columnName: string): Promise<void> {
  const form = new FormData()
  form.append('File', file)
  form.append('SColumnName', columnName)  // must match SelectedColumnName.SColumnName on backend
  const res = await fetch(API.uploadActualValues, { method: 'POST', body: form })
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: `Upload failed (${res.status})` }))
    throw new Error(err.message)
  }
}

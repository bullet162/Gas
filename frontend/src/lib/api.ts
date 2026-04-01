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
  // flat fields (prediction endpoint returns ALgoOutput directly)
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
  timeComputed: string
  // error metrics (flat — used when prediction endpoint embeds them)
  mse?: number
  mae?: number
  mape?: number
  rmse?: number
  mse2?: number
  mae2?: number
  mape2?: number
  rmse2?: number
  mse3?: number
  mae3?: number
  mape3?: number
  rmse3?: number
  // nested shape returned by /forecast endpoint
  algoOutput?: {
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
    timeComputed: string
  }
  errorOutput?: {
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
  }
}

export async function fetchColumns(): Promise<ColumnData[]> {
  const res = await fetch(API.columnNames)
  if (!res.ok) throw new Error(`Failed to load columns (${res.status})`)
  return res.json()
}

/** Flatten the nested { algoOutput, errorOutput } shape from /forecast into ForecastResult */
function normalizeForecast(raw: any): ForecastResult {
  if (raw.algoOutput) {
    const a = raw.algoOutput
    const e = raw.errorOutput ?? {}
    return {
      ...a,
      mse: e.mse ?? e.MSE,
      mae: e.mae ?? e.MAE,
      rmse: e.rmse ?? e.RMSE,
      mape: e.mape ?? e.MAPE,
      mse2: e.mse2 ?? e.MSE2,
      mae2: e.mae2 ?? e.MAE2,
      rmse2: e.rmse2 ?? e.RMSE2,
      mape2: e.mape2 ?? e.MAPE2,
      mse3: e.mse3 ?? e.MSE3,
      mae3: e.mae3 ?? e.MAE3,
      rmse3: e.rmse3 ?? e.RMSE3,
      mape3: e.mape3 ?? e.MAPE3,
    } as ForecastResult
  }
  return raw as ForecastResult
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
  const raw = await res.json()
  return normalizeForecast(raw)
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

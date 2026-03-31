import { writable } from 'svelte/store'
import type { ColumnData, ForecastResult } from './api'

export type Page = 'home' | 'validate' | 'forecast'

export const currentPage = writable<Page>('home')
export const columns = writable<ColumnData[]>([])
export const lastForecastResult = writable<ForecastResult | null>(null)
export const selectedColumn = writable<string>('')

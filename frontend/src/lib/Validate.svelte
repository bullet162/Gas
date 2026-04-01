<script lang="ts">
  import { onMount } from 'svelte'
  import { currentPage, columns, lastForecastResult, selectedColumn } from './store'
  import { fetchColumns, runForecast } from './api'
  import type { ForecastResult } from './api'
  import Chart from './Chart.svelte'
  import UploadModal from './UploadModal.svelte'

  let loading = false
  let error = ''
  let uploadOpen = false
  let result: ForecastResult | null = null

  let colName = ''
  let algo = 'ses'
  let logTransform = 'no'
  let seasonLength = 0
  let showSeason = false

  $: showSeason = algo === 'hwes' || algo === 'gas'
  $: if (!showSeason) seasonLength = 0

  // Chart data
  let chartLabels: string[] = []
  let chartDatasets: any[] = []

  onMount(async () => {
    await loadColumns()
  })

  async function loadColumns() {
    try {
      loading = true
      const data = await fetchColumns()
      columns.set(data)
    } catch (e: any) {
      error = e.message
    } finally {
      loading = false
    }
  }

  async function forecast() {
    if (!colName) { error = 'Select a data column first.'; return }
    error = ''
    loading = true
    try {
      const col = $columns.find(c => c.columnName === colName)
      if (!col) throw new Error('Column not found.')

      const payload = {
        input: col.actualValues,
        algoType: algo,
        columnName: colName,
        logTransform,
        seasonLength: algo === 'ses' ? 1 : seasonLength,
      }

      result = await runForecast(payload)
      lastForecastResult.set(result)
      selectedColumn.set(colName)
      buildChart(col.actualValues.map(Number), result)
    } catch (e: any) {
      error = e.message
    } finally {
      loading = false
    }
  }

  function buildChart(actual: number[], r: ForecastResult) {
    const len = actual.length
    const predLen = r.predictionValues?.length ?? 0
    const trainLen = len - predLen
    chartLabels = actual.map((_, i) => `P${i + 1}`)

    // Pad predictions to align with test portion
    const paddedPred = [...Array(trainLen).fill(null), ...(r.predictionValues?.map(Number) ?? [])]
    const paddedHwes = r.predictionValues2
      ? [...Array(trainLen).fill(null), ...r.predictionValues2.map(Number)]
      : null

    chartDatasets = [
      {
        label: 'Actual',
        data: actual,
        borderColor: '#6366f1',
        backgroundColor: 'rgba(99,102,241,0.08)',
        tension: 0.4, fill: true,
        pointRadius: len > 100 ? 0 : 3,
        borderWidth: 2,
      },
      {
        label: algo === 'gas' ? 'Weighted SES' : 'Forecast',
        data: paddedPred,
        borderColor: '#10b981',
        backgroundColor: 'transparent',
        tension: 0.4, fill: false,
        borderDash: [4, 3],
        pointRadius: 0,
        borderWidth: 1.5,
      },
    ]

    if (algo === 'gas' && paddedHwes) {
      chartDatasets.push({
        label: 'Weighted HWES',
        data: paddedHwes,
        borderColor: '#06b6d4',
        backgroundColor: 'transparent',
        tension: 0.4, fill: false,
        borderDash: [4, 3],
        pointRadius: 0,
        borderWidth: 1.5,
      })
    }
  }

  function formatNum(n: number | undefined | null) {
    if (n === undefined || n === null || isNaN(Number(n))) return '—'
    return Number(n).toFixed(4)
  }

  function getMetrics(r: ForecastResult) {
    if (algo === 'gas') {
      return [
        { label: 'MSE (SES)', value: formatNum(r.mse) },
        { label: 'MAE (SES)', value: formatNum(r.mae) },
        { label: 'RMSE (SES)', value: formatNum(r.rmse) },
        { label: 'MAPE (SES)', value: formatNum(r.mape) },
        { label: 'MSE (HWES)', value: formatNum(r.mse2) },
        { label: 'MAE (HWES)', value: formatNum(r.mae2) },
        { label: 'RMSE (HWES)', value: formatNum(r.rmse2) },
        { label: 'MAPE (HWES)', value: formatNum(r.mape2) },
      ]
    }
    return [
      { label: 'MSE', value: formatNum(r.mse) },
      { label: 'MAE', value: formatNum(r.mae) },
      { label: 'RMSE', value: formatNum(r.rmse) },
      { label: 'MAPE', value: formatNum(r.mape) },
    ]
  }
</script>

<div class="page">
  <header>
    <button class="back-btn" on:click={() => currentPage.set('home')}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M19 12H5M12 5l-7 7 7 7"/></svg>
      Home
    </button>
    <h1>Forecasting Simulator</h1>
    <button class="nav-btn accent" on:click={() => currentPage.set('forecast')}>
      Go to Prediction
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
    </button>
  </header>

  <!-- Controls -->
  <div class="controls">
    <div class="control-group">
      <span class="ctrl-label">Dataset</span>
      <button class="ctrl-btn upload" aria-label="Upload CSV" on:click={() => uploadOpen = true}>
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
        Upload CSV
      </button>
    </div>

    <div class="control-group">
      <label for="col">Data Column</label>
      <select id="col" bind:value={colName}>
        <option value="">Select column</option>
        {#each $columns as col}
          <option value={col.columnName}>{col.columnName}</option>
        {/each}
      </select>
    </div>

    <div class="control-group">
      <label for="algo">Algorithm</label>
      <select id="algo" bind:value={algo}>
        <option value="ses">SES</option>
        <option value="hwes">HWES</option>
        <option value="gas">GAS (Hybrid)</option>
      </select>
    </div>

    <div class="control-group">
      <label for="log">Log Transform</label>
      <select id="log" bind:value={logTransform}>
        <option value="no">No</option>
        <option value="yes">Yes (log₁₀)</option>
      </select>
    </div>

    {#if showSeason}
      <div class="control-group">
        <label for="season">Season Length</label>
        <input id="season" type="number" min="0" bind:value={seasonLength} />
      </div>
    {/if}

    <div class="control-group run-group">
      <button class="ctrl-btn run" aria-label="Run Forecast" on:click={forecast} disabled={loading}>
        {#if loading}
          <span class="spinner"></span> Running…
        {:else}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Run Forecast
        {/if}
      </button>
    </div>
  </div>

  {#if error}
    <div class="error-bar">{error}</div>
  {/if}

  <div class="main">
    <!-- Chart -->
    <div class="chart-card">
      <div class="card-header">
        <span class="card-title">Forecast Chart</span>
        {#if result}
          <span class="tag">{result.algoType?.toUpperCase()} · {result.timeComputed}</span>
        {/if}
      </div>
      <div class="chart-wrap">
        {#if chartDatasets.length}
          <Chart labels={chartLabels} datasets={chartDatasets} />
        {:else}
          <div class="empty-chart">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#334155" stroke-width="1.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
            <p>Run a forecast to see results</p>
          </div>
        {/if}
      </div>
    </div>

    {#if result}
      <!-- Metrics -->
      <div class="metrics-card">
        <div class="card-header"><span class="card-title">Performance Metrics</span></div>
        <div class="metrics-grid">
          {#each getMetrics(result) as m}
            <div class="metric">
              <div class="metric-label">{m.label}</div>
              <div class="metric-value">{m.value}</div>
            </div>
          {/each}
        </div>
        {#if result.alphaSes}
          <div class="params">
            <span class="param">α<sub>SES</sub> = {result.alphaSes?.toFixed(4)}</span>
            {#if algo !== 'ses'}
              <span class="param">α<sub>HWES</sub> = {result.alphaHwes?.toFixed(4)}</span>
              <span class="param">β = {result.beta?.toFixed(4)}</span>
              <span class="param">γ = {result.gamma?.toFixed(4)}</span>
            {/if}
          </div>
        {/if}
      </div>

      <!-- Table -->
      <div class="table-card">
        <div class="card-header"><span class="card-title">Forecast Values</span></div>
        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Period</th>
                <th>Actual</th>
                <th>{algo === 'gas' ? 'Weighted SES' : 'Forecast'}</th>
                {#if algo === 'gas'}<th>Weighted HWES</th>{/if}
                <th>Residual</th>
              </tr>
            </thead>
            <tbody>
              {#each $columns.find(c => c.columnName === colName)?.actualValues ?? [] as actual, i}
                {@const allActuals = $columns.find(c => c.columnName === colName)?.actualValues ?? []}
                {@const totalLen = allActuals.length}
                {@const predLen = result.predictionValues?.length ?? 0}
                {@const trainLen = totalLen - predLen}
                {@const predIdx = i - trainLen}
                {@const forecast_val = predIdx >= 0 ? result.predictionValues?.[predIdx] : null}
                {@const hwes_val = predIdx >= 0 ? result.predictionValues2?.[predIdx] : null}
                {@const residual = forecast_val != null ? (Number(actual) - Number(forecast_val)).toFixed(4) : '—'}
                <tr>
                  <td>{i + 1}</td>
                  <td>{Number(actual).toFixed(4)}</td>
                  <td>{forecast_val != null ? Number(forecast_val).toFixed(4) : '—'}</td>
                  {#if algo === 'gas'}<td>{hwes_val != null ? Number(hwes_val).toFixed(4) : '—'}</td>{/if}
                  <td class:neg={Number(residual) < 0}>{residual}</td>
                </tr>
              {/each}
            </tbody>
          </table>
        </div>
      </div>
    {/if}
  </div>
</div>

<UploadModal bind:open={uploadOpen} on:uploaded={loadColumns} />

{#if loading}
  <div class="loading-overlay">
    <div class="spinner large"></div>
  </div>
{/if}

<style>
  .page { min-height: 100vh; display: flex; flex-direction: column; }

  header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.9rem 1.5rem;
    background: #0f172a; border-bottom: 1px solid #1e293b;
    position: sticky; top: 0; z-index: 10;
  }
  h1 { font-size: 1.1rem; font-weight: 600; color: #e2e8f0; }
  .back-btn, .nav-btn {
    display: inline-flex; align-items: center; gap: 0.4rem;
    padding: 0.5rem 1rem; border-radius: 8px;
    font-size: 0.85rem; font-weight: 500; cursor: pointer;
    transition: all 0.2s; border: 1px solid #334155;
    background: none; color: #94a3b8;
  }
  .back-btn:hover { color: #e2e8f0; border-color: #475569; }
  .nav-btn.accent { background: linear-gradient(135deg, #6366f1, #8b5cf6); border: none; color: white; }
  .nav-btn.accent:hover { opacity: 0.9; }

  .controls {
    display: flex; flex-wrap: wrap; gap: 1rem; align-items: flex-end;
    padding: 1rem 1.5rem;
    background: #0f172a; border-bottom: 1px solid #1e293b;
  }
  .control-group { display: flex; flex-direction: column; gap: 0.35rem; min-width: 140px; }
  label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.6px; color: #475569; font-weight: 600; }
  .ctrl-label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.6px; color: #475569; font-weight: 600; }
  select, input[type="number"] {
    padding: 0.6rem 0.8rem;
    background: #1e293b; border: 1px solid #334155;
    border-radius: 8px; color: #e2e8f0; font-size: 0.88rem;
    transition: border-color 0.2s;
  }
  select:focus, input:focus { outline: none; border-color: #6366f1; }
  .ctrl-btn {
    display: inline-flex; align-items: center; gap: 0.5rem;
    padding: 0.6rem 1.1rem; border-radius: 8px;
    font-size: 0.88rem; font-weight: 600; cursor: pointer;
    border: none; transition: all 0.2s;
  }
  .ctrl-btn.upload { background: #1e293b; border: 1px solid #334155; color: #94a3b8; }
  .ctrl-btn.upload:hover { border-color: #6366f1; color: #e2e8f0; }
  .ctrl-btn.run { background: linear-gradient(135deg, #6366f1, #8b5cf6); color: white; }
  .ctrl-btn.run:hover:not(:disabled) { opacity: 0.9; }
  .ctrl-btn.run:disabled { opacity: 0.5; cursor: not-allowed; }

  .error-bar {
    margin: 0.8rem 1.5rem; padding: 0.7rem 1rem;
    background: rgba(248,113,113,0.1); border: 1px solid rgba(248,113,113,0.3);
    border-radius: 8px; color: #f87171; font-size: 0.88rem;
  }

  .main { flex: 1; padding: 1.5rem; display: flex; flex-direction: column; gap: 1.5rem; }

  /* Cards */
  .chart-card, .metrics-card, .table-card {
    background: #1e293b; border: 1px solid #334155; border-radius: 12px; overflow: hidden;
  }
  .card-header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.9rem 1.2rem; border-bottom: 1px solid #334155;
  }
  .card-title { font-size: 0.88rem; font-weight: 600; color: #e2e8f0; }
  .tag { font-size: 0.75rem; color: #6366f1; background: rgba(99,102,241,0.1); padding: 0.25rem 0.7rem; border-radius: 100px; }

  .chart-wrap { height: 360px; padding: 1rem; }
  .empty-chart {
    height: 100%; display: flex; flex-direction: column;
    align-items: center; justify-content: center; gap: 0.8rem;
  }
  .empty-chart p { color: #334155; font-size: 0.88rem; }

  /* Metrics */
  .metrics-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 1px; background: #334155; }
  .metric { background: #1e293b; padding: 1rem 1.2rem; }
  .metric-label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.5px; color: #475569; margin-bottom: 0.4rem; }
  .metric-value { font-size: 1.3rem; font-weight: 700; color: #6366f1; font-variant-numeric: tabular-nums; }
  .params { display: flex; flex-wrap: wrap; gap: 0.8rem; padding: 0.8rem 1.2rem; border-top: 1px solid #334155; }
  .param { font-size: 0.82rem; color: #64748b; background: #0f172a; padding: 0.3rem 0.7rem; border-radius: 6px; }

  /* Table */
  .table-wrap { max-height: 320px; overflow-y: auto; }
  .table-wrap::-webkit-scrollbar { width: 5px; }
  .table-wrap::-webkit-scrollbar-track { background: #0f172a; }
  .table-wrap::-webkit-scrollbar-thumb { background: #334155; border-radius: 3px; }
  table { width: 100%; border-collapse: collapse; }
  th, td { padding: 0.55rem 1rem; text-align: left; border-bottom: 1px solid #1e293b; font-size: 0.82rem; }
  th { color: #475569; font-weight: 600; font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.5px; position: sticky; top: 0; background: #0f172a; }
  td { color: #94a3b8; }
  tr:hover td { background: rgba(99,102,241,0.04); }
  td.neg { color: #f87171; }

  /* Loading */
  .loading-overlay {
    position: fixed; inset: 0;
    background: rgba(10,15,28,0.8); backdrop-filter: blur(4px);
    display: flex; align-items: center; justify-content: center; z-index: 50;
  }
  .spinner {
    width: 28px; height: 28px;
    border: 2px solid rgba(255,255,255,0.1);
    border-top-color: #6366f1;
    border-radius: 50%;
    animation: spin 0.7s linear infinite;
    display: inline-block;
  }
  .spinner.large { width: 44px; height: 44px; border-width: 3px; }
  @keyframes spin { to { transform: rotate(360deg); } }

  @media (max-width: 768px) {
    header { flex-direction: column; gap: 0.6rem; text-align: center; }
    .controls { flex-direction: column; }
    .control-group { min-width: 100%; }
  }
</style>

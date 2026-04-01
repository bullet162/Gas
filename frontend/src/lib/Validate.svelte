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
  let showLast25 = false

  $: showSeason = algo === 'hwes' || algo === 'gas'
  $: if (!showSeason) seasonLength = 0

  let chartLabels: string[] = []
  let chartDatasets: any[] = []
  let fullActuals: number[] = []
  let fullPreds: number[] = []
  let fullPreds2: number[] = []

  onMount(async () => { await loadColumns() })

  async function loadColumns() {
    try {
      loading = true
      const data = await fetchColumns()
      columns.set(data)
    } catch (e: any) { error = e.message }
    finally { loading = false }
  }

  async function forecast() {
    if (!colName) { error = 'Select a data column first.'; return }
    error = ''
    loading = true
    showLast25 = false
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
      fullActuals = col.actualValues.map(Number)
      fullPreds = result.predictionValues?.map(Number) ?? []
      fullPreds2 = result.predictionValues2?.map(Number) ?? []
      buildChart(fullActuals, fullPreds, fullPreds2)
    } catch (e: any) { error = e.message }
    finally { loading = false }
  }

  function buildChart(actuals: number[], preds: number[], preds2: number[]) {
    const len = actuals.length
    const predLen = preds.length
    const trainLen = len - predLen
    chartLabels = actuals.map((_, i) => `P${i + 1}`)
    const padded = [...Array(trainLen).fill(null), ...preds]
    const padded2 = preds2.length ? [...Array(trainLen).fill(null), ...preds2] : null

    chartDatasets = [
      {
        label: 'Actual',
        data: actuals,
        borderColor: '#7b68ee',
        backgroundColor: 'rgba(123,104,238,0.08)',
        tension: 0.4, fill: true,
        pointRadius: len > 100 ? 0 : 3,
        borderWidth: 2,
      },
      {
        label: algo === 'gas' ? 'Weighted SES' : 'Forecast',
        data: padded,
        borderColor: '#00d4aa',
        backgroundColor: 'transparent',
        tension: 0.4, fill: false,
        borderDash: [4, 3],
        pointRadius: 0,
        borderWidth: 1.5,
      },
    ]
    if (algo === 'gas' && padded2) {
      chartDatasets.push({
        label: 'Weighted HWES',
        data: padded2,
        borderColor: '#fd79a8',
        backgroundColor: 'transparent',
        tension: 0.4, fill: false,
        borderDash: [4, 3],
        pointRadius: 0,
        borderWidth: 1.5,
      })
    }
  }

  function toggleLast25() {
    showLast25 = !showLast25
    if (showLast25) {
      const start = Math.floor(fullActuals.length * 0.75)
      buildChart(fullActuals.slice(start), fullPreds, fullPreds2)
      chartLabels = fullActuals.slice(start).map((_, i) => `P${start + i + 1}`)
    } else {
      buildChart(fullActuals, fullPreds, fullPreds2)
    }
  }

  function exportCSV() {
    if (!result) return
    const col = $columns.find(c => c.columnName === colName)
    const actuals = col?.actualValues ?? []
    const predLen = fullPreds.length
    const trainLen = actuals.length - predLen
    const rows = [['Period', 'Actual', algo === 'gas' ? 'Weighted SES' : 'Forecast', ...(algo === 'gas' ? ['Weighted HWES'] : []), 'Residual']]
    actuals.forEach((v, i) => {
      const pi = i - trainLen
      const fv = pi >= 0 ? fullPreds[pi] : null
      const fv2 = pi >= 0 && fullPreds2.length ? fullPreds2[pi] : null
      const res = fv != null ? (Number(v) - fv).toFixed(4) : ''
      rows.push([`P${i+1}`, String(v), fv != null ? fv.toFixed(4) : '', ...(algo === 'gas' ? [fv2 != null ? fv2.toFixed(4) : ''] : []), res])
    })
    const csv = rows.map(r => r.join(',')).join('\n')
    const a = document.createElement('a')
    a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }))
    a.download = `${colName}_${algo}_forecast.csv`
    a.click()
  }

  function fmt(n: number | undefined | null) {
    if (n == null || isNaN(Number(n))) return '—'
    return Number(n).toFixed(4)
  }

  $: metrics = result ? (algo === 'gas' ? [
    { label: 'MSE (SES)',  value: fmt(result.mse),   color: 'indigo' },
    { label: 'MAE (SES)',  value: fmt(result.mae),   color: 'indigo' },
    { label: 'RMSE (SES)', value: fmt(result.rmse),  color: 'indigo' },
    { label: 'MAPE (SES)', value: fmt(result.mape),  color: 'indigo' },
    { label: 'MSE (HWES)', value: fmt(result.mse2),  color: 'pink' },
    { label: 'MAE (HWES)', value: fmt(result.mae2),  color: 'pink' },
    { label: 'RMSE (HWES)',value: fmt(result.rmse2), color: 'pink' },
    { label: 'MAPE (HWES)',value: fmt(result.mape2), color: 'pink' },
  ] : [
    { label: 'MSE',  value: fmt(result.mse),  color: 'indigo' },
    { label: 'MAE',  value: fmt(result.mae),  color: 'indigo' },
    { label: 'RMSE', value: fmt(result.rmse), color: 'indigo' },
    { label: 'MAPE', value: fmt(result.mape), color: 'indigo' },
  ]) : []

  $: tableRows = (() => {
    if (!result) return []
    const col = $columns.find(c => c.columnName === colName)
    const actuals = col?.actualValues ?? []
    const predLen = fullPreds.length
    const trainLen = actuals.length - predLen
    return actuals.map((v, i) => {
      const pi = i - trainLen
      const fv  = pi >= 0 ? fullPreds[pi]  : null
      const fv2 = pi >= 0 && fullPreds2.length ? fullPreds2[pi] : null
      const res = fv != null ? Number(v) - fv : null
      return { period: i + 1, actual: Number(v), fv, fv2, res }
    }).filter(r => r.fv != null)
  })()
</script>

<div class="page">
  <header>
    <button class="nav-btn" on:click={() => currentPage.set('home')}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M19 12H5M12 5l-7 7 7 7"/></svg>
      Home
    </button>
    <h1>Forecasting Simulator</h1>
    <button class="nav-btn accent" on:click={() => currentPage.set('forecast')}>
      Go to Prediction
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
    </button>
  </header>

  <div class="controls">
    <div class="control-group">
      <span class="ctrl-label">Dataset</span>
      <button class="ctrl-btn upload" on:click={() => uploadOpen = true}>
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
        Upload CSV
      </button>
    </div>
    <div class="control-group">
      <label for="col">Data Column</label>
      <select id="col" bind:value={colName}>
        <option value="">Select column</option>
        {#each $columns as col}<option value={col.columnName}>{col.columnName}</option>{/each}
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
      <button class="ctrl-btn run" on:click={forecast} disabled={loading}>
        {#if loading}<span class="spinner"></span> Running…
        {:else}
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Run Forecast
        {/if}
      </button>
    </div>
    {#if result}
      <div class="control-group">
        <span class="ctrl-label">View</span>
        <button class="ctrl-btn toggle {showLast25 ? 'active' : ''}" on:click={toggleLast25}>
          {showLast25 ? 'Show Full Data' : 'Last 25%'}
        </button>
      </div>
      <div class="control-group">
        <span class="ctrl-label">Export</span>
        <button class="ctrl-btn export" on:click={exportCSV}>
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          Export CSV
        </button>
      </div>
    {/if}
  </div>

  {#if error}<div class="error-bar">{error}</div>{/if}

  <div class="main">
    <div class="chart-card">
      <div class="card-header">
        <span class="card-title">Forecast Chart</span>
        {#if result}<span class="tag">{result.algoType?.toUpperCase()} · {result.timeComputed}</span>{/if}
      </div>
      <div class="chart-wrap">
        {#if chartDatasets.length}
          <Chart labels={chartLabels} datasets={chartDatasets} />
        {:else}
          <div class="empty-state">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#334155" stroke-width="1.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
            <p>Run a forecast to see results</p>
          </div>
        {/if}
      </div>
    </div>

    {#if result}
      <div class="metrics-card">
        <div class="card-header"><span class="card-title">Performance Metrics</span></div>
        <div class="metrics-grid">
          {#each metrics as m}
            <div class="metric metric-{m.color}">
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

      <div class="table-card">
        <div class="card-header">
          <span class="card-title">Forecast Values <span class="sub">(test set — last 25%)</span></span>
          <span class="tag">{tableRows.length} periods</span>
        </div>
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
              {#each tableRows as row}
                <tr>
                  <td class="period">P{row.period}</td>
                  <td>{row.actual.toFixed(4)}</td>
                  <td>{row.fv != null ? row.fv.toFixed(4) : '—'}</td>
                  {#if algo === 'gas'}<td>{row.fv2 != null ? row.fv2.toFixed(4) : '—'}</td>{/if}
                  <td class:pos={row.res != null && row.res > 0} class:neg={row.res != null && row.res < 0}>
                    {row.res != null ? row.res.toFixed(4) : '—'}
                  </td>
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
  <div class="loading-overlay"><div class="spinner large"></div></div>
{/if}

<style>
  .page { min-height: 100vh; display: flex; flex-direction: column; background: #0a0a0f; }

  header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.85rem 1.5rem;
    background: #151520; border-bottom: 1px solid #3a3a4a;
    position: sticky; top: 0; z-index: 10;
  }
  h1 { font-size: 1.1rem; font-weight: 700; color: #f0f0f5; }

  .nav-btn {
    display: inline-flex; align-items: center; gap: 0.4rem;
    padding: 0.5rem 1rem; border-radius: 7px;
    font-size: 0.84rem; font-weight: 500; cursor: pointer;
    transition: all 0.2s; border: 1px solid #3a3a4a;
    background: none; color: #a0a0b0;
  }
  .nav-btn:hover { color: #f0f0f5; border-color: #5a5a6a; }
  .nav-btn.accent { background: linear-gradient(135deg, #7b68ee, #6a5acd); border: none; color: white; }
  .nav-btn.accent:hover { opacity: 0.9; }

  .controls {
    display: flex; flex-wrap: wrap; gap: 0.9rem; align-items: flex-end;
    padding: 1rem 1.5rem;
    background: #151520; border-bottom: 1px solid #3a3a4a;
  }
  .control-group { display: flex; flex-direction: column; gap: 0.35rem; min-width: 130px; }
  label, .ctrl-label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.6px; color: #5a5a7a; font-weight: 600; }
  select, input[type="number"] {
    padding: 0.6rem 0.8rem;
    background: #202030; border: 1px solid #3a3a4a;
    border-radius: 7px; color: #f0f0f5; font-size: 0.87rem;
    transition: border-color 0.2s;
  }
  select:focus, input:focus { outline: none; border-color: #7b68ee; }

  .ctrl-btn {
    display: inline-flex; align-items: center; gap: 0.45rem;
    padding: 0.6rem 1rem; border-radius: 7px;
    font-size: 0.87rem; font-weight: 600; cursor: pointer;
    border: none; transition: all 0.2s;
  }
  .ctrl-btn.upload { background: #202030; border: 1px solid #3a3a4a; color: #a0a0b0; }
  .ctrl-btn.upload:hover { border-color: #7b68ee; color: #f0f0f5; }
  .ctrl-btn.run { background: linear-gradient(135deg, #7b68ee, #6a5acd); color: white; }
  .ctrl-btn.run:hover:not(:disabled) { opacity: 0.88; transform: translateY(-1px); }
  .ctrl-btn.run:disabled { opacity: 0.45; cursor: not-allowed; }
  .ctrl-btn.toggle { background: #202030; border: 1px solid #3a3a4a; color: #a0a0b0; }
  .ctrl-btn.toggle:hover, .ctrl-btn.toggle.active { border-color: #7b68ee; color: #7b68ee; }
  .ctrl-btn.export { background: #202030; border: 1px solid #3a3a4a; color: #a0a0b0; }
  .ctrl-btn.export:hover { border-color: #ffcc66; color: #ffcc66; }

  .error-bar {
    margin: 0.8rem 1.5rem; padding: 0.7rem 1rem;
    background: rgba(255,107,107,0.1); border: 1px solid rgba(255,107,107,0.3);
    border-radius: 7px; color: #ff6b6b; font-size: 0.87rem;
  }

  .main { flex: 1; padding: 1.5rem; display: flex; flex-direction: column; gap: 1.5rem; }

  .chart-card, .metrics-card, .table-card {
    background: #151520; border: 1px solid #3a3a4a; border-radius: 10px; overflow: hidden;
  }
  .card-header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.85rem 1.2rem; border-bottom: 1px solid #3a3a4a;
  }
  .card-title { font-size: 0.87rem; font-weight: 600; color: #f0f0f5; }
  .card-title .sub { font-size: 0.75rem; color: #5a5a7a; font-weight: 400; margin-left: 0.4rem; }
  .tag { font-size: 0.73rem; color: #7b68ee; background: rgba(123,104,238,0.12); padding: 0.22rem 0.65rem; border-radius: 100px; }

  .chart-wrap { height: 360px; padding: 1rem; }
  .empty-state {
    height: 100%; display: flex; flex-direction: column;
    align-items: center; justify-content: center; gap: 0.8rem;
  }
  .empty-state p { color: #3a3a4a; font-size: 0.87rem; }

  .metrics-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 1px; background: #3a3a4a; }
  .metric { background: #151520; padding: 1rem 1.2rem; transition: background 0.2s; }
  .metric:hover { background: #1e1e2e; }
  .metric-label { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.5px; color: #5a5a7a; margin-bottom: 0.4rem; }
  .metric-value { font-size: 1.25rem; font-weight: 700; font-variant-numeric: tabular-nums; }
  .metric-indigo .metric-value { color: #7b68ee; }
  .metric-pink .metric-value { color: #fd79a8; }

  .params { display: flex; flex-wrap: wrap; gap: 0.7rem; padding: 0.8rem 1.2rem; border-top: 1px solid #3a3a4a; }
  .param { font-size: 0.81rem; color: #5a5a7a; background: #0a0a0f; padding: 0.28rem 0.65rem; border-radius: 6px; }

  .table-wrap { max-height: 340px; overflow-y: auto; }
  .table-wrap::-webkit-scrollbar { width: 5px; }
  .table-wrap::-webkit-scrollbar-track { background: #0a0a0f; }
  .table-wrap::-webkit-scrollbar-thumb { background: #3a3a4a; border-radius: 3px; }
  table { width: 100%; border-collapse: collapse; }
  th, td { padding: 0.52rem 1rem; text-align: left; border-bottom: 1px solid #1e1e2e; font-size: 0.81rem; }
  th { color: #5a5a7a; font-weight: 600; font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.5px; position: sticky; top: 0; background: #0a0a0f; }
  td { color: #a0a0b0; }
  td.period { color: #5a5a7a; }
  td.pos { color: #00d4aa; }
  td.neg { color: #ff6b6b; }
  tr:hover td { background: rgba(123,104,238,0.04); }

  .loading-overlay {
    position: fixed; inset: 0;
    background: rgba(10,10,15,0.85); backdrop-filter: blur(4px);
    display: flex; align-items: center; justify-content: center; z-index: 50;
  }
  .spinner {
    width: 28px; height: 28px;
    border: 2px solid rgba(255,255,255,0.08);
    border-top-color: #7b68ee;
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

<script lang="ts">
  import { onMount } from 'svelte'
  import { currentPage, columns, selectedColumn } from './store'
  import { fetchColumns, runPrediction } from './api'
  import type { ForecastResult } from './api'
  import Chart from './Chart.svelte'

  let loading = false
  let error = ''
  let result: ForecastResult | null = null

  let colName = $selectedColumn || ''
  let algo = 'gas'
  let logTransform = 'no'
  let seasonLength = 0
  let horizon = 12
  let showZoomed = false

  $: showSeason = algo === 'hwes' || algo === 'gas'
  $: if (!showSeason) seasonLength = 0

  let chartLabels: string[] = []
  let chartDatasets: any[] = []
  let actualValues: number[] = []

  onMount(async () => {
    if ($columns.length === 0) {
      try { const data = await fetchColumns(); columns.set(data) } catch {}
    }
  })

  async function predict() {
    if (!colName) { error = 'Select a data column first.'; return }
    error = ''
    loading = true
    showZoomed = false
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
      result = await runPrediction(payload, horizon)
      actualValues = col.actualValues.map(Number)
      buildChart(actualValues, result)
    } catch (e: any) { error = e.message }
    finally { loading = false }
  }

  function buildChart(actuals: number[], r: ForecastResult, zoomed = false) {
    const actLen = actuals.length
    const futureLabels = Array.from({ length: horizon }, (_, i) => `F${i + 1}`)

    let displayActuals = actuals
    let nullPad: null[]

    if (zoomed) {
      const start = Math.floor(actLen * 0.75)
      displayActuals = actuals.slice(start)
      chartLabels = [
        ...displayActuals.map((_, i) => `P${start + i + 1}`),
        ...futureLabels
      ]
      nullPad = Array(displayActuals.length).fill(null)
    } else {
      chartLabels = [...actuals.map((_, i) => `P${i + 1}`), ...futureLabels]
      nullPad = Array(actLen).fill(null)
    }

    chartDatasets = [
      {
        label: 'Historical',
        data: [...displayActuals, ...Array(horizon).fill(null)],
        borderColor: '#7b68ee',
        backgroundColor: 'rgba(123,104,238,0.07)',
        tension: 0.4, fill: true,
        pointRadius: displayActuals.length > 100 ? 0 : 2,
        borderWidth: 2,
      },
      {
        label: algo === 'gas' ? 'Weighted SES' : 'Prediction',
        data: [...nullPad, ...(r.predictionValues?.map(Number) ?? [])],
        borderColor: '#00d4aa',
        backgroundColor: 'transparent',
        tension: 0.4, fill: false,
        borderDash: [5, 3],
        pointRadius: 3,
        borderWidth: 2,
      },
    ]

    if (algo === 'gas') {
      if (r.predictionValues2?.length) {
        chartDatasets.push({
          label: 'Weighted HWES',
          data: [...nullPad, ...r.predictionValues2.map(Number)],
          borderColor: '#fd79a8',
          backgroundColor: 'transparent',
          tension: 0.4, fill: false,
          borderDash: [5, 3],
          pointRadius: 3,
          borderWidth: 2,
        })
      }
      if (r.preditionValuesAverage?.length) {
        chartDatasets.push({
          label: 'GAS Combined',
          data: [...nullPad, ...r.preditionValuesAverage.map(Number)],
          borderColor: '#ffcc66',
          backgroundColor: 'transparent',
          tension: 0.4, fill: false,
          pointRadius: 3,
          borderWidth: 2,
        })
      }
    }
  }

  function toggleZoom() {
    if (!result) return
    showZoomed = !showZoomed
    buildChart(actualValues, result, showZoomed)
  }

  function exportCSV() {
    if (!result) return
    const rows = [['Period', 'Type', 'Value']]
    actualValues.forEach((v, i) => rows.push([`P${i + 1}`, 'Actual', String(v)]))
    result.predictionValues?.forEach((v, i) => rows.push([`F${i + 1}`, algo === 'gas' ? 'Weighted SES' : 'Prediction', String(v)]))
    result.predictionValues2?.forEach((v, i) => rows.push([`F${i + 1}`, 'Weighted HWES', String(v)]))
    result.preditionValuesAverage?.forEach((v, i) => rows.push([`F${i + 1}`, 'GAS Combined', String(v)]))
    const csv = rows.map(r => r.join(',')).join('\n')
    const a = document.createElement('a')
    a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }))
    a.download = `${colName}_${algo}_prediction.csv`
    a.click()
  }

  function fmt(v: any) {
    if (v == null) return '—'
    return Number(v).toFixed(4)
  }
</script>

<div class="page">
  <header>
    <button class="nav-btn" on:click={() => currentPage.set('validate')}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M19 12H5M12 5l-7 7 7 7"/></svg>
      Back to Validation
    </button>
    <h1>Prediction — Future Forecast</h1>
    <button class="nav-btn export" on:click={exportCSV} disabled={!result}>
      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
      Export CSV
    </button>
  </header>

  <div class="controls">
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
    <div class="control-group">
      <label for="horizon">Forecast Horizon</label>
      <input id="horizon" type="number" min="1" max="365" bind:value={horizon} />
    </div>
    <div class="control-group run-group">
      <button class="ctrl-btn run" on:click={predict} disabled={loading}>
        {#if loading}<span class="spinner"></span> Predicting…
        {:else}
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Generate Prediction
        {/if}
      </button>
    </div>
    {#if result}
      <div class="control-group">
        <span class="ctrl-label">View</span>
        <button class="ctrl-btn toggle {showZoomed ? 'active' : ''}" on:click={toggleZoom}>
          {showZoomed ? 'Reset Zoom' : 'Zoom Last 25%'}
        </button>
      </div>
    {/if}
  </div>

  {#if error}<div class="error-bar">{error}</div>{/if}

  <div class="main">
    <div class="chart-card">
      <div class="card-header">
        <span class="card-title">Prediction Chart</span>
        {#if result}<span class="tag">{horizon} periods ahead · {result.algoType?.toUpperCase()}</span>{/if}
      </div>
      <div class="chart-wrap">
        {#if chartDatasets.length}
          <Chart labels={chartLabels} datasets={chartDatasets} />
        {:else}
          <div class="empty-state">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#3a3a4a" stroke-width="1.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
            <p>Generate a prediction to see future values</p>
          </div>
        {/if}
      </div>
    </div>

    {#if result}
      <div class="tables-row">
        <div class="table-card">
          <div class="card-header">
            <span class="card-title">Future Predictions</span>
            <span class="tag">{horizon} periods</span>
          </div>
          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Period</th>
                  {#if algo === 'gas'}
                    <th>Weighted SES</th>
                    <th>Weighted HWES</th>
                    <th class="highlight-th">GAS Combined</th>
                  {:else}
                    <th>Predicted Value</th>
                  {/if}
                </tr>
              </thead>
              <tbody>
                {#each Array(horizon) as _, i}
                  <tr>
                    <td class="period">F{i + 1}</td>
                    {#if algo === 'gas'}
                      <td>{fmt(result.predictionValues?.[i])}</td>
                      <td>{fmt(result.predictionValues2?.[i])}</td>
                      <td class="highlight">{fmt(result.preditionValuesAverage?.[i])}</td>
                    {:else}
                      <td>{fmt(result.predictionValues?.[i])}</td>
                    {/if}
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>

        <div class="table-card">
          <div class="card-header">
            <span class="card-title">Historical Data</span>
            <span class="tag">{actualValues.length} periods</span>
          </div>
          <div class="table-wrap">
            <table>
              <thead><tr><th>Period</th><th>Actual Value</th></tr></thead>
              <tbody>
                {#each actualValues as v, i}
                  <tr>
                    <td class="period">P{i + 1}</td>
                    <td>{Number(v).toFixed(4)}</td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {#if result.alphaSes}
        <div class="params-card">
          <div class="card-header"><span class="card-title">Model Parameters</span></div>
          <div class="params">
            <div class="param-item">
              <span class="param-label">α SES</span>
              <span class="param-val">{result.alphaSes?.toFixed(4)}</span>
            </div>
            {#if algo !== 'ses'}
              <div class="param-item">
                <span class="param-label">α HWES</span>
                <span class="param-val">{result.alphaHwes?.toFixed(4)}</span>
              </div>
              <div class="param-item">
                <span class="param-label">β (Beta)</span>
                <span class="param-val">{result.beta?.toFixed(4)}</span>
              </div>
              <div class="param-item">
                <span class="param-label">γ (Gamma)</span>
                <span class="param-val">{result.gamma?.toFixed(4)}</span>
              </div>
            {/if}
            <div class="param-item">
              <span class="param-label">Time</span>
              <span class="param-val time">{result.timeComputed}</span>
            </div>
          </div>
        </div>
      {/if}
    {/if}
  </div>
</div>

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
  .nav-btn.export { color: #a0a0b0; }
  .nav-btn.export:not(:disabled):hover { border-color: #ffcc66; color: #ffcc66; }
  .nav-btn.export:disabled { opacity: 0.3; cursor: not-allowed; }

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
  .ctrl-btn.run { background: linear-gradient(135deg, #7b68ee, #6a5acd); color: white; }
  .ctrl-btn.run:hover:not(:disabled) { opacity: 0.88; transform: translateY(-1px); }
  .ctrl-btn.run:disabled { opacity: 0.45; cursor: not-allowed; }
  .ctrl-btn.toggle { background: #202030; border: 1px solid #3a3a4a; color: #a0a0b0; }
  .ctrl-btn.toggle:hover, .ctrl-btn.toggle.active { border-color: #7b68ee; color: #7b68ee; }

  .error-bar {
    margin: 0.8rem 1.5rem; padding: 0.7rem 1rem;
    background: rgba(255,107,107,0.1); border: 1px solid rgba(255,107,107,0.3);
    border-radius: 7px; color: #ff6b6b; font-size: 0.87rem;
  }

  .main { flex: 1; padding: 1.5rem; display: flex; flex-direction: column; gap: 1.5rem; }

  .chart-card, .table-card, .params-card {
    background: #151520; border: 1px solid #3a3a4a; border-radius: 10px; overflow: hidden;
  }
  .card-header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.85rem 1.2rem; border-bottom: 1px solid #3a3a4a;
  }
  .card-title { font-size: 0.87rem; font-weight: 600; color: #f0f0f5; }
  .tag { font-size: 0.73rem; color: #00d4aa; background: rgba(0,212,170,0.1); padding: 0.22rem 0.65rem; border-radius: 100px; }

  .chart-wrap { height: 380px; padding: 1rem; }
  .empty-state {
    height: 100%; display: flex; flex-direction: column;
    align-items: center; justify-content: center; gap: 0.8rem;
  }
  .empty-state p { color: #3a3a4a; font-size: 0.87rem; }

  .tables-row { display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; }

  .table-wrap { max-height: 340px; overflow-y: auto; }
  .table-wrap::-webkit-scrollbar { width: 5px; }
  .table-wrap::-webkit-scrollbar-track { background: #0a0a0f; }
  .table-wrap::-webkit-scrollbar-thumb { background: #3a3a4a; border-radius: 3px; }
  table { width: 100%; border-collapse: collapse; }
  th, td { padding: 0.52rem 1rem; text-align: left; border-bottom: 1px solid #1e1e2e; font-size: 0.81rem; }
  th { color: #5a5a7a; font-weight: 600; font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.5px; position: sticky; top: 0; background: #0a0a0f; }
  td { color: #a0a0b0; }
  td.period { color: #5a5a7a; }
  td.highlight { color: #ffcc66; font-weight: 600; }
  .highlight-th { color: #ffcc66 !important; }
  tr:hover td { background: rgba(123,104,238,0.04); }

  .params { display: flex; flex-wrap: wrap; gap: 1rem; padding: 1rem 1.2rem; }
  .param-item { display: flex; flex-direction: column; gap: 0.25rem; }
  .param-label { font-size: 0.68rem; text-transform: uppercase; letter-spacing: 0.5px; color: #5a5a7a; }
  .param-val { font-size: 1rem; font-weight: 600; color: #7b68ee; font-variant-numeric: tabular-nums; }
  .param-val.time { color: #00d4aa; font-size: 0.87rem; }

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

  @media (max-width: 900px) { .tables-row { grid-template-columns: 1fr; } }
  @media (max-width: 768px) {
    header { flex-direction: column; gap: 0.6rem; text-align: center; }
    .controls { flex-direction: column; }
    .control-group { min-width: 100%; }
  }
</style>

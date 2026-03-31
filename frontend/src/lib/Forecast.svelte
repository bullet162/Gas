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
  let showSeason = false

  $: showSeason = algo === 'hwes' || algo === 'gas'
  $: if (!showSeason) seasonLength = 0

  let chartLabels: string[] = []
  let chartDatasets: any[] = []

  onMount(async () => {
    if ($columns.length === 0) {
      try {
        const data = await fetchColumns()
        columns.set(data)
      } catch {}
    }
  })

  async function predict() {
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

      result = await runPrediction(payload, horizon)
      buildChart(col.actualValues.map(Number), result)
    } catch (e: any) {
      error = e.message
    } finally {
      loading = false
    }
  }

  function buildChart(actual: number[], r: ForecastResult) {
    const actLen = actual.length
    const futureLabels = Array.from({ length: horizon }, (_, i) => `F${i + 1}`)
    chartLabels = [...actual.map((_, i) => `P${i + 1}`), ...futureLabels]

    const nullPad = Array(actLen).fill(null)

    chartDatasets = [
      {
        label: 'Historical',
        data: [...actual, ...Array(horizon).fill(null)],
        borderColor: '#6366f1',
        backgroundColor: 'rgba(99,102,241,0.06)',
        tension: 0.4, fill: true,
        pointRadius: actLen > 100 ? 0 : 2,
        borderWidth: 2,
      },
      {
        label: algo === 'gas' ? 'Weighted SES' : 'Prediction',
        data: [...nullPad, ...(r.predictionValues?.map(Number) ?? [])],
        borderColor: '#10b981',
        backgroundColor: 'transparent',
        tension: 0.4, fill: false,
        borderDash: [5, 3],
        pointRadius: 3,
        borderWidth: 2,
      },
    ]

    if (algo === 'gas') {
      if (r.predictionValues2) {
        chartDatasets.push({
          label: 'Weighted HWES',
          data: [...nullPad, ...r.predictionValues2.map(Number)],
          borderColor: '#06b6d4',
          backgroundColor: 'transparent',
          tension: 0.4, fill: false,
          borderDash: [5, 3],
          pointRadius: 3,
          borderWidth: 2,
        })
      }
      if (r.preditionValuesAverage) {
        chartDatasets.push({
          label: 'GAS Combined',
          data: [...nullPad, ...r.preditionValuesAverage.map(Number)],
          borderColor: '#f59e0b',
          backgroundColor: 'transparent',
          tension: 0.4, fill: false,
          pointRadius: 3,
          borderWidth: 2,
        })
      }
    }
  }

  function exportCSV() {
    if (!result) return
    const col = $columns.find(c => c.columnName === colName)
    const rows = [['Period', 'Type', 'Value']]
    col?.actualValues.forEach((v, i) => rows.push([`P${i + 1}`, 'Actual', String(v)]))
    result.predictionValues?.forEach((v, i) => rows.push([`F${i + 1}`, algo === 'gas' ? 'Weighted SES' : 'Prediction', String(v)]))
    result.predictionValues2?.forEach((v, i) => rows.push([`F${i + 1}`, 'Weighted HWES', String(v)]))
    result.preditionValuesAverage?.forEach((v, i) => rows.push([`F${i + 1}`, 'GAS Combined', String(v)]))
    const csv = rows.map(r => r.join(',')).join('\n')
    const a = document.createElement('a')
    a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }))
    a.download = `${colName}_${algo}_prediction.csv`
    a.click()
  }
</script>

<div class="page">
  <header>
    <button class="back-btn" on:click={() => currentPage.set('validate')}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M19 12H5M12 5l-7 7 7 7"/></svg>
      Back to Validation
    </button>
    <h1>Prediction — Future Forecast</h1>
    <button class="export-btn" on:click={exportCSV} disabled={!result}>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
      Export CSV
    </button>
  </header>

  <div class="controls">
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

    <div class="control-group">
      <label for="horizon">Forecast Horizon</label>
      <input id="horizon" type="number" min="1" max="365" bind:value={horizon} />
    </div>

    <div class="control-group run-group">
      <button class="ctrl-btn run" aria-label="Generate Prediction" on:click={predict} disabled={loading}>
        {#if loading}
          <span class="spinner"></span> Predicting…
        {:else}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Generate Prediction
        {/if}
      </button>
    </div>
  </div>

  {#if error}
    <div class="error-bar">{error}</div>
  {/if}

  <div class="main">
    <div class="chart-card">
      <div class="card-header">
        <span class="card-title">Prediction Chart</span>
        {#if result}
          <span class="tag">{horizon} periods ahead</span>
        {/if}
      </div>
      <div class="chart-wrap">
        {#if chartDatasets.length}
          <Chart labels={chartLabels} datasets={chartDatasets} />
        {:else}
          <div class="empty-chart">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#334155" stroke-width="1.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
            <p>Generate a prediction to see future values</p>
          </div>
        {/if}
      </div>
    </div>

    {#if result}
      <div class="tables-row">
        <!-- Prediction values -->
        <div class="table-card">
          <div class="card-header"><span class="card-title">Future Predictions</span></div>
          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Period</th>
                  {#if algo === 'gas'}
                    <th>Weighted SES</th>
                    <th>Weighted HWES</th>
                    <th>GAS Combined</th>
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
                      <td>{result.predictionValues?.[i] != null ? Number(result.predictionValues[i]).toFixed(4) : '—'}</td>
                      <td>{result.predictionValues2?.[i] != null ? Number(result.predictionValues2[i]).toFixed(4) : '—'}</td>
                      <td class="highlight">{result.preditionValuesAverage?.[i] != null ? Number(result.preditionValuesAverage[i]).toFixed(4) : '—'}</td>
                    {:else}
                      <td>{result.predictionValues?.[i] != null ? Number(result.predictionValues[i]).toFixed(4) : '—'}</td>
                    {/if}
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>

        <!-- Historical data -->
        <div class="table-card">
          <div class="card-header"><span class="card-title">Historical Data</span></div>
          <div class="table-wrap">
            <table>
              <thead>
                <tr><th>Period</th><th>Actual Value</th></tr>
              </thead>
              <tbody>
                {#each $columns.find(c => c.columnName === colName)?.actualValues ?? [] as v, i}
                  <tr>
                    <td>P{i + 1}</td>
                    <td>{Number(v).toFixed(4)}</td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    {/if}
  </div>
</div>

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
  .back-btn {
    display: inline-flex; align-items: center; gap: 0.4rem;
    padding: 0.5rem 1rem; border-radius: 8px;
    font-size: 0.85rem; font-weight: 500; cursor: pointer;
    transition: all 0.2s; border: 1px solid #334155;
    background: none; color: #94a3b8;
  }
  .back-btn:hover { color: #e2e8f0; border-color: #475569; }
  .export-btn {
    display: inline-flex; align-items: center; gap: 0.4rem;
    padding: 0.5rem 1rem; border-radius: 8px;
    font-size: 0.85rem; font-weight: 500; cursor: pointer;
    border: 1px solid #334155; background: none; color: #94a3b8;
    transition: all 0.2s;
  }
  .export-btn:not(:disabled):hover { border-color: #10b981; color: #10b981; }
  .export-btn:disabled { opacity: 0.3; cursor: not-allowed; }

  .controls {
    display: flex; flex-wrap: wrap; gap: 1rem; align-items: flex-end;
    padding: 1rem 1.5rem;
    background: #0f172a; border-bottom: 1px solid #1e293b;
  }
  .control-group { display: flex; flex-direction: column; gap: 0.35rem; min-width: 140px; }
  label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.6px; color: #475569; font-weight: 600; }
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
  .ctrl-btn.run { background: linear-gradient(135deg, #6366f1, #8b5cf6); color: white; }
  .ctrl-btn.run:hover:not(:disabled) { opacity: 0.9; }
  .ctrl-btn.run:disabled { opacity: 0.5; cursor: not-allowed; }

  .error-bar {
    margin: 0.8rem 1.5rem; padding: 0.7rem 1rem;
    background: rgba(248,113,113,0.1); border: 1px solid rgba(248,113,113,0.3);
    border-radius: 8px; color: #f87171; font-size: 0.88rem;
  }

  .main { flex: 1; padding: 1.5rem; display: flex; flex-direction: column; gap: 1.5rem; }

  .chart-card, .table-card {
    background: #1e293b; border: 1px solid #334155; border-radius: 12px; overflow: hidden;
  }
  .card-header {
    display: flex; align-items: center; justify-content: space-between;
    padding: 0.9rem 1.2rem; border-bottom: 1px solid #334155;
  }
  .card-title { font-size: 0.88rem; font-weight: 600; color: #e2e8f0; }
  .tag { font-size: 0.75rem; color: #10b981; background: rgba(16,185,129,0.1); padding: 0.25rem 0.7rem; border-radius: 100px; }

  .chart-wrap { height: 380px; padding: 1rem; }
  .empty-chart {
    height: 100%; display: flex; flex-direction: column;
    align-items: center; justify-content: center; gap: 0.8rem;
  }
  .empty-chart p { color: #334155; font-size: 0.88rem; }

  .tables-row { display: grid; grid-template-columns: 2fr 1fr; gap: 1.5rem; }
  .table-wrap { max-height: 340px; overflow-y: auto; }
  .table-wrap::-webkit-scrollbar { width: 5px; }
  .table-wrap::-webkit-scrollbar-track { background: #0f172a; }
  .table-wrap::-webkit-scrollbar-thumb { background: #334155; border-radius: 3px; }
  table { width: 100%; border-collapse: collapse; }
  th, td { padding: 0.55rem 1rem; text-align: left; border-bottom: 1px solid #1e293b; font-size: 0.82rem; }
  th { color: #475569; font-weight: 600; font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.5px; position: sticky; top: 0; background: #0f172a; }
  td { color: #94a3b8; }
  td.period { color: #64748b; }
  td.highlight { color: #f59e0b; font-weight: 600; }
  tr:hover td { background: rgba(99,102,241,0.04); }

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

  @media (max-width: 900px) {
    .tables-row { grid-template-columns: 1fr; }
  }
  @media (max-width: 768px) {
    header { flex-direction: column; gap: 0.6rem; text-align: center; }
    .controls { flex-direction: column; }
    .control-group { min-width: 100%; }
  }
</style>

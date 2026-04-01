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
  let activeTab: 'chart' | 'table' | 'errors' = 'chart'

  let colName = ''
  let algo = 'ses'
  let logTransform = 'no'
  let seasonLength = 0
  let showLast25 = false

  $: showSeason = algo === 'hwes' || algo === 'gas'
  $: if (!showSeason) seasonLength = 0

  let chartLabels: string[] = []
  let chartDatasets: any[] = []
  let fullActuals: number[] = []
  let fullPreds: number[] = []
  let fullPreds2: number[] = []
  let fullPreds3: number[] = []

  onMount(async () => { await loadColumns() })

  async function loadColumns() {
    try { loading = true; const d = await fetchColumns(); columns.set(d) }
    catch (e: any) { error = e.message }
    finally { loading = false }
  }

  async function forecast() {
    if (!colName) { error = 'Select a data column first.'; return }
    error = ''; loading = true; showLast25 = false; activeTab = 'chart'
    try {
      const col = $columns.find(c => c.columnName === colName)
      if (!col) throw new Error('Column not found.')
      result = await runForecast({
        input: col.actualValues, algoType: algo, columnName: colName,
        logTransform, seasonLength: algo === 'ses' ? 1 : seasonLength,
      })
      lastForecastResult.set(result); selectedColumn.set(colName)
      fullActuals = col.actualValues.map(Number)
      fullPreds  = result.predictionValues?.map(Number) ?? []
      fullPreds2 = result.predictionValues2?.map(Number) ?? []
      fullPreds3 = result.preditionValuesAverage?.map(Number) ?? []
      buildChart(fullActuals, false)
    } catch (e: any) { error = e.message }
    finally { loading = false }
  }

  function buildChart(actuals: number[], last25: boolean) {
    const predLen = fullPreds.length
    const trainLen = actuals.length - predLen
    let display = actuals
    let offset = 0
    if (last25) { offset = Math.floor(actuals.length * 0.75); display = actuals.slice(offset) }
    chartLabels = display.map((_, i) => `P${offset + i + 1}`)
    const pad = (arr: number[]) => {
      const sliced = last25 ? arr.slice(Math.max(0, arr.length - display.length)) : arr
      const gap = display.length - sliced.length
      return [...Array(Math.max(0, gap)).fill(null), ...sliced]
    }
    chartDatasets = [
      { label: 'Actual', data: display, borderColor: '#7b68ee', backgroundColor: 'rgba(123,104,238,0.07)', tension: 0.4, fill: true, pointRadius: display.length > 100 ? 0 : 3, borderWidth: 2 },
      { label: algo === 'gas' ? 'Weighted SES' : 'Forecast', data: pad(fullPreds), borderColor: '#00d4aa', backgroundColor: 'transparent', tension: 0.4, fill: false, borderDash: [4,3], pointRadius: 0, borderWidth: 1.5 },
    ]
    if (algo === 'gas' && fullPreds2.length) chartDatasets.push(
      { label: 'Weighted HWES', data: pad(fullPreds2), borderColor: '#fd79a8', backgroundColor: 'transparent', tension: 0.4, fill: false, borderDash: [4,3], pointRadius: 0, borderWidth: 1.5 }
    )
    if (algo === 'gas' && fullPreds3.length) chartDatasets.push(
      { label: 'GAS Combined', data: pad(fullPreds3), borderColor: '#ffcc66', backgroundColor: 'transparent', tension: 0.4, fill: false, pointRadius: 0, borderWidth: 2 }
    )
  }

  function toggleLast25() {
    showLast25 = !showLast25
    buildChart(fullActuals, showLast25)
  }

  function exportCSV() {
    if (!result) return
    const col = $columns.find(c => c.columnName === colName)
    const actuals = col?.actualValues ?? []
    const predLen = fullPreds.length
    const trainLen = actuals.length - predLen
    const headers = ['Period','Actual', algo==='gas'?'Weighted SES':'Forecast', ...(algo==='gas'?['Weighted HWES','GAS Combined']:[]), 'Residual']
    const rows = [headers]
    actuals.forEach((v, i) => {
      const pi = i - trainLen
      const fv = pi >= 0 ? fullPreds[pi] : null
      const fv2 = pi >= 0 ? fullPreds2[pi] : null
      const fv3 = pi >= 0 ? fullPreds3[pi] : null
      const res = fv != null ? (Number(v) - fv).toFixed(4) : ''
      rows.push([`P${i+1}`, String(v), fv!=null?fv.toFixed(4):'', ...(algo==='gas'?[fv2!=null?fv2.toFixed(4):'', fv3!=null?fv3.toFixed(4):'']:[]), res])
    })
    const a = document.createElement('a')
    a.href = URL.createObjectURL(new Blob([rows.map(r=>r.join(',')).join('\n')], {type:'text/csv'}))
    a.download = `${colName}_${algo}_forecast.csv`; a.click()
  }

  function fmt(n: any) { return n==null||isNaN(Number(n)) ? '—' : Number(n).toFixed(4) }

  $: errorSets = result ? (algo === 'gas' ? [
    { label: 'Weighted SES',  color: '#00d4aa', mse: result.mse,  mae: result.mae,  rmse: result.rmse,  mape: result.mape  },
    { label: 'Weighted HWES', color: '#fd79a8', mse: result.mse2, mae: result.mae2, rmse: result.rmse2, mape: result.mape2 },
    { label: 'GAS Combined',  color: '#ffcc66', mse: result.mse3, mae: result.mae3, rmse: result.rmse3, mape: result.mape3 },
  ] : [
    { label: algo.toUpperCase(), color: '#7b68ee', mse: result.mse, mae: result.mae, rmse: result.rmse, mape: result.mape },
  ]) : []

  $: tableRows = (() => {
    if (!result) return []
    const col = $columns.find(c => c.columnName === colName)
    const actuals = col?.actualValues ?? []
    const trainLen = actuals.length - fullPreds.length
    return actuals.map((v, i) => {
      const pi = i - trainLen
      if (pi < 0) return null
      const fv = fullPreds[pi] ?? null
      const fv2 = fullPreds2[pi] ?? null
      const fv3 = fullPreds3[pi] ?? null
      return { period: i+1, actual: Number(v), fv, fv2, fv3, res: fv!=null ? Number(v)-fv : null }
    }).filter(Boolean) as any[]
  })()
</script>

<div class="page">
  <!-- Step bar -->
  <div class="stepbar">
    <button class="step-home" on:click={() => currentPage.set('home')}>
      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
    </button>
    <div class="steps">
      <span class="step active">
        <span class="step-num">1</span> Validate
      </span>
      <span class="step-arrow">›</span>
      <button class="step inactive" on:click={() => currentPage.set('forecast')}>
        <span class="step-num">2</span> Predict
      </button>
    </div>
    <div class="stepbar-right">
      <button class="icon-btn" title="Upload CSV" on:click={() => uploadOpen = true}>
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
        Upload CSV
      </button>
      {#if result}
        <button class="icon-btn" title="Export CSV" on:click={exportCSV}>
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          Export
        </button>
      {/if}
    </div>
  </div>

  <div class="layout">
    <!-- LEFT: config panel -->
    <aside class="sidebar">
      <div class="sidebar-title">Configuration</div>

      <div class="field">
        <label for="col">Data Column</label>
        <select id="col" bind:value={colName}>
          <option value="">Select column</option>
          {#each $columns as col}<option value={col.columnName}>{col.columnName}</option>{/each}
        </select>
      </div>

      <div class="field">
        <label for="algo">Algorithm</label>
        <div class="algo-pills">
          {#each [['ses','SES'],['hwes','HWES'],['gas','GAS']] as [v,l]}
            <button class="pill {algo===v?'active':''}" on:click={() => algo=v}>{l}</button>
          {/each}
        </div>
      </div>

      <div class="field">
        <label for="log">Log Transform</label>
        <div class="toggle-row">
          <button class="pill {logTransform==='no'?'active':''}" on:click={() => logTransform='no'}>No</button>
          <button class="pill {logTransform==='yes'?'active':''}" on:click={() => logTransform='yes'}>log₁₀</button>
        </div>
      </div>

      {#if showSeason}
        <div class="field">
          <label for="season">Season Length</label>
          <input id="season" type="number" min="0" bind:value={seasonLength} />
        </div>
      {/if}

      <button class="run-btn" on:click={forecast} disabled={loading}>
        {#if loading}<span class="spinner"></span> Running…
        {:else}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Run Forecast
        {/if}
      </button>

      {#if result}
        <div class="divider"></div>
        <div class="sidebar-title">Result Info</div>
        <div class="info-row"><span>Algorithm</span><span class="info-val">{result.algoType?.toUpperCase()}</span></div>
        <div class="info-row"><span>Column</span><span class="info-val">{result.columnName}</span></div>
        <div class="info-row"><span>Time</span><span class="info-val teal">{result.timeComputed}</span></div>
        {#if result.alphaSes}
          <div class="divider"></div>
          <div class="sidebar-title">Parameters</div>
          <div class="info-row"><span>α SES</span><span class="info-val purple">{result.alphaSes?.toFixed(4)}</span></div>
          {#if algo !== 'ses'}
            <div class="info-row"><span>α HWES</span><span class="info-val purple">{result.alphaHwes?.toFixed(4)}</span></div>
            <div class="info-row"><span>β</span><span class="info-val purple">{result.beta?.toFixed(4)}</span></div>
            <div class="info-row"><span>γ</span><span class="info-val purple">{result.gamma?.toFixed(4)}</span></div>
          {/if}
        {/if}
        <div class="divider"></div>
        <button class="view-btn {showLast25?'active':''}" on:click={toggleLast25}>
          {showLast25 ? '↩ Full Data' : '🔍 Last 25%'}
        </button>
      {/if}
    </aside>

    <!-- RIGHT: results -->
    <main class="results">
      {#if error}<div class="error-bar">{error}</div>{/if}

      {#if !result}
        <div class="empty-page">
          <svg width="56" height="56" viewBox="0 0 24 24" fill="none" stroke="#3a3a4a" stroke-width="1"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          <p>Configure and run a forecast to see results</p>
        </div>
      {:else}
        <!-- Tabs -->
        <div class="tabs">
          <button class="tab {activeTab==='chart'?'active':''}" on:click={() => activeTab='chart'}>
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
            Chart
          </button>
          <button class="tab {activeTab==='table'?'active':''}" on:click={() => activeTab='table'}>
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="3" y1="15" x2="21" y2="15"/><line x1="9" y1="3" x2="9" y2="21"/></svg>
            Values Table
          </button>
          <button class="tab {activeTab==='errors'?'active':''}" on:click={() => activeTab='errors'}>
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 20V10"/><path d="M12 20V4"/><path d="M6 20v-6"/></svg>
            Error Metrics
          </button>
        </div>

        <!-- Chart tab -->
        {#if activeTab === 'chart'}
          <div class="card">
            <div class="chart-wrap">
              <Chart labels={chartLabels} datasets={chartDatasets} />
            </div>
          </div>
        {/if}

        <!-- Table tab -->
        {#if activeTab === 'table'}
          <div class="card">
            <div class="table-header-row">
              <span class="tbl-title">Test Set Forecast Values <span class="sub">(last 25% of data)</span></span>
              <span class="badge">{tableRows.length} periods</span>
            </div>
            <div class="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Period</th>
                    <th>Actual</th>
                    {#if algo === 'gas'}
                      <th class="col-ses">Weighted SES</th>
                      <th class="col-hwes">Weighted HWES</th>
                      <th class="col-gas">GAS Combined</th>
                    {:else}
                      <th>Forecast</th>
                    {/if}
                    <th>Residual</th>
                  </tr>
                </thead>
                <tbody>
                  {#each tableRows as row}
                    <tr>
                      <td class="dim">P{row.period}</td>
                      <td>{row.actual.toFixed(4)}</td>
                      {#if algo === 'gas'}
                        <td class="col-ses">{fmt(row.fv)}</td>
                        <td class="col-hwes">{fmt(row.fv2)}</td>
                        <td class="col-gas">{fmt(row.fv3)}</td>
                      {:else}
                        <td>{fmt(row.fv)}</td>
                      {/if}
                      <td class:pos={row.res!=null&&row.res>0} class:neg={row.res!=null&&row.res<0}>{fmt(row.res)}</td>
                    </tr>
                  {/each}
                </tbody>
              </table>
            </div>
          </div>
        {/if}

        <!-- Error metrics tab -->
        {#if activeTab === 'errors'}
          <div class="errors-grid">
            {#each errorSets as es}
              <div class="error-card" style="--accent:{es.color}">
                <div class="error-card-title" style="color:{es.color}">{es.label}</div>
                <div class="error-metrics">
                  <div class="em"><span class="em-label">MSE</span><span class="em-val">{fmt(es.mse)}</span></div>
                  <div class="em"><span class="em-label">MAE</span><span class="em-val">{fmt(es.mae)}</span></div>
                  <div class="em"><span class="em-label">RMSE</span><span class="em-val">{fmt(es.rmse)}</span></div>
                  <div class="em"><span class="em-label">MAPE</span><span class="em-val">{fmt(es.mape)}</span></div>
                </div>
              </div>
            {/each}
          </div>
        {/if}
      {/if}
    </main>
  </div>
</div>

<UploadModal bind:open={uploadOpen} on:uploaded={loadColumns} />
{#if loading}<div class="loading-overlay"><div class="spinner large"></div></div>{/if}

<style>
  :root { --bg: #0a0a0f; --surface: #131318; --surface2: #1c1c26; --border: #2e2e3e; --text: #f0f0f5; --muted: #6a6a8a; --purple: #7b68ee; --teal: #00d4aa; --pink: #fd79a8; --gold: #ffcc66; }
  .page { min-height:100vh; display:flex; flex-direction:column; background:var(--bg); color:var(--text); font-family:'Inter','Segoe UI',system-ui,sans-serif; }

  /* stepbar */
  .stepbar { display:flex; align-items:center; gap:1rem; padding:0.7rem 1.2rem; background:var(--surface); border-bottom:1px solid var(--border); position:sticky; top:0; z-index:20; }
  .step-home { background:none; border:1px solid var(--border); border-radius:6px; color:var(--muted); padding:0.4rem 0.5rem; cursor:pointer; display:flex; align-items:center; transition:all .2s; }
  .step-home:hover { color:var(--text); border-color:var(--purple); }
  .steps { display:flex; align-items:center; gap:0.5rem; }
  .step { display:flex; align-items:center; gap:0.4rem; font-size:0.82rem; font-weight:600; color:var(--muted); background:none; border:none; cursor:default; padding:0; }
  .step.active { color:var(--purple); }
  .step.inactive { cursor:pointer; transition:color .2s; }
  .step.inactive:hover { color:var(--text); }
  .step-num { width:20px; height:20px; border-radius:50%; background:var(--surface2); border:1px solid var(--border); display:flex; align-items:center; justify-content:center; font-size:0.7rem; }
  .step.active .step-num { background:var(--purple); border-color:var(--purple); color:#fff; }
  .step-arrow { color:var(--border); font-size:1.1rem; }
  .stepbar-right { margin-left:auto; display:flex; gap:0.5rem; }
  .icon-btn { display:inline-flex; align-items:center; gap:0.4rem; padding:0.4rem 0.8rem; border-radius:6px; font-size:0.8rem; font-weight:500; cursor:pointer; border:1px solid var(--border); background:none; color:var(--muted); transition:all .2s; }
  .icon-btn:hover { color:var(--text); border-color:var(--purple); }

  /* layout */
  .layout { display:grid; grid-template-columns:220px 1fr; flex:1; min-height:0; }

  /* sidebar */
  .sidebar { background:var(--surface); border-right:1px solid var(--border); padding:1.2rem; display:flex; flex-direction:column; gap:0.9rem; overflow-y:auto; }
  .sidebar-title { font-size:0.68rem; text-transform:uppercase; letter-spacing:1px; color:var(--muted); font-weight:700; }
  .field { display:flex; flex-direction:column; gap:0.35rem; }
  label { font-size:0.72rem; color:var(--muted); font-weight:600; }
  select, input[type="number"] { padding:0.55rem 0.7rem; background:var(--surface2); border:1px solid var(--border); border-radius:7px; color:var(--text); font-size:0.85rem; transition:border-color .2s; width:100%; }
  select:focus, input:focus { outline:none; border-color:var(--purple); }
  .algo-pills, .toggle-row { display:flex; gap:0.4rem; }
  .pill { flex:1; padding:0.45rem 0; border-radius:6px; font-size:0.8rem; font-weight:600; cursor:pointer; border:1px solid var(--border); background:var(--surface2); color:var(--muted); transition:all .2s; text-align:center; }
  .pill.active { background:var(--purple); border-color:var(--purple); color:#fff; }
  .pill:hover:not(.active) { border-color:var(--purple); color:var(--text); }
  .run-btn { display:flex; align-items:center; justify-content:center; gap:0.5rem; padding:0.65rem; border-radius:8px; font-size:0.88rem; font-weight:700; cursor:pointer; border:none; background:linear-gradient(135deg,var(--purple),#6a5acd); color:#fff; transition:all .2s; margin-top:0.2rem; }
  .run-btn:hover:not(:disabled) { opacity:.88; transform:translateY(-1px); }
  .run-btn:disabled { opacity:.4; cursor:not-allowed; }
  .divider { height:1px; background:var(--border); margin:0.2rem 0; }
  .info-row { display:flex; justify-content:space-between; align-items:center; font-size:0.78rem; color:var(--muted); }
  .info-val { color:var(--text); font-weight:600; font-size:0.78rem; max-width:110px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
  .info-val.teal { color:var(--teal); }
  .info-val.purple { color:var(--purple); }
  .view-btn { padding:0.5rem; border-radius:7px; font-size:0.8rem; font-weight:600; cursor:pointer; border:1px solid var(--border); background:var(--surface2); color:var(--muted); transition:all .2s; text-align:center; }
  .view-btn.active, .view-btn:hover { border-color:var(--purple); color:var(--purple); }

  /* results */
  .results { padding:1.2rem; display:flex; flex-direction:column; gap:1rem; overflow-y:auto; }
  .empty-page { flex:1; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:1rem; min-height:400px; }
  .empty-page p { color:var(--border); font-size:0.88rem; }
  .error-bar { padding:0.7rem 1rem; background:rgba(255,107,107,.1); border:1px solid rgba(255,107,107,.3); border-radius:7px; color:#ff6b6b; font-size:0.85rem; }

  /* tabs */
  .tabs { display:flex; gap:0.3rem; border-bottom:1px solid var(--border); padding-bottom:0; }
  .tab { display:inline-flex; align-items:center; gap:0.4rem; padding:0.55rem 1rem; border-radius:7px 7px 0 0; font-size:0.82rem; font-weight:600; cursor:pointer; border:1px solid transparent; border-bottom:none; background:none; color:var(--muted); transition:all .2s; }
  .tab:hover { color:var(--text); }
  .tab.active { background:var(--surface); border-color:var(--border); color:var(--purple); margin-bottom:-1px; }

  /* card */
  .card { background:var(--surface); border:1px solid var(--border); border-radius:0 10px 10px 10px; overflow:hidden; }
  .chart-wrap { height:420px; padding:1rem; }

  /* table */
  .table-header-row { display:flex; align-items:center; justify-content:space-between; padding:0.85rem 1.2rem; border-bottom:1px solid var(--border); }
  .tbl-title { font-size:0.87rem; font-weight:600; color:var(--text); }
  .tbl-title .sub { font-size:0.75rem; color:var(--muted); font-weight:400; margin-left:0.4rem; }
  .badge { font-size:0.72rem; padding:0.2rem 0.6rem; border-radius:100px; background:rgba(123,104,238,.12); color:var(--purple); }
  .table-wrap { max-height:460px; overflow-y:auto; }
  .table-wrap::-webkit-scrollbar { width:4px; }
  .table-wrap::-webkit-scrollbar-thumb { background:var(--border); border-radius:2px; }
  table { width:100%; border-collapse:collapse; }
  th,td { padding:0.5rem 1rem; text-align:left; border-bottom:1px solid #1a1a24; font-size:0.8rem; }
  th { font-size:0.68rem; text-transform:uppercase; letter-spacing:.5px; font-weight:700; position:sticky; top:0; background:#0a0a0f; color:var(--muted); }
  td { color:#b0b0c8; }
  td.dim { color:var(--muted); }
  td.pos { color:var(--teal); font-weight:600; }
  td.neg { color:#ff6b6b; font-weight:600; }
  th.col-ses, td.col-ses { color:var(--teal); }
  th.col-hwes, td.col-hwes { color:var(--pink); }
  th.col-gas, td.col-gas { color:var(--gold); font-weight:700; }
  tr:hover td { background:rgba(123,104,238,.04); }

  /* error cards */
  .errors-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(220px,1fr)); gap:1rem; }
  .error-card { background:var(--surface); border:1px solid var(--border); border-top:3px solid var(--accent,var(--purple)); border-radius:10px; padding:1.2rem; }
  .error-card-title { font-size:0.82rem; font-weight:700; text-transform:uppercase; letter-spacing:.5px; margin-bottom:1rem; }
  .error-metrics { display:grid; grid-template-columns:1fr 1fr; gap:0.8rem; }
  .em { display:flex; flex-direction:column; gap:0.2rem; }
  .em-label { font-size:0.68rem; text-transform:uppercase; letter-spacing:.5px; color:var(--muted); }
  .em-val { font-size:1.15rem; font-weight:700; color:var(--text); font-variant-numeric:tabular-nums; }

  /* loading */
  .loading-overlay { position:fixed; inset:0; background:rgba(10,10,15,.85); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; z-index:50; }
  .spinner { width:28px; height:28px; border:2px solid rgba(255,255,255,.08); border-top-color:var(--purple); border-radius:50%; animation:spin .7s linear infinite; display:inline-block; }
  .spinner.large { width:44px; height:44px; border-width:3px; }
  @keyframes spin { to { transform:rotate(360deg); } }

  @media(max-width:768px) {
    .layout { grid-template-columns:1fr; }
    .sidebar { border-right:none; border-bottom:1px solid var(--border); }
    .errors-grid { grid-template-columns:1fr; }
  }
</style>

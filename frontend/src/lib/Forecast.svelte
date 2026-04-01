<script lang="ts">
  import { onMount } from 'svelte'
  import { currentPage, columns, selectedColumn } from './store'
  import { fetchColumns, runPrediction } from './api'
  import type { ForecastResult } from './api'
  import Chart from './Chart.svelte'

  let loading = false
  let error = ''
  let result: ForecastResult | null = null
  let activeTab: 'chart' | 'table' | 'history' = 'chart'

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
      try { const d = await fetchColumns(); columns.set(d) } catch {}
    }
  })

  async function predict() {
    if (!colName) { error = 'Select a data column first.'; return }
    error = ''; loading = true; showZoomed = false; activeTab = 'chart'
    try {
      const col = $columns.find(c => c.columnName === colName)
      if (!col) throw new Error('Column not found.')
      result = await runPrediction({
        input: col.actualValues, algoType: algo, columnName: colName,
        logTransform, seasonLength: algo === 'ses' ? 1 : seasonLength,
      }, horizon)
      actualValues = col.actualValues.map(Number)
      buildChart(actualValues, false)
    } catch (e: any) { error = e.message }
    finally { loading = false }
  }

  function buildChart(actuals: number[], zoomed: boolean) {
    if (!result) return
    const futureLabels = Array.from({ length: horizon }, (_, i) => `F${i+1}`)
    let display = actuals
    let nullPad: null[]
    if (zoomed) {
      const start = Math.floor(actuals.length * 0.75)
      display = actuals.slice(start)
      chartLabels = [...display.map((_, i) => `P${start+i+1}`), ...futureLabels]
    } else {
      chartLabels = [...actuals.map((_, i) => `P${i+1}`), ...futureLabels]
    }
    nullPad = Array(display.length).fill(null)

    chartDatasets = [
      { label:'Historical', data:[...display,...Array(horizon).fill(null)], borderColor:'#7b68ee', backgroundColor:'rgba(123,104,238,0.07)', tension:0.4, fill:true, pointRadius:display.length>100?0:2, borderWidth:2 },
      { label:algo==='gas'?'Weighted SES':'Prediction', data:[...nullPad,...(result.predictionValues?.map(Number)??[])], borderColor:'#00d4aa', backgroundColor:'transparent', tension:0.4, fill:false, borderDash:[5,3], pointRadius:3, borderWidth:2 },
    ]
    if (algo === 'gas') {
      if (result.predictionValues2?.length) chartDatasets.push(
        { label:'Weighted HWES', data:[...nullPad,...result.predictionValues2.map(Number)], borderColor:'#fd79a8', backgroundColor:'transparent', tension:0.4, fill:false, borderDash:[5,3], pointRadius:3, borderWidth:2 }
      )
      if (result.preditionValuesAverage?.length) chartDatasets.push(
        { label:'GAS Combined', data:[...nullPad,...result.preditionValuesAverage.map(Number)], borderColor:'#ffcc66', backgroundColor:'transparent', tension:0.4, fill:false, pointRadius:3, borderWidth:2.5 }
      )
    }
  }

  function toggleZoom() {
    showZoomed = !showZoomed
    buildChart(actualValues, showZoomed)
  }

  function exportCSV() {
    if (!result) return
    const rows = [['Period','Type','Value']]
    actualValues.forEach((v,i) => rows.push([`P${i+1}`,'Actual',String(v)]))
    result.predictionValues?.forEach((v,i) => rows.push([`F${i+1}`,algo==='gas'?'Weighted SES':'Prediction',String(v)]))
    result.predictionValues2?.forEach((v,i) => rows.push([`F${i+1}`,'Weighted HWES',String(v)]))
    result.preditionValuesAverage?.forEach((v,i) => rows.push([`F${i+1}`,'GAS Combined',String(v)]))
    const a = document.createElement('a')
    a.href = URL.createObjectURL(new Blob([rows.map(r=>r.join(',')).join('\n')],{type:'text/csv'}))
    a.download = `${colName}_${algo}_prediction.csv`; a.click()
  }

  function fmt(v: any) { return v==null||isNaN(Number(v)) ? '—' : Number(v).toFixed(4) }
</script>

<div class="page">
  <!-- Step bar -->
  <div class="stepbar">
    <button class="step-home" on:click={() => currentPage.set('home')}>
      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
    </button>
    <div class="steps">
      <button class="step inactive" on:click={() => currentPage.set('validate')}>
        <span class="step-num">1</span> Validate
      </button>
      <span class="step-arrow">›</span>
      <span class="step active">
        <span class="step-num">2</span> Predict
      </span>
    </div>
    <div class="stepbar-right">
      <button class="icon-btn" on:click={exportCSV} disabled={!result}>
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
        Export CSV
      </button>
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
        <label>Algorithm</label>
        <div class="algo-pills">
          {#each [['ses','SES'],['hwes','HWES'],['gas','GAS']] as [v,l]}
            <button class="pill {algo===v?'active':''}" on:click={() => algo=v}>{l}</button>
          {/each}
        </div>
      </div>

      <div class="field">
        <label>Log Transform</label>
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

      <div class="field">
        <label for="horizon">Forecast Horizon</label>
        <input id="horizon" type="number" min="1" max="365" bind:value={horizon} />
      </div>

      <button class="run-btn" on:click={predict} disabled={loading}>
        {#if loading}<span class="spinner"></span> Predicting…
        {:else}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Generate Prediction
        {/if}
      </button>

      {#if result}
        <div class="divider"></div>
        <div class="sidebar-title">Result Info</div>
        <div class="info-row"><span>Algorithm</span><span class="info-val">{result.algoType?.toUpperCase()}</span></div>
        <div class="info-row"><span>Horizon</span><span class="info-val teal">{horizon} periods</span></div>
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
        <button class="view-btn {showZoomed?'active':''}" on:click={toggleZoom}>
          {showZoomed ? '↩ Full View' : '🔍 Zoom Last 25%'}
        </button>
      {/if}
    </aside>

    <!-- RIGHT: results -->
    <main class="results">
      {#if error}<div class="error-bar">{error}</div>{/if}

      {#if !result}
        <div class="empty-page">
          <svg width="56" height="56" viewBox="0 0 24 24" fill="none" stroke="#3a3a4a" stroke-width="1"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          <p>Configure and generate a prediction to see future values</p>
        </div>
      {:else}
        <!-- Tabs -->
        <div class="tabs">
          <button class="tab {activeTab==='chart'?'active':''}" on:click={() => activeTab='chart'}>
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
            Prediction Chart
          </button>
          <button class="tab {activeTab==='table'?'active':''}" on:click={() => activeTab='table'}>
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="3" y1="15" x2="21" y2="15"/><line x1="9" y1="3" x2="9" y2="21"/></svg>
            Future Values
          </button>
          <button class="tab {activeTab==='history'?'active':''}" on:click={() => activeTab='history'}>
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            Historical
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

        <!-- Future values tab -->
        {#if activeTab === 'table'}
          <div class="card">
            <div class="table-header-row">
              <span class="tbl-title">Future Predictions</span>
              <span class="badge">{horizon} periods ahead</span>
            </div>
            <div class="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Period</th>
                    {#if algo === 'gas'}
                      <th class="col-ses">Weighted SES</th>
                      <th class="col-hwes">Weighted HWES</th>
                      <th class="col-gas">GAS Combined ★</th>
                    {:else}
                      <th>Predicted Value</th>
                    {/if}
                  </tr>
                </thead>
                <tbody>
                  {#each Array(horizon) as _, i}
                    <tr>
                      <td class="dim">F{i+1}</td>
                      {#if algo === 'gas'}
                        <td class="col-ses">{fmt(result.predictionValues?.[i])}</td>
                        <td class="col-hwes">{fmt(result.predictionValues2?.[i])}</td>
                        <td class="col-gas">{fmt(result.preditionValuesAverage?.[i])}</td>
                      {:else}
                        <td>{fmt(result.predictionValues?.[i])}</td>
                      {/if}
                    </tr>
                  {/each}
                </tbody>
              </table>
            </div>
          </div>
        {/if}

        <!-- Historical tab -->
        {#if activeTab === 'history'}
          <div class="card">
            <div class="table-header-row">
              <span class="tbl-title">Historical Data</span>
              <span class="badge">{actualValues.length} periods</span>
            </div>
            <div class="table-wrap">
              <table>
                <thead><tr><th>Period</th><th>Actual Value</th></tr></thead>
                <tbody>
                  {#each actualValues as v, i}
                    <tr>
                      <td class="dim">P{i+1}</td>
                      <td>{Number(v).toFixed(4)}</td>
                    </tr>
                  {/each}
                </tbody>
              </table>
            </div>
          </div>
        {/if}
      {/if}
    </main>
  </div>
</div>

{#if loading}<div class="loading-overlay"><div class="spinner large"></div></div>{/if}

<style>
  :root { --bg:#0a0a0f; --surface:#131318; --surface2:#1c1c26; --border:#2e2e3e; --text:#f0f0f5; --muted:#6a6a8a; --purple:#7b68ee; --teal:#00d4aa; --pink:#fd79a8; --gold:#ffcc66; }
  .page { min-height:100vh; display:flex; flex-direction:column; background:var(--bg); color:var(--text); font-family:'Inter','Segoe UI',system-ui,sans-serif; }

  .stepbar { display:flex; align-items:center; gap:1rem; padding:0.7rem 1.2rem; background:var(--surface); border-bottom:1px solid var(--border); position:sticky; top:0; z-index:20; }
  .step-home { background:none; border:1px solid var(--border); border-radius:6px; color:var(--muted); padding:0.4rem 0.5rem; cursor:pointer; display:flex; align-items:center; transition:all .2s; }
  .step-home:hover { color:var(--text); border-color:var(--purple); }
  .steps { display:flex; align-items:center; gap:0.5rem; }
  .step { display:flex; align-items:center; gap:0.4rem; font-size:0.82rem; font-weight:600; color:var(--muted); background:none; border:none; cursor:default; padding:0; }
  .step.active { color:var(--teal); }
  .step.inactive { cursor:pointer; transition:color .2s; }
  .step.inactive:hover { color:var(--text); }
  .step-num { width:20px; height:20px; border-radius:50%; background:var(--surface2); border:1px solid var(--border); display:flex; align-items:center; justify-content:center; font-size:0.7rem; }
  .step.active .step-num { background:var(--teal); border-color:var(--teal); color:#000; }
  .step-arrow { color:var(--border); font-size:1.1rem; }
  .stepbar-right { margin-left:auto; }
  .icon-btn { display:inline-flex; align-items:center; gap:0.4rem; padding:0.4rem 0.8rem; border-radius:6px; font-size:0.8rem; font-weight:500; cursor:pointer; border:1px solid var(--border); background:none; color:var(--muted); transition:all .2s; }
  .icon-btn:not(:disabled):hover { color:var(--gold); border-color:var(--gold); }
  .icon-btn:disabled { opacity:.3; cursor:not-allowed; }

  .layout { display:grid; grid-template-columns:220px 1fr; flex:1; min-height:0; }

  .sidebar { background:var(--surface); border-right:1px solid var(--border); padding:1.2rem; display:flex; flex-direction:column; gap:0.9rem; overflow-y:auto; }
  .sidebar-title { font-size:0.68rem; text-transform:uppercase; letter-spacing:1px; color:var(--muted); font-weight:700; }
  .field { display:flex; flex-direction:column; gap:0.35rem; }
  label { font-size:0.72rem; color:var(--muted); font-weight:600; }
  select, input[type="number"] { padding:0.55rem 0.7rem; background:var(--surface2); border:1px solid var(--border); border-radius:7px; color:var(--text); font-size:0.85rem; transition:border-color .2s; width:100%; }
  select:focus, input:focus { outline:none; border-color:var(--teal); }
  .algo-pills, .toggle-row { display:flex; gap:0.4rem; }
  .pill { flex:1; padding:0.45rem 0; border-radius:6px; font-size:0.8rem; font-weight:600; cursor:pointer; border:1px solid var(--border); background:var(--surface2); color:var(--muted); transition:all .2s; text-align:center; }
  .pill.active { background:var(--teal); border-color:var(--teal); color:#000; }
  .pill:hover:not(.active) { border-color:var(--teal); color:var(--text); }
  .run-btn { display:flex; align-items:center; justify-content:center; gap:0.5rem; padding:0.65rem; border-radius:8px; font-size:0.88rem; font-weight:700; cursor:pointer; border:none; background:linear-gradient(135deg,var(--teal),#00a884); color:#000; transition:all .2s; margin-top:0.2rem; }
  .run-btn:hover:not(:disabled) { opacity:.88; transform:translateY(-1px); }
  .run-btn:disabled { opacity:.4; cursor:not-allowed; }
  .divider { height:1px; background:var(--border); margin:0.2rem 0; }
  .info-row { display:flex; justify-content:space-between; align-items:center; font-size:0.78rem; color:var(--muted); }
  .info-val { color:var(--text); font-weight:600; font-size:0.78rem; max-width:110px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
  .info-val.teal { color:var(--teal); }
  .info-val.purple { color:var(--purple); }
  .view-btn { padding:0.5rem; border-radius:7px; font-size:0.8rem; font-weight:600; cursor:pointer; border:1px solid var(--border); background:var(--surface2); color:var(--muted); transition:all .2s; text-align:center; }
  .view-btn.active, .view-btn:hover { border-color:var(--teal); color:var(--teal); }

  .results { padding:1.2rem; display:flex; flex-direction:column; gap:1rem; overflow-y:auto; }
  .empty-page { flex:1; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:1rem; min-height:400px; }
  .empty-page p { color:var(--border); font-size:0.88rem; }
  .error-bar { padding:0.7rem 1rem; background:rgba(255,107,107,.1); border:1px solid rgba(255,107,107,.3); border-radius:7px; color:#ff6b6b; font-size:0.85rem; }

  .tabs { display:flex; gap:0.3rem; border-bottom:1px solid var(--border); }
  .tab { display:inline-flex; align-items:center; gap:0.4rem; padding:0.55rem 1rem; border-radius:7px 7px 0 0; font-size:0.82rem; font-weight:600; cursor:pointer; border:1px solid transparent; border-bottom:none; background:none; color:var(--muted); transition:all .2s; }
  .tab:hover { color:var(--text); }
  .tab.active { background:var(--surface); border-color:var(--border); color:var(--teal); margin-bottom:-1px; }

  .card { background:var(--surface); border:1px solid var(--border); border-radius:0 10px 10px 10px; overflow:hidden; }
  .chart-wrap { height:440px; padding:1rem; }

  .table-header-row { display:flex; align-items:center; justify-content:space-between; padding:0.85rem 1.2rem; border-bottom:1px solid var(--border); }
  .tbl-title { font-size:0.87rem; font-weight:600; color:var(--text); }
  .badge { font-size:0.72rem; padding:0.2rem 0.6rem; border-radius:100px; background:rgba(0,212,170,.1); color:var(--teal); }
  .table-wrap { max-height:500px; overflow-y:auto; }
  .table-wrap::-webkit-scrollbar { width:4px; }
  .table-wrap::-webkit-scrollbar-thumb { background:var(--border); border-radius:2px; }
  table { width:100%; border-collapse:collapse; }
  th,td { padding:0.5rem 1rem; text-align:left; border-bottom:1px solid #1a1a24; font-size:0.8rem; }
  th { font-size:0.68rem; text-transform:uppercase; letter-spacing:.5px; font-weight:700; position:sticky; top:0; background:#0a0a0f; color:var(--muted); }
  td { color:#b0b0c8; }
  td.dim { color:var(--muted); }
  th.col-ses, td.col-ses { color:var(--teal); }
  th.col-hwes, td.col-hwes { color:var(--pink); }
  th.col-gas, td.col-gas { color:var(--gold); font-weight:700; }
  tr:hover td { background:rgba(0,212,170,.03); }

  .loading-overlay { position:fixed; inset:0; background:rgba(10,10,15,.85); backdrop-filter:blur(4px); display:flex; align-items:center; justify-content:center; z-index:50; }
  .spinner { width:28px; height:28px; border:2px solid rgba(255,255,255,.08); border-top-color:var(--teal); border-radius:50%; animation:spin .7s linear infinite; display:inline-block; }
  .spinner.large { width:44px; height:44px; border-width:3px; }
  @keyframes spin { to { transform:rotate(360deg); } }

  @media(max-width:768px) {
    .layout { grid-template-columns:1fr; }
    .sidebar { border-right:none; border-bottom:1px solid var(--border); }
  }
</style>

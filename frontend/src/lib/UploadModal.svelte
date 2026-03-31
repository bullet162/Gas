<script lang="ts">
  import { createEventDispatcher } from 'svelte'
  import { uploadCsvForColumns, uploadCsvActualValues } from './api'

  export let open = false

  const dispatch = createEventDispatcher<{ uploaded: void; close: void }>()

  let file: File | null = null
  let csvColumns: string[] = []
  let selectedCol = ''
  let status = ''
  let statusType: 'idle' | 'loading' | 'success' | 'error' = 'idle'

  async function onFileChange(e: Event) {
    const input = e.target as HTMLInputElement
    file = input.files?.[0] ?? null
    csvColumns = []
    selectedCol = ''
    status = ''
    if (!file) return
    if (!file.name.endsWith('.csv')) {
      status = 'Please select a .csv file.'
      statusType = 'error'
      return
    }
    try {
      statusType = 'loading'
      status = 'Reading columns…'
      csvColumns = await uploadCsvForColumns(file)
      status = ''
      statusType = 'idle'
    } catch (err: any) {
      status = err.message
      statusType = 'error'
    }
  }

  async function confirm() {
    if (!file || !selectedCol) return
    try {
      statusType = 'loading'
      status = 'Uploading data…'
      await uploadCsvActualValues(file, selectedCol)
      statusType = 'success'
      status = 'Uploaded successfully.'
      dispatch('uploaded')
      setTimeout(close, 800)
    } catch (err: any) {
      statusType = 'error'
      status = err.message
    }
  }

  function close() {
    open = false
    file = null
    csvColumns = []
    selectedCol = ''
    status = ''
    statusType = 'idle'
    dispatch('close')
  }
</script>

{#if open}
  <!-- svelte-ignore a11y-click-events-have-key-events a11y-no-static-element-interactions -->
  <div class="overlay" on:click|self={close}>
    <div class="modal" role="dialog" aria-modal="true" aria-label="Upload CSV">
      <div class="modal-header">
        <span class="modal-title">Upload Dataset</span>
        <button class="icon-btn" on:click={close} aria-label="Close">✕</button>
      </div>

      <div class="modal-body">
        <label class="file-label">
          <input type="file" accept=".csv" on:change={onFileChange} />
          <span class="file-btn">{file ? file.name : 'Choose CSV file'}</span>
        </label>

        {#if csvColumns.length > 0}
          <div class="field">
            <label for="col-select">Select column to forecast</label>
            <select id="col-select" bind:value={selectedCol}>
              <option value="">— pick a column —</option>
              {#each csvColumns as col}
                <option value={col}>{col}</option>
              {/each}
            </select>
          </div>
        {/if}

        {#if status}
          <p class="status {statusType}">{status}</p>
        {/if}
      </div>

      <div class="modal-footer">
        <button class="btn-ghost" on:click={close}>Cancel</button>
        <button class="btn-primary" disabled={!selectedCol || statusType === 'loading'} on:click={confirm}>
          {statusType === 'loading' ? 'Uploading…' : 'Confirm'}
        </button>
      </div>
    </div>
  </div>
{/if}

<style>
  .overlay {
    position: fixed; inset: 0;
    background: rgba(0,0,0,0.6);
    backdrop-filter: blur(4px);
    display: flex; align-items: center; justify-content: center;
    z-index: 100;
  }
  .modal {
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 12px;
    width: 420px; max-width: 95vw;
    box-shadow: 0 24px 48px rgba(0,0,0,0.4);
  }
  .modal-header {
    display: flex; justify-content: space-between; align-items: center;
    padding: 1.2rem 1.4rem;
    border-bottom: 1px solid #334155;
  }
  .modal-title { font-weight: 600; font-size: 1rem; color: #e2e8f0; }
  .icon-btn {
    background: none; border: none; color: #64748b;
    cursor: pointer; font-size: 1rem; padding: 4px 8px; border-radius: 4px;
    transition: color 0.2s;
  }
  .icon-btn:hover { color: #e2e8f0; }
  .modal-body { padding: 1.4rem; display: flex; flex-direction: column; gap: 1rem; }
  .file-label { display: block; cursor: pointer; }
  .file-label input { display: none; }
  .file-btn {
    display: block; padding: 0.7rem 1rem;
    background: #0f172a; border: 1px dashed #475569;
    border-radius: 8px; color: #94a3b8; font-size: 0.9rem;
    text-align: center; transition: border-color 0.2s;
  }
  .file-label:hover .file-btn { border-color: #6366f1; color: #e2e8f0; }
  .field { display: flex; flex-direction: column; gap: 0.4rem; }
  .field label { font-size: 0.8rem; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
  select {
    padding: 0.65rem 0.9rem; background: #0f172a;
    border: 1px solid #334155; border-radius: 8px;
    color: #e2e8f0; font-size: 0.9rem;
  }
  select:focus { outline: none; border-color: #6366f1; }
  .status { font-size: 0.85rem; padding: 0.5rem 0.8rem; border-radius: 6px; }
  .status.loading { color: #94a3b8; background: #1e293b; }
  .status.success { color: #34d399; background: rgba(52,211,153,0.1); }
  .status.error { color: #f87171; background: rgba(248,113,113,0.1); }
  .modal-footer {
    display: flex; justify-content: flex-end; gap: 0.8rem;
    padding: 1rem 1.4rem; border-top: 1px solid #334155;
  }
  .btn-ghost {
    padding: 0.6rem 1.2rem; background: none;
    border: 1px solid #334155; border-radius: 8px;
    color: #94a3b8; cursor: pointer; font-size: 0.9rem;
    transition: all 0.2s;
  }
  .btn-ghost:hover { border-color: #64748b; color: #e2e8f0; }
  .btn-primary {
    padding: 0.6rem 1.4rem;
    background: linear-gradient(135deg, #6366f1, #8b5cf6);
    border: none; border-radius: 8px;
    color: white; font-weight: 600; cursor: pointer; font-size: 0.9rem;
    transition: opacity 0.2s;
  }
  .btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }
  .btn-primary:not(:disabled):hover { opacity: 0.9; }
</style>

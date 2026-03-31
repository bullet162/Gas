<script lang="ts">
  import { onMount, onDestroy } from 'svelte'
  import {
    Chart,
    LineController,
    LineElement,
    PointElement,
    LinearScale,
    CategoryScale,
    Tooltip,
    Legend,
    Filler,
    Title,
  } from 'chart.js'

  Chart.register(LineController, LineElement, PointElement, LinearScale, CategoryScale, Tooltip, Legend, Filler, Title)

  export let labels: string[] = []
  export let datasets: any[] = []
  export let title = ''

  let canvas: HTMLCanvasElement
  let chart: Chart | null = null

  $: if (chart && labels.length) {
    chart.data.labels = labels
    chart.data.datasets = datasets
    chart.update('active')
  }

  onMount(() => {
    chart = new Chart(canvas, {
      type: 'line',
      data: { labels, datasets },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: { duration: 400 },
        plugins: {
          title: { display: !!title, text: title, color: '#e2e8f0', font: { size: 13, weight: '500' } },
          legend: { labels: { color: '#94a3b8', boxWidth: 12, padding: 16 } },
          tooltip: { mode: 'index', intersect: false, backgroundColor: '#1e293b', titleColor: '#e2e8f0', bodyColor: '#94a3b8', borderColor: '#334155', borderWidth: 1 },
        },
        scales: {
          x: {
            grid: { color: 'rgba(148,163,184,0.08)' },
            ticks: { color: '#64748b', maxTicksLimit: 20, maxRotation: 0 },
          },
          y: {
            grid: { color: 'rgba(148,163,184,0.08)' },
            ticks: { color: '#64748b' },
          },
        },
      },
    })
  })

  onDestroy(() => chart?.destroy())
</script>

<canvas bind:this={canvas}></canvas>

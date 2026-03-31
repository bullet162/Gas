<script lang="ts">
  import { currentPage } from './lib/store'
  import Home from './lib/Home.svelte'
  import Validate from './lib/Validate.svelte'
  import Forecast from './lib/Forecast.svelte'
</script>

{#if $currentPage === 'home'}
  <Home />
{:else if $currentPage === 'validate'}
  <Validate />
{:else if $currentPage === 'forecast'}
  <Forecast />
{/if}

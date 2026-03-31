<script lang="ts">
  import { currentPage } from './store'
</script>

<div class="home">
  <!-- Hero -->
  <section class="hero">
    <div class="hero-glow"></div>
    <div class="hero-content">
      <div class="badge">BS Computer Science · Isabela State University</div>
      <h1>Guided-Adaptive<br/>Smoothing System</h1>
      <p class="subtitle">
        An error-driven hybrid SES–HWES framework for multi-path range forecasting in uncertain time series.
      </p>
      <div class="hero-actions">
        <button class="btn-primary" on:click={() => currentPage.set('validate')}>
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="5 3 19 12 5 21 5 3"/></svg>
          Launch Simulator
        </button>
        <a href="#about" class="btn-ghost">Learn More</a>
      </div>
    </div>
    <div class="hero-visual">
      <div class="chart-preview">
        <svg viewBox="0 0 300 120" fill="none" xmlns="http://www.w3.org/2000/svg">
          <polyline points="0,90 30,75 60,80 90,55 120,60 150,40 180,45 210,25 240,30" stroke="#6366f1" stroke-width="2" fill="none"/>
          <polyline points="210,25 240,20 270,10 300,5" stroke="#8b5cf6" stroke-width="2" stroke-dasharray="5,3" fill="none"/>
          <polyline points="210,25 240,28 270,18 300,12" stroke="#06b6d4" stroke-width="2" stroke-dasharray="5,3" fill="none"/>
          <polyline points="210,25 240,24 270,14 300,8" stroke="#10b981" stroke-width="1.5" stroke-dasharray="3,3" fill="none"/>
          <circle cx="210" cy="25" r="4" fill="#6366f1"/>
        </svg>
        <div class="chart-labels">
          <span class="cl ses">SES Path</span>
          <span class="cl hwes">HWES Path</span>
          <span class="cl gas">GAS Combined</span>
        </div>
      </div>
    </div>
  </section>

  <!-- About -->
  <section id="about" class="section">
    <div class="section-inner">
      <div class="section-label">Thesis Overview</div>
      <h2>Research Background</h2>
      <div class="two-col">
        <div>
          <p>This system is the practical implementation of a thesis submitted to the <strong>College of Computing Studies, Information and Communication Technology</strong> at Isabela State University – Echague Campus.</p>
          <p class="research-title">"Guided-Adaptive Smoothing: An Error-Driven Hybrid SES–HWES Framework for Multi-Path Range Forecasting in Uncertain Time Series"</p>
          <div class="authors">
            <div class="author-chip">Jherson Aguto</div>
            <div class="author-chip">Mark John Panganiban</div>
            <div class="author-chip advisor">Adviser: Dr. Ricardo Q. Camungao</div>
          </div>
        </div>
        <div class="tech-stack">
          <div class="stack-title">Technical Stack</div>
          <div class="stack-grid">
            <div class="stack-item"><span class="dot indigo"></span>ASP.NET Core 8</div>
            <div class="stack-item"><span class="dot violet"></span>PostgreSQL / Neon</div>
            <div class="stack-item"><span class="dot cyan"></span>Svelte + Vite</div>
            <div class="stack-item"><span class="dot emerald"></span>Chart.js</div>
            <div class="stack-item"><span class="dot indigo"></span>RESTful API</div>
            <div class="stack-item"><span class="dot violet"></span>C# Algorithms</div>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- Algorithm -->
  <section class="section alt">
    <div class="section-inner">
      <div class="section-label">Methodology</div>
      <h2>How GAS Works</h2>
      <div class="steps">
        <div class="step">
          <div class="step-num">01</div>
          <h3>Train / Test Split</h3>
          <p>75% of data used for training, 25% for validation. Grid-search optimizes α, β, γ parameters.</p>
        </div>
        <div class="step">
          <div class="step-num">02</div>
          <h3>Parallel Execution</h3>
          <p>SES and HWES run simultaneously on training data, each producing independent forecasts.</p>
        </div>
        <div class="step">
          <div class="step-num">03</div>
          <h3>Inverse-Error Weighting</h3>
          <p>Dynamic weights computed from MSE performance — better-performing model gets higher weight.</p>
        </div>
        <div class="step">
          <div class="step-num">04</div>
          <h3>Multi-Path Output</h3>
          <p>Three prediction paths: Weighted SES, Weighted HWES, and GAS Combined (average).</p>
        </div>
      </div>
    </div>
  </section>

  <!-- Metrics -->
  <section class="section">
    <div class="section-inner">
      <div class="section-label">Evaluation</div>
      <h2>Error Metrics</h2>
      <div class="metrics-row">
        <div class="metric-card">
          <div class="metric-abbr">MSE</div>
          <div class="metric-name">Mean Squared Error</div>
        </div>
        <div class="metric-card">
          <div class="metric-abbr">MAE</div>
          <div class="metric-name">Mean Absolute Error</div>
        </div>
        <div class="metric-card">
          <div class="metric-abbr">RMSE</div>
          <div class="metric-name">Root Mean Squared Error</div>
        </div>
        <div class="metric-card">
          <div class="metric-abbr">MAPE</div>
          <div class="metric-name">Mean Absolute Percentage Error</div>
        </div>
      </div>
    </div>
  </section>

  <!-- CTA -->
  <section class="section cta-section">
    <div class="section-inner cta-inner">
      <h2>Ready to forecast?</h2>
      <p>Upload your CSV and run SES, HWES, or GAS on your time series data.</p>
      <button class="btn-primary large" on:click={() => currentPage.set('validate')}>
        Open Simulator
      </button>
    </div>
  </section>

  <!-- Footer -->
  <footer>
    <p>© 2025 GAS Forecasting System · BS Computer Science Thesis · Isabela State University</p>
  </footer>
</div>

<style>
  .home { min-height: 100vh; }

  /* Hero */
  .hero {
    min-height: 100vh;
    display: grid;
    grid-template-columns: 1fr 1fr;
    align-items: center;
    gap: 4rem;
    padding: 6rem 6vw 4rem;
    position: relative;
    overflow: hidden;
  }
  .hero-glow {
    position: absolute; top: -200px; left: -200px;
    width: 600px; height: 600px;
    background: radial-gradient(circle, rgba(99,102,241,0.15) 0%, transparent 70%);
    pointer-events: none;
  }
  .badge {
    display: inline-block;
    padding: 0.35rem 0.9rem;
    background: rgba(99,102,241,0.15);
    border: 1px solid rgba(99,102,241,0.3);
    border-radius: 100px;
    font-size: 0.78rem;
    color: #a5b4fc;
    letter-spacing: 0.3px;
    margin-bottom: 1.5rem;
  }
  h1 {
    font-size: clamp(2.4rem, 5vw, 3.8rem);
    font-weight: 800;
    line-height: 1.1;
    letter-spacing: -0.03em;
    background: linear-gradient(135deg, #e2e8f0 30%, #a5b4fc);
    -webkit-background-clip: text; background-clip: text;
    color: transparent;
    margin-bottom: 1.2rem;
  }
  .subtitle {
    font-size: 1.05rem;
    color: #64748b;
    max-width: 480px;
    line-height: 1.7;
    margin-bottom: 2rem;
  }
  .hero-actions { display: flex; gap: 1rem; flex-wrap: wrap; }
  .btn-primary {
    display: inline-flex; align-items: center; gap: 0.5rem;
    padding: 0.75rem 1.5rem;
    background: linear-gradient(135deg, #6366f1, #8b5cf6);
    border: none; border-radius: 10px;
    color: white; font-weight: 600; font-size: 0.95rem;
    cursor: pointer; transition: opacity 0.2s, transform 0.2s;
  }
  .btn-primary:hover { opacity: 0.9; transform: translateY(-1px); }
  .btn-primary.large { padding: 0.9rem 2rem; font-size: 1rem; }
  .btn-ghost {
    display: inline-flex; align-items: center;
    padding: 0.75rem 1.5rem;
    background: none;
    border: 1px solid #334155; border-radius: 10px;
    color: #94a3b8; font-size: 0.95rem;
    cursor: pointer; text-decoration: none;
    transition: all 0.2s;
  }
  .btn-ghost:hover { border-color: #6366f1; color: #e2e8f0; }

  /* Chart preview */
  .hero-visual { display: flex; align-items: center; justify-content: center; }
  .chart-preview {
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 16px;
    padding: 2rem;
    width: 100%;
    max-width: 380px;
    box-shadow: 0 24px 48px rgba(0,0,0,0.3);
  }
  .chart-preview svg { width: 100%; height: auto; }
  .chart-labels { display: flex; gap: 1rem; margin-top: 1rem; flex-wrap: wrap; }
  .cl { font-size: 0.75rem; display: flex; align-items: center; gap: 0.4rem; color: #64748b; }
  .cl::before { content: ''; display: inline-block; width: 20px; height: 2px; border-radius: 1px; }
  .cl.ses::before { background: #8b5cf6; }
  .cl.hwes::before { background: #06b6d4; }
  .cl.gas::before { background: #10b981; }

  /* Sections */
  .section { padding: 6rem 6vw; }
  .section.alt { background: #0f172a; }
  .section-inner { max-width: 1100px; margin: 0 auto; }
  .section-label {
    font-size: 0.75rem; text-transform: uppercase; letter-spacing: 1.5px;
    color: #6366f1; font-weight: 600; margin-bottom: 0.6rem;
  }
  h2 {
    font-size: clamp(1.8rem, 3vw, 2.4rem);
    font-weight: 700; color: #e2e8f0;
    margin-bottom: 2.5rem; letter-spacing: -0.02em;
  }

  /* Two col */
  .two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 4rem; align-items: start; }
  p { color: #64748b; line-height: 1.7; margin-bottom: 1rem; }
  strong { color: #94a3b8; }
  .research-title {
    font-style: italic; color: #a5b4fc;
    border-left: 3px solid #6366f1;
    padding-left: 1rem; margin: 1.5rem 0;
    line-height: 1.6;
  }
  .authors { display: flex; flex-wrap: wrap; gap: 0.6rem; margin-top: 1.5rem; }
  .author-chip {
    padding: 0.35rem 0.9rem;
    background: #1e293b; border: 1px solid #334155;
    border-radius: 100px; font-size: 0.82rem; color: #94a3b8;
  }
  .author-chip.advisor { border-color: #6366f1; color: #a5b4fc; }

  /* Tech stack */
  .tech-stack { background: #1e293b; border: 1px solid #334155; border-radius: 12px; padding: 1.5rem; }
  .stack-title { font-size: 0.8rem; text-transform: uppercase; letter-spacing: 1px; color: #475569; margin-bottom: 1rem; }
  .stack-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.8rem; }
  .stack-item { display: flex; align-items: center; gap: 0.6rem; font-size: 0.88rem; color: #94a3b8; }
  .dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
  .dot.indigo { background: #6366f1; }
  .dot.violet { background: #8b5cf6; }
  .dot.cyan { background: #06b6d4; }
  .dot.emerald { background: #10b981; }

  /* Steps */
  .steps { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1.5rem; }
  .step {
    background: #1e293b; border: 1px solid #334155;
    border-radius: 12px; padding: 1.5rem;
    transition: border-color 0.2s;
  }
  .step:hover { border-color: #6366f1; }
  .step-num { font-size: 2rem; font-weight: 800; color: #1e3a5f; margin-bottom: 0.8rem; font-variant-numeric: tabular-nums; }
  .step h3 { font-size: 1rem; font-weight: 600; color: #e2e8f0; margin-bottom: 0.5rem; }
  .step p { font-size: 0.88rem; color: #64748b; margin: 0; }

  /* Metrics */
  .metrics-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; }
  .metric-card {
    background: #1e293b; border: 1px solid #334155;
    border-radius: 12px; padding: 1.5rem; text-align: center;
    transition: border-color 0.2s;
  }
  .metric-card:hover { border-color: #6366f1; }
  .metric-abbr { font-size: 1.8rem; font-weight: 800; color: #6366f1; margin-bottom: 0.4rem; }
  .metric-name { font-size: 0.82rem; color: #64748b; }

  /* CTA */
  .cta-section { background: linear-gradient(135deg, #0f172a, #1e1b4b); }
  .cta-inner { text-align: center; }
  .cta-inner p { max-width: 400px; margin: 0 auto 2rem; }

  /* Footer */
  footer {
    padding: 2rem 6vw; text-align: center;
    border-top: 1px solid #1e293b;
    color: #334155; font-size: 0.82rem;
  }

  @media (max-width: 900px) {
    .hero { grid-template-columns: 1fr; padding-top: 5rem; text-align: center; }
    .hero-actions { justify-content: center; }
    .subtitle { margin: 0 auto 2rem; }
    .hero-visual { display: none; }
    .two-col { grid-template-columns: 1fr; gap: 2rem; }
  }
</style>

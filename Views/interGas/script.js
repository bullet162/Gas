const sections = document.querySelectorAll('.section');
const showSection = id => {
  sections.forEach(sec => sec.classList.add('hidden'));
  document.getElementById(id).classList.remove('hidden');
};

const fileInput = document.getElementById('fileInput');
const previewSection = document.getElementById('previewSection');
const dataTable = document.getElementById('dataTable');
const showChartBtn = document.getElementById('showChartBtn');
const forecastChart = document.getElementById('forecastChart');
const showMetricsBtn = document.getElementById('showMetricsBtn');
const metricsSection = document.getElementById('metricsSection');

const dummyData = [
  { date: '2023-01-01', value: 100 },
  { date: '2023-01-02', value: 105 },
  { date: '2023-01-03', value: 110 },
  { date: '2023-01-04', value: 108 },
  { date: '2023-01-05', value: 115 },
  { date: '2023-12-27', value: 155 },
  { date: '2023-12-28', value: 157 },
  { date: '2023-12-29', value: 160 },
  { date: '2023-12-30', value: 165 },
  { date: '2023-12-31', value: 170 }
];

fileInput.addEventListener('change', () => {
  setTimeout(() => {
    previewSection.classList.remove('hidden');
    dataTable.innerHTML = '';
    [...dummyData.slice(0, 5), ...dummyData.slice(-5)].forEach(row => {
      dataTable.innerHTML += `<tr><td class="p-3">${row.date}</td><td class="p-3">${row.value}</td></tr>`;
    });
  }, 7000);
});

showChartBtn.addEventListener('click', () => {
  showChartBtn.disabled = true;
  showChartBtn.innerText = 'Loading Chart...';
  setTimeout(() => {
    forecastChart.classList.remove('hidden');
    showMetricsBtn.classList.remove('hidden');
    new Chart(forecastChart.getContext('2d'), {
      type: 'line',
      data: {
        labels: dummyData.map(d => d.date),
        datasets: [{
          label: 'Forecasted Value',
          data: dummyData.map(d => d.value),
          borderColor: '#001f3f',
          backgroundColor: 'rgba(0,0,128,0.1)',
          tension: 0.4,
          fill: true
        }]
      }
    });
    showChartBtn.innerText = 'Chart Loaded';
  }, 7000);
});

showMetricsBtn.addEventListener('click', () => {
  showMetricsBtn.disabled = true;
  showMetricsBtn.innerText = 'Computing...';
  setTimeout(() => {
    metricsSection.classList.remove('hidden');
    showMetricsBtn.innerText = 'Metrics Computed';

    // Metrics Bar Chart
    new Chart(document.getElementById('metricsChart').getContext('2d'), {
      type: 'bar',
      data: {
        labels: ['RMSE', 'MSE', 'MAE', 'MAPE'],
        datasets: [
          {
            label: 'SES',
            data: [5.23, 27.34, 4.90, 6.8],
            backgroundColor: 'rgba(59, 130, 246, 0.7)'
          },
          {
            label: 'Holt-Winters',
            data: [3.91, 15.29, 3.64, 4.9],
            backgroundColor: 'rgba(16, 185, 129, 0.7)'
          },
          {
            label: 'GAS',
            data: [2.48, 6.15, 2.11, 2.7],
            backgroundColor: 'rgba(245, 158, 11, 0.7)'
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: true, text: 'Model Accuracy Comparison' },
          legend: { position: 'top' }
        }
      }
    });

    // Forecast Comparison Line Chart
    new Chart(document.getElementById('forecastComparisonChart').getContext('2d'), {
      type: 'line',
      data: {
        labels: dummyData.map(d => d.date),
        datasets: [
          {
            label: 'SES Forecast',
            data: [100, 105, 110, 115, 120, 125, 130, 135, 140, 145],
            borderColor: '#3b82f6',
            fill: false,
            tension: 0.3
          },
          {
            label: 'Holt-Winters Forecast',
            data: [98, 106, 112, 118, 122, 128, 134, 140, 146, 150],
            borderColor: '#10b981',
            fill: false,
            tension: 0.3
          },
          {
            label: 'GAS Forecast',
            data: [102, 108, 113, 120, 125, 132, 138, 145, 152, 158],
            borderColor: '#f59e0b',
            fill: false,
            tension: 0.3
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: true, text: 'Forecast Comparison (SES vs HWES vs GAS)' },
          legend: { position: 'top' }
        }
      }
    });
  }, 7000);
});

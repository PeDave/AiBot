let candlestickChart = null;

window.initializeCandlestickChart = function(canvasId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    candlestickChart = new Chart(ctx, {
        type: 'candlestick',
        data: {
            datasets: [{
                label: 'Price',
                data: []
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'hour'
                    },
                    grid: {
                        color: '#1a1f3a'
                    },
                    ticks: {
                        color: '#00ff41'
                    }
                },
                y: {
                    grid: {
                        color: '#1a1f3a'
                    },
                    ticks: {
                        color: '#00ff41'
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
};

window.updateCandlestickChart = function(data) {
    if (!candlestickChart) return;
    
    candlestickChart.data.datasets[0].data = data;
    candlestickChart.update();
};

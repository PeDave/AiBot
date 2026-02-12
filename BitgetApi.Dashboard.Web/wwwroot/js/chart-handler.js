let candlestickChart = null;

window.initializeCandlestickChart = function (canvasId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) {
        console.error('Canvas element not found:', canvasId);
        return;
    }

    // Check if Chart.js Financial is loaded
    if (!Chart.controllers || !Chart.controllers.candlestick) {
        console.error('Chart.js Financial plugin not loaded!');
        return;
    }

    candlestickChart = new Chart(ctx, {
        type: 'candlestick',
        data: {
            datasets: [{
                label: 'Price',
                data: [],
                color: {
                    up: '#00ff41',
                    down: '#ff0040',
                    unchanged: '#999'
                }
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'hour',
                        displayFormats: {
                            hour: 'MMM dd HH:mm'
                        }
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
                        color: '#00ff41',
                        callback: function (value) {
                            return '$' + value.toFixed(2);
                        }
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: '#1a1f3a',
                    borderColor: '#00ff41',
                    borderWidth: 1,
                    titleColor: '#00ff41',
                    bodyColor: '#00ff41'
                }
            }
        }
    });
};

window.updateCandlestickChart = function (data) {
    if (!candlestickChart) {
        console.error('Chart not initialized');
        return;
    }

    console.log('Updating chart with data:', data);

    // Convert data to proper format
    const formattedData = data.map(d => ({
        x: new Date(d.x), // ✅ Convert Unix timestamp to Date
        o: d.o,
        h: d.h,
        l: d.l,
        c: d.c
    }));

    candlestickChart.data.datasets[0].data = formattedData;
    candlestickChart.update('none'); // 'none' = no animation for performance
};
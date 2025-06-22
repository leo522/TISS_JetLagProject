const GanttChartRenderer = (function () {
    function render(containerId = "ganttChart", dataScriptId = "ganttData") {
        const dataElement = document.getElementById(dataScriptId);
        if (!dataElement) return;

        const rawData = dataElement.textContent;
        if (!rawData) return;

        const ganttData = JSON.parse(rawData);
        if (!Array.isArray(ganttData)) return;

        // ✅ 將字串轉為 Date 物件
        ganttData.forEach(item => {
            item.Start = new Date(item.Start);
            item.End = new Date(item.End);
        });

        const ctx = document.getElementById(containerId)?.getContext('2d');
        if (!ctx) return;

        const datasets = ganttData.map((item, index) => ({
            label: item.Label,
            data: [{
                x: item.Start,
                x2: item.End,
                y: index
            }],
            backgroundColor: item.Color,
            borderColor: item.Color,
            borderWidth: 1,
            barThickness: 18
        }));

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ganttData.map(g => g.Label),
                datasets: datasets
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                parsing: {
                    xAxisKey: 'x',
                    x2AxisKey: 'x2',
                    yAxisKey: 'y'
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const item = ganttData[context.datasetIndex];
                                return item.TooltipText || item.Label;
                            }
                        }
                    },
                    legend: {
                        display: false
                    },
                    datalabels: {
                        anchor: 'start',
                        align: 'end',
                        offset: 4,
                        color: '#000',
                        font: {
                            weight: 'bold',
                            size: 11
                        },
                        formatter: function (value, context) {
                            const item = ganttData[context.datasetIndex];
                            return item.TooltipText || '';
                        }
                    }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'hour',
                            tooltipFormat: 'MM/dd HH:mm'
                        },
                        title: {
                            display: true,
                            text: 'UTC 時間軸'
                        }
                    },
                    y: {
                        type: 'category',
                        labels: ganttData.map(g => g.Label),
                        title: {
                            display: false
                        }
                    }
                }
            },
            plugins: [ChartDataLabels] // ✅ 一定要加這行
        });
    }

    return { render };
})();

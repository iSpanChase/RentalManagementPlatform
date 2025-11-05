<script setup>
    import { ref, onMounted, watch } from "vue";
    import { Chart, registerables } from "chart.js";
    import 'chartjs-adapter-date-fns';
    Chart.register(...registerables);

    const props = defineProps({
        label: { String, default: ""},
        data: Object, // Change to Object, as it contains the 'points' array
        chartType: { type: String, default: "line" },
        timeUnit: { type: String, default: "month" }, // Add timeUnit prop
        yAxisDataKey: { type: String, default: "revenue" }, // New prop for Y-axis data key
    });

    const chartCanvas = ref();
    let chartInstance;

    onMounted(() => drawChart());

    watch(
        () => props.data,
        () => drawChart(),
        { deep: true }
    );

    function drawChart() {
        if (chartInstance) {
            chartInstance.destroy();
        }

        const historicalPoints = props.data?.historicalPoints || props.data?.points || [];
        const regressionPoints = props.data?.regressionPoints || [];
        const isPrediction = regressionPoints.length > 0;

        if (!chartCanvas.value) {
            console.error("Canvas element not found for chart initialization.");
            return;
        }

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const datasets = [];

        // Dataset 1: Historical Data (actuals)
        if (historicalPoints.length > 0) {
            const historicalDataset = {
                label: '歷史數據',
                data: historicalPoints.map(p => ({ x: p.date, y: p[props.yAxisDataKey] })),
                backgroundColor: 'rgba(150, 150, 150, 0.5)',
                borderColor: 'rgba(150, 150, 150, 1)',
                borderWidth: 1.5,
                pointRadius: 0,
                fill: false,
            };

            // For prediction charts, historical data can be a different type
            if (isPrediction) {
                historicalDataset.type = props.chartType;
            }
            
            datasets.push(historicalDataset);
        }

        // Dataset 2: Regression Line (past and future)
        if (isPrediction) {
            datasets.push({
                type: 'line', // This dataset is always a line
                label: '趨勢線',
                data: regressionPoints.map(p => ({ x: p.date, y: p[props.yAxisDataKey] })),
                borderColor: '#4A90E2',
                borderWidth: 2,
                pointRadius: 0,
                fill: false,
                segment: {
                    borderDash: ctx => {
                        const pointDate = new Date(ctx.p1.parsed.x);
                        return pointDate >= today ? [5, 5] : undefined;
                    }
                }
            });
        }

        chartInstance = new Chart(chartCanvas.value, {
            type: props.chartType, // Base chart type
            data: {
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false,
                },
                plugins: {
                    legend: {
                        display: true
                    }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'day',
                            displayFormats: {
                                day: 'yyyy-MM-dd',
                                week: 'yyyy-MM-dd',
                                month: 'yyyy-MM'
                            }
                        },
                        title: {
                            display: true,
                            text: '日期'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: props.yAxisDataKey === 'revenue' ? '收益' : '入住率 (%)'
                        }
                    }
                }
            }
        });
    }
</script>

<template>
    <div style="height:280px">
        <canvas ref="chartCanvas"></canvas>
    </div>
</template>

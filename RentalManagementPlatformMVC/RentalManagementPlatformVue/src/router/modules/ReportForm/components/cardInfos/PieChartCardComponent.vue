<script setup>
import { ref, onMounted, watch } from "vue";
import { Chart, registerables } from "chart.js";
Chart.register(...registerables);

const props = defineProps({
    data: Object, // { points: [ { roomTitle: 'Room A', totalRevenue: 5000 }, ... ] }
    labelKey: { type: String, required: true },
    dataKey: { type: String, required: true },
    unit: { type: String, default: 'count' } // 'count' or 'currency'
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

    const chartDataPoints = props.data?.points || [];

    if (!chartCanvas.value || chartDataPoints.length === 0) {
        return;
    }

    chartInstance = new Chart(chartCanvas.value, {
        type: 'pie',
        data: {
            labels: chartDataPoints.map(d => d[props.labelKey]),
            datasets: [{
                label: '來源分析',
                data: chartDataPoints.map(d => d[props.dataKey]),
                backgroundColor: [
                    'rgba(255, 99, 132, 0.7)',
                    'rgba(54, 162, 235, 0.7)',
                    'rgba(255, 206, 86, 0.7)',
                    'rgba(75, 192, 192, 0.7)',
                    'rgba(153, 102, 255, 0.7)',
                    'rgba(255, 159, 64, 0.7)',
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)',
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            let label = context.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed !== null) {
                                const total = context.dataset.data.reduce((acc, value) => acc + value, 0);
                                const percentage = ((context.parsed / total) * 100).toFixed(1) + '%';
                                
                                if (props.unit === 'currency') {
                                    label += new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', minimumFractionDigits: 0 }).format(context.raw) + ` (${percentage})`;
                                } else {
                                    label += `${context.raw} 次 (${percentage})`;
                                }
                            }
                            return label;
                        }
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

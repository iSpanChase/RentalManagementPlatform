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
        if (chartInstance)
            chartInstance.destroy();

        // Access the 'points' array from props.data
        const chartDataPoints = props.data && props.data.points ? props.data.points : [];

        console.log("Chart Data Points:", chartDataPoints); // Debug log
        console.log("Chart Canvas Element:", chartCanvas.value); // Debug log

        if (!chartCanvas.value) {
            console.error("Canvas element not found for chart initialization.");
            return; // Exit if canvas is not ready
        }

        chartInstance = new Chart(chartCanvas.value, {
            type: props.chartType,
            data: {
                labels: chartDataPoints.map(d => d.date),
                datasets: [{
                    label: props.label,
                    data: chartDataPoints.map(d => d.revenue),
                    borderWidth: 2,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: props.timeUnit, // Use the prop
                            displayFormats: {
                                day: 'yyyy-MM-dd',
                                week: 'yyyy-MM-dd',
                                month: 'yyyy-MM'
                            }
                        },
                        title: {
                            display: true,
                            text: '日期' // Generic title
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

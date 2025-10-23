<script setup>
    import { ref, onMounted, watch } from "vue";
    import { Chart, registerables } from "chart.js";
    Chart.register(...registerables);

    const props = defineProps({
        label: { String, default: ""},
        data: Array,
        chartType: { type: String, default: "line" },
    });

    const chartCanvas = ref();
    let chartInstance;

    onMounted(() => drawChart());

    watch(
        () => props.data,//要監聽的值
        () => drawChart(),//值改變時要執行的函式
        { deep: true }//可設定內容，此為"深層監聽"，如果僅props.data裡面單屬性改變，也會觸發 watch
    );

    function drawChart() {
        if (chartInstance)
            chartInstance.destroy();

        chartInstance = new Chart(chartCanvas.value, {
            type: props.chartType,
            data: {
                labels: props.data.map(d => d.Date),
                datasets: [{
                    label: props.label,
                    data: props.data.map(d => d.Revenue),
                    borderWidth: 2,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    }
</script>

<template>
    <div style="height:280px">
        <canvas ref="chartCanvas"></canvas>
    </div>
</template>

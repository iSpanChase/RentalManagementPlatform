<script setup lang="ts">
    interface RevenueConfig {
        propertyIds: string[];
        range: { start: string; end: string };
        groupBy: 'day' | 'week' | 'month';
        chartType: 'line' | 'bar' | 'pie';
    }

    const props = defineProps<{ modelValue: RevenueConfig }>();
    const emit = defineEmits(['update:modelValue']);

    function updateField(key: keyof RevenueConfig, value: any) {
        emit('update:modelValue', { ...props.modelValue, [key]: value });
    }

    function updateRange(field: 'start' | 'end', value: string) {
        emit('update:modelValue', {
            ...props.modelValue,
            range: { ...props.modelValue.range, [field]: value }
        });
    }
</script>

<template>
    <div>
        <div class="mb-2">
        <label>選擇房源</label>
        <select
            multiple
            class="form-select"
            @change="(e) => updateField('propertyIds', Array.from((e.target as HTMLSelectElement).selectedOptions).map(o => o.value))"
        >
            <option value="A001">台北市信義區房源</option>
            <option value="A002">新竹東區房源</option>
            <option value="A003">台中西屯區房源</option>
        </select>
        </div>

        <div class="mb-2">
        <label>日期區間</label>
        <div class="d-flex gap-2">
            <input type="date" class="form-control" :value="modelValue.range.start" @change="(e) => updateRange('start', (e.target as HTMLInputElement).value)" />
            <input type="date" class="form-control" :value="modelValue.range.end" @change="(e) => updateRange('end', (e.target as HTMLInputElement).value)" />
        </div>
        </div>

        <div class="mb-2">
        <label>群組單位</label>
        <select class="form-select" :value="modelValue.groupBy" @change="(e) => updateField('groupBy', (e.target as HTMLInputElement).value)">
            <option value="day">每日</option>
            <option value="week">每週</option>
            <option value="month">每月</option>
        </select>
        </div>

        <div class="mb-2">
        <label>圖表類型</label>
        <select class="form-select" :value="modelValue.chartType" @change="(e) => updateField('chartType', (e.target as HTMLInputElement).value)">
            <option value="line">折線圖</option>
            <option value="bar">長條圖</option>
            <option value="pie">圓餅圖</option>
        </select>
        </div>
    </div>
</template>

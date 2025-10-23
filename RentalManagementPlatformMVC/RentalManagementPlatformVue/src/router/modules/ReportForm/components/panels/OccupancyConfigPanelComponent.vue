<script setup lang="ts">
    interface OccupancyConfig {
        propertyIds: string[];
        range: { start: string; end: string };
        breakdownBy?: 'roomType' | 'channel';
    }

    const props = defineProps<{ modelValue: OccupancyConfig }>();
    const emit = defineEmits(['update:modelValue']);

    function updateField(key: keyof OccupancyConfig, value: any) {
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
        <label>房源</label>
        <select
            multiple
            class="form-select"
            @change="(e) => updateField('propertyIds', Array.from((e.target as HTMLSelectElement).selectedOptions).map(o => o.value))"
        >
            <option value="A001">台北市信義區房源</option>
            <option value="A002">新竹東區房源</option>
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
        <label>分類依據</label>
        <select class="form-select" :value="modelValue.breakdownBy" @change="(e) => updateField('breakdownBy', (e.target as HTMLInputElement).value)">
            <option value="roomType">房型</option>
            <option value="channel">來源管道</option>
        </select>
        </div>
    </div>
</template>

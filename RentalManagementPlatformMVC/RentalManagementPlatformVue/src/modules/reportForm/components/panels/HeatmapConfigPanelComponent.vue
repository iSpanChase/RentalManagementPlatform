<script setup lang="ts">
    interface HeatmapConfig {
    propertyIds: string[];
    center: { lat: number; lng: number };
    zoom: number;
    }

    const props = defineProps<{ modelValue: HeatmapConfig }>();
    const emit = defineEmits(['update:modelValue']);

    function updateField(key: keyof HeatmapConfig, value: any) {
    emit('update:modelValue', { ...props.modelValue, [key]: value });
    }

    function updateCenter(field: 'lat' | 'lng', value: string) {
    emit('update:modelValue', {
        ...props.modelValue,
        center: { ...props.modelValue.center, [field]: Number(value) }
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
        <label>地圖中心</label>
        <div class="d-flex gap-2">
            <input type="number" step="0.0001" class="form-control" :value="modelValue.center.lat" @change="(e) => updateCenter('lat', (e.target as HTMLInputElement).value)" />
            <input type="number" step="0.0001" class="form-control" :value="modelValue.center.lng" @change="(e) => updateCenter('lng', (e.target as HTMLInputElement).value)" />
        </div>
        </div>

        <div class="mb-2">
        <label>縮放層級</label>
        <input type="number" min="1" max="20" class="form-control" :value="modelValue.zoom" @change="(e) => updateField('zoom', Number((e.target as HTMLInputElement).value))" />
        </div>
    </div>
</template>

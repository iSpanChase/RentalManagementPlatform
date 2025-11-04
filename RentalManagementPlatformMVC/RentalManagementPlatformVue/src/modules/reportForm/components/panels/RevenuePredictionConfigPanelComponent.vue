 <script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import type { RevenuePredictionConfig } from '../../api/reportForm';

const props = defineProps<{ modelValue: RevenuePredictionConfig }>();
const emit = defineEmits(['update:modelValue']);

const hostRooms = ref<{ roomId: number; title: string }[]>([]);

onMounted(async () => {
  try {
    const response = await axios.get('/api/ReportForm/Rooms/ForHost');
    hostRooms.value = response.data;
  } catch (error) {
    console.error('Failed to fetch host rooms:', error);
  }
});

const selectedPropertyIds = computed({
  get: () => props.modelValue.propertyIds,
  set: (value: number[]) => {
    emit('update:modelValue', { ...props.modelValue, propertyIds: value });
  }
});

function updateField(key: keyof RevenuePredictionConfig, value: any) {
  emit('update:modelValue', { ...props.modelValue, [key]: value });
}

</script>

<template>
    <div>
        <div class="mb-2">
            <label>選擇房源 (不選代表全選)</label>
            <select
                multiple
                class="form-select"
                v-model="selectedPropertyIds" 
            >
                <option v-for="room in hostRooms" :key="room.roomId" :value="room.roomId">
                    {{ room.title }}
                </option>
            </select>
        </div>

        <div class="mb-2">
            <label>預測未來期間</label>
            <select class="form-select" :value="modelValue.forecastDays" @change="(e) => updateField('forecastDays', parseInt((e.target as HTMLInputElement).value))">
                <option value="7">未來 7 天</option>
                <option value="30">未來 30 天</option>
                <option value="90">未來 90 天</option>
            </select>
        </div>

        <div class="mb-2">
            <label>圖表類型</label>
            <select class="form-select" :value="modelValue.chartType" @change="(e) => updateField('chartType', (e.target as HTMLInputElement).value)">
                <option value="line">折線圖</option>
                <option value="bar">長條圖</option>
            </select>
        </div>
    </div>
</template>

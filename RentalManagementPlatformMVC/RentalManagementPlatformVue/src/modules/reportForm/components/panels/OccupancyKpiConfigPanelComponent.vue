<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '@/api/axiosInstance';
import type { OccupancyKpiConfig } from '../../api/reportForm';

const props = defineProps<{ modelValue: OccupancyKpiConfig }>();
const emit = defineEmits(['update:modelValue']);

// State for host rooms
const hostRooms = ref<{ roomId: number; title: string }[]>([]);

// Fetch host rooms on mount
onMounted(async () => {
  try {
    const { data } = await apiClient.get('/ReportForm/Rooms/ForHost');
    hostRooms.value = data;
  } catch (error) {
    console.error('Failed to fetch host rooms:', error);
  }
});

// Computed property for v-model on multi-select
const selectedPropertyIds = computed({
  get: () => props.modelValue.propertyIds,
  set: (value: number[]) => {
    updateField('propertyIds', value);
  }
});

// Computed property for v-model on lastDays input
const lastDays = computed({
    get: () => props.modelValue.lastDays,
    set: (value: number) => {
        updateField('lastDays', value);
    }
});

function updateField(key: keyof OccupancyKpiConfig, value: any) {
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
            <label>時間範圍 (近 N 日)</label>
            <input 
                type="number" 
                class="form-control" 
                v-model.number="lastDays" 
                min="1"
                placeholder="例如：30"
            />
        </div>
    </div>
</template>

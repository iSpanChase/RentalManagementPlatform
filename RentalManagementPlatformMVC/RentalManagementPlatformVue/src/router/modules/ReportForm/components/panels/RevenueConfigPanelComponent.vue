<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import type { RevenueConfig } from '../../api/reportForm';

const props = defineProps<{ modelValue: RevenueConfig }>();
const emit = defineEmits(['update:modelValue']);

// 用於儲存從後端獲取的房源列表
const hostRooms = ref<{ roomId: number; title: string }[]>([]);

// 在元件掛載時獲取房源資料
onMounted(async () => {
  try {
    const response = await axios.get('/api/ReportForm/Rooms/ForHost');
    hostRooms.value = response.data;
  } catch (error) {
    console.error('Failed to fetch host rooms:', error);
  }
});

// 創建一個 computed 屬性作為 propertyIds 的 v-model 代理
const selectedPropertyIds = computed({
  get: () => props.modelValue.propertyIds,
  set: (value: number[]) => {
    emit('update:modelValue', { ...props.modelValue, propertyIds: value });
  }
});

function updateField(key: keyof RevenueConfig, value: any) {
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
            <label>日期區間</label>
            <div class="d-flex gap-2">
                <input type="date" class="form-control" :value="modelValue.startDate" @change="(e) => updateField('startDate', (e.target as HTMLInputElement).value)" />
                <input type="date" class="form-control" :value="modelValue.endDate" @change="(e) => updateField('endDate', (e.target as HTMLInputElement).value)" />
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

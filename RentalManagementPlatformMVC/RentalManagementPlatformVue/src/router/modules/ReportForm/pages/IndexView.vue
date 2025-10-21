<!-- src/modules/ReportForm/pages/IndexView.vue -->
<template>
<div>
<h1>Report Form Index</h1>
<router-link :to="{ name: 'ReportForm.OtherView' }">Other</router-link>
<router-view />
</div class="container">
<div class="row">
    <BaseCardComponent class="col-4" title="地區收益熱力（示意）" :loading="false">
        <template #header-extra>
            <select v-model="range">
                <option value="mtd">本月</option>
                <option value="last30">近30天</option>
            </select>
        </template>

        <!-- MapHeatmapCard 內建 280px 高度，即使父層沒高度也會顯示 -->
        <MapHeatmapCardComponent :points="demoPoints" />

        <template #footer>
            <small>最後更新：{{ updatedAt }}</small>
        </template>
    </BaseCardComponent>
    <BaseCardComponent class="col-4" title="地區收益熱力（示意）" :loading="false">
        <template #header-extra>
            <select v-model="range">
                <option value="mtd">本月</option>
                <option value="last30">近30天</option>
            </select>
        </template>

        <!-- MapHeatmapCard 內建 280px 高度，即使父層沒高度也會顯示 -->
        <MapHeatmapCardComponent :points="demoPoints" />

        <template #footer>
            <small>最後更新：{{ updatedAt }}</small>
        </template>
    </BaseCardComponent>
    <BaseCardComponent class="col-4" title="地區收益熱力（示意）" :loading="false">
        <template #header-extra>
            <select v-model="range">
                <option value="mtd">本月</option>
                <option value="last30">近30天</option>
            </select>
        </template>

        <!-- MapHeatmapCard 內建 280px 高度，即使父層沒高度也會顯示 -->
        <MapHeatmapCardComponent :points="demoPoints" />

        <template #footer>
            <small>最後更新：{{ updatedAt }}</small>
        </template>
    </BaseCardComponent>
    <BaseCardComponent class="col-4" title="地區收益熱力（示意）" :loading="false">
        <template #header-extra>
            <select v-model="range">
                <option value="mtd">本月</option>
                <option value="last30">近30天</option>
            </select>
        </template>

        <!-- MapHeatmapCard 內建 280px 高度，即使父層沒高度也會顯示 -->
        <MapHeatmapCardComponent :points="demoPoints" />

        <template #footer>
            <small>最後更新：{{ updatedAt }}</small>
        </template>
    </BaseCardComponent>
</div>
    
</template>


<script setup>
import BaseCardComponent from '../components/BaseCardComponent.vue';
import MapHeatmapCardComponent from '../components/MapHeatmapCardComponent.vue';

import { ref } from 'vue'
const range = ref('mtd')
const updatedAt = new Date().toLocaleString()

// 你可以改這裡的數值，很容易看到顏色變化
const demoPoints = [
  { x: 0, y: 42, weight: 26000 },
  { x: 53, y: 61, weight: 12000 },
  { x: 72, y: 33, weight: 4000  },
  { x: 41, y: 76, weight: 8000  },
]
</script>
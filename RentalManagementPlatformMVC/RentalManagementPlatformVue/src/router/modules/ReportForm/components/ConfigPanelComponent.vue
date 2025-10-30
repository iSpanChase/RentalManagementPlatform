
<template>
  <div v-if="visible" class="modal-backdrop d-flex align-items-center justify-content-center">
    <div class="modal-card p-4 bg-white rounded-4 shadow-lg" style="min-width: 720px; max-width: 900px;">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h5 class="mb-0">{{ isEdit ? '修改卡片' : '新增卡片' }}</h5>
        <button class="btn btn-sm btn-outline-secondary" @click="onCancel">✕</button>
      </div>

      <div class="row g-3">
        <div class="col-4">
          <label class="form-label">卡片種類</label>
          <select class="form-select" v-model="localDraft.type">
            <option value="revenue">收益分析</option>
            <option value="occupancy">入住率</option>
            <option value="occupancy_kpi">入住率KPI</option>
            <option value="revenue_kpi">收益KPI</option>
            <option value="revenue_source">收益來源分析</option>
            <option value="occupancy_source">入住來源分析</option>
            <option value="heatmap">收益熱力</option>
          </select>
        </div>
        <div class="col-4">
          <label class="form-label">卡片名稱</label>
          <input class="form-control" v-model="localDraft.title" placeholder="例如：近3個月收益" />
        </div>
        <div class="col-4">
          <label class="form-label">副標題</label>
          <input class="form-control" v-model="localDraft.subtitle" placeholder="選填" />
        </div>
      </div>

      
      <!-- typed config panels -->
      <div class="mt-3">
        <RevenueConfigPanelComponent
          v-if="localDraft.type === 'revenue'"
          v-model="(localDraft.config as RevenueConfig)"
        />
        <OccupancyConfigPanelComponent
          v-else-if="localDraft.type === 'occupancy'"
          v-model="(localDraft.config as OccupancyConfig)"
        />
        <OccupancyKpiConfigPanelComponent
          v-else-if="localDraft.type === 'occupancy_kpi'"
          v-model="(localDraft.config as OccupancyKpiConfig)"
        />
        <RevenueKpiConfigPanelComponent
          v-else-if="localDraft.type === 'revenue_kpi'"
          v-model="(localDraft.config as RevenueKpiConfig)"
        />
        <RevenueSourceConfigPanelComponent
          v-else-if="localDraft.type === 'revenue_source'"
          v-model="(localDraft.config as RevenueSourceConfig)"
        />
        <OccupancySourceConfigPanelComponent
          v-else-if="localDraft.type === 'occupancy_source'"
          v-model="(localDraft.config as OccupancySourceConfig)"
        />
        <HeatmapConfigPanelComponent
          v-else
          v-model="(localDraft.config as HeatmapConfig)"
        />
      </div>


      <div class="d-flex justify-content-end gap-2 mt-4">
        <button class="btn btn-light" @click="onCancel">取消</button>
        <button class="btn btn-primary" @click="onConfirm">確認</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch, nextTick } from 'vue'
import type { CardDraft, CardType, RevenueConfig, OccupancyConfig, HeatmapConfig, OccupancyKpiConfig, RevenueKpiConfig, RevenueSourceConfig, OccupancySourceConfig } from '../api/reportForm'
import RevenueConfigPanelComponent from './panels/RevenueConfigPanelComponent.vue'
import OccupancyConfigPanelComponent from './panels/OccupancyConfigPanelComponent.vue'
import OccupancyKpiConfigPanelComponent from './panels/OccupancyKpiConfigPanelComponent.vue'
import RevenueKpiConfigPanelComponent from './panels/RevenueKpiConfigPanelComponent.vue'
import RevenueSourceConfigPanelComponent from './panels/RevenueSourceConfigPanelComponent.vue'
import OccupancySourceConfigPanelComponent from './panels/OccupancySourceConfigPanelComponent.vue'

const props = defineProps<{
  modelValue: CardDraft | null,
  visible: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', v: boolean): void
  (e: 'update:modelValue', v: CardDraft | null): void
  (e: 'confirm', draft: CardDraft): void
  (e: 'cancel'): void
}>()

const isEdit = computed(() => !!props.modelValue)

const defaultByType = (type: CardType): any => {
  if (type === 'revenue') {
    const cfg: RevenueConfig = {
      propertyIds: [],
      startDate: '2025-09-01',
      endDate: '2025-10-23',
      groupBy: 'month',
    }
    return cfg
  }
  if (type === 'occupancy') {
    const cfg: OccupancyConfig = {
      propertyIds: [],
      startDate: '2025-09-01',
      endDate: '2025-10-23',
      groupBy: 'month',
      chartType: 'line',
    }
    return cfg
  }
  if (type === 'occupancy_kpi') {
      const cfg: OccupancyKpiConfig = {
          propertyIds: [],
          lastDays: 30
      }
      return cfg;
  }
  if (type === 'revenue_kpi') {
      const cfg: RevenueKpiConfig = {
          propertyIds: [],
          lastDays: 30
      }
      return cfg;
  }
  if (type === 'revenue_source') {
      const cfg: RevenueSourceConfig = {
          propertyIds: [],
          lastDays: 30
      }
      return cfg;
  }
  if (type === 'occupancy_source') {
      const cfg: OccupancySourceConfig = {
          propertyIds: [],
          lastDays: 30
      }
      return cfg;
  }
  const cfg: HeatmapConfig = {
    propertyIds: [],
    center: { lat: 25.0330, lng: 121.5654 },
    zoom: 10
  }
  return cfg
}

const localDraft = reactive<CardDraft>({
  type: 'revenue',
  title: '新卡片',
  subtitle: '',
  config: defaultByType('revenue')
})

watch([() => props.visible, () => props.modelValue], ([v, mv]) => {
  if (v) {
    nextTick(() => {
      if (props.modelValue) {
        Object.assign(localDraft, JSON.parse(JSON.stringify(props.modelValue)))
      } else {
        Object.assign(localDraft, {
          type: 'revenue',
          title: '新卡片',
          subtitle: '',
          config: defaultByType('revenue')
        })
      }
    })
  }
}, { immediate: true, deep: true })


function onCancel(){
  emit('cancel')
  emit('update:visible', false)
}

function onConfirm(){
  // very light validation
  if(!localDraft.title?.trim()){
    alert('請輸入卡片名稱')
    return
  }
  emit('confirm', JSON.parse(JSON.stringify(localDraft)))
  emit('update:visible', false)
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,.3);
  z-index: 1050;
}
.modal-card { max-height: 90vh; overflow: auto; }
</style>

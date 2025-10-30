
<template>
  <div class="container-fluid">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h3 class="mb-0">動態報表（可自定義卡片）</h3>
      
      <!-- Favorites Section -->
      <div class="d-flex align-items-center gap-2">
        <button class="btn btn-success btn-sm" @click="onSaveToFavorites">加入我的最愛</button>
        <span class="fw-bold">我的最愛:</span>
        <select class="form-select form-select-sm w-auto" v-model="selectedFavoriteId">
            <option v-if="favoriteReports.length === 0" :value="null" disabled>沒有已儲存的報表</option>
            <option v-for="fav in favoriteReports" :key="fav.id" :value="fav.id">{{ fav.name }}</option>
        </select>
        <button class="btn btn-secondary btn-sm" @click="onLoadFavorite" :disabled="!selectedFavoriteId">載入</button>
        <button class="btn btn-danger btn-sm" @click="onDeleteFavorite" :disabled="!selectedFavoriteId">刪除</button>
      </div>
    </div>

    <!-- Add Card Button -->
    <div class="mb-3">
        <button class="btn btn-primary" @click="onAdd">新增卡片</button>
    </div>

    <!-- Cards Grid -->
    <div class="row">
      <BaseCardComponent
        v-for="c in cards"
        :key="c.id"
        :title="c.title"
        :subtitle="c.subtitle"
        :loading="loadingIds.has(c.id)"
        @edit="onEdit(c.id)"
        @remove="onRemove(c.id)"
      >
        <template #default>
          <component :is="cardBody(c)" v-bind="getComponentProps(c)" />
        </template>
        <template #footer>
          <small class="text-muted">最後更新：{{ updatedAt }}</small>
        </template>
      </BaseCardComponent>
    </div>

    <!-- Config Panel -->
    <ConfigPanelComponent
      v-model:visible="panelVisible"
      v-model="draft"
      @confirm="onConfirm"
      @cancel="onCancel"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, onMounted } from 'vue'
import BaseCardComponent from '../components/BaseCardComponent.vue'
import ConfigPanelComponent from '../components/ConfigPanelComponent.vue'
import MapHeatmapCardComponent from '../components/cardInfos/MapHeatmapCardComponent.vue'
import ChartCardComponent from '../components/ChartCardComponent.vue'
import KpiCardComponent from '../components/cardInfos/KpiCardComponent.vue'
import PieChartCardComponent from '../components/cardInfos/PieChartCardComponent.vue'
import {
    createCard, updateCard, refetchCardData, type Card, type CardDraft,
    getFavoriteReports, loadFavoriteReport, saveFavoriteReport, deleteFavoriteReport, type FavoriteReport
} from '../api/reportForm'

// ---------- state ----------
const cards = reactive<Card[]>([])
const loadingIds = reactive(new Set<string>())
const panelVisible = ref(false)
const editingId = ref<string | null>(null)
const draft = ref<CardDraft | null>(null)
const updatedAt = new Date().toLocaleString()

// ---------- favorite reports state ----------
const favoriteReports = ref<FavoriteReport[]>([]);
const selectedFavoriteId = ref<number | null>(null);

// ---------- component chooser and props generator ----------
const cardBody = (c: Card) => {
  if (c.type === 'heatmap') return MapHeatmapCardComponent
  if (c.type === 'revenue' || c.type === 'occupancy') return ChartCardComponent
  if (c.type === 'occupancy_kpi' || c.type === 'revenue_kpi') return KpiCardComponent
  if (c.type === 'revenue_source' || c.type === 'occupancy_source') return PieChartCardComponent
  
  return {
    props: ['card','data'],
    template: `<div><pre class="small mb-0">{{ JSON.stringify(data, null, 2) }}</pre></div>`
  } as any
}

const getComponentProps = (c: Card) => {
    const type = c.type;
    if (type === 'revenue' || type === 'occupancy') {
        return {
            data: c.data,
            timeUnit: (c.config as any).groupBy,
            yAxisDataKey: type === 'revenue' ? 'revenue' : 'occupancyRate',
            chartType: (c.config as any).chartType
        };
    }
    if (type === 'occupancy_kpi') {
        return {
            data: c.data,
            dataKey: 'occupancyRate',
            label: '入住率',
            unit: 'percentage'
        };
    }
    if (type === 'revenue_kpi') {
        return {
            data: c.data,
            dataKey: 'totalRevenue',
            label: '總收益',
            unit: 'currency'
        };
    }
    if (type === 'revenue_source') {
        return {
            data: c.data,
            labelKey: 'roomTitle',
            dataKey: 'totalRevenue',
            unit: 'currency'
        };
    }
    if (type === 'occupancy_source') {
        return {
            data: c.data,
            labelKey: 'roomTitle',
            dataKey: 'bookingCount',
            unit: 'count'
        };
    }
    return { card: c, data: c.data };
}

// ---------- actions ----------
function onAdd(){
  editingId.value = null
  draft.value = null
  panelVisible.value = true
}

function onEdit(id: string){
  const found = cards.find(x => x.id === id)
  if(!found) return
  editingId.value = id
  draft.value = {
    type: found.type,
    title: found.title,
    subtitle: found.subtitle,
    config: JSON.parse(JSON.stringify(found.config))
  }
  panelVisible.value = true
}

async function onConfirm(d: CardDraft){
  if (editingId.value === null){
    const created = await createCard(d)
    cards.unshift(created)
  } else {
    const id = editingId.value
    loadingIds.add(id)
    try {
      const updated = await updateCard(id, d)
      const idx = cards.findIndex(x => x.id === id)
      if (idx >= 0) cards[idx] = updated
    } finally {
      loadingIds.delete(id)
    }
  }
}

function onCancel(){
  // just close panel
}

function onRemove(id: string){
  const idx = cards.findIndex(x => x.id === id)
  if (idx >= 0) cards.splice(idx, 1)
}

// ---------- Favorite Reports Actions ----------
async function fetchFavorites() {
    favoriteReports.value = await getFavoriteReports();
    if (selectedFavoriteId.value && !favoriteReports.value.some(f => f.id === selectedFavoriteId.value)) {
        selectedFavoriteId.value = null;
    }
}

async function onSaveToFavorites() {
    const name = prompt('請輸入我的最愛名稱 (最多20個字):');
    if (!name) return;
    if (name.length > 20) {
        alert('名稱不能超過20個字');
        return;
    }

    const reportContent = cards.map(c => ({
        type: c.type,
        title: c.title,
        subtitle: c.subtitle,
        config: c.config
    }));

    const newFavorite = await saveFavoriteReport(name, JSON.stringify(reportContent));
    if (newFavorite) {
        await fetchFavorites();
        selectedFavoriteId.value = newFavorite.id;
        alert('成功加入我的最愛!');
    }
}

async function onLoadFavorite() {
    if (!selectedFavoriteId.value) return;
    if (!confirm('這將會清空當前的報表並載入選定的我的最愛，確定要繼續嗎?')) return;

    const favorite = await loadFavoriteReport(selectedFavoriteId.value);
    if (!favorite) {
        alert('載入失敗!');
        return;
    }

    try {
        const drafts = JSON.parse(favorite.content) as CardDraft[];
        cards.splice(0, cards.length); // Clear current cards
        
        // Recreate cards from drafts
        const newCards = await Promise.all(drafts.map(d => createCard(d)));
        newCards.forEach(c => cards.push(c));

    } catch (e) {
        console.error('Failed to parse or load favorite report content', e);
        alert('載入失敗，報表內容格式錯誤。');
    }
}

async function onDeleteFavorite() {
    if (!selectedFavoriteId.value) return;
    if (!confirm('確定要刪除這個我的最愛報表嗎?')) return;

    const success = await deleteFavoriteReport(selectedFavoriteId.value);
    if (success) {
        selectedFavoriteId.value = null;
        await fetchFavorites();
        alert('刪除成功!');
    } else {
        alert('刪除失敗!');
    }
}

// ---------- Lifecycle ----------
onMounted(async () => {
    await fetchFavorites();

    // (optional) example: add one default card for demo
    if (cards.length === 0) {
        const created = await createCard({
            type: 'heatmap',
            title: '地區收益熱力（示意）',
            subtitle: '本月',
            config: { propertyIds: [], center: { lat: 25.0330, lng: 121.5654 }, zoom: 10 } as any
        });
        cards.push(created);
    }
});

</script>

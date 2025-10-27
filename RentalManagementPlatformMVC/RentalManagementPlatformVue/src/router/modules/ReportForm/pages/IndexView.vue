
<template>
  <div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h3 class="mb-0">動態報表（可自定義卡片）</h3>
      <button class="btn btn-primary" @click="onAdd">新增</button>
    </div>

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
          <component :is="cardBody(c)" :card="c" :data="c.data" />
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
import { computed, reactive, ref } from 'vue'
import BaseCardComponent from '../components/BaseCardComponent.vue'
import ConfigPanelComponent from '../components/ConfigPanelComponent.vue'
import MapHeatmapCardComponent from '../components/cardInfos/MapHeatmapCardComponent.vue'
import ChartCardComponent from '../components/ChartCardComponent.vue' // Import ChartCardComponent
import { createCard, updateCard, refetchCardData, type Card, type CardDraft } from '../api/reportForm'

// ---------- state ----------
const cards = reactive<Card[]>([])
const loadingIds = reactive(new Set<string>())
const panelVisible = ref(false)
const editingId = ref<string|null>(null)
const draft = ref<CardDraft|null>(null)
const updatedAt = new Date().toLocaleString()

// ---------- body component chooser ----------
const cardBody = (c: Card) => {
  if (c.type === 'heatmap') return MapHeatmapCardComponent
  if (c.type === 'revenue') return ChartCardComponent // Add this line for revenue cards
  // fallback simple display
  return {
    props: ['card','data'],
    template: `<div><pre class="small mb-0">{{ JSON.stringify(data, null, 2) }}</pre></div>`
  } as any
}

// ---------- actions ----------
function onAdd(){
  editingId.value = null
  draft.value = null // panel will init default draft
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
    // create
    const created = await createCard(d)
    cards.unshift(created)
  } else {
    // update that one card only
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
  // just close panel; nothing else to do
}

function onRemove(id: string){
  const idx = cards.findIndex(x => x.id === id)
  if (idx >= 0) cards.splice(idx, 1)
}

// (optional) example: add one default card for demo
(async () => {
  const created = await createCard({
    type: 'heatmap',
    title: '地區收益熱力（示意）',
    subtitle: '本月',
    config: { propertyIds: [], center: { lat: 25.0330, lng: 121.5654 }, zoom: 10 } as any
  })
  cards.push(created)
})()
</script>

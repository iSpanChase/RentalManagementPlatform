<template>
  <div class="map-heatmap-card" :style="{ height }">
    <div class="fake-map">
      <div
        v-for="(p, i) in points"
        :key="i"
        class="heat-point"
        :style="{
          left: p.x + '%',
          top: p.y + '%',
          backgroundColor: getColor(p.weight),
        }"
        :title="'收益：' + p.weight"
      ></div>
      <p class="map-hint">（地圖示意區，僅用於測試 BaseCard 插槽）</p>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  /** 以百分比定位的假資料；x/y: 0~100；weight: 數值 */
  points: {
    type: Array,
    default: () => [
      { x: 30, y: 40, weight: 20000 },
      { x: 50, y: 60, weight: 8000 },
      { x: 70, y: 30, weight: 15000 },
      { x: 40, y: 70, weight: 5000 },
    ],
  },
  /** 卡片自身高度（避免父層沒高度導致不顯示） */
  height: { type: String, default: '280px' },
})

function getColor(value) {
  if (value > 15000) return 'rgba(255, 0, 0, 0.7)'     // 高收益：紅
  if (value > 8000)  return 'rgba(255, 140, 0, 0.7)'   // 中收益：橘
  return 'rgba(0, 200, 255, 0.7)'                      // 低收益：青
}
</script>

<style scoped>
.map-heatmap-card { position: relative; width: 100%; overflow: hidden; }
.fake-map {
  position: relative; width: 100%; height: 100%;
  background: linear-gradient(135deg, #e0f7fa, #b2ebf2);
  border-radius: 12px; overflow: hidden; border: 1px dashed #9adbe1;
}
.heat-point {
  position: absolute; width: 42px; height: 42px; border-radius: 50%;
  filter: blur(10px); transform: translate(-50%, -50%);
}
.map-hint {
  position: absolute; bottom: 8px; right: 8px; font-size: 12px; color: #555;
  background: rgba(255,255,255,.7); padding: 2px 6px; border-radius: 6px;
}
</style>
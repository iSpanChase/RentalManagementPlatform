<template>
  <div style="border: 2px solid red; padding: 10px; margin: 20px 0;">
    <h3 style="font-size: 1.5rem; font-weight: bold; color: red; text-align: center;">這是一個佈局測試元件</h3>
    <p style="text-align: center; margin-bottom: 10px;">如果您看到三個並排的色塊（紅、綠、藍），表示佈局正常。如果它們是垂直堆疊的，表示專案的 CSS 設定有問題。</p>
    <div class="flex w-full" style="display: flex; width: 100%; text-align: center; color: white;">
      <div class="w-1/3 bg-red-500 p-4" style="width: 33.33%; background-color: #ef4444; padding: 1rem;">
        區塊 1
      </div>
      <div class="w-1/3 bg-green-500 p-4" style="width: 33.33%; background-color: #22c55e; padding: 1rem;">
        區塊 2
      </div>
      <div class="w-1/3 bg-blue-500 p-4" style="width: 33.33%; background-color: #3b82f6; padding: 1rem;">
        區塊 3
      </div>
    </div>
  </div>
</template>

<script setup>
// No script logic needed for this test
</script>

<template>
  <div class="weather-forecast">
    <h1>天氣預報</h1>
    <p v-if="loading">載入中...</p>
    <p v-if="error">{{ error }}</p>
    <div v-if="weatherData">
      <div v-for="forecast in weatherData" :key="forecast.date" class="forecast-item">
        <p>日期: {{ forecast.date }}</p>
        <p>溫度 (C): {{ forecast.temperatureC }}</p>
        <p>溫度 (F): {{ forecast.temperatureF }}</p>
        <p>摘要: {{ forecast.summary }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

const weatherData = ref<WeatherForecast[] | null>(null);
const loading = ref(true);
const error = ref<string | null>(null);

onMounted(async () => {
  try {
    // 假設 Web API 運行在 http://localhost:5000 (ASP.NET Core 預設埠號)
    // 實際部署時，請確保這裡的 URL 是正確的
    const response = await axios.get<WeatherForecast[]>('http://localhost:5000/WeatherForecast');
    weatherData.value = response.data;
  } catch (err) {
    if (axios.isAxiosError(err)) {
      error.value = '無法載入天氣預報: ' + (err.response?.data || err.message);
    } else {
      error.value = '發生未知錯誤';
    }
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.weather-forecast {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.forecast-item {
  border: 1px solid #ccc;
  padding: 15px;
  margin-bottom: 10px;
  border-radius: 8px;
}
</style>

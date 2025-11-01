<template>
  <div class="recommendation-view">
    <h1 class="page-title">推薦房間</h1>

    <div v-if="isLoading" class="loading-message">載入中...</div>
    <div v-else-if="isError" class="error-message">載入推薦時發生錯誤。</div>      

    <div class="room-grid" v-else-if="displayedRooms && displayedRooms.length > 0">
      <router-link v-for="room in displayedRooms" :key="room.roomId" :to="`/rooms/${room.roomId}`" class="room-card-link">
        <RoomCardComponent :room="room" />
      </router-link>
    </div>
    <div v-else>
      <p class="no-results">沒有找到推薦房間。</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'; // 引入 computed
import { useQuery } from '@tanstack/vue-query';
import RoomCardComponent from '@/modules/RoomManagement/components/RoomCard.vue';  
import { getGuestRecommendations, type RecommendationRequest } from
'../api/recommendation';
import type { RoomCard } from '@/api/roomSearchApi';

// --- 獲取當前使用者 ID 的部分 (待補齊) ---
// 這裡需要根據您的專案實際的狀態管理 (例如 Pinia 或 Vuex) 來獲取登入使用者的 ID。
// 假設您有一個 Pinia store 叫做 useAuthStore，並且其中有 currentUser 包含 id。
// 範例：
// import { useAuthStore } from '@/stores/auth'; // 假設的 auth store 路徑
// const authStore = useAuthStore();
// const currentGuestId = computed(() => authStore.currentUser?.id || null);
// 為了讓程式碼能跑，我們暫時將 guestId 設為 null，之後會引導您修改。
const currentGuestId = ref<number | null>(98); // 暫時設為 null，之後會替換為實際的登入使用者 ID

// 使用 useQuery 獲取推薦資料
const { data: recommendedRooms, isLoading, isError } = useQuery<RoomCard[]>({
  queryKey: ['guestRecommendations', currentGuestId], // 查詢鍵，包含 currentGuestId        
  queryFn: async () => {
    const request: RecommendationRequest = {
      guestId: currentGuestId.value, // 使用動態獲取的 guestId
      topN: 20,
      displayM: 10,
    };
    return getGuestRecommendations(request);
  },
  enabled: true, // 頁面載入時自動啟用查詢
  staleTime: 1000 * 60 * 5, // 5 分鐘內資料視為新鮮
});

const displayedRooms = recommendedRooms;
</script>

<style scoped>
.recommendation-view {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.page-title {
  margin-bottom: 1.5rem;
  color: #333;
  text-align: center;
}

.loading-message,
.error-message,
.no-results {
  text-align: center;
  font-size: 1.1rem;
  color: #666;
  margin-top: 2rem;
}

.error-message {
  color: #dc3545;
  font-weight: bold;
}

.room-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 2rem;
}

.room-card-link {
  text-decoration: none;
  color: inherit;
}
</style>

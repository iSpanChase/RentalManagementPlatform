<template>
  <div class="recommendation-view">
    <h1 class="page-title m-5">推薦房源</h1>

    <div v-if="isLoading" class="loading-message">載入中...</div>
    <div v-else-if="isError" class="error-message">載入推薦時發生錯誤。</div>      

    <div class="room-grid" v-else-if="displayedRooms && displayedRooms.length > 0">
      <router-link v-for="room in displayedRooms" :key="room.roomId" :to="`/rooms/${room.roomId}`" class="room-card-link">
        <RoomCardComponent :room="room" />
      </router-link>
    </div>
    <div v-else>
      <p class="no-results">沒有找到推薦房源</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import RoomCardComponent from '@/modules/RoomManagement/components/RoomCard.vue';  
import { getGuestRecommendations, type RecommendationRequest } from '@/modules/reportForm/api/recommendation';
import type { RoomCard } from '@/api/roomSearchApi';
import { useAuthStore } from '@/stores/auth';

const authStore = useAuthStore();
const currentGuestId = computed(() => authStore.state.profile?.userId || null);

// 使用 useQuery 獲取推薦資料
const { data: recommendedRooms, isLoading, isError } = useQuery<RoomCard[]> ({
  queryKey: ['guestRecommendations', currentGuestId],
  queryFn: async () => {
    const request: RecommendationRequest = {
      guestId: currentGuestId.value,
      topN: 15,
      displayM: 6,
    };
    const result = await getGuestRecommendations(request);
    return result;
  },
  enabled: true,
  staleTime: 1000 * 60 * 5,
});

const displayedRooms = computed(() => {
  if (!recommendedRooms.value) {
    return [];
  }
  return recommendedRooms.value.map(room => {
    const fullAddress = room.addressLine || '';
    let cityName = null;
    let districtName = null;
    let streetAddress = null;

    if (fullAddress) {
      const cityRegex = /^(.+?[縣市])/;
      const cityMatch = fullAddress.match(cityRegex);

      if (cityMatch) {
        cityName = cityMatch[0];
        const remainingAddress = fullAddress.substring(cityName.length);
        const districtRegex = /^(.+?[區鄉鎮市])/;
        const districtMatch = remainingAddress.match(districtRegex);

        if (districtMatch) {
          districtName = districtMatch[0];
          streetAddress = remainingAddress.substring(districtName.length);
        } else {
          streetAddress = remainingAddress;
        }
      } else {
        streetAddress = fullAddress;
      }
    }

    const mappedRoom = {
      roomId: room.roomId,
      name: room.title,
      price: room.pricePerNight,
      rating: room.ratingAvg,
      imageUrl: room.mainImageUrl,
      location: fullAddress,
      cityName: cityName || room.cityName,
      districtName: districtName || room.districtName,
      addressLine: streetAddress,
    };
    return mappedRoom;
  });
});
</script>

<style scoped>
.recommendation-view {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.page-title {
  margin-bottom: 1rem;
  margin-top: 2rem;
  padding: 0;
  background: none;
  color: #212529;
  text-align: center;
  font-size: 45px;
  letter-spacing: 0.2em;
  line-height: 1.2;
  font-weight: 600;
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

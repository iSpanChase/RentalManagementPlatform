<template>
  <div class="search-view">
    <h1 class="page-title">Explore Rooms</h1>

    <div class="search-bar">
      <input type="text" v-model="searchKeyword" placeholder="Search by keyword..." />
    </div>

    <div v-if="isLoading">Loading...</div>
    <div v-if="isError">Error fetching data.</div>

    <div class="room-grid" v-if="!isLoading && !isError">
      <router-link v-for="room in displayedRooms" :key="room.id" :to="`/rooms/${room.id}`" class="room-card-link">
        <RoomCardComponent :room="room" />
      </router-link>
    </div>
    <div v-if="!isLoading && displayedRooms && displayedRooms.length === 0">
      <p>No rooms found.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useDebounceFn } from '@vueuse/core';
import RoomCardComponent from '@/modules/RoomManagement/components/RoomCard.vue';
import { fetchHotRooms, searchRooms, mapRoomDetailToCard, type RoomCard, type RoomDetail } from '@/api/roomSearchApi';

const searchKeyword = ref('');
const debouncedSearchKeyword = ref('');
const queryClient = useQueryClient();

const debounceSearch = useDebounceFn((value) => {
  debouncedSearchKeyword.value = value;
}, 300);

watch(searchKeyword, (newValue) => {
  debounceSearch(newValue);
});

// Query for popular rooms
const { data: hotRooms, isLoading: isHotLoading, isError: isHotError } = useQuery<RoomDetail[]>({
  queryKey: ['hotRooms'],
  queryFn: fetchHotRooms,
});

const hasSearchKeyword = computed(() => !!debouncedSearchKeyword.value);

// Query for search results, only enabled when there is a debounced search keyword
const { data: searchResults, isLoading: isSearchLoading, isError: isSearchError } = useQuery({
  queryKey: ['searchRooms', debouncedSearchKeyword],
  queryFn: () => searchRooms(debouncedSearchKeyword.value),
  enabled: hasSearchKeyword,
});

const isLoading = computed(() => isHotLoading.value || isSearchLoading.value);
const isError = computed(() => isHotError.value || isSearchError.value);

interface RoomCardViewModel {
  id: number;
  name: string;
  price: number;
  rating: number;
  imageUrl?: string;
  cityName: string;
  districtName: string;
  addressLine: string;
}

const toCardFromSearch = (room: RoomCard): RoomCardViewModel => {
  const rating = room.ratingAvg > 0 ? room.ratingAvg : 3;
  return {
    id: room.roomId,
    name: room.title,
    price: room.pricePerNight,
    rating,
    imageUrl: room.mainImageUrl,
    cityName: room.cityName ?? '',
    districtName: room.districtName ?? '',
    addressLine: room.addressLine ?? '',
  };
};

const toCardFromDetail = (room: RoomDetail): RoomCardViewModel => {
  const card = mapRoomDetailToCard(room);
  return toCardFromSearch(card);
};

const displayedRooms = computed<RoomCardViewModel[]>(() => {
  if (hasSearchKeyword.value) {
    const rooms = searchResults.value || [];
    return rooms.map(toCardFromSearch);
  }
  const rooms = hotRooms.value || [];
  return rooms.map(toCardFromDetail);
});

watch(hotRooms, (rooms) => {
  if (!rooms) {
    return;
  }
  rooms.forEach((room) => {
    queryClient.setQueryData(['roomDetail', room.roomId], room);
  });
});

</script>

<style scoped>
.search-view {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.page-title {
  margin-bottom: 1rem;
  padding: 0;
  background: none;
  color: #212529;
  text-align: left;
  font-size: 2rem;
  line-height: 1.2;
  font-weight: 600;
}

.search-bar {
  margin-bottom: 2rem;
}

.search-bar input {
  width: 100%;
  padding: 0.75rem;
  font-size: 1rem;
  border: 1px solid #ccc;
  border-radius: 8px;
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

<template>
  <div class="search-view">
    <h1 class="page-title">Explore Rooms</h1>

  <div class="search-bar">
      <div class="search-controls">
        <div class="input-with-dropdown" ref="containerRef">
          <input
            type="text"
            v-model="searchKeyword"
            placeholder="搜尋房源或景點..."
            @focus="dropdownOpen = true"
            @keydown.esc="dropdownOpen = false"
          />
          <ul v-if="dropdownOpen && showPlaceDropdown" class="dropdown" ref="dropdownRef">
            <li v-for="p in placeOptions" :key="p.name + p.lat" @mousedown.prevent.stop="selectPlace(p)">
              {{ p.name }}
            </li>
            <li v-if="!placeOptions || placeOptions.length === 0" class="empty">無結果</li>
          </ul>
        </div>
        <div v-if="selectedPlace" class="selected-chip">
          <span class="label">景點：</span>
          <span class="chip">{{ selectedPlace?.name }} <button class="clear" @click="clearPlace" aria-label="清除景點">×</button></span>
        </div>
      </div>
  </div>

    <div v-if="hasSelectedPlace" class="radius-group">
      <span class="radius-label">搜尋半徑：</span>
      <button type="button" :class="['radius-btn', { active: radiusKm === 5 }]" @click="radiusKm = 5">5km</button>
      <button type="button" :class="['radius-btn', { active: radiusKm === 10 }]" @click="radiusKm = 10">10km</button>
      <button type="button" :class="['radius-btn', { active: radiusKm === 15 }]" @click="radiusKm = 15">15km</button>
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
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue';
import { useQuery, useQueryClient } from '@tanstack/vue-query';
import { useDebounceFn } from '@vueuse/core';
import RoomCardComponent from '@/modules/RoomManagement/components/RoomCard.vue';
import { fetchHotRooms, searchRooms, searchRoomsNearby, searchPlaces, mapRoomDetailToCard, type RoomCard, type RoomDetail, type PlaceSuggestion } from '@/api/roomSearchApi';

const searchKeyword = ref('');
const debouncedSearchKeyword = ref('');
const queryClient = useQueryClient();
const selectedPlace = ref<PlaceSuggestion | null>(null);
const radiusKm = ref(15);
const dropdownOpen = ref(false);
const dropdownRef = ref<HTMLElement | null>(null);
const containerRef = ref<HTMLElement | null>(null);

const debounceSearch = useDebounceFn((value) => {
  debouncedSearchKeyword.value = value;
}, 300);

watch(searchKeyword, (newValue) => {
  debounceSearch(newValue);
});

// 點擊外部關閉下拉（簡化：輸入變動時自動開啟，下方列表會依關鍵字出現）

// Query for popular rooms
const { data: hotRooms, isLoading: isHotLoading, isError: isHotError } = useQuery<RoomDetail[]>({
  queryKey: ['hotRooms'],
  queryFn: fetchHotRooms,
});

const hasSearchKeyword = computed(() => !!debouncedSearchKeyword.value);
const hasSelectedPlace = computed(() => !!selectedPlace.value);

// Query for search results, only enabled when there is a debounced search keyword
const { data: searchResults, isLoading: isSearchLoading, isError: isSearchError } = useQuery({
  queryKey: ['searchRooms', debouncedSearchKeyword],
  queryFn: () => searchRooms(debouncedSearchKeyword.value),
  enabled: hasSearchKeyword,
});

// Places dropdown options
const { data: placesData } = useQuery({
  queryKey: ['places', debouncedSearchKeyword],
  queryFn: () => searchPlaces(debouncedSearchKeyword.value),
  enabled: computed(() => debouncedSearchKeyword.value.length > 0),
});
const placeOptions = computed(() => placesData.value ?? []);
const showPlaceDropdown = computed(() => debouncedSearchKeyword.value.length > 0);

// Nearby rooms based on selected place
const { data: nearbyRooms, isLoading: isNearbyLoading, isError: isNearbyError, refetch: refetchNearby } = useQuery({
  queryKey: ['nearbyRooms', selectedPlace, radiusKm],
  queryFn: () => {
    if (!selectedPlace.value) return Promise.resolve([] as RoomCard[]);
    return searchRoomsNearby(selectedPlace.value.lat, selectedPlace.value.lng, radiusKm.value, debouncedSearchKeyword.value);
  },
  enabled: hasSelectedPlace,
});

const isLoading = computed(() => isHotLoading.value || isSearchLoading.value || isNearbyLoading.value);
const isError = computed(() => isHotError.value || isSearchError.value || isNearbyError.value);

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
  if (hasSelectedPlace.value) {
    const rooms = nearbyRooms.value || [];
    return rooms.map(toCardFromSearch);
  }
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

function selectPlace(p: PlaceSuggestion) {
  selectedPlace.value = p;
  searchKeyword.value = p.name;
  dropdownOpen.value = false;
}

function clearPlace() {
  selectedPlace.value = null;
}

function handleDocumentClick(e: MouseEvent) {
  if (!dropdownOpen.value) return;
  const listEl = dropdownRef.value;
  const containerEl = containerRef.value;
  if (
    e.target instanceof Node &&
    ((listEl && !listEl.contains(e.target)) && (containerEl && !containerEl.contains(e.target)))
  ) {
    dropdownOpen.value = false;
  }
}

onMounted(() => {
  document.addEventListener('click', handleDocumentClick);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', handleDocumentClick);
});

// 當輸入內容變動時，自動開關下拉；不與 chip 景點名稱一致就離開附近模式。
watch(debouncedSearchKeyword, (val) => {
  const text = (val ?? '').toString();
  // 若已選定景點且輸入與景點名稱相同，不再開啟下拉
  if (selectedPlace.value && text === selectedPlace.value.name) {
    dropdownOpen.value = false;
    return;
  }
  dropdownOpen.value = text.length > 0;
});

watch(searchKeyword, (val) => {
  if (selectedPlace.value && val !== selectedPlace.value.name) {
    selectedPlace.value = null;
  }
  if (!val || val.trim().length === 0) {
    // 清空輸入時，關閉下拉並清除景點
    dropdownOpen.value = false;
    selectedPlace.value = null;
  }
});

// （移除距離顯示相關計算）

</script>

<style scoped>
.search-view {
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
  text-align: left;
  font-size: 2rem;
  line-height: 1.2;
  font-weight: 600;
}

.search-bar {
  margin-bottom: 2rem;
}

.search-controls {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.5rem;
  position: relative;
}

.input-with-dropdown input {
  width: 100%;
  padding: 0.75rem;
  font-size: 1rem;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-shadow: 0 6px 16px rgba(0,0,0,0.1);
  z-index: 10;
  max-height: 300px;
  overflow: auto;
  padding: 0.25rem 0;
}

.dropdown li {
  list-style: none;
  padding: 0.5rem 0.75rem;
  cursor: pointer;
}

.dropdown li:hover {
  background: #f6f7f9;
}

.dropdown li.empty {
  color: #888;
  cursor: default;
}

.selected-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.95rem;
}

.selected-chip .label {
  color: #666;
}

.selected-chip .chip {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: #eef2ff;
  color: #243b6b;
  border: 1px solid #c7d2fe;
  border-radius: 999px;
  padding: 0.25rem 0.5rem;
}

.selected-chip .clear {
  background: transparent;
  border: 0;
  color: #243b6b;
  font-size: 1rem;
  cursor: pointer;
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

.distance-badge {
  margin-top: 0.5rem;
  font-size: 0.9rem;
  color: #334155;
}

.radius-group {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0.5rem 0 1rem;
}

.radius-label {
  color: #666;
}

.radius-btn {
  border: 1px solid #cbd5e1;
  background: #f8fafc;
  color: #0f172a;
  padding: 0.25rem 0.5rem;
  border-radius: 0.5rem;
  cursor: pointer;
}

.radius-btn.active {
  background: #1d4ed8;
  border-color: #1d4ed8;
  color: #fff;
}
</style>


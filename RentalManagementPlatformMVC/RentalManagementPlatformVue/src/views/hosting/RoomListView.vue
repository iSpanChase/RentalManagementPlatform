<template>
  <div class="search-view">
    <h1 class="page-title">My Rooms</h1>
    <div class="search-bar">
      <router-link to="/hosting/rooms/new" class="btn btn-primary">Add New Room</router-link>
    </div>

    <div v-if="isLoading">Loading...</div>
    <div v-if="isError">Error fetching data.</div>

    <div class="room-grid" v-if="!isLoading && !isError">
      <div v-for="room in displayedRooms" :key="room.roomId" class="room-card-container">
        <router-link :to="`/rooms/${room.roomId}`" class="room-card-link">
          <RoomCardComponent :room="room" />
        </router-link>
        <div class="card-actions">
          <router-link :to="`/hosting/rooms/${room.roomId}/edit`" class="btn btn-sm btn-outline-primary">Edit</router-link>
          <button @click="deleteRoom(room.roomId)" class="btn btn-sm btn-outline-danger">Delete</button>
        </div>
      </div>
    </div>
    <div v-if="!isLoading && displayedRooms && displayedRooms.length === 0">
      <p>No rooms found.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios';
import { ref, onMounted, computed } from 'vue';
import { getRoomsByHostId, deleteRoom as deleteRoomApi } from '@/api/roomApi';
import { fetchRoomDetail, mapRoomDetailToCard, type RoomCard, type RoomDetail } from '@/api/roomSearchApi';
import RoomCardComponent from '@/modules/RoomManagement/components/RoomCard.vue';

interface RoomCardViewModel {
  roomId: number;
  name: string;
  price: number;
  rating: number;
  imageUrl?: string;
  cityName: string;
  districtName: string;
  addressLine: string;
}

const HOST_ID = 1; // TODO: replace with authenticated host context
const roomCards = ref<RoomCard[]>([]);
const isLoading = ref(true);
const isError = ref(false);

const toViewModel = (room: RoomCard): RoomCardViewModel => {
  const rating = room.ratingAvg > 0 ? room.ratingAvg : 3;
  return {
    roomId: room.roomId,
    name: room.title,
    price: room.pricePerNight,
    rating,
    imageUrl: room.mainImageUrl,
    cityName: room.cityName ?? '',
    districtName: room.districtName ?? '',
    addressLine: room.addressLine ?? '',
  };
};

const displayedRooms = computed<RoomCardViewModel[]>(() =>
  roomCards.value.map(toViewModel)
);

const fetchRooms = async () => {
  try {
    isLoading.value = true;
    isError.value = false;
    const summaries = await getRoomsByHostId(HOST_ID);
    if (!Array.isArray(summaries)) {
      throw new Error('Unexpected response when fetching host rooms.');
    }
    const activeSummaries = summaries.filter((summary) => {
      const isDeleted = (summary as any)?.isDeleted ?? (summary as any)?.IsDeleted;
      const status = (summary as any)?.status ?? (summary as any)?.Status;
      return isDeleted !== true && status !== '已刪除';
    });
    if (activeSummaries.length === 0) {
      roomCards.value = [];
      return;
    }

    const detailPromises = activeSummaries.map(async (summary) => {
      const roomId = summary?.roomId ?? summary?.RoomId;
      if (typeof roomId !== 'number') {
        throw new Error('Room summary missing numeric roomId.');
      }
      try {
        const detail: RoomDetail = await fetchRoomDetail(roomId);
        if (detail.isDeleted) {
          return null;
        }
        return mapRoomDetailToCard(detail);
      } catch (err) {
        console.error('Failed to fetch room detail for room', roomId, err);
        return null;
      }
    });

    const resolvedCards = await Promise.all(detailPromises);
    roomCards.value = resolvedCards.filter((card): card is RoomCard => card !== null);
  } catch (error) {
    console.error('Error fetching rooms:', error);
    isError.value = true;
    roomCards.value = [];
  } finally {
    isLoading.value = false;
  }
};

const deleteRoom = async (roomId: number) => {
  if (!confirm('Are you sure you want to delete this room?')) {
    return;
  }
  try {
    await deleteRoomApi(roomId);
    await fetchRooms();
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      // Room already deleted server-side; refresh list to hide it.
      await fetchRooms();
      return;
    }
    console.error('Error deleting room:', error);
  }
};

onMounted(fetchRooms);
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

.room-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 2rem;
}

.room-card-container {
  position: relative;
}

.room-card-link {
  text-decoration: none;
  color: inherit;
}

.card-actions {
  position: absolute;
  top: 10px;
  right: 10px;
  display: flex;
  gap: 5px;
}
</style>

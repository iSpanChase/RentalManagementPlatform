<template>
  <div class="container mt-5 mb-5">
    <div class="row justify-content-center">
      <div class="col-md-8">
        
        <div v-if="isLoading" class="text-center">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p>載入資料中...</p>
        </div>

        <div v-else-if="error" class="alert alert-danger">
          載入房源資料失敗: {{ error.message }}
        </div>

        <div v-else-if="roomData">
          <!-- Step 1: Form -->
          <div v-if="step === 1">
            <h2>修改您的房源</h2>
            <p class="text-muted">步驟 1/2: 更新您的房源資訊。</p>
            <hr class="my-4">
            <RoomForm 
              :initial-data="roomData" 
              @submit="handleUpdateRoom"
              :cities="cities"
              :districts="districts"
              :districts-loading="districtsLoading"
              @city-changed="handleCityChange"
              mode="edit"
            />
          </div>

          <!-- Step 2: Photo Manager -->
          <div v-else-if="step === 2">
            <h2>管理您的房源照片</h2>
            <p class="text-muted">步驟 2/2: 新增或刪除您的房源照片。</p>
            <hr class="my-4">
            <RoomPhotoManager 
              :room-id="roomId" 
              :photos="roomData.photos || []" 
              @refresh="fetchDetails"
            />
            <hr class="my-4">
            <button class="btn btn-success" @click="finishEditing">完成並前往首頁</button>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import RoomForm from '@/components/forms/RoomForm.vue';
import RoomPhotoManager from '@/components/hosting/RoomPhotoManager.vue';
import { updateRoom } from '@/api/roomApi.ts';
import { fetchRoomDetail } from '@/api/roomSearchApi.ts';
import { fetchCities, fetchDistricts } from '@/api/locationApi.ts';

const route = useRoute();
const router = useRouter();
const roomId = Number(route.params.id);

// Wizard step management
const step = ref(1);

const roomData = ref(null);
const isLoading = ref(true);
const error = ref(null);

// State for dropdowns
const cities = ref([]);
const districts = ref([]);
const districtsLoading = ref(false);

async function fetchDetails() {
  try {
    const details = await fetchRoomDetail(roomId);
    roomData.value = details;
    // If the room has a city, fetch its districts
    if (details && details.cityId) {
      await handleCityChange(details.cityId);
    }
  } catch (e) {
    console.error('Failed to fetch room details:', e);
    error.value = e;
  }
}

onMounted(async () => {
  if (!roomId) {
    error.value = new Error('無效的房源 ID');
    isLoading.value = false;
    return;
  }
  isLoading.value = true;
  // Fetch cities first
  try {
    cities.value = await fetchCities();
  } catch (e) {
    console.error("Failed to fetch cities:", e);
  }
  // Then fetch the main room data
  await fetchDetails();
  isLoading.value = false;
});

async function handleCityChange(cityId) {
  if (!cityId) {
    districts.value = [];
    return;
  }
  districtsLoading.value = true;
  try {
    districts.value = await fetchDistricts(cityId);
  } catch (error) {
    console.error("Failed to fetch districts:", error);
    districts.value = []; // Clear districts on error
  } finally {
    districtsLoading.value = false;
  }
}

async function handleUpdateRoom(formData) {
  try {
    await updateRoom(roomId, formData);
    alert('房源已成功更新！現在請管理您的照片。');
    step.value = 2; // Move to photo management step
  } catch (error) {
    console.error('Failed to update room:', error);
    alert('更新房源失敗，請檢查主控台中的錯誤訊息。');
  }
}

function finishEditing() {
    router.push({ path: '/' });
}

</script>

<style scoped>
.container {
  max-width: 960px;
}
</style>

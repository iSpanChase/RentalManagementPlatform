<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-8">
        <!-- Step 1: Form -->
        <div v-if="step === 1">
          <h2>新增您的房源</h2>
          <p class="text-muted">步驟 1/2: 請填寫以下資訊來建立您的新房源。</p>
          <hr class="my-4">
          <RoomForm 
            @submit="handleCreateRoom" 
            :cities="cities"
            :districts="districts"
            :districts-loading="districtsLoading"
            @city-changed="handleCityChange"
            mode="create"
          />
        </div>

        <!-- Step 2: Photo Manager -->
        <div v-else-if="step === 2">
          <h2>管理您的房源照片</h2>
          <p class="text-muted">步驟 2/2: 新增或刪除您的房源照片。</p>
          <hr class="my-4">
          <RoomPhotoManager 
            v-if="newlyCreatedRoomId"
            :room-id="newlyCreatedRoomId" 
            :photos="[]" 
            @refresh="()=>{}" 
          />
          <hr class="my-4">
          <button class="btn btn-success" @click="finishCreation">完成並前往首頁</button>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import RoomForm from '@/components/forms/RoomForm.vue';
import RoomPhotoManager from '@/components/hosting/RoomPhotoManager.vue';
import { createRoom } from '@/api/roomApi.ts';
import { fetchCities, fetchDistricts } from '@/api/locationApi.ts';

const router = useRouter();

// Wizard step management
const step = ref(1);
const newlyCreatedRoomId = ref(null);

// State for dropdowns
const cities = ref([]);
const districts = ref([]);
const districtsLoading = ref(false);

// Fetch cities when the component is mounted
onMounted(async () => {
  try {
    cities.value = await fetchCities();
  } catch (error) {
    console.error("Failed to fetch cities:", error);
  }
});

// Handle city change to fetch districts
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

async function handleCreateRoom(formData) {
  try {
    const newRoom = await createRoom(formData);
    alert('房源已成功建立！現在請上傳您的房源照片。');
    newlyCreatedRoomId.value = newRoom.roomId;
    step.value = 2; // Move to the next step
  } catch (error) {
    console.error('Failed to create room:', error);
    alert('建立房源失敗，請檢查主控台中的錯誤訊息。');
  }
}

function finishCreation() {
    router.push({ path: '/' });
}

</script>

<style scoped>
.container {
  max-width: 960px;
}
</style>

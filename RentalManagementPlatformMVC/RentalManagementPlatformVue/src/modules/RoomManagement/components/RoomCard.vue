<template>
  <div class="room-card">
    <div class="room-image-container">
      <img
        v-if="room.imageUrl"
        :src="room.imageUrl"
        class="room-image"
        :alt="`${room.name} image`"
      />
      <div v-else class="room-image-placeholder"></div>
    </div>

    <div class="card-body">
      <h5 class="card-title mb-0">{{ fullAddress }}</h5>
      <p class="card-text text-muted mt-1">{{ room.name }}</p>
      <div class="d-flex justify-content-between align-items-center mt-2">
        <p class="card-text mb-0">
          <span class="fw-bold">${{ room.price }}</span> / 晚
        </p>
        <span v-if="room.rating > 0" class="rating d-flex align-items-center">
          <font-awesome-icon :icon="['fas', 'star']" class="me-1" />
          {{ room.rating.toFixed(1) }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, computed } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const props = defineProps({
  room: {
    type: Object,
    required: true,
    default: () => ({
      imageUrl: '',
      location: '地點未提供',
      name: '名稱未提供',
      price: 0,
      rating: 3,
      cityName: '',
      districtName: '',
      addressLine: '',
    }),
  },
});

const fullAddress = computed(() => {
  if (props.room.cityName && props.room.districtName && props.room.addressLine) {
    return `${props.room.cityName}${props.room.districtName}${props.room.addressLine}`;
  }
  return props.room.location || '地點未提供';
});
</script>

<style scoped>
.room-card {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  border: none;
  background-color: transparent;
}

.room-image-container {
  border-radius: 12px;
  overflow: hidden;
  position: relative;
  width: 100%;
  aspect-ratio: 1 / 0.95;
  background-color: #f0f0f0;
}

.room-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.room-image-placeholder {
  width: 100%;
  height: 100%;
  background-color: #e0e0e0;
}

.card-body {
  padding: 8px 4px;
}

.card-title {
  font-size: 0.95rem;
  font-weight: 600;
}

.rating {
  font-size: 0.9rem;
  white-space: nowrap;
}

.card-text {
  font-size: 0.9rem;
  margin-bottom: 0;
}

.text-muted {
  color: #6a6a6a !important;
}

.fw-bold {
  font-weight: 700 !important;
}
</style>

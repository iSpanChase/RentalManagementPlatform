<template>
  <div class="photo-manager p-3 border rounded">
    
    <!-- Section for existing photos -->
    <h5 class="mb-3">現有照片</h5>
    <div v-if="!photos || photos.length === 0" class="alert alert-secondary">
      目前沒有任何照片。
    </div>
    <div v-else id="photo-gallery" class="photo-grid">
      <div v-for="photo in photos" :key="photo.photoId" class="photo-card">
        <a :href="photo.url" 
           :data-pswp-width="1600" 
           :data-pswp-height="1067" 
           target="_blank" 
           rel="noreferrer">
          <img :src="photo.url" :alt="`Room photo ${photo.photoId}`" class="img-fluid rounded">
        </a>
        <button @click="handleDelete(photo.photoId)" class="btn btn-danger btn-sm delete-btn" :disabled="isDeleting">
          <span v-if="isDeleting === photo.photoId" class="spinner-border spinner-border-sm"></span>
          <span v-else>&times;</span>
        </button>
      </div>
    </div>

    <hr class="my-4">

    <!-- Uppy Upload Section -->
    <h5 class="mb-3">上傳新照片</h5>
    <div ref="uppyContainer"></div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { deleteRoomPhoto } from '@/api/photoApi';
import apiClient from '@/api/axiosInstance';

// Uppy imports
import Uppy from '@uppy/core';
import Dashboard from '@uppy/dashboard';
import XHRUpload from '@uppy/xhr-upload';
import '@uppy/core/css/style.min.css';
import '@uppy/dashboard/css/style.min.css';

// PhotoSwipe imports
import PhotoSwipeLightbox from 'photoswipe/lightbox';
import 'photoswipe/photoswipe.css';

const props = defineProps({
  roomId: {
    type: Number,
    required: true
  },
  photos: {
    type: Array,
    default: () => []
  }
});

const emit = defineEmits(['refresh']);

const isDeleting = ref(null);
const uppyContainer = ref(null);

let uppy = null;
let lightbox = null;
const apiBaseUrl = apiClient.defaults.baseURL?.replace(/\/$/, '');

const getAccessToken = () => {
  return (
    localStorage.getItem('rmp.accessToken') ||
    localStorage.getItem('access_token') ||
    localStorage.getItem('jwt_token') ||
    ''
  );
}

// --- Uppy Logic ---
const setupUppy = () => {
  if (uppy) uppy.close();

  uppy = new Uppy({
    autoProceed: false,
    restrictions: {
      maxNumberOfFiles: 10,
      allowedFileTypes: ['image/*'],
    },
  })
  // 預設照片類型，避免後端收到空字串
  
  .use(Dashboard, {
    inline: true,
    target: uppyContainer.value,
    height: 300,
    proudlyDisplayPoweredByUppy: false,
  })
  .use(XHRUpload, {
    endpoint: apiBaseUrl
      ? `${apiBaseUrl}/Rooms/${props.roomId}/upload-image`
      : `/api/Rooms/${props.roomId}/upload-image`,
    fieldName: 'ImageFile',
    allowedMetaFields: ['PhotoType'],
    headers: () => {
      const token = getAccessToken();
      return token && token.trim().length > 0
        ? { Authorization: `Bearer ${token}` }
        : {};
    },
    // Note: Uppy sends the file and metadata separately. 
    // The backend `UploadImageDto` will need to handle this.
    // For simplicity, we are not adding extra metadata here for now.
  })
  .on('complete', (result) => {
    console.log('Upload complete! Successful files:', result.successful);
    if (result.successful.length > 0) {
        alert('所有照片上傳成功！');
        emit('refresh');
    }
  });
};

// --- PhotoSwipe Logic ---
const setupPhotoSwipe = () => {
    if (lightbox) {
        lightbox.destroy();
        lightbox = null;
    }
    if (props.photos && props.photos.length > 0) {
        lightbox = new PhotoSwipeLightbox({
            gallery: '#photo-gallery',
            children: 'a',
            pswpModule: () => import('photoswipe'),
        });
        lightbox.init();
    }
};

// --- Delete Logic ---
const handleDelete = async (photoId) => {
  if (!confirm('您確定要刪除這張照片嗎？此操作無法復原。')) {
    return;
  }

  isDeleting.value = photoId;
  try {
    await deleteRoomPhoto(photoId);
    alert('照片刪除成功！');
    emit('refresh');
  } catch (error) {
    console.error('Delete failed:', error);
    alert('照片刪除失敗。');
  } finally {
    isDeleting.value = null;
  }
};

// --- Lifecycle Hooks ---
onMounted(() => {
  setupUppy();
  setupPhotoSwipe();
});

onUnmounted(() => {
  if (uppy) uppy.close();
  if (lightbox) lightbox.destroy();
});

// Watch for changes in photos prop to re-initialize photoswipe
watch(() => props.photos, () => {
    setupPhotoSwipe();
});

</script>

<style scoped>
.photo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
}

.photo-card {
  position: relative;
  aspect-ratio: 4 / 3;
}

.photo-card img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.delete-btn {
  position: absolute;
  top: 8px;
  right: 8px;
  opacity: 0.75;
  transition: opacity 0.2s ease;
}

.photo-card:hover .delete-btn {
  opacity: 1;
}

/* Customizing Uppy's appearance to be less intrusive */
:deep(.uppy-Dashboard-inner) {
    border: 1px dashed #ced4da;
}
:deep(.uppy-Dashboard-AddFiles) {
    border: none;
}
</style>

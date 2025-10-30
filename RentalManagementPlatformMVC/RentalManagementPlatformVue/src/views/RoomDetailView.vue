<template>
  <div class="room-detail-view">
    <div v-if="isRoomLoading">Loading room details...</div>
    <div v-else-if="isRoomError">Error fetching room details: {{ roomDetailError?.message }}</div>
    <div v-else-if="roomDetail">
      <div class="room-header">
        <div class="cover" v-if="roomDetail.mainImageUrl">
          <img :src="roomDetail.mainImageUrl" :alt="`${roomDetail.title} cover`" />
        </div>
        <div class="info">
          <h1 class="title">{{ roomDetail.title }}</h1>
          <p class="address" v-if="displayAddress">{{ displayAddress }}</p>
          <p class="pricing">${{ roomDetail.pricePerNight }} / 晚</p>
          <p class="rating">評分：{{ displayRating.toFixed(1) }} ・ 評論 {{ roomDetail.reviewsCount }}</p>
          <p class="host" v-if="roomDetail.hostName">房東：{{ roomDetail.hostName }}</p>
        </div>
      </div>

      <section class="description" v-if="roomDetail.description">
        <h2>房源介紹</h2>
        <p>{{ roomDetail.description }}</p>
      </section>

      <section class="gallery" v-if="galleryItems.length">
        <h2>照片</h2>
        <div class="photo-grid" :id="galleryId">
          <a
            v-for="(image, index) in galleryItems"
            :key="image.src"
            class="photo-link"
            :href="image.src"
            :data-pswp-width="image.width"
            :data-pswp-height="image.height"
            :data-index="index"
            @click.prevent="openGallery(index)"
          >
            <img :src="image.src" class="photo" :alt="image.alt" />
          </a>
        </div>
      </section>
    </div>

    <div v-else>
      <p>未找到房源詳細資訊。</p>
    </div>

    <section class="review-section">
      <h2>旅客評論</h2>
      <div v-if="isReviewsLoading">Loading reviews...</div>
      <div v-else-if="isReviewsError">Error fetching reviews: {{ reviewsError?.message }}</div>
      <div v-else-if="reviews && reviews.length">
        <ReviewList :reviews="reviews" />
      </div>
      <div v-else>
        <p>尚無評論。</p>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { fetchReviewsByRoomId, createReview } from '@/api/reviewApi';
import { fetchRoomDetail, type RoomDetail } from '@/api/roomSearchApi';
import ReviewList from '@/modules/RoomManagement/ReviewList.vue';
import ReviewForm from '@/modules/RoomManagement/ReviewForm.vue';
import PhotoSwipeLightbox from 'photoswipe/lightbox';
import 'photoswipe/photoswipe.css';

const route = useRoute();
const roomId = Number(route.params.id);

const queryClient = useQueryClient();
const reviewForm = ref<{ resetForm: () => void } | null>(null);

const cachedRoomDetail = queryClient.getQueryData<RoomDetail>(['roomDetail', roomId]);

const {
  data: roomDetail,
  isLoading: isRoomLoading,
  isError: isRoomError,
  error: roomDetailError,
} = useQuery<RoomDetail, Error>({
  queryKey: ['roomDetail', roomId],
  queryFn: () => fetchRoomDetail(roomId),
  enabled: Number.isFinite(roomId) && !cachedRoomDetail,
  initialData: cachedRoomDetail,
});

const galleryId = 'room-detail-gallery';

const galleryItems = computed(() => {
  const detail = roomDetail.value;
  if (!detail) {
    return [] as Array<{ src: string; width: number; height: number; alt: string }>;
  }
  return detail.photoUrls.map((src, index) => ({
    src,
    width: 1600,
    height: 1067,
    alt: `${detail.title} photo ${index + 1}`,
  }));
});

let lightbox: PhotoSwipeLightbox | null = null;

const destroyLightbox = () => {
  if (lightbox) {
    lightbox.destroy();
    lightbox = null;
  }
};

const initLightbox = async () => {
  destroyLightbox();
  if (!galleryItems.value.length) {
    return;
  }
  await nextTick();
  if (!galleryItems.value.length) {
    return;
  }
  lightbox = new PhotoSwipeLightbox({
    gallery: `#${galleryId}`,
    children: 'a',
    pswpModule: () => import('photoswipe'),
  });
  lightbox.init();
};

watch(
  galleryItems,
  async (items) => {
    if (!items.length) {
      destroyLightbox();
      return;
    }
    await initLightbox();
  },
  { flush: 'post' },
);

onMounted(() => {
  if (galleryItems.value.length) {
    initLightbox();
  }
});

onUnmounted(() => {
  destroyLightbox();
});

const openGallery = (index: number) => {
  if (!lightbox) {
    return;
  }
  lightbox.loadAndOpen(index);
};

const displayAddress = computed(() => {
  const detail = roomDetail.value;
  if (!detail) {
    return '';
  }
  return detail.fullAddress ?? [detail.cityName, detail.districtName, detail.addressLine].filter(Boolean).join('');
});

const displayRating = computed(() => {
  const detail = roomDetail.value;
  if (!detail) {
    return 0;
  }
  return detail.ratingAvg && detail.ratingAvg > 0 ? detail.ratingAvg : 3;
});

// Query to fetch reviews
const {
  data: reviews,
  isLoading: isReviewsLoading,
  isError: isReviewsError,
  error: reviewsError,
} = useQuery({
  queryKey: ['reviews', roomId],
  queryFn: () => fetchReviewsByRoomId(roomId),
});

// Mutation to create a review
const { mutate, isPending } = useMutation({
  mutationFn: createReview,
  onSuccess: () => {
    // Invalidate and refetch the reviews query to show the new review
    queryClient.invalidateQueries({ queryKey: ['reviews', roomId] });
    // Reset the form
    reviewForm.value?.resetForm();
  },
  onError: (err) => {
    alert(`Error creating review: ${err.message}`);
  }
});

const handleCreateReview = (reviewData: { rating: number; comment: string }) => {
  mutate({
    ...reviewData,
    roomId: roomId,
    bookingId: 999, // Placeholder for bookingId
  });
};

</script>

<style scoped>
.room-detail-view {
  padding: 2rem;
  max-width: 960px;
  margin: 0 auto;
}

.room-header {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.cover img {
  width: 320px;
  height: 220px;
  object-fit: cover;
  border-radius: 12px;
}

.info {
  flex: 1;
  min-width: 240px;
}

.title {
  margin-bottom: 0.5rem;
}

.address,
.pricing,
.rating,
.host {
  margin: 0.25rem 0;
}

.description,
.gallery,
.review-section {
  margin-top: 2rem;
}

.photo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 0.75rem;
}

.photo-link {
  display: block;
  border-radius: 8px;
  overflow: hidden;
  position: relative;
  cursor: zoom-in;
}

.photo {
  width: 100%;
  aspect-ratio: 4 / 3;
  object-fit: cover;
  border-radius: 8px;
}

.review-section {
  max-width: 800px;
}
</style>

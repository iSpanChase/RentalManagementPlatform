<template>
  <div v-if="isRoomLoading">Loading room details...</div>
  <div v-else-if="isRoomError">Error fetching room details: {{ roomDetailError?.message }}</div>
  <div v-else-if="roomDetail" class="room-detail-container">
    <!-- Header: Title, Rating, Location -->
    <section class="room-header">
      <h1>{{ roomDetail.title }}</h1>
      <div class="sub-header">
        <span>★ {{ displayRating.toFixed(1) }}</span>
        <span>·</span>
        <a>{{ roomDetail.reviewsCount }} reviews</a>
        <span>·</span>
        <a class="address-link">{{ displayAddress }}</a>
      </div>
    </section>

    <!-- Image Gallery -->
    <section class="image-gallery" :id="galleryId">
      <a :href="galleryItems[0]?.src" class="main-image" @click.prevent="openGallery(0)">
        <img :src="galleryItems[0]?.src" alt="Main room image" v-if="galleryItems[0]" />
      </a>
      <div class="small-images">
        <a v-for="i in 4" :key="i" :href="galleryItems[i]?.src" @click.prevent="openGallery(i)">
           <img v-if="galleryItems[i]" :src="galleryItems[i]?.src" :alt="`Room image ${i+1}`"/>
        </a>
      </div>
    </section>

    <!-- Main Content (2 columns) -->
    <section class="main-content">
      <!-- Left Column -->
      <div class="left-column">
        <div class="host-info">
            <h2>Entire home hosted by {{ roomDetail.hostName }}</h2>
            <span>{{ roomDetail.maxGuests }} guests</span>
        </div>
        <hr />
        <section class="description" v-if="roomDetail.description">
            <h2>About this space</h2>
            <p>{{ roomDetail.description }}</p>
        </section>
        <hr />
        <!-- Reviews -->
        <section class="review-section">
          <h2>★ {{ displayRating.toFixed(1) }} · {{ reviews ? reviews.length : 0 }} reviews</h2>
           <div v-if="isReviewsLoading">Loading reviews...</div>
          <div v-else-if="isReviewsError">Error fetching reviews: {{ reviewsError?.message }}</div>
          <div v-else-if="reviews && reviews.length">
            <ReviewList :reviews="reviews" />
          </div>
          <div v-else>
            <p>No reviews yet.</p>
          </div>
        </section>
      </div>

      <!-- Right Column (Sticky Booking Card) -->
      <div class="right-column">
        <div class="booking-card">
          <div class="booking-price">
            <span class="price-amount">${{ roomDetail.pricePerNight }}</span>
            <span class="price-unit">night</span>
          </div>

          <div class="booking-form">
             <date-picker
                v-model:value="dateRange"
                range
                placeholder="Select check-in & check-out date"
                format="YYYY-MM-DD"
                :editable="false"
                class="custom-datepicker"
                :disabled-date="disabledDate"
              />
             <div class="guest-input">
                <label for="guests">GUESTS</label>
                <input type="number" id="guests" v-model.number="guestCount" min="1" :max="roomDetail.maxGuests">
             </div>
          </div>

          <button class="reserve-button" @click="handleReserve">Reserve</button>
          <p class="charge-notice">You won't be charged yet</p>

          <div class="price-breakdown" v-if="dateRange && dateRange[0] && dateRange[1]">
            <div class="price-item">
                <span>${{ roomDetail.pricePerNight }} x {{ nights }} nights</span>
                <span>${{ roomDetail.pricePerNight * nights }}</span>
            </div>
            <div class="price-item">
                <span>Service fee</span>
                <span>${{ serviceFee }}</span>
            </div>
            <hr/>
            <div class="price-item total">
                <span>Total</span>
                <span>${{ totalPriceWithFee }}</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query';
import { fetchReviewsByRoomId, createReview } from '@/api/reviewApi';
import { fetchRoomDetail, type RoomDetail } from '@/api/roomSearchApi';
import { useBookingStore } from '@/stores/bookingStore.js';
import { useAuthStore } from '@/stores/auth';
import ReviewList from '@/modules/RoomManagement/ReviewList.vue';
import ReviewForm from '@/modules/RoomManagement/ReviewForm.vue';
import PhotoSwipeLightbox from 'photoswipe/lightbox';
import 'photoswipe/photoswipe.css';
import DatePicker from 'vue-datepicker-next';
import 'vue-datepicker-next/index.css';

const route = useRoute();
const router = useRouter();
const bookingStore = useBookingStore();
const auth = useAuthStore();
const roomId = Number(route.params.id);

// State for booking form
const dateRange = ref([]);
const guestCount = ref(1);

// 禁用今天之前的日期
const disabledDate = (date: Date) => {
  const today = new Date();
  today.setHours(0, 0, 0, 0); // 設定為今天的開始時間
  return date < today;
};

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
  if (!detail || !detail.photoUrls) {
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
  return detail.fullAddress ?? [detail.cityName, detail.districtName, detail.addressLine].filter(Boolean).join(', ');
});

const displayRating = computed(() => {
  const detail = roomDetail.value;
  if (!detail) {
    return 0;
  }
  return detail.ratingAvg && detail.ratingAvg > 0 ? detail.ratingAvg : 5.0;
});

const nights = computed(() => {
    if (!dateRange.value || dateRange.value.length < 2 || !dateRange.value[0] || !dateRange.value[1]) return 0;
    const checkIn = new Date(dateRange.value[0]);
    const checkOut = new Date(dateRange.value[1]);
    const diffTime = Math.abs(checkOut.getTime() - checkIn.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays : 0;
});

const serviceFee = computed(() => {
  if (!roomDetail.value || nights.value <= 0) return 0;
  const subtotal = roomDetail.value.pricePerNight * nights.value;
  return Math.round(subtotal * 0.1); // 10% service fee
});

const totalPriceWithFee = computed(() => {
  if (!roomDetail.value || nights.value <= 0) return 0;
  const subtotal = roomDetail.value.pricePerNight * nights.value;
  return subtotal + serviceFee.value;
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

const toLocalISODateString = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

const handleReserve = () => {
    if (!dateRange.value || dateRange.value.length < 2 || !dateRange.value[0] || !dateRange.value[1]) {
        alert('Please select check-in and check-out dates.');
        return;
    }
    if (guestCount.value <= 0) {
        alert('Please specify the number of guests.');
        return;
    }
    if (!roomDetail.value) {
        alert('Room details not loaded yet.');
        return;
    }

    const [checkIn, checkOut] = dateRange.value;

    const bookingData = {
        roomId: roomDetail.value.roomId,
        guestId: auth.state.profile?.userId,
        checkIn: toLocalISODateString(checkIn as Date),
        checkOut: toLocalISODateString(checkOut as Date),
        guestCount: guestCount.value,
        roomTitle: roomDetail.value.title,
        roomImage: roomDetail.value.mainImageUrl || galleryItems.value[0]?.src || '',
        pricePerNight: roomDetail.value.pricePerNight,
        serviceFee: roomDetail.value.pricePerNight * nights.value * 0.1, // Example: 10% service fee
    };

    bookingStore.setBookingDraft(bookingData);
    router.push('/booking/confirm');
};

</script>

<style scoped>
/* Inspired by Airbnb's design */
.room-detail-container {
  max-width: 1120px;
  margin: 0 auto;
  padding: 24px 40px;
  font-family: 'Circular', -apple-system, BlinkMacSystemFont, 'Roboto', 'Helvetica Neue', sans-serif;
  color: #222;
}

hr {
  border: none;
  border-top: 1px solid #ebebeb;
  margin: 32px 0;
}

/* Header */
.room-header h1 {
  font-size: 26px;
  font-weight: 600;
  margin: 0;
}
.sub-header {
  display: flex;
  gap: 8px;
  align-items: center;
  font-size: 14px;
  font-weight: 600;
  margin-top: 8px;
}
.sub-header a {
  text-decoration: underline;
  cursor: pointer;
}

/* Image Gallery */
.image-gallery {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 8px;
  margin-top: 24px;
  border-radius: 12px;
  overflow: hidden;
  height: 450px;
  background-color: #f7f7f7;
}
.main-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  cursor: pointer;
}
.small-images {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr;
  gap: 8px;
}
.small-images img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  cursor: pointer;
}
.small-images a:first-child {
    border-top-right-radius: 0;
}
.small-images a:last-child {
    border-bottom-right-radius: 0;
}


/* Main Content */
.main-content {
  display: grid;
  grid-template-columns: minmax(0, 2fr) minmax(300px, 1fr);
  gap: 80px;
  margin-top: 48px;
}

.left-column h2 {
    font-size: 22px;
    font-weight: 600;
}

.host-info {
    padding-bottom: 24px;
}
.host-info h2 {
    margin: 0;
}
.host-info span {
    margin-top: 8px;
    display: block;
    font-size: 16px;
}


.right-column {
  position: relative;
}
.booking-card {
  position: sticky;
  top: 100px;
  border: 1px solid #dddddd;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 6px 16px rgba(0,0,0,0.12);
}
.booking-price {
    display: flex;
    align-items: baseline;
    margin-bottom: 24px;
}
.price-amount {
    font-size: 22px;
    font-weight: 600;
}
.price-unit {
    font-size: 16px;
    margin-left: 4px;
}

.booking-form {
    border: 1px solid #b0b0b0;
    border-radius: 8px;
    margin-bottom: 16px;
}

.guest-input {
    padding: 10px;
    border-top: 1px solid #b0b0b0;
}

.guest-input label {
    display: block;
    font-size: 10px;
    font-weight: 800;
    margin-bottom: 2px;
}

.guest-input input {
    border: none;
    width: 100%;
    padding: 2px 0;
}

.guest-input input:focus {
    outline: none;
}

.reserve-button {
  background: linear-gradient(to right, #E61E4D 0%, #E31C5F 50%, #D70466 100%);
  color: white;
  border: none;
  border-radius: 8px;
  padding: 14px 24px;
  width: 100%;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: box-shadow 0.2s ease;
}
.reserve-button:hover {
    box-shadow: 0 2px 4px rgba(0,0,0,0.18);
}
.charge-notice {
    text-align: center;
    font-size: 12px;
    margin-top: 12px;
}
.price-breakdown {
    margin-top: 24px;
    font-size: 16px;
}
.price-item {
    display: flex;
    justify-content: space-between;
    margin-bottom: 12px;
}
.price-item.total {
    font-weight: 600;
}
.price-breakdown hr {
    margin: 16px 0;
}

.review-section h2 {
    margin-bottom: 24px;
}

/* vue-datepicker-next custom styles */
.custom-datepicker {
    width: 100%;
}

:deep(.mx-datepicker-range) {
    width: 100%;
}
:deep(.mx-input) {
    border: none;
    box-shadow: none;
    height: 40px;
    padding-left: 12px;
    font-size: 14px;
}

</style>


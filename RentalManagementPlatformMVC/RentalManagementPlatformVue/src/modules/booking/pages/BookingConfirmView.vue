<script setup>
import { onMounted, computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useBookingStore } from '@/stores/bookingStore';
import { useAuthStore } from '@/stores/auth';
import { fetchRoomDetail } from '@/api/roomSearchApi';
import { useToast } from 'vue-toastification';
import BookingPaymentsStepsCard from '../components/BookingPaymentsStepsCard.vue';
import BookingSummaryCard from '../components/BookingSummaryCard.vue';
import PriceSummary from '../components/PriceSummary.vue';

const bookingStore = useBookingStore();
const route = useRoute();
const router = useRouter();
const toast = useToast();
const auth = useAuthStore();

const isLoading = ref(true);
const isError = ref(false);

// 價格摘要元件引用
const priceSummary = ref(null);

// 檢查使用者是否已登入
if (!auth.isAuthenticated.value) {
  router.push({
    name: 'LoginView',
    query: { redirect: route.fullPath },
  });
}

onMounted(async () => {
  isLoading.value = true;
  const roomId = Number(route.query.roomId);

  if (roomId && !isNaN(roomId)) {
    bookingStore.clearBookingDraft();
    try {
      const roomDetail = await fetchRoomDetail(roomId);
      if (roomDetail) {
        // 再次確認使用者已登入且有profile資料
        if (!auth.state.profile?.userId) {
          toast.error('無法取得使用者資訊，請重新登入');
          router.push({ name: 'LoginView', query: { redirect: route.fullPath } });
          return;
        }

        // 取得今天日期作為入住日
        const checkInDate = new Date();

        // 退房為隔天上午 11 點
        const checkOutDate = new Date();
        checkOutDate.setDate(checkOutDate.getDate() + 1);
        const bookingData = {
          roomId: roomDetail.roomId,
          guestId: authStore.state.profile?.userId, // 暫時使用硬編碼的 userId = 1，直到會員模組完成
          guestCount: 1,
          roomTitle: roomDetail.title,
          roomImage:
            roomDetail.mainImageUrl || (roomDetail.photoUrls && roomDetail.photoUrls[0]) || '',
          pricePerNight: roomDetail.pricePerNight,
          checkIn: checkInDate.toISOString(),
          checkOut: checkOutDate.toISOString(),
        };
        bookingStore.setBookingDraft(bookingData);
      } else {
        throw new Error('找不到房源資料');
      }
    } catch (error) {
      toast.error(error.message || '載入房源資料失敗');
      isError.value = true;
    } finally {
      isLoading.value = false;
    }
  } else {
    if (!bookingStore.hasBookingDraft) {
      toast.error('訂房資料不存在，請重新選擇房源');
      isError.value = true;
      router.push({ name: 'home' });
    }
    isLoading.value = false;
  }
});

// 取得最終價格的計算值
const finalPrice = computed(() => priceSummary.value?.finalPrice || bookingStore.totalPrice);
</script>

<template>
  <div class="booking-confirm-page">
    <h1>確認預定</h1>

    <!-- Loading -->
    <div v-if="isLoading || bookingStore.isLoading" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p>{{ bookingStore.isLoading ? '正在處理您的訂單...' : '正在載入房源資訊...' }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="isError" class="error-message-container">
      <div class="error-card">
        <h2>無法載入頁面</h2>
        <p>抱歉，載入房源資訊時發生錯誤，或該房源不存在。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">返回首頁</button>
      </div>
    </div>

    <!-- Content -->
    <div v-else-if="bookingStore.hasBookingDraft" class="container">
      <!-- 左側:付款步驟 -->
      <BookingPaymentsStepsCard :total-price="finalPrice" />

      <!-- 右側:預訂摘要 -->
      <BookingSummaryCard>
        <template #price-summary>
          <PriceSummary ref="priceSummary" />
        </template>
      </BookingSummaryCard>
    </div>
  </div>
</template>

<style lang="scss" scoped>
// Mobile-first 設計：從最小螢幕開始設計，然後向上擴展
.booking-confirm-page {
  // Mobile (320px+)
  padding: 16px;
  min-height: 100vh;
  margin: 0 auto;

  h1 {
    font-size: 24px;
    font-weight: bold;
    margin-bottom: 16px;
    color: #222;
    text-align: center;
    padding: 0 8px;
  }

  // Small mobile (375px+)
  @media (min-width: 375px) {
    padding: 20px;

    h1 {
      font-size: 26px;
      margin-bottom: 20px;
    }
  }

  // Large mobile / Small tablet (576px+)
  @media (min-width: 576px) {
    max-width: 540px;

    h1 {
      font-size: 28px;
      text-align: left;
      margin-left: 10px;
    }
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    max-width: 720px;
  }

  // Large tablet / Small desktop (992px+)
  @media (min-width: 992px) {
    max-width: 960px;
  }

  // Desktop (1200px+)
  @media (min-width: 1200px) {
    max-width: 1024px;
  }
}

.container {
  // Mobile: 單欄布局
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: 100%;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    gap: 24px;
  }

  // Tablet (768px+): 開始使用雙欄布局
  @media (min-width: 768px) {
    display: grid;
    grid-template-columns: 1fr;
    gap: 32px;
  }

  // Large tablet (992px+): 真正的雙欄布局
  @media (min-width: 992px) {
    grid-template-columns: 1fr 350px;
    gap: 40px;
    align-items: start;
  }

  // Desktop (1200px+): 更寬的側邊欄
  @media (min-width: 1200px) {
    grid-template-columns: 1fr 400px;
    gap: 40px;
  }

  // Large desktop (1400px+): 調整比例
  @media (min-width: 1400px) {
    grid-template-columns: 1fr 420px;
    gap: 48px;
  }
}

/* Loading & Error Styles - Mobile-first */
.loading-overlay,
.error-message-container {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(248, 249, 250, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  // Mobile
  padding: 16px;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 20px;
  }
}

.loading-content,
.error-card {
  background: white;
  border-radius: 12px;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 320px;
  // Mobile
  padding: 24px 20px;

  p {
    margin-top: 12px;
    color: #333;
    font-size: 14px;
    font-weight: 500;
    line-height: 1.4;
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 32px 24px;
    border-radius: 16px;
    max-width: 350px;

    p {
      font-size: 16px;
      margin-top: 16px;
    }
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    padding: 40px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  }
}

.loading-spinner {
  // Mobile
  width: 40px;
  height: 40px;
  border: 3px solid #f0f0f0;
  border-top: 3px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    width: 50px;
    height: 50px;
    border: 4px solid #f0f0f0;
    border-top: 4px solid #007bff;
  }
}

.error-card {
  h2 {
    // Mobile
    font-size: 18px;
    font-weight: 600;
    color: #d9534f;
    margin-bottom: 12px;
    line-height: 1.3;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 20px;
      margin-bottom: 15px;
    }

    // Tablet (768px+)
    @media (min-width: 768px) {
      font-size: 22px;
    }
  }

  p {
    color: #484848;
    line-height: 1.5;

    // Tablet (768px+)
    @media (min-width: 768px) {
      line-height: 1.6;
    }
  }

  .btn-back-home {
    // Mobile
    margin-top: 16px;
    padding: 12px 20px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s;
    width: 100%;
    font-size: 14px;

    &:hover {
      background-color: #0056b3;
    }

    // Large mobile (480px+)
    @media (min-width: 480px) {
      margin-top: 20px;
      width: auto;
      min-width: 120px;
      font-size: 16px;
      padding: 10px 20px;
    }
  }
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>

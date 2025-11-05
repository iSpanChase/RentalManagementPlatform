<template>
  <div v-if="!authStore.shouldHideCouponFeature">
       <!-- 這是一個臨時的 div，用來確保 Tailwind JIT
      編譯器能生成我們需要的樣式，之後可以移除 -->
      <div class="hidden">
         <span class="bg-transparent border border-yellow-400
      text-yellow-400 hover:bg-yellow-400 hover:text-gray-900"></span>
        <span class="bg-gray-800 text-white border-gray-800"></span>
       <span class="bg-yellow-400 text-gray-900"></span>
  </div>
  <div class="p-4 sm:p-6 bg-gray-50 min-h-screen">
    <h1 class="text-2xl sm:text-3xl font-bold text-gray-800 mb-4">優惠券中心</h1>

<!-- Control Bar -->
<div class="mb-8 p-6 rounded-xl bg-white border border-gray-200 shadow-sm flex flex-row items-center justify-between gap-6 w-full border-2 border-red-500">
  <nav class="flex flex-wrap items-center gap-4 border-2 border-red-500" aria-label="Tabs">
    <button 
      v-for="tab in tabs" 
      :key="tab.key"
      @click="activeTab = tab.key as TabKey"
      class="tab-button"
      :class="{ 'is-active': activeTab === tab.key }"
    >
      {{ tab.name }} ({{ tab.count }})
    </button>
  </nav>

  <!-- Sorting and Search -->
  <div class="flex flex-wrap items-center gap-5 border-2 border-red-500">
    <div class="flex flex-wrap items-center gap-3">
      <button class="sort-button" @click="selectedSort = 'none'" :class="sortButtonClass('none')">預設排序</button>
      <button class="sort-button" @click="selectedSort = 'expiryDate'" :class="sortButtonClass('expiryDate')">到期日</button>
      <button class="sort-button" @click="selectedSort = 'discountAmount'" :class="sortButtonClass('discountAmount')">折扣金額</button>
    </div>
    <div class="relative">
      <input
        type="text"
        v-model="searchQuery"
        placeholder="搜尋優惠券..."
        class="w-56 sm:w-64 pl-4 pr-4 py-2 border rounded-md focus:ring-yellow-400 focus:border-yellow-400 bg-gray-800 text-white border-gray-700"
      />
    </div>
  </div>
</div>


    <!-- Coupon Grid -->
    <div class="mt-8">
      <div v-if="isLoadingUser || isLoadingPublic" class="text-center text-gray-500 py-10">載入中...</div>
      <div v-else-if="filteredCoupons.length === 0" class="text-center text-gray-400 py-10">此分類中沒有優惠券。</div>
      <div v-else>
        <CouponList
          :coupons="filteredCoupons"
          :userId="userId"
          @gotoUse="gotoUse"
          @showDetails="openModal"
        />
      </div>
    </div>

    <!-- Coupon Detail Modal -->
    <BaseModal :show="isModalOpen" @close="closeModal">
      <template #header>
        <h3 class="text-lg font-semibold">優惠券詳情</h3>
      </template>
      <template #body>
        <CouponDetail v-if="selectedCoupon" :coupon="selectedCoupon" />
      </template>
    </BaseModal>
  </div>
  </div>
  <div v-else class="text-center p-10">
    <h1 class="text-xl text-gray-600">此功能不適用於目前帳戶。</h1>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useCouponStore } from '@/stores/couponStore.js';
import { useQuery } from '@tanstack/vue-query';
import { getUserCoupons, getPublicCoupons } from '@/services/CouponService';
import CouponList from '@/components/coupons/CouponList.vue';
import BaseModal from '@/components/BaseModal.vue';
import CouponDetail from '@/components/coupons/CouponDetail.vue';
import type { Coupon } from '@/types/coupon';
import { useRouter } from 'vue-router';
import { faFilter, faSort, faSearch } from '@fortawesome/free-solid-svg-icons';

type TabKey = 'claimable' | 'usable' | 'used' | 'expired';
import { useAuthStore } from '@/stores/auth';

// --- Basic Setup ---
const authStore = useAuthStore();
const userId = computed(() => authStore.state.profile?.userId);const couponStore = useCouponStore();
const router = useRouter();
const searchQuery = ref('');

// --- Modal State ---
const isModalOpen = ref(false);
const selectedCoupon = ref<Coupon | null>(null);

function openModal(coupon: Coupon) {
  selectedCoupon.value = coupon;
  isModalOpen.value = true;
}

function closeModal() {
  isModalOpen.value = false;
}

// --- Data Fetching ---
const { data: userCoupons, isLoading: isLoadingUser } = useQuery<Coupon[]>({
  queryKey: ['userCoupons', userId],
  queryFn: async () => {
    console.log(`[CouponCenterView] Starting fetch for user coupons, userId: ${userId.value}`);
    try {
      const rawCoupons = await getUserCoupons();
      console.log('[CouponCenterView] Raw user coupons from API:', rawCoupons);
      const now = new Date();
      return rawCoupons.map((c: any) => ({
        ...c,
        isExpired: new Date(c.endAt) < now,
        isAvailable: new Date(c.endAt) >= now && c.status !== 'used',
      }));
    } catch (err) {
      console.error('[CouponCenterView] Error fetching user coupons:', err);
      throw err; // Re-throw the error so that useQuery can handle it
    }
  },
  enabled: computed(() => typeof userId.value === 'number'),
  initialData: [],
});

const { data: publicCoupons, isLoading: isLoadingPublic } = useQuery<Coupon[]>({
  queryKey: ['publicCoupons'],
  queryFn: async () => {
    const rawCoupons = await getPublicCoupons();
    const now = new Date();
    return rawCoupons.map((c: any) => ({
      ...c,
      isExpired: new Date(c.endAt) < now,
      isAvailable: new Date(c.endAt) >= now && !c.isRedeemed, // Assuming isRedeemed is on the raw data
    }));
  },
  initialData: [],
});

// --- Tab & Filtering Logic ---
const activeTab = ref<TabKey>('claimable');

const allCoupons = computed(() => {
  const userCouponIds = new Set(userCoupons.value.map(c => c.couponId));
  const now = new Date();
  const threeDaysLater = new Date(now.getTime() + 3 * 24 * 60 * 60 * 1000);

  const claimable = publicCoupons.value.filter(p => !userCouponIds.has(p.couponId) && new Date(p.endAt) > now);
  const usable = userCoupons.value.filter(c => (c.status === '可使用' || c.status === 'unused') && new Date(c.endAt) > now);
  const used = userCoupons.value.filter(c => c.status === '已使用' || c.status === 'used');
  const expired = userCoupons.value.filter(c => c.status === '已過期' || c.status === 'expired' || new Date(c.endAt) <= now);
  
  return { claimable, usable, used, expired };
});

const selectedSort = ref('none');

const sortOptions = [
  { key: 'none', name: '預設排序' },
  { key: 'expiryDate', name: '到期日' },
  { key: 'discountAmount', name: '折扣金額' }
];

const filteredCoupons = computed(() => {
  let coupons = allCoupons.value[activeTab.value] || [];

  // Apply sorting
  if (selectedSort.value === 'expiryDate') {
    coupons = [...coupons].sort((a, b) => new Date(a.endAt).getTime() - new Date(b.endAt).getTime());
  } else if (selectedSort.value === 'discountAmount') {
    coupons = [...coupons].sort((a, b) => b.discountQuota - a.discountQuota);
  }

  // Apply search filter
  if (searchQuery.value) {
    coupons = coupons.filter(coupon => 
      (coupon.couponName && coupon.couponName.toLowerCase().includes(searchQuery.value.toLowerCase())) ||
      (coupon.description && coupon.description.toLowerCase().includes(searchQuery.value.toLowerCase()))
    );
  }

  return coupons;
});

const tabs = computed(() => [
  { key: 'claimable' as TabKey, name: '可領取', count: allCoupons.value.claimable.length },
  { key: 'usable' as TabKey, name: '可使用', count: allCoupons.value.usable.length },
  { key: 'used' as TabKey, name: '已使用', count: allCoupons.value.used.length },
  { key: 'expired' as TabKey, name: '已過期', count: allCoupons.value.expired.length },
]);

const sortButtonClass = (sortType: 'none' | 'expiryDate' | 'discountAmount') => {
  return selectedSort.value === sortType ? 'is-selected' : '';
};

// --- Actions ---
function gotoUse(couponId: number) {
  console.log('Navigating to search...');
  router.push('/search');
}
</script>

<style scoped>
/* ===== 黑金高級感 Tabs & 排序 & 搜尋 ===== */
.tab-count {
  font-weight: 600;
  color: inherit; /* 跟 Tab 本身文字同色 */
  text-shadow: 0 0 6px rgba(212, 175, 55, 0.6);
}
.tab-button {
  background: linear-gradient(135deg, #f4e27b, #d4af37); /* 金色漸層 */
  color: #1f1f1f; /* 深色文字 */
  padding: 8px 16px;
  border-radius: 10px;
  font-weight: 600;
  transition: all 0.25s ease;
  cursor: pointer;
  border: 1px solid #d4af37;
}

.tab-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(212, 175, 55, 0.5);
}

.tab-button.is-active {
  box-shadow: 0 0 14px rgba(212, 175, 55, 0.7);
}

.sort-button {
  background: linear-gradient(135deg, #f4e27b, #d4af37);
  color: #1f1f1f;
  padding: 6px 14px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid #d4af37;
  transition: all 0.25s ease;
}

.sort-button:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(212, 175, 55, 0.5);
}

.sort-button.is-selected {
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(212, 175, 55, 0.5);
}

input[type="text"] {
  background-color: #1c1c1c;
  border: 1px solid #d4af37;
  color: #f4e27b;
  padding: 8px 14px;
  border-radius: 8px;
  transition: all 0.25s ease;
  width: 260px;
}

input[type="text"]::placeholder {
  color: #a0a0a0;
}

input[type="text"]:focus {
  outline: none;
  border-color: #f4e27b;
  box-shadow: 0 0 8px rgba(212, 175, 55, 0.4);
  background-color: #2a2a2a;
}

.coupon-card {
  background: linear-gradient(180deg, #3b3b3b 0%, #1f1f1f 100%); /* 黑灰漸層 */
  border-radius: 12px;
  border: 1px solid #d4af37; /* 金色邊框 */
  box-shadow: 0 4px 14px rgba(0, 0, 0, 0.4);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  font-family: 'Georgia', serif; /* 高貴字體 */
  width: 230px;
  position: relative;
  transition: transform 0.2s, box-shadow 0.2s;
}
.coupon-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 16px rgba(212, 175, 55, 0.25);
}

/* 圓孔造型 */
.coupon-card::before,
.coupon-card::after {
  content: '';
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 20px;
  height: 20px;
  background: #1f1f1f;
  border: 1px solid #d4af37;
  border-radius: 50%;
  z-index: 1;
}
.coupon-card::before { left: -10px; }
.coupon-card::after { right: -10px; }

/* 折扣區 */
.top-section {
  padding: 20px 16px 16px;
  text-align: center;
  display: flex;
  justify-content: center;
  align-items: baseline;
  gap: 8px;
  border-bottom: 1px dashed #d4af37;
  background-clip: text;
  color: #d4af37;
  text-shadow: 0 1px 2px rgba(0,0,0,0.5), 0 0 4px rgba(212,175,55,0.6);
}
.discount-value {
  font-size: 3rem;
  font-weight: bold;
  background: linear-gradient(45deg, #f4e27b, #d4af37);
  /* -webkit-background-clip: text; */
  -webkit-text-fill-color: transparent;
  text-shadow: 0 1px 2px rgba(0,0,0,0.5), 0 0 4px rgba(212,175,55,0.6);
}
.discount-unit {
  font-size: 1.25rem;
  font-weight: 600;
  background: linear-gradient(45deg, #f4e27b, #d4af37);
  /* -webkit-background-clip: text; */
  -webkit-text-fill-color: transparent;
  text-shadow: 0 1px 2px rgba(0,0,0,0.5), 0 0 4px rgba(212,175,55,0.6);
}

/* 內容區 */
.middle-section {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  border-bottom: 1px dashed #d4af37;
}
.coupon-name {
  font-size: 1.1rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
  background: linear-gradient(45deg, #f4e27b, #d4af37);
  /* -webkit-background-clip: text; */
  -webkit-text-fill-color: transparent;
  text-shadow: 0 1px 2px rgba(0,0,0,0.5), 0 0 4px rgba(212,175,55,0.6);
}
.icon { 
  color: #d4af37; 
}
.description {
  color: #ccc;
  font-size: 0.9rem;
}
.expiry-date {
  color: #999;
  font-size: 0.8rem;
}

/* 底部區塊 */
.bottom-section {
  padding: 16px;
  background: linear-gradient(180deg, #2a2a2a, #1c1c1c);
  margin-top: auto;
  border-top: 1px dashed #d4af37;
}

/* 按鈕 */
:deep(.redeem-style) {
  width: 100%;
  background: linear-gradient(145deg, #f4e27b, #d4af37);
  font-family: 'Georgia', serif;
  color: #1f1f1f;
  font-weight: bold;
  padding: 12px;
  border-radius: 8px;
  border: none;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.25);
}
:deep(.redeem-style:hover) {
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(212, 175, 55, 0.5);
}

/* 即將到期標籤 */
.expiring-soon-banner {
  position: absolute;
  top: 0;
  right: 0;
  background-color: #e53e3e;
  color: white;
  padding: 4px 8px;
  font-size: 0.75rem;
  font-weight: bold;
  border-top-right-radius: 12px;
  border-bottom-left-radius: 8px;
  z-index: 3;
}
</style>

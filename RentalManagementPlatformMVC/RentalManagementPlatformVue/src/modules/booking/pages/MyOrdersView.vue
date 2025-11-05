<script setup>
import { ref, onMounted, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { formatDate } from '@/composables/useBookingFormatters';
import { usePagination } from '@/composables/usePagination';
import SimplePaginator from '@/components/SimplePaginator.vue';
import StatusBadge from '@/components/StatusBadge.vue';

const bookingStore = useBookingStore();
const auth = useAuthStore();
const router = useRouter();
const toast = useToast();

const allOrders = ref([]);
const isLoading = ref(true);
const isError = ref(false);
const selectedOrder = ref(null);

// 篩選和排序狀態
const statusFilter = ref('');
const sortBy = ref('newest');
const searchKeyword = ref('');

// 篩選和排序後的訂單
const filteredAndSortedOrders = computed(() => {
  let filtered = [...allOrders.value];

  // 狀態篩選
  if (statusFilter.value) {
    filtered = filtered.filter((order) => order.paymentStatus === statusFilter.value);
  }

  // 關鍵字搜尋（房客名稱、房源名稱、訂單編號）
  if (searchKeyword.value.trim()) {
    const keyword = searchKeyword.value.toLowerCase().trim();
    filtered = filtered.filter(
      (order) =>
        order.guestName?.toLowerCase().includes(keyword) ||
        order.room?.toLowerCase().includes(keyword) ||
        order.orderNumber?.toLowerCase().includes(keyword) ||
        order.contactEmail?.toLowerCase().includes(keyword)
    );
  }

  // 排序
  filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'newest':
        return new Date(b.createdAt) - new Date(a.createdAt);
      case 'oldest':
        return new Date(a.createdAt) - new Date(b.createdAt);
      case 'checkin-asc':
        return new Date(a.checkIn) - new Date(b.checkIn);
      case 'checkin-desc':
        return new Date(b.checkIn) - new Date(a.checkIn);
      case 'amount-high':
        return b.totalPrice - a.totalPrice;
      case 'amount-low':
        return a.totalPrice - b.totalPrice;
      case 'guest-name':
        return (a.guestName || '').localeCompare(b.guestName || '');
      default:
        return 0;
    }
  });

  return filtered;
});

// 使用篩選後的資料進行分頁
const {
  currentPage,
  totalPages,
  paginatedItems: paginatedOrders,
  onPageChange,
} = usePagination(filteredAndSortedOrders, 5);

/**
 * 查看訂單詳情
 */
const viewOrderDetails = (orderNumber, orders) => {
  const order = orders.find((o) => o.orderNumber === orderNumber);
  if (order) {
    selectedOrder.value = order;
  }
};

/**
 * 清除所有篩選條件
 */
const clearFilters = () => {
  statusFilter.value = '';
  searchKeyword.value = '';
  sortBy.value = 'newest';
};

/**
 * 重新載入資料
 */
const reloadData = async () => {
  isLoading.value = true;
  isError.value = false;

  try {
    const hostId = auth.state.profile?.userId;
    if (!hostId) {
      toast.error('無法獲取房東資訊，請確認您的帳號是否為房東');
      isError.value = true;
      return;
    }

    const fetchedOrders = await bookingStore.fetchMyOrders(hostId);
    allOrders.value = fetchedOrders || [];

    if (allOrders.value.length === 0) {
      toast.info('目前沒有房客預訂記錄');
    }
  } catch (error) {
    console.error('載入訂單失敗:', error);
    toast.error(error.message || '載入訂單資料失敗');
    isError.value = true;
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  // 檢查使用者是否已登入
  if (!auth.isAuthenticated.value) {
    toast.error('請先登入查看訂單記錄');
    router.push({ name: 'LoginView' });
    return;
  }

  await reloadData();
});
</script>

<template>
  <div class="host-orders-page">
    <h1>房客預訂清單</h1>

    <!-- Loading -->
    <div v-if="isLoading || bookingStore.isLoading" class="loading-overlay">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p>{{ bookingStore.isLoading ? '正在處理訂單...' : '正在載入房客預訂資料...' }}</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="isError" class="error-message-container">
      <div class="error-card">
        <h2>無法載入頁面</h2>
        <p>抱歉，載入房客預訂資料時發生錯誤，請確認您的房東權限。</p>
        <button @click="reloadData()" class="btn-back-home">重新載入</button>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="allOrders.length === 0" class="error-message-container">
      <div class="error-card">
        <h2>尚無房客預訂</h2>
        <p>目前沒有任何房客預訂您的房源。</p>
        <button @click="router.push({ name: 'home' })" class="btn-back-home">前往房源管理</button>
      </div>
    </div>

    <!-- Content -->
    <div v-else class="container">
      <!-- 篩選和排序控制 -->
      <div class="filter-controls">
        <div class="filter-section">
          <div class="search-box">
            <input
              type="text"
              v-model="searchKeyword"
              placeholder="搜尋房客名稱、房源名稱、訂單編號或Email..."
              class="search-input"
            />
            <i class="fa-solid fa-search search-icon"></i>
            <button v-if="searchKeyword" @click="searchKeyword = ''" class="clear-btn">
              <i class="fa-solid fa-times"></i>
            </button>
          </div>

          <div class="filter-dropdowns">
            <select v-model="statusFilter" class="filter-select">
              <option value="">所有狀態</option>
              <option value="pending">待付款</option>
              <option value="deferred">延後付款</option>
              <option value="completed">已付款</option>
              <option value="cancelled">已取消</option>
              <option value="refunded">已退款</option>
            </select>

            <select v-model="sortBy" class="sort-select">
              <option value="newest">最新訂單</option>
              <option value="oldest">最舊訂單</option>
              <option value="checkin-asc">入住日期（近到遠）</option>
              <option value="checkin-desc">入住日期（遠到近）</option>
              <option value="amount-high">金額（高到低）</option>
              <option value="amount-low">金額（低到高）</option>
              <option value="guest-name">房客姓名（A-Z）</option>
            </select>
          </div>
        </div>

        <div class="results-info">
          <span class="results-count">
            找到 {{ filteredAndSortedOrders.length }} 筆房客訂單
            <span v-if="allOrders.length !== filteredAndSortedOrders.length">
              （共 {{ allOrders.length }} 筆）
            </span>
          </span>

          <button v-if="statusFilter || searchKeyword" @click="clearFilters" class="clear-all-btn">
            <i class="fa-solid fa-filter-circle-xmark"></i>
            清除篩選
          </button>
        </div>
      </div>

      <div class="orders-list">
        <div v-for="order in paginatedOrders" :key="order.orderNumber" class="order-card">
          <div class="guest-avatar-wrapper">
            <img
              :src="order.guestAvatarUrl || 'https://placehold.co/80x80/EBEBEB/717171?text=Guest'"
              alt="房客頭像"
              class="guest-avatar"
            />
          </div>

          <div class="order-details-wrapper">
            <div class="card-section top-section">
              <div class="guest-info">
                <h3>{{ order.guestName }} ({{ order.guestCount }}位)</h3>
              </div>
              <StatusBadge :status="order.paymentStatus" />
            </div>

            <div class="card-section mid-section">
              <div class="info-item date-info">
                <i class="fa-solid fa-calendar-days"></i>
                <span>{{ formatDate(order.checkIn) }} - {{ formatDate(order.checkOut) }}</span>
              </div>
              <div class="info-item room-info">
                <i class="fa-solid fa-house"></i>
                <span>{{ order.room }}</span>
              </div>
            </div>

            <div class="card-section bottom-section">
              <div class="price-preview">
                <strong>TWD {{ order.totalPrice.toLocaleString() }}</strong>
              </div>
              <button
                class="btn-details"
                data-bs-toggle="modal"
                data-bs-target="#orderDetailModal"
                @click="viewOrderDetails(order.orderNumber, allOrders)"
              >
                查看詳情 / 聯絡
              </button>
            </div>
          </div>
        </div>
      </div>

      <SimplePaginator
        :current-page="currentPage"
        :total-pages="totalPages"
        @page-changed="onPageChange"
      />
    </div>
  </div>

  <!-- 訂單詳情 Modal -->
  <Teleport to="body">
    <div
      class="modal fade"
      id="orderDetailModal"
      tabindex="-1"
      aria-labelledby="orderDetailModalLabel"
      aria-hidden="true"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content" v-if="selectedOrder">
          <div class="modal-header">
            <div class="modal-header-content">
              <img
                :src="
                  selectedOrder.guestAvatarUrl ||
                  'https://placehold.co/60x60/EBEBEB/717171?text=Guest'
                "
                alt="房客頭像"
                class="modal-avatar"
              />
              <div>
                <h5 class="modal-title" id="orderDetailModalLabel">
                  {{ selectedOrder.guestName }}
                </h5>
                <span class="modal-subtitle">訂單 #{{ selectedOrder.orderNumber }}</span>
              </div>
            </div>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
            ></button>
          </div>

          <div class="modal-body">
            <!-- 房客聯絡資訊（高亮） -->
            <div class="detail-section highlight">
              <h6><i class="fa-solid fa-user"></i> 房客聯絡人資訊</h6>
              <div class="detail-row">
                <p><strong>Email:</strong></p>
                <p>{{ selectedOrder.contactEmail }}</p>
              </div>
              <div class="detail-row">
                <p><strong>電話:</strong></p>
                <p>{{ selectedOrder.contactPhone }}</p>
              </div>
            </div>

            <hr />

            <!-- 預訂與入住細節 -->
            <div class="detail-section">
              <h6><i class="fa-solid fa-house"></i> 預訂與入住細節</h6>
              <div class="detail-row">
                <p><strong>房源名稱:</strong></p>
                <p>{{ selectedOrder.room }}</p>
              </div>
              <div class="detail-row">
                <p><strong>入住/退房:</strong></p>
                <p>
                  {{ formatDate(selectedOrder.checkIn) }} - {{ formatDate(selectedOrder.checkOut) }}
                </p>
              </div>
              <div class="detail-row">
                <p><strong>入住人數:</strong></p>
                <p>{{ selectedOrder.guestCount }} 位</p>
              </div>
              <div class="detail-row">
                <p><strong>訂單狀態:</strong></p>
                <p>
                  <StatusBadge :status="selectedOrder.paymentStatus" />
                </p>
              </div>
              <div class="detail-row" v-if="selectedOrder.contactNotes">
                <p><strong>房客備註:</strong></p>
                <p class="notes-text">{{ selectedOrder.contactNotes }}</p>
              </div>
            </div>

            <hr />

            <!-- 價格與款項 -->
            <div class="detail-section">
              <h6><i class="fa-solid fa-credit-card"></i> 價格與款項</h6>
              <div class="detail-row">
                <p><strong>總預訂金額:</strong></p>
                <p class="price-value">TWD {{ selectedOrder.totalPrice.toLocaleString() }}</p>
              </div>
              <div class="detail-row">
                <p><strong>您的淨收入:</strong></p>
                <p class="net-income-value">
                  <strong
                    >TWD {{ Math.round(selectedOrder.totalPrice * 0.85).toLocaleString() }}</strong
                  >
                  <span class="text-muted"> (15% 平台佣金)</span>
                </p>
              </div>
              <div class="detail-row">
                <p><strong>付款方式:</strong></p>
                <p>{{ selectedOrder.paymentTiming === 'full' ? '立即支付' : '延後支付' }}</p>
              </div>
              <div class="detail-row warning" v-if="selectedOrder.paymentStatus === 'deferred'">
                <p><strong>付款截止日:</strong></p>
                <p>{{ formatDate(selectedOrder.paymentDeadline) }}</p>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
            <button type="button" class="btn btn-chat">
              <i class="fa-solid fa-comments"></i> 聯絡房客
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
$primary-color: #222;
$secondary-color: #f7a800;
$chat-color: #008489;
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;

.host-orders-page {
  max-width: 1024px;
  margin: 0 auto;
  padding: 20px;
  min-height: 100vh;

  h1 {
    font-size: 28px;
    font-weight: bold;
    margin-left: 10px;
    margin-bottom: 20px;
    color: #222;
  }
}

.container {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
}

/* Loading & Error Styles */
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
  padding: 20px;
}

.loading-content,
.error-card {
  background: white;
  padding: 40px;
  border-radius: 16px;
  text-align: center;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  max-width: 350px;
  width: 90%;

  p {
    margin-top: 16px;
    color: #333;
    font-size: 16px;
    font-weight: 500;
  }
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #f0f0f0;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

.error-card {
  h2 {
    font-size: 22px;
    font-weight: 600;
    color: #d9534f;
    margin-bottom: 15px;
  }
  p {
    color: #484848;
    line-height: 1.6;
  }
  .btn-back-home {
    margin-top: 20px;
    padding: 10px 20px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s;
    &:hover {
      background-color: #0056b3;
    }
  }
}

.orders-list {
  display: grid;
  gap: 20px;

  .order-card {
    display: flex;
    align-items: center;
    background: white;
    border: 1px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
    padding: 20px;
    gap: 20px;
    transition: box-shadow 0.3s ease;

    &:hover {
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }

    .guest-avatar-wrapper {
      flex-shrink: 0;
      .guest-avatar {
        width: 80px;
        height: 80px;
        border-radius: 50%;
        object-fit: cover;
        border: 2px solid $border-color;
      }
    }

    .order-details-wrapper {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .card-section {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .top-section .guest-info h3 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: $primary-color;
    }

    .mid-section {
      flex-direction: column;
      gap: 8px;
      padding-bottom: 10px;
      border-bottom: 1px dashed $border-color;

      .info-item {
        display: flex;
        align-items: center;
        color: $text-dark;
        font-size: 15px;

        i {
          margin-right: 10px;
          color: $text-light;
          width: 16px;
          text-align: center;
        }

        &.date-info {
          font-weight: 500;
        }
        &.room-info {
          font-size: 14px;
          color: $text-light;
        }
      }
    }

    .bottom-section {
      align-items: center;
      .price-preview {
        font-size: 16px;
        font-weight: 600;
        color: $primary-color;
      }
    }
  }
}

.order-status {
  padding: 5px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
  white-space: nowrap;
}

.status-deferred,
.status-unpaid {
  background-color: #fff3cd;
  color: #856404;
}
.status-completed,
.status-paid {
  background-color: #d4edda;
  color: #155724;
}
.status-cancelled {
  background-color: #f8d7da;
  color: #721c24;
}
.status-refunded {
  background-color: #e2e3e5;
  color: #383d41;
}

.btn-details {
  background-color: $secondary-color;
  color: $primary-color;
  border: 1px solid darken($secondary-color, 10%);
  padding: 10px 18px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;

  &:hover {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 15%);
  }
}

.modal-content {
  border-radius: 15px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
}

.modal-header {
  padding: 20px 25px;
  border-bottom: 1px solid $border-color;
  align-items: center;

  .modal-header-content {
    display: flex;
    align-items: center;
    gap: 15px;
    flex-grow: 1;
  }

  .modal-avatar {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid $border-color;
  }

  .modal-title {
    font-weight: 600;
    color: $primary-color;
    margin-bottom: 2px;
  }

  .modal-subtitle {
    font-size: 13px;
    color: $text-light;
  }

  .btn-close {
    margin: -10px -10px -10px 10px;
  }
}

.modal-body {
  padding: 25px;
}

.detail-section {
  margin-bottom: 20px;
  h6 {
    font-size: 16px;
    font-weight: 700;
    margin-bottom: 15px;
    color: $primary-color;
    i {
      margin-right: 8px;
      color: $text-light;
    }
  }
}

.detail-section.highlight {
  background-color: #fffaf0;
  padding: 20px;
  border-radius: 10px;
  border: 1px solid $secondary-color;
  margin-top: -10px;

  h6 {
    border-bottom: 1px solid #feedcd;
    padding-bottom: 10px;
    i {
      color: $secondary-color;
    }
  }
}

.detail-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  margin-bottom: 10px;
  font-size: 14px;
  align-items: start;

  p {
    margin: 0;
    line-height: 1.6;
  }
  strong {
    color: $text-light;
    font-weight: 500;
  }

  .price-value {
    font-weight: 700;
    color: $primary-color;
    font-size: 15px;
  }

  .net-income-value {
    color: #155724;
    font-weight: 500;
    strong {
      color: #155724;
      font-weight: 700;
      font-size: 15px;
    }
    .text-muted {
      font-weight: normal;
      color: $text-light !important;
      font-size: 13px;
    }
  }

  .notes-text {
    background-color: #f7f7f7;
    padding: 10px;
    border-radius: 8px;
    color: $primary-color;
    white-space: pre-wrap;
    font-style: italic;
  }

  &.warning p:last-child {
    color: #e63946;
    font-weight: 600;
  }
}

hr {
  border-color: $border-color;
  margin: 25px 0;
}

.modal-footer {
  border-top: 1px solid $border-color;
  padding: 15px 25px;

  .btn-secondary {
    background-color: $text-light;
    border-color: $text-light;
    color: white;
    padding: 8px 20px;
    border-radius: 8px;
    &:hover {
      background-color: darken($text-light, 10%);
      border-color: darken($text-light, 10%);
    }
  }

  .btn-chat {
    background-color: $chat-color;
    color: white;
    border: 1px solid $chat-color;
    padding: 8px 20px;
    border-radius: 8px;
    font-weight: 600;
    transition: all 0.2s;
    i {
      margin-right: 6px;
    }
    &:hover {
      background-color: darken($chat-color, 10%);
      border-color: darken($chat-color, 10%);
    }
  }
}

@media (max-width: 768px) {
  .container {
    padding: 0 15px;
  }
  .host-orders-page {
    padding: 20px 0;
  }
  h1 {
    font-size: 24px;
    margin-bottom: 20px;
  }

  .orders-list .order-card {
    flex-direction: column;
    align-items: center;
    padding: 16px;
    gap: 15px;

    .order-details-wrapper {
      width: 100%;
    }

    .top-section {
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;
    }

    .bottom-section {
      flex-direction: column;
      align-items: stretch;
      gap: 10px;
      .price-preview {
        text-align: center;
      }
      .btn-details {
        width: 100%;
      }
    }
  }

  .modal-body {
    padding: 15px;
  }
  .detail-section.highlight {
    padding: 15px;
  }
  .detail-row {
    grid-template-columns: 90px 1fr;
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

// 篩選控制樣式
.filter-controls {
  background: white;
  border: 1px solid $border-color;
  border-radius: 16px;
  padding: 20px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  .filter-section {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 16px;

    @media (min-width: 768px) {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }
  }

  .search-box {
    position: relative;
    flex: 1;
    max-width: 500px; // 房東版搜尋框稍寬一些

    .search-icon {
      position: absolute;
      left: 12px;
      top: 50%;
      transform: translateY(-50%);
      color: $text-light;
      font-size: 14px;
      z-index: 1;
      pointer-events: none;
    }

    .search-input {
      width: 100%;
      padding: 12px 16px 12px 40px;
      border: 1px solid $border-color;
      border-radius: 8px;
      font-size: 14px;
      transition: border-color 0.2s;

      &:focus {
        outline: none;
        border-color: $secondary-color;
        box-shadow: 0 0 0 3px rgba(247, 168, 0, 0.1);
      }

      &::placeholder {
        color: $text-light;
      }
    }

    .clear-btn {
      position: absolute;
      right: 8px;
      top: 50%;
      transform: translateY(-50%);
      background: none;
      border: none;
      color: $text-light;
      cursor: pointer;
      padding: 4px;
      border-radius: 4px;
      font-size: 12px;
      z-index: 1;

      &:hover {
        background: #f0f0f0;
        color: $text-dark;
      }
    }
  }

  .filter-dropdowns {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;

    .filter-select,
    .sort-select {
      padding: 10px 12px;
      border: 1px solid $border-color;
      border-radius: 8px;
      background: white;
      font-size: 14px;
      color: $text-dark;
      cursor: pointer;
      transition: border-color 0.2s;
      min-width: 140px;

      &:focus {
        outline: none;
        border-color: $secondary-color;
      }

      &:hover {
        border-color: #bbb;
      }
    }
  }

  .results-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-top: 16px;
    border-top: 1px solid $border-color;
    flex-wrap: wrap;
    gap: 12px;

    .results-count {
      font-size: 14px;
      color: $text-light;

      span {
        color: $text-light;
        font-weight: normal;
      }
    }

    .clear-all-btn {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 6px 12px;
      background: #fff3cd;
      border: 1px solid #ffc107;
      border-radius: 6px;
      color: #856404;
      font-size: 13px;
      cursor: pointer;
      transition: all 0.2s;

      &:hover {
        background: #ffc107;
        color: white;
      }

      i {
        font-size: 14px;
      }
    }
  }
}

// 手機版響應式調整
@media (max-width: 768px) {
  .filter-controls {
    padding: 16px;

    .filter-dropdowns {
      .filter-select,
      .sort-select {
        flex: 1;
        min-width: auto;
      }
    }

    .results-info {
      flex-direction: column;
      align-items: flex-start;
      gap: 8px;
    }
  }
}
</style>

<!--
  注意：本元件使用 Font Awesome 圖示，需確保專案已引入：
  <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" rel="stylesheet">
-->

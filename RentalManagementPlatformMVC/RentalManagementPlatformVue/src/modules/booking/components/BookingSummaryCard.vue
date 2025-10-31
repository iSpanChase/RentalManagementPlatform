<script setup>
// 1. 移除 onMounted, onUnmounted，恢復原狀
import { ref, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { Modal } from 'bootstrap';
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css';

// ==================== Props ====================
defineProps({
  hideInternalTotal: {
    type: Boolean,
    default: false
  }
});

// ==================== Store ====================
const bookingStore = useBookingStore();

/** 是否有訂房草稿資料 */
const hasBookingData = computed(() => {
  return bookingStore.hasBookingDraft && bookingStore.bookingDraft !== null;
});

// 日期選擇器
const dateRange = ref();
const dateError = ref('');
const minDate = new Date();

// 客人人數
const newGuests = ref(1);

// ==================== Modal 關閉修復 (新增) ====================
// 2. 建立 Ref 來綁定隱藏的關閉按鈕
const dateModalCloser = ref(null);
const guestsModalCloser = ref(null);
// ======================================================

/**
 * 格式化日期為「2025年12月31日」
 * @param {string} dateStr - ISO 日期字串
 * @returns {string}
 */
const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

/**
 * 將 Date 物件轉為 'YYYY-MM-DD' 字串
 * @param {Date} date
 * @returns {string}
 */
const toISODateString = (date) => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

/** 初始化日期選擇器 */
const initDatePickers = () => {
  if (!hasBookingData.value) return;
  const { checkIn, checkOut } = bookingStore.bookingDraft;
  if (checkIn && checkOut) {
    dateRange.value = [new Date(checkIn), new Date(checkOut)];
  } else {
    dateRange.value = [];
  }
  dateError.value = '';
};

/** 初始化客人選擇器 */
const initGuestsPicker = () => {
  if (!hasBookingData.value) return;
  newGuests.value = bookingStore.bookingDraft.guestCount || 1;
};

/** 儲存變更後的日期 */
const handleChangeDates = () => {
  if (dateRange.value?.[0] && dateRange.value?.[1]) {
    const [newCheckIn, newCheckOut] = dateRange.value;
    bookingStore.bookingDraft.checkIn = toISODateString(newCheckIn);
    bookingStore.bookingDraft.checkOut = toISODateString(newCheckOut);
    dateError.value = '';

    // 3. 修改：不再呼叫 Modal.getInstance()
    //    改為點擊隱藏的關閉按鈕
    dateModalCloser.value?.click();

  } else {
    dateError.value = '請選擇有效的入住和退房日期';
  }
};

/** 清除日期 */
const handleClearDates = () => {
  dateRange.value = null;
  dateError.value = '';
};

/** 儲存變更後的客人數 */
const handleChangeGuests = () => {
  if (newGuests.value > 0) {
    bookingStore.bookingDraft.guestCount = newGuests.value;

    // 4. 修改：不再呼叫 Modal.getInstance()
    //    改為點擊隱藏的關閉按鈕
    guestsModalCloser.value?.click();
  }
};

/** 增加客人 */
const increaseGuests = () => newGuests.value++;

/** 減少客人 */
const decreaseGuests = () => {
  if (newGuests.value > 1) newGuests.value--;
};
</script>

<template>
  <div class="right-section">
    <div v-if="hasBookingData" class="summary-card">
      <div class="property-info">
        <router-link to="/">
          <img :src="bookingStore.bookingDraft.roomImage" alt="房源圖片" />
        </router-link>
        <div>
          <h4>{{ bookingStore.bookingDraft.roomTitle }}</h4>
        </div>
      </div>

      <hr />

      <div class="cancellation">
        <div v-if="bookingStore.isRefundable">
          <strong>可免費取消</strong>
          <p>
            {{ bookingStore.refundableDate }}前取消可以全額退款。
            <button type="button" class="full-cancellation" data-bs-toggle="modal" data-bs-target="#cancellationModal">
              完整政策
            </button>
          </p>
        </div>
        <div v-else>
          <strong>不可退款</strong>
          <p>
            此預訂在入住前7天內將無法退款。
            <button type="button" class="full-cancellation" data-bs-toggle="modal" data-bs-target="#cancellationModal">
              完整政策
            </button>
          </p>
        </div>
      </div>

      <hr />

      <div class="info-row">
        <strong>日期</strong>
        <button type="button" class="btn-edit" data-bs-toggle="modal" data-bs-target="#dateChangeModal" @click="initDatePickers">
          變更
        </button>
      </div>
      <div class="date-info">
        {{ formatDate(bookingStore.bookingDraft.checkIn) }} 至 {{ formatDate(bookingStore.bookingDraft.checkOut) }}
      </div>

      <hr />

      <div class="info-row">
        <strong>客人</strong>
        <button type="button" class="btn-edit" data-bs-toggle="modal" data-bs-target="#guestsChangeModal" @click="initGuestsPicker">
          變更
        </button>
      </div>
      <div>{{ bookingStore.bookingDraft.guestCount }} 名成人</div>

      <hr />

      <div class="price-section">
        <h4>價格詳情</h4>
        <div class="price-row">
          <span>{{ bookingStore.nights }} 晚 x ${{ bookingStore.bookingDraft.pricePerNight.toLocaleString() }} TWD</span>
          <span>${{ bookingStore.subtotal.toLocaleString() }} TWD</span>
        </div>



        <slot name="coupon"></slot>

        <hr />

        <div class="price-row total" v-if="!hideInternalTotal">
          <strong>總計 TWD</strong>
          <strong>${{ bookingStore.totalPrice.toLocaleString() }} TWD</strong>
        </div>
        <button class="btn-details" data-bs-toggle="modal" data-bs-target="#priceDetailsModal">
          價格明細
        </button>
      </div>

      <div class="special-offer" v-if="bookingStore.discountAmount > 0">
        <span class="icon"><i class="fa-solid fa-alarm-clock"></i></span>
        <p>
          特別優惠：節省 ${{ bookingStore.discountAmount.toLocaleString() }} TWD。
          這位房東為新 3 筆預訂提供折扣。
        </p>
      </div>
    </div>

    <div v-else class="summary-card placeholder-card">
      <div class="placeholder-content">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">載入中...</span>
        </div>
        <p class="mt-3 text-muted">處理訂單中...</p>
      </div>
    </div>
  </div>

  <Teleport to="body">
    <div class="modal fade" id="cancellationModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">《取消政策》</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="policy-section">
              <div class="policy-item">
                <div class="policy-inner-item1">
                  <strong class="policy-label">此時間之前：</strong>
                  <p class="policy-date">{{ bookingStore.refundableDate }}</p>
                </div>
                <div class="policy-inner-item2">
                  <p>可獲得全額退款</p>
                  <p class="policy-description">你將拿回已支付的全額退款</p>
                </div>
              </div>
              <hr class="policy-divider" />
              <div class="policy-item">
                <div class="policy-inner-item1">
                  <strong class="policy-label">此時間之後：</strong>
                  <p class="policy-date">{{ bookingStore.refundableDate }}</p>
                </div>
                <div class="policy-inner-item2">
                  <p>無法獲得退款</p>
                  <p class="policy-description">這筆訂單不可退款</p>
                </div>
              </div>
            </div>
            <div class="policy-footer">
              <p class="policy-note">時間是根據房源當地時間顯示。</p>
              <div class="refund-info">
                <h6>退款資格</h6>
                <p>若選擇排定付款，退款或應付金額將取決於你在退訂時已支付的金額。</p>
              </div>
              <a href="#" class="policy-link">如何查看退款政策</a>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="modal fade" id="dateChangeModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">變更日期</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div v-if="dateError" class="alert alert-danger" role="alert">{{ dateError }}</div>
            <div class="mb-3">
              <label class="form-label">入住 - 退房日期</label>
              <VueDatePicker
                v-model="dateRange"
                range
                :enable-time-picker="false"
                :min-date="minDate"
                min-range="1"
                placeholder="請選擇入住和退房日期"
                auto-apply
                format="yyyy/MM/dd"
                :clearable="false"
                input-class-name="form-control"
                locale="zh-TW"
                :teleport-center="true"
              />
            </div>
          </div>
          <div class="modal-footer">
            <button
              ref="dateModalCloser"
              type="button"
              data-bs-dismiss="modal"
              style="display: none;"
            ></button>
            <div class="date-clear" @click="handleClearDates">清除日期</div>
            <button type="button" class="btn date-save" @click="handleChangeDates">儲存</button>
          </div>
        </div>
      </div>
    </div>

    <div class="modal fade" id="guestsChangeModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">變更入住人數</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3 d-flex align-items-center justify-content-between">
              <div>
                <div><span>成人</span></div>
                <div><small>年滿13歲</small></div>
              </div>
              <div class="d-flex align-items-center gap-3">
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  @click="decreaseGuests"
                  :disabled="newGuests <= 1"
                >-</button>
                <span class="fw-bold guest-count">{{ newGuests }}</span>
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  @click="increaseGuests"
                >+</button>
              </div>
            </div>
            <div class="guest-limit-text">
              <small>此住宿地點最多可容納 4 名房客 (未含嬰幼兒)。不允許攜帶寵物。</small>
            </div>
          </div>
          <div class="modal-footer">
            <button
              ref="guestsModalCloser"
              type="button"
              data-bs-dismiss="modal"
              style="display: none;"
            ></button>
            <div class="customer-clear" @click="initGuestsPicker">清除</div>
            <button type="button" class="btn customer-save" @click="handleChangeGuests">儲存</button>
          </div>
        </div>
      </div>
    </div>

    <div class="modal fade" id="priceDetailsModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">價格明細</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body" v-if="hasBookingData">

            <slot name="price-details-body">
              <div class="price-row">
                <span>{{ bookingStore.nights }} 晚 x ${{ bookingStore.bookingDraft.pricePerNight.toLocaleString() }} TWD</span>
                <span>${{ bookingStore.subtotal.toLocaleString() }} TWD</span>
              </div>

              <div class="price-row discount" v-if="bookingStore.discountAmount > 0">
                <span>特別優惠</span>
                <span class="green">-${{ bookingStore.discountAmount.toLocaleString() }} TWD</span>
              </div>

              <hr>

              <div class="price-row total">
                <strong>總計 TWD</strong>
                <strong>${{ bookingStore.totalPrice.toLocaleString() }} TWD</strong>
              </div>
            </slot>

          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
// 你的 SCSS 樣式 (保持不變)
$border-color: #ddd;
$divider-color: #ebebeb;
$bg-muted: #f7f7f7;
$text-muted: #717171;
$text-dark: #222;
$primary: #222;
$accent: #008489;

.right-section {
  position: sticky;
  top: 80px;
  height: fit-content;
}

.summary-card {
  border: 1px solid $border-color;
  border-radius: 25px;
  padding: 24px;
  background: white;
}

.placeholder-card {
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
  .placeholder-content {
    text-align: center;
    p { margin: 0; font-size: 14px; }
  }
}

.property-info {
  display: flex;
  gap: 16px;
  margin-bottom: 20px;
  img {
    width: 30rem;
    height: 7rem;
    border-radius: 8px;
    object-fit: cover;
  }
  h4 {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
    color: $text-dark;
  }
}

.cancellation {
  margin: 16px 0;
  strong { display: block; margin-bottom: 4px; }
  p { font-size: 14px; color: $text-muted; margin: 0; }
  .full-cancellation {
    background: none;
    border: none;
    text-decoration: underline;
    cursor: pointer;
    font-size: 14px;
    color: $text-dark;
  }
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  .btn-edit {
    background: none;
    border: none;
    text-decoration: underline;
    cursor: pointer;
    font-size: 14px;
    color: $text-dark;
  }
}

.date-info {
  color: $text-muted;
  font-size: 14px;
  margin-bottom: 16px;
}

.price-section {
  margin-top: 20px;
  h4 { font-size: 18px; margin-bottom: 16px; }
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 14px;
  &.discount .green { color: $accent; }
  &.total {
    font-size: 16px;
    padding-top: 12px;
    font-weight: 600;
  }
}

.btn-details {
  width: 100%;
  padding: 12px;
  background: white;
  border: 1px solid $primary;
  border-radius: 8px;
  cursor: pointer;
  margin-top: 16px;
  font-weight: 600;
  &:hover { background-color: #f7f7f7; }
}

.special-offer {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  padding: 12px;
  background: $bg-muted;
  border-radius: 8px;
  font-size: 14px;
  .icon i { color: $accent; }
  p { margin: 0; line-height: 1.5; }
}

hr {
  border: none;
  border-top: 1px solid $divider-color;
  margin: 16px 0;
}

.modal-content {
  border-radius: 25px;
  .modal-title {
    width: 100%;
    text-align: center;
    font-weight: 600;
  }
  .modal-body { padding: 24px; }
  .modal-footer {
    display: flex;
    justify-content: space-between;
    padding: 16px 24px;
  }
}

.policy-item {
  padding: 10px 0;
  display: flex;
  .policy-inner-item2 { margin-left: 4rem; }
  .policy-label { color: $text-dark; font-size: 16px; margin-bottom: 8px; }
  .policy-date { color: $text-muted; font-size: 14px; margin-bottom: 8px; }
  .policy-description { color: $accent; font-size: 14px; font-weight: 500; margin: 0; }
}

.policy-divider {
  border: none;
  border-top: 1px solid $bg-muted;
  margin: 10px 0;
}

.policy-footer {
  background-color: $bg-muted;
  padding: 16px;
  border-radius: 8px;
  margin-top: 16px;
  .policy-note { color: $text-muted; font-size: 12px; margin-bottom: 16px; text-align: start; }
  .refund-info h6 { color: $text-dark; font-size: 14px; font-weight: 600; margin-bottom: 8px; }
  .refund-info p { color: $text-muted; font-size: 13px; line-height: 1.4; margin: 0; }
  .policy-link {
    color: $accent;
    font-size: 13px;
    text-decoration: underline;
    display: block;
    text-align: start;
    &:hover { color: darken($accent, 10%); }
  }
}

.date-save, .customer-save {
  background-color: $primary;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 32px;
  font-weight: 600;
  &:hover { background-color: #000; }
}

.date-clear, .customer-clear {
  color: $text-dark;
  cursor: pointer;
  border-radius: 8px;
  padding: 8px 16px;
  &:hover { background-color: #e0e0e0; }
}

.guest-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid $border-color;
  background: white;
  font-size: 18px;
  font-weight: bold;
  &:hover:not(:disabled) {
    background-color: #f8f9fa;
    border-color: #adb5bd;
  }
  &:disabled {
    opacity: 0.3;
    cursor: not-allowed;
  }
}

.guest-count {
  min-width: 20px;
  text-align: center;
  font-size: 16px;
  font-weight: 600;
}

.guest-limit-text {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid $divider-color;
  color: $text-muted;
  font-size: 13px;
}

@media (max-width: 768px) {
  .right-section { position: static; }
  .property-info {
    flex-direction: column;
    img { width: 100%; height: auto; }
  }
}

:deep(.dp__menu) {
  z-index: 1060 !important;
}
</style>

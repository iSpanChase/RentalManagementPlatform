<script setup>
import { ref, computed, onMounted } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
import { Modal } from 'bootstrap'

// ==================== Store ====================
const bookingStore = useBookingStore()

// ==================== 計算屬性：檢查是否有訂房資料 ====================
const hasBookingData = computed(() => {
  return bookingStore.hasBookingDraft && bookingStore.bookingDraft !== null
})

// ==================== 日期相關 ====================
const newCheckIn = ref('')
const newCheckOut = ref('')
const dateError = ref('')

// 格式化日期
const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

// 初始化日期選擇器
const initDatePickers = () => {
  if (!hasBookingData.value) return

  newCheckIn.value = bookingStore.bookingDraft.checkIn
  newCheckOut.value = bookingStore.bookingDraft.checkOut
  dateError.value = ''
}

// 處理變更日期
const handleChangeDates = () => {
  if (newCheckIn.value && newCheckOut.value) {
    bookingStore.bookingDraft.checkIn = newCheckIn.value
    bookingStore.bookingDraft.checkOut = newCheckOut.value
    dateError.value = ''

    // 關閉 modal
    const modal = Modal.getInstance(document.getElementById('dateChangeModal'))
    if (modal) modal.hide()
  } else {
    dateError.value = '請選擇有效的入住和退房日期'
  }
}

// 清除日期
const handleClearDates = () => {
  newCheckIn.value = ''
  newCheckOut.value = ''
  dateError.value = ''
}

// ==================== 客人人數相關 ====================
const newGuests = ref(1)

// 初始化客人選擇器
const initGuestsPicker = () => {
  if (!hasBookingData.value) return

  newGuests.value = bookingStore.bookingDraft.guestCount || 1
}

// 處理變更客人
const handleChangeGuests = () => {
  if (newGuests.value > 0) {
    bookingStore.bookingDraft.guestCount = newGuests.value

    // 關閉 modal
    const modal = Modal.getInstance(document.getElementById('guestsChangeModal'))
    if (modal) modal.hide()
  }
}

// 增加客人數
const increaseGuests = () => {
  newGuests.value++
}

// 減少客人數
const decreaseGuests = () => {
  if (newGuests.value > 1) {
    newGuests.value--
  }
}

// ==================== 開發測試：自動載入資料 ====================
onMounted(() => {
  // 設定訂房草稿
  bookingStore.setBookingDraft({
    roomId: 1,
    guestId: 1,
    couponId: null,
    checkIn: '2025-12-31',
    checkOut: '2026-01-02',
    guestCount: 2,
    roomTitle: '🏠Sonoya客用住房，安靜的satoyama旅館，每日可供一組客人私人租...',
    roomImage: 'https://picsum.photos/120/90',
    pricePerNight: 3598,
    coupon: {
      discountAmount: 200,
    }
  })

  console.log('測試訂房資料已載入')
  console.log('bookingDraft:', bookingStore.bookingDraft)
})
</script>

<template>
  <div class="right-section">
    <!-- 有訂房資料時顯示完整卡片 -->
    <div v-if="hasBookingData" class="summary-card">
      <!-- ==================== 房源資訊 ==================== -->
      <div class="property-info">
        <router-link>
          <img :src="bookingStore.bookingDraft.roomImage" alt="房源圖片">
        </router-link>
        <div>
          <h4>{{ bookingStore.bookingDraft.roomTitle }}</h4>
        </div>
      </div>

      <hr>

      <!-- ==================== 取消政策 ==================== -->
      <div class="cancellation">
        <strong>可免費取消</strong>
        <p>
          {{ bookingStore.refundableDate }}前取消可以全額退款。
          <button
            type="button"
            class="full-cancellation"
            data-bs-toggle="modal"
            data-bs-target="#cancellationModal"
          >
            完整政策
          </button>
        </p>
      </div>

      <hr>

      <!-- ==================== 日期資訊 ==================== -->
      <div class="info-row">
        <strong>日期</strong>
        <button
          type="button"
          class="btn-edit"
          data-bs-toggle="modal"
          data-bs-target="#dateChangeModal"
          @click="initDatePickers"
        >
          變更
        </button>
      </div>
      <div class="date-info">
        {{ formatDate(bookingStore.bookingDraft.checkIn) }} 至
        {{ formatDate(bookingStore.bookingDraft.checkOut) }}
      </div>

      <hr>

      <!-- ==================== 人數資訊 ==================== -->
      <div class="info-row">
        <strong>客人</strong>
        <button
          type="button"
          class="btn-edit"
          data-bs-toggle="modal"
          data-bs-target="#guestsChangeModal"
          @click="initGuestsPicker"
        >
          變更
        </button>
      </div>
      <div>{{ bookingStore.bookingDraft.guestCount }} 名成人</div>

      <hr>

      <!-- ==================== 價格詳情 ==================== -->
      <div class="price-section">
        <h4>價格詳情</h4>

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

        <button
          class="btn-details"
          data-bs-toggle="modal"
          data-bs-target="#priceDetailsModal"
        >
          價格明細
        </button>
      </div>

      <!-- ==================== 特別優惠 ==================== -->
      <div class="special-offer" v-if="bookingStore.discountAmount > 0">
        <span class="icon"><i class="fa-solid fa-alarm-clock"></i></span>
        <p>
          特別優惠：節省 ${{ bookingStore.discountAmount.toLocaleString() }} TWD。
          這位房東為新 3 筆預訂提供折扣。
        </p>
      </div>
    </div>

    <!-- 沒有訂房資料時顯示載入中 -->
    <div v-else class="summary-card placeholder-card">
      <div class="placeholder-content">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">載入中...</span>
        </div>
        <p class="mt-3 text-muted">處理訂單中...</p>
      </div>
    </div>
  </div>

  <!-- ==================== Modal 區塊 ==================== -->
  <Teleport to="body">
    <!-- Modal 1: 取消政策 -->
    <div class="modal fade" id="cancellationModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">《取消政策》</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>

          <div class="modal-body">
            <div class="policy-section">
              <!-- 可退款條款 -->
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

              <hr class="policy-divider">

              <!-- 不可退款條款 -->
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

            <!-- 政策說明 -->
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

    <!-- Modal 2: 日期變更 -->
    <div class="modal fade" id="dateChangeModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">變更日期</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>

          <div class="modal-body">
            <!-- 錯誤訊息 -->
            <div v-if="dateError" class="alert alert-danger" role="alert">
              {{ dateError }}
            </div>

            <!-- 入住日期 -->
            <div class="mb-3">
              <label class="form-label">入住日期</label>
              <input
                type="date"
                class="form-control"
                v-model="newCheckIn"
                :min="new Date().toISOString().split('T')[0]"
              >
            </div>

            <!-- 退房日期 -->
            <div class="mb-3">
              <label class="form-label">退房日期</label>
              <input
                type="date"
                class="form-control"
                v-model="newCheckOut"
                :min="newCheckIn"
              >
            </div>
          </div>

          <div class="modal-footer">
            <div class="date-clear" @click="handleClearDates">清除日期</div>
            <button type="button" class="btn date-save" @click="handleChangeDates">儲存</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal 3: 人數變更 -->
    <div class="modal fade" id="guestsChangeModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">變更入住人數</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>

          <div class="modal-body">
            <!-- 成人 -->
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
                >
                  -
                </button>
                <span class="fw-bold guest-count">{{ newGuests }}</span>
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  @click="increaseGuests"
                >
                  +
                </button>
              </div>
            </div>

            <!-- 兒童 -->
            <div class="mb-3 d-flex align-items-center justify-content-between">
              <div>
                <div><span>兒童</span></div>
                <div><small>2-12歲</small></div>
              </div>
              <div class="d-flex align-items-center gap-3">
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  disabled
                >
                  -
                </button>
                <span class="fw-bold guest-count">0</span>
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  disabled
                >
                  +
                </button>
              </div>
            </div>

            <!-- 嬰幼兒 -->
            <div class="mb-3 d-flex align-items-center justify-content-between">
              <div>
                <div><span>嬰幼兒</span></div>
                <div><small>未滿2歲</small></div>
              </div>
              <div class="d-flex align-items-center gap-3">
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  disabled
                >
                  -
                </button>
                <span class="fw-bold guest-count">0</span>
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  disabled
                >
                  +
                </button>
              </div>
            </div>

            <!-- 寵物 -->
            <div class="mb-3 d-flex align-items-center justify-content-between">
              <div>
                <div><span>寵物</span></div>
                <div><small>需要帶服務性動物嗎？</small></div>
              </div>
              <div class="d-flex align-items-center gap-3">
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  disabled
                >
                  -
                </button>
                <span class="fw-bold guest-count">0</span>
                <button
                  type="button"
                  class="btn btn-outline-secondary rounded-circle guest-btn"
                  disabled
                >
                  +
                </button>
              </div>
            </div>

            <!-- 客人限制說明 -->
            <div class="guest-limit-text">
              <small>此住宿地點最多可容納 4 名房客 (未含嬰幼兒)。不允許攜帶寵物。</small>
            </div>
          </div>

          <div class="modal-footer">
            <div class="customer-clear" @click="initGuestsPicker">清除</div>
            <button type="button" class="btn customer-save" @click="handleChangeGuests">儲存</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal 4: 價格明細 -->
    <div class="modal fade" id="priceDetailsModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">價格明細</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>

          <div class="modal-body" v-if="hasBookingData">
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
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
// ==================== 主要區塊 ====================
.right-section {
  position: sticky;
  top: 80px;
  height: fit-content;
}

.summary-card {
  border: 1px solid #ddd;
  border-radius: 25px;
  padding: 24px;
}

// 佔位卡片樣式
.placeholder-card {
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;

  .placeholder-content {
    text-align: center;

    p {
      margin: 0;
      font-size: 14px;
    }
  }
}

// ==================== 房源資訊 ====================
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
}

// ==================== 取消政策 ====================
.cancellation {
  margin: 16px 0;

  strong {
    display: block;
    margin-bottom: 4px;
  }

  p {
    font-size: 14px;
    color: #717171;
  }

  .full-cancellation {
    background: none;
    border: none;
    text-decoration: underline;
    cursor: pointer;
    font-size: 14px;
  }
}

// ==================== 資訊列 ====================
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
  }
}

.date-info {
  color: #717171;
  font-size: 14px;
  margin-bottom: 16px;
}

// ==================== 價格區塊 ====================
.price-section {
  margin-top: 20px;
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;

  &.discount {
    .green {
      color: #008489;
    }
  }

  &.total {
    font-size: 16px;
    padding-top: 12px;
  }
}

.btn-details {
  width: 100%;
  padding: 12px;
  background: white;
  border: 1px solid #222;
  border-radius: 8px;
  cursor: pointer;
  margin-top: 16px;

  &:hover {
    background-color: #f7f7f7;
  }
}

// ==================== 特別優惠 ====================
.special-offer {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  padding: 12px;
  background: #f7f7f7;
  border-radius: 8px;
  font-size: 14px;

  p {
    margin: 0;
    line-height: 1.5;
  }
}

// ==================== 分隔線 ====================
hr {
  border: none;
  border-top: 1px solid #ebebeb;
  margin: 16px 0;
}

// ==================== Modal 樣式 ====================
.modal-content {
  border-radius: 25px;

  .modal-title {
    width: 100%;
    text-align: center;
  }

  .modal-body {
    padding: 24px;
  }

  .modal-footer {
    display: flex;
    justify-content: space-between;
    padding: 16px 24px;
  }

  // 取消政策 Modal
  .policy-section {
    margin-bottom: 10px;
  }

  .policy-item {
    padding: 10px 0;
    display: flex;

    .policy-inner-item2 {
      margin-left: 4rem;
    }

    .policy-label {
      display: block;
      color: #222;
      font-size: 16px;
      margin-bottom: 8px;
    }

    .policy-date {
      color: #717171;
      font-size: 14px;
      margin-bottom: 8px;
    }

    .policy-description {
      color: #008489;
      font-size: 14px;
      font-weight: 500;
      margin: 0;
    }
  }

  .policy-divider {
    border: none;
    border-top: 1px solid #f7f7f7;
    margin: 10px 0;
  }

  .policy-footer {
    background-color: #f7f7f7;
    padding: 16px;
    border-radius: 8px;
    margin-top: 16px;

    .policy-note {
      color: #717171;
      font-size: 12px;
      margin-bottom: 16px;
      text-align: start;
    }

    .refund-info {
      margin-bottom: 16px;

      h6 {
        color: #222;
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 8px;
      }

      p {
        color: #717171;
        font-size: 13px;
        line-height: 1.4;
        margin: 0;
      }
    }

    .policy-link {
      color: #008489;
      font-size: 13px;
      text-decoration: underline;
      display: block;
      text-align: start;

      &:hover {
        color: #00696d;
      }
    }
  }

  // 日期變更 Modal
  .date-save {
    background-color: #222;
    color: white;
    border: none;
    border-radius: 8px;
    padding: 8px 32px;

    &:hover {
      background-color: #000;
    }
  }

  .date-clear {
    color: #222;
    cursor: pointer;
    border-radius: 8px;
    padding: 8px 16px;

    &:hover {
      background-color: #e0e0e0;
    }
  }

  // 客人變更 Modal
  .customer-save {
    background-color: #222;
    color: white;
    border: none;
    border-radius: 8px;
    padding: 8px 32px;

    &:hover {
      background-color: #000;
    }
  }

  .customer-clear {
    color: #222;
    cursor: pointer;
    border-radius: 8px;
    padding: 8px 16px;

    &:hover {
      background-color: #e0e0e0;
    }
  }

  .guest-btn {
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #ddd;
    background: white;

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
  }

  .guest-limit-text {
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid #ebebeb;
    color: #717171;
  }
}

// ==================== 響應式設計 ====================
@media (max-width: 768px) {
  .right-section {
    position: static;
  }

  .property-info {
    img {
      width: 100%;
      height: auto;
    }
  }
}
</style>

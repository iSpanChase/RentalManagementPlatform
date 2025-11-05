<script setup>
import { ref, computed } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { formatDate, formatPrice } from '@/composables/useBookingFormatters';
import DateGuestEditorModal from './DateGuestEditorModal.vue';

// ==================== Store ====================
const bookingStore = useBookingStore();

/** 是否有訂房草稿資料 */
const hasBookingData = computed(() => {
  return bookingStore.hasBookingDraft && bookingStore.bookingDraft !== null;
});

// 日期和客人數編輯器引用
const dateGuestEditor = ref(null);
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
        <div v-else>
          <strong>不可退款</strong>
          <p>
            此預訂在入住前7天內將無法退款。
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
      </div>

      <hr />

      <div class="info-row">
        <strong>日期</strong>
        <button
          type="button"
          class="btn-edit"
          data-bs-toggle="modal"
          data-bs-target="#dateChangeModal"
          @click="dateGuestEditor?.initDatePickers"
        >
          變更
        </button>
      </div>
      <div class="date-info">
        <strong>{{ formatDate(bookingStore.bookingDraft.checkIn, 'checkIn') }}</strong> 至
        <strong>{{ formatDate(bookingStore.bookingDraft.checkOut, 'checkOut') }}</strong>
      </div>

      <hr />

      <div class="info-row">
        <strong>客人</strong>
        <button
          type="button"
          class="btn-edit"
          data-bs-toggle="modal"
          data-bs-target="#guestsChangeModal"
          @click="dateGuestEditor?.initGuestsPicker"
        >
          變更
        </button>
      </div>
      <div>{{ bookingStore.bookingDraft.guestCount }} 名成人</div>

      <hr />

      <div class="price-section">
        <!-- 使用價格摘要元件 -->
        <slot name="price-summary"></slot>
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

  <!-- 日期和客人數編輯器 -->
  <DateGuestEditorModal ref="dateGuestEditor" />

  <Teleport to="body">
    <div
      class="modal fade"
      id="cancellationModal"
      tabindex="-1"
      aria-labelledby="cancellationModalLabel"
      aria-hidden="true"
      data-bs-backdrop="true"
      data-bs-keyboard="true"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="cancellationModalLabel">《取消政策》</h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="關閉"
            ></button>
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
  </Teleport>
</template>

<style lang="scss" scoped>
$border-color: #ddd;
$divider-color: #ebebeb;
$bg-muted: #f7f7f7;
$text-muted: #717171;
$text-dark: #222;
$primary: #222;
$accent: #008489;

// Mobile-first 設計：從最小螢幕開始設計，然後向上擴展
.right-section {
  // Mobile: 不使用 sticky，正常流動
  position: static;
  height: auto;

  // Tablet (768px+): 使用 sticky
  @media (min-width: 768px) {
    position: sticky;
    top: 80px;
    height: fit-content;
  }
}

.summary-card {
  border: 1px solid $border-color;
  background: white;
  // Mobile
  border-radius: 12px;
  padding: 16px;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    border-radius: 16px;
    padding: 20px;
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    padding: 24px;
  }

  // Desktop (992px+)
  @media (min-width: 992px) {
    border-radius: 25px;
  }
}

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

.property-info {
  display: flex;
  margin-bottom: 16px;
  // Mobile: 垂直排列
  flex-direction: column;
  gap: 12px;

  img {
    border-radius: 8px;
    object-fit: cover;
    // Mobile: 全寬
    width: 100%;
    height: 150px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      height: 120px;
    }
  }

  h4 {
    margin: 0;
    font-weight: 600;
    color: $text-dark;
    // Mobile
    font-size: 16px;
    line-height: 1.3;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 17px;
    }
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    flex-direction: row;
    gap: 16px;
    margin-bottom: 20px;

    img {
      width: 120px;
      height: 80px;
      flex-shrink: 0;
    }
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    img {
      width: 140px;
      height: 90px;
    }
  }

  // Desktop (992px+)
  @media (min-width: 992px) {
    img {
      width: 160px;
      height: 100px;
    }
  }
}

.cancellation {
  margin: 16px 0;

  strong {
    display: block;
    margin-bottom: 4px;
    // Mobile
    font-size: 15px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 16px;
    }
  }

  p {
    color: $text-muted;
    margin: 0;
    line-height: 1.4;
    // Mobile
    font-size: 14px;
  }

  .full-cancellation {
    background: none;
    border: none;
    text-decoration: underline;
    cursor: pointer;
    color: $text-dark;
    // Mobile
    font-size: 14px;
    padding: 2px 0;
    min-height: 32px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      min-height: auto;
      padding: 0;
    }
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
    color: $text-dark;
    // Mobile
    font-size: 14px;
    padding: 4px 8px;
    min-height: 32px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      padding: 0;
      min-height: auto;
    }
  }
}

.date-info {
  color: $text-muted;
  margin-bottom: 16px;
  line-height: 1.4;
  // Mobile
  font-size: 14px;
}

.price-section {
  // Mobile
  margin-top: 16px;

  h4 {
    margin-bottom: 16px;
    // Mobile
    font-size: 17px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 18px;
    }
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    margin-top: 20px;
  }
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 14px;
  &.discount .green {
    color: $accent;
  }
  &.total {
    font-size: 16px;
    padding-top: 12px;
    font-weight: 600;
  }
}

hr {
  border: none;
  border-top: 1px solid $divider-color;
  // Mobile
  margin: 12px 0;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    margin: 16px 0;
  }
}

.modal-content {
  // Mobile
  border-radius: 12px;

  .modal-title {
    width: 100%;
    text-align: center;
    font-weight: 600;
    // Mobile
    font-size: 18px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 20px;
    }
  }

  .modal-body {
    // Mobile
    padding: 16px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      padding: 24px;
    }
  }

  .modal-footer {
    display: flex;
    justify-content: space-between;
    // Mobile
    padding: 12px 16px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      padding: 16px 24px;
    }
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    border-radius: 16px;
  }

  // Desktop (992px+)
  @media (min-width: 992px) {
    border-radius: 25px;
  }
}

.policy-item {
  padding: 10px 0;
  display: flex;
  .policy-inner-item2 {
    margin-left: 4rem;
  }
  .policy-label {
    color: $text-dark;
    font-size: 16px;
    margin-bottom: 8px;
  }
  .policy-date {
    color: $text-muted;
    font-size: 14px;
    margin-bottom: 8px;
  }
  .policy-description {
    color: $accent;
    font-size: 14px;
    font-weight: 500;
    margin: 0;
  }
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
  .policy-note {
    color: $text-muted;
    font-size: 12px;
    margin-bottom: 16px;
    text-align: start;
  }
  .refund-info h6 {
    color: $text-dark;
    font-size: 14px;
    font-weight: 600;
    margin-bottom: 8px;
  }
  .refund-info p {
    color: $text-muted;
    font-size: 13px;
    line-height: 1.4;
    margin: 0;
  }
  .policy-link {
    color: $accent;
    font-size: 13px;
    text-decoration: underline;
    display: block;
    text-align: start;
    &:hover {
      color: darken($accent, 10%);
    }
  }
}
</style>

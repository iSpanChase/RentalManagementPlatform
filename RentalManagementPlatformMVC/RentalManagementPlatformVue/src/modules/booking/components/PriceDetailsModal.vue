<script setup>
import { useBookingStore } from '@/stores/bookingStore';
import { formatPrice } from '@/composables/useBookingFormatters';

const props = defineProps({
  discountAmount: {
    type: Number,
    default: 0,
  },
  finalPrice: {
    type: Number,
    required: true,
  },
  modalId: {
    type: String,
    default: 'priceDetailsModal',
  },
});

const bookingStore = useBookingStore();
</script>

<template>
  <Teleport to="body">
    <div
      class="modal fade"
      :id="modalId"
      tabindex="-1"
      :aria-labelledby="`${modalId}Label`"
      aria-hidden="true"
      data-bs-backdrop="true"
      data-bs-keyboard="true"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" :id="`${modalId}Label`">價格明細</h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="關閉"
            ></button>
          </div>
          <div class="modal-body" v-if="bookingStore.hasBookingDraft">
            <slot name="price-details-body">
              <div class="price-row">
                <span
                  >{{ bookingStore.nights }} 晚 x
                  {{ formatPrice(bookingStore.bookingDraft.pricePerNight) }}</span
                >
                <span>{{ formatPrice(bookingStore.subtotal) }}</span>
              </div>

              <div class="price-row">
                <span>服務費</span>
                <span>{{ formatPrice(bookingStore.serviceFee) }}</span>
              </div>

              <div class="price-row discount" v-if="bookingStore.discountAmount > 0">
                <span>特別優惠</span>
                <span class="green">-{{ formatPrice(bookingStore.discountAmount) }}</span>
              </div>

              <div class="price-row discount" v-if="discountAmount > 0">
                <span>優惠券折扣</span>
                <span class="green">-{{ formatPrice(discountAmount) }}</span>
              </div>

              <hr />

              <div class="price-row total">
                <strong>總計 TWD</strong>
                <strong>{{ formatPrice(finalPrice) }}</strong>
              </div>
            </slot>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
.modal {
  z-index: 1055;
}

.modal-content {
  border-radius: 25px;

  .modal-title {
    width: 100%;
    text-align: center;
    font-weight: 600;
  }

  .modal-body {
    padding: 24px;
  }

  .modal-header {
    border-bottom: 1px solid #dee2e6;
    padding: 1rem 1.5rem;

    .btn-close {
      padding: 0.5rem;
      margin: -0.5rem -0.5rem -0.5rem auto;
    }
  }
}

.price-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 14px;

  &.discount .green {
    color: #008489;
    font-weight: 600;
  }

  &.total {
    font-size: 16px;
    padding-top: 12px;
    font-weight: 600;
  }
}

hr {
  border: none;
  border-top: 1px solid #ebebeb;
  margin: 16px 0;
}
</style>

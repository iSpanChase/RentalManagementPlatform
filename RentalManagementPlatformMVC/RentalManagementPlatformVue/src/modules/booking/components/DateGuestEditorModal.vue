<script setup>
import { ref } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useBookingValidation } from '@/composables/useBookingValidation';
import { toISODateString } from '@/composables/useBookingFormatters';
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css';

const bookingStore = useBookingStore();
const { validateDateRange } = useBookingValidation();

// 日期選擇器
const dateRange = ref();
const dateError = ref('');
const minDate = new Date();

// 客人人數
const newGuests = ref(1);

// Modal 關閉按鈕引用
const dateModalCloser = ref(null);
const guestsModalCloser = ref(null);

/** 初始化日期選擇器 */
const initDatePickers = () => {
  if (!bookingStore.hasBookingDraft) return;
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
  if (!bookingStore.hasBookingDraft) return;
  newGuests.value = bookingStore.bookingDraft.guestCount || 1;
};

/** 儲存變更後的日期 */
const handleChangeDates = () => {
  const validation = validateDateRange(dateRange.value);

  if (validation.isValid) {
    const [newCheckIn, newCheckOut] = dateRange.value;
    bookingStore.bookingDraft.checkIn = toISODateString(newCheckIn);
    bookingStore.bookingDraft.checkOut = toISODateString(newCheckOut);
    dateError.value = '';
    dateModalCloser.value?.click();
  } else {
    dateError.value = validation.error;
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
    guestsModalCloser.value?.click();
  }
};

/** 增加客人 */
const increaseGuests = () => newGuests.value++;

/** 減少客人 */
const decreaseGuests = () => {
  if (newGuests.value > 1) newGuests.value--;
};

// 暴露方法給父元件使用
defineExpose({
  initDatePickers,
  initGuestsPicker,
});
</script>

<template>
  <Teleport to="body">
    <!-- 日期變更 Modal -->
    <div
      class="modal fade"
      id="dateChangeModal"
      tabindex="-1"
      aria-labelledby="dateChangeModalLabel"
      aria-hidden="true"
      data-bs-backdrop="true"
      data-bs-keyboard="true"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="dateChangeModalLabel">變更日期</h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="關閉"
            ></button>
          </div>
          <div class="modal-body">
            <div class="alert alert-danger" role="alert" v-if="dateError">{{ dateError }}</div>
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
              style="display: none"
            ></button>
            <div class="date-clear" @click="handleClearDates">清除日期</div>
            <button type="button" class="btn date-save" @click="handleChangeDates">儲存</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 客人數變更 Modal -->
    <div
      class="modal fade"
      id="guestsChangeModal"
      tabindex="-1"
      aria-labelledby="guestsChangeModalLabel"
      aria-hidden="true"
      data-bs-backdrop="true"
      data-bs-keyboard="true"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="guestsChangeModalLabel">變更入住人數</h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="關閉"
            ></button>
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
            <div class="guest-limit-text">
              <small>此住宿地點最多可容納 4 名房客 (未含嬰幼兒)。不允許攜帶寵物。</small>
            </div>
          </div>
          <div class="modal-footer">
            <button
              ref="guestsModalCloser"
              type="button"
              data-bs-dismiss="modal"
              style="display: none"
            ></button>
            <div class="customer-clear" @click="initGuestsPicker">清除</div>
            <button type="button" class="btn customer-save" @click="handleChangeGuests">
              儲存
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" scoped>
$border-color: #ddd;
$text-dark: #222;
$text-muted: #717171;
$divider-color: #ebebeb;

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

  .modal-footer {
    display: flex;
    justify-content: space-between;
    padding: 16px 24px;
  }
}

.date-save,
.customer-save {
  background-color: $text-dark;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 32px;
  font-weight: 600;

  &:hover {
    background-color: #000;
  }
}

.date-clear,
.customer-clear {
  color: $text-dark;
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

:deep(.dp__menu) {
  z-index: 1060 !important;
}
</style>

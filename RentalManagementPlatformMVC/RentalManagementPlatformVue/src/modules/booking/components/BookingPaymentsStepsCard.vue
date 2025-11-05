<script setup>
import { ref, onMounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { formatPrice } from '@/composables/useBookingFormatters';
import BillingForm from './BillingForm.vue';

// ==================== Props ====================
const props = defineProps({
  totalPrice: {
    type: Number,
    required: true,
  },
});

// ==================== 狀態管理 ====================
const bookingStore = useBookingStore();
const router = useRouter();
const toast = useToast();

// 當前步驟
const currentStep = ref(1);

// 步驟完成狀態
const stepCompleted = ref({
  step1: false,
  step2: false,
  step3: false,
});

// 付款時間選擇
const selectedPaymentTiming = ref('full');

// 帳單表單資料
const billingFormData = ref({
  billingInfo: {
    name: '',
    email: '',
    phone: '',
    notes: '',
  },
  billingAddress: {
    street: '',
    apartment: '',
    city: '',
    state: '',
    zipCode: '',
    country: 'TW',
  },
});

// 表單驗證狀態
const formValidation = ref({
  isValid: false,
  errors: {},
});

// 帳單表單引用
const billingForm = ref(null);

// 處理表單驗證狀態變更
const handleValidationChange = (validation) => {
  formValidation.value = validation;
};

/**
 * Step 1：繼續 → 進入 Step 2
 */
const handleContinue = () => {
  stepCompleted.value.step1 = true;
  currentStep.value = 2;
};

/**
 * Step 2：驗證並繼續 → 進入 Step 3
 */
const handleStep2Continue = () => {
  // 使用表單元件的驗證方法
  const isValid = billingForm.value?.validate();

  if (!isValid || !formValidation.value.isValid) {
    toast.info('請填寫完整且正確的資訊');
    return;
  }

  stepCompleted.value.step2 = true;
  currentStep.value = 3;
};

/**
 * 返回上一步
 */
const handleBack = () => {
  if (currentStep.value <= 1) return;

  currentStep.value--;

  if (currentStep.value === 1) {
    stepCompleted.value.step1 = false;
  } else if (currentStep.value === 2) {
    stepCompleted.value.step2 = false;
  }
};

/**
 * 確認並付款
 */
const handleConfirmPayment = async () => {
  if (!bookingStore.hasBookingDraft) {
    toast.info('訂房資料不完整，請重新選擇房源');
    return;
  }

  try {
    const paymentData = {
      finalAmount: props.totalPrice,
      paymentTiming: selectedPaymentTiming.value,
      billingInfo: billingFormData.value.billingInfo,
      billingAddress: billingFormData.value.billingAddress,
    };

    const result = await bookingStore.createBooking(paymentData);

    if (result.paymentRequired) {
      // 立即付款：插入綠界表單並提交
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = result.ecpayFormHtml;
      document.body.appendChild(tempDiv);

      const form = tempDiv.querySelector('form');
      if (form) {
        setTimeout(() => form.submit(), 500);
      } else {
        alert('無法載入付款表單，請聯繫客服');
      }
    } else {
      await router.push('/my-bookings');
      bookingStore.clearBookingDraft();
    }
  } catch (error) {
    const message = error.response?.data?.message || '訂單建立失敗';
    if (error.response) {
      alert(`錯誤：${message}`);
    } else if (error.request) {
      alert('網路連線失敗，請檢查網路');
    } else {
      alert('發生未知錯誤，請稍後再試');
    }
  }
};
</script>

<template>
  <div class="left-section">
    <!-- Step 1: 選擇付款時間 -->
    <div class="step-card" :class="{ active: currentStep === 1, completed: stepCompleted.step1 }">
      <div class="step-header">
        <h3>1。選擇付款時間</h3>
        <button
          v-if="stepCompleted.step1 && currentStep !== 1"
          type="button"
          class="btn-change"
          @click="
            currentStep = 1;
            stepCompleted.step1 = false;
            stepCompleted.step2 = false;
          "
        >
          更改
        </button>
      </div>

      <div v-if="currentStep === 1">
        <div class="payment-option">
          <input
            type="radio"
            id="full"
            name="payment"
            value="full"
            v-model="selectedPaymentTiming"
          />
          <label for="full">
            <div>立即支付 {{ formatPrice(Math.round(props.totalPrice)) }}</div>
          </label>
        </div>

        <div class="payment-option" :class="{ disabled: bookingStore.isRefundable === false }">
          <input
            type="radio"
            id="partial"
            name="payment"
            value="partial"
            v-model="selectedPaymentTiming"
            :disabled="!bookingStore.isRefundable"
          />
          <label for="partial">
            <div>立即支付 $0 TWD</div>
            <small v-if="bookingStore.isRefundable === false" class="text-muted"
              >此訂單不符合延後付款資格</small
            >
            <small v-else>
              將於 {{ bookingStore.refundableDate }} 收取
              {{ formatPrice(Math.round(props.totalPrice)) }}。無須支付額外費用。
              <a href="#">更多資訊</a>
            </small>
          </label>
        </div>

        <button type="button" class="btn-continue" @click="handleContinue">繼續</button>
      </div>

      <div v-else-if="stepCompleted.step1" class="step-summary">
        <p v-if="selectedPaymentTiming === 'full'">
          立即支付 {{ formatPrice(Math.round(props.totalPrice)) }}
        </p>
        <p v-else>
          已於 {{ bookingStore.refundableDate }} 收取
          {{ formatPrice(Math.round(props.totalPrice)) }}。無須支付額外費用。
        </p>
      </div>
    </div>

    <!-- Step 2: 付款資訊 -->
    <div
      class="step-card"
      :class="{
        active: currentStep === 2,
        completed: stepCompleted.step2,
        disabled: currentStep < 2,
      }"
    >
      <div class="step-header">
        <h3>2。付款資訊</h3>
        <button
          v-if="stepCompleted.step2 && currentStep !== 2"
          type="button"
          class="btn-change"
          @click="
            currentStep = 2;
            stepCompleted.step2 = false;
          "
        >
          更改
        </button>
      </div>

      <div v-if="currentStep === 2" class="payment-form">
        <div class="payment-method-header">
          <div class="payment-icon"><i class="fa-solid fa-credit-card"></i></div>
          <div>
            <strong>信用卡付款</strong>
            <div class="payment-description">
              <p class="text-muted">點擊「繼續」後，您將被導向綠界安全付款頁面完成信用卡付款</p>
              <div class="security-badges">
                <span class="badge"><i class="fa-solid fa-lock"></i> SSL 安全加密</span>
                <span class="badge"><i class="fa-solid fa-check"></i> PCI DSS 認證</span>
              </div>
            </div>
          </div>
        </div>

        <div class="supported-cards">
          <span class="label">支援卡別：</span>
          <div class="card-logos">
            <img
              src="https://upload.wikimedia.org/wikipedia/commons/4/41/Visa_Logo.png"
              alt="VISA"
            />
            <img
              src="https://upload.wikimedia.org/wikipedia/commons/2/2a/Mastercard-logo.svg"
              alt="Mastercard"
            />
            <img
              src="https://upload.wikimedia.org/wikipedia/commons/f/fa/American_Express_logo_%282018%29.svg"
              alt="AMEX"
            />
            <img
              src="https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/JCB_logo.svg/320px-JCB_logo.svg.png"
              alt="JCB"
            />
          </div>
        </div>

        <BillingForm
          ref="billingForm"
          v-model="billingFormData"
          @validation-change="handleValidationChange"
        />

        <div class="info-box">
          <div class="info-icon"><i class="fa-solid fa-circle-info"></i></div>
          <div class="info-content">
            <strong>付款流程說明</strong>
            <ul>
              <li>下一步將導向綠界金流安全付款頁面</li>
              <li>請準備您的信用卡資訊</li>
              <li>完成付款後將自動返回訂單確認頁面</li>
            </ul>
          </div>
        </div>

        <div class="cancellation-reminder">
          <i class="fa-solid fa-shield-halved"></i>
          <span><strong>取消政策：</strong> {{ bookingStore.refundableDate }} 前可免費取消</span>
        </div>

        <div class="button-group">
          <button type="button" class="btn-back" @click="handleBack">返回</button>
          <button type="button" class="btn-continue" @click="handleStep2Continue">繼續</button>
        </div>
      </div>

      <div v-else-if="stepCompleted.step2" class="step-summary">
        <div class="summary-icon"><i class="fa-solid fa-check"></i></div>
        <div>
          <p>
            <strong>{{ billingFormData.billingInfo.name }}</strong>
          </p>
          <p class="text-muted">{{ billingFormData.billingInfo.email }}</p>
          <p class="text-muted">
            {{ billingFormData.billingAddress.city }}, {{ billingFormData.billingAddress.country }}
          </p>
        </div>
      </div>
    </div>

    <!-- Step 3: 查看預訂 -->
    <div class="step-card" :class="{ active: currentStep === 3, disabled: currentStep < 3 }">
      <h3>3。查看預訂</h3>

      <div v-if="currentStep === 3">
        <label>
          <small>點選該按鈕,即代表我同意 <a href="#">《預訂條款》</a>。</small>
        </label>

        <div class="button-group">
          <button
            type="button"
            class="btn-back"
            @click="handleBack"
            :disabled="bookingStore.isLoading"
          >
            返回
          </button>
          <button
            type="button"
            class="btn-continue"
            @click="handleConfirmPayment"
            :disabled="bookingStore.isLoading"
          >
            <span v-if="bookingStore.isLoading" class="loading-spinner-btn"></span>
            <span v-if="bookingStore.isLoading">處理中...</span>
            <span v-else>確認並付款</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
// Mobile-first 設計：從最小螢幕開始設計，然後向上擴展
.left-section {
  display: flex;
  flex-direction: column;
  max-width: 100%;
  // Mobile
  gap: 16px;

  // Large mobile (480px+)
  @media (min-width: 480px) {
    gap: 20px;
  }

  // Tablet (768px+)
  @media (min-width: 768px) {
    gap: 24px;
  }

  // Desktop (992px+)
  @media (min-width: 992px) {
    gap: 30px;
  }
}

.step-card {
  border: 1px solid #ddd;
  transition: all 0.3s ease;
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
    padding: 30px;
  }

  h3 {
    margin: 0 0 12px;
    // Mobile
    font-size: 18px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      font-size: 19px;
      margin-bottom: 14px;
    }

    // Tablet (768px+)
    @media (min-width: 768px) {
      font-size: 20px;
      margin-bottom: 16px;
    }
  }

  &.active {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    border-color: #222;
  }

  &.completed {
    opacity: 0.7;
    background-color: #f7f7f7;
  }

  &.disabled {
    opacity: 0.5;
    pointer-events: none;
    background-color: #fafafa;
  }

  > div > .btn-continue {
    width: 100%;
    background: #222;
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.2s;
    // Mobile
    padding: 16px;
    font-size: 16px;
    margin-top: 16px;
    min-height: 48px;

    &:hover {
      background: #000;
    }

    // Large mobile (480px+)
    @media (min-width: 480px) {
      padding: 14px;
      min-height: auto;
    }
  }
}

.step-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;

  h3 {
    margin: 0;
  }
}

.btn-change {
  background: none;
  border: none;
  color: #222;
  text-decoration: underline;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  padding: 4px 8px;
  transition: color 0.2s;

  &:hover {
    color: #000;
  }
}

.step-summary {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
  color: #717171;
  font-size: 14px;

  p {
    margin: 0;
    line-height: 1.5;
  }
  .text-muted {
    color: #717171;
    font-size: 14px;
  }
  strong {
    color: #222;
    font-weight: 600;
  }
}

.summary-icon {
  font-size: 24px;
  flex-shrink: 0;
  color: #4caf50;
}

.payment-option {
  margin-bottom: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  transition: border-color 0.2s;
  // Mobile
  padding: 12px;

  &:hover {
    border-color: #bbb;
  }

  input[type='radio'] {
    // Mobile: 更大的選擇區域
    margin-right: 10px;
    transform: scale(1.2);

    // Large mobile (480px+)
    @media (min-width: 480px) {
      margin-right: 12px;
      transform: scale(1);
    }
  }

  label {
    cursor: pointer;
    display: block;
    width: 100%;

    div {
      font-weight: 600;
      margin-bottom: 4px;
      // Mobile
      font-size: 15px;

      // Large mobile (480px+)
      @media (min-width: 480px) {
        font-size: 16px;
      }
    }

    small {
      color: #666;
      line-height: 1.4;
      // Mobile
      font-size: 13px;

      a {
        color: #222;
        text-decoration: underline;
        &:hover {
          color: #000;
        }
      }

      // Large mobile (480px+)
      @media (min-width: 480px) {
        font-size: 14px;
      }
    }
  }

  // Large mobile (480px+)
  @media (min-width: 480px) {
    padding: 16px;
  }
}

.payment-form {
  margin-top: 16px;
}

.payment-method-header {
  display: flex;
  align-items: flex-start;
  gap: 15px;
  margin-bottom: 20px;
  padding: 16px;
  background: white;
  border-radius: 8px;
  border: 1px solid #e0e0e0;

  .payment-icon {
    font-size: 24px;
    flex-shrink: 0;
  }
  strong {
    display: block;
    margin-bottom: 8px;
    font-size: 16px;
  }
}

.payment-description {
  .text-muted {
    font-size: 14px;
    line-height: 1.5;
    margin-bottom: 8px;
  }
}

.security-badges {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;

  .badge {
    padding: 4px 12px;
    background: #e8f5e9;
    color: #2e7d32;
    border-radius: 16px;
    font-size: 12px;
    font-weight: 500;
  }
}

.supported-cards {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: #f9f9f9;
  border-radius: 8px;
  margin-bottom: 24px;
  flex-wrap: wrap;

  .label {
    font-size: 14px;
    color: #666;
    font-weight: 500;
  }
  .card-logos {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
    img {
      height: 24px;
      width: auto;
    }
  }
}

.info-box {
  display: flex;
  gap: 12px;
  padding: 16px;
  background: #e3f2fd;
  border-left: 4px solid #2196f3;
  border-radius: 8px;
  margin: 24px 0;

  .info-icon {
    font-size: 24px;
    flex-shrink: 0;
  }
  .info-content {
    flex: 1;
    strong {
      display: block;
      margin-bottom: 8px;
      color: #1976d2;
    }
    ul {
      margin: 0;
      padding-left: 20px;
      li {
        color: #555;
        font-size: 14px;
        line-height: 1.6;
        margin-bottom: 4px;
      }
    }
  }
}

.cancellation-reminder {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: #fff3cd;
  border: 1px solid #ffc107;
  border-radius: 8px;
  margin-bottom: 16px;

  i {
    color: #ff9800;
    font-size: 20px;
    flex-shrink: 0;
  }
  span {
    font-size: 14px;
    color: #856404;
    line-height: 1.5;
  }
  strong {
    font-weight: 600;
  }
}

.button-group {
  display: flex;
  margin-top: 16px;
  // Mobile: 垂直排列
  flex-direction: column;
  gap: 12px;

  button {
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    // Mobile
    padding: 16px;
    font-size: 16px;
    min-height: 48px;

    // Large mobile (480px+)
    @media (min-width: 480px) {
      padding: 14px;
      min-height: auto;
    }
  }

  .btn-back {
    background: white;
    color: #222;
    border: 1px solid #222;

    &:hover {
      background: #f7f7f7;
    }
  }

  .btn-continue {
    background: #222;
    color: white;
    border: none;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;

    &:hover:not(:disabled) {
      background: #000;
    }
    &:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
  }

  .btn-back:disabled {
    background: #f7f7f7;
    color: #ccc;
    border-color: #ccc;
    cursor: not-allowed;
  }

  // Large mobile (480px+): 水平排列
  @media (min-width: 480px) {
    flex-direction: row;

    button {
      flex: 1;
    }
  }
}

.loading-spinner-btn {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>

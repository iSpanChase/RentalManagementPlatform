<script setup>
import { ref, computed } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
import { getData } from 'country-list'

// ==================== 狀態管理 ====================
const bookingStore = useBookingStore()

// 當前進行的步驟 (1, 2, 3)
const currentStep = ref(1)

// 每個步驟的完成狀態
const stepCompleted = ref({
  step1: false,
  step2: false,
  step3: false
})

// 用戶選擇的付款時間 ('full' = 立即支付, 'partial' = 延後支付)
const selectedPaymentTiming = ref('full')

// ==================== 表單資料 ====================
// 信用卡資訊
const cardInfo = ref({
  cardNumber: '',
  expiry: '',
  cvv: ''
})

// 帳單地址
const billingAddress = ref({
  street: '',
  apartment: '',
  city: '',
  state: '',
  zipCode: '',
  country: 'TW'
})

// 國家列表 (從 country-list 套件獲取)
const countries = ref(
  getData().map(country => ({
    code: country.code,
    name: country.name
  }))
)

// ==================== 表單驗證 ====================
const formErrors = ref({
  cardNumber: '',
  expiry: '',
  cvv: '',
  street: '',
  city: '',
  zipCode: ''
})

/**
 * 驗證信用卡號碼（簡單驗證長度）
 */
const validateCardNumber = () => {
  const numbers = cardInfo.value.cardNumber.replace(/\s/g, '')
  if (numbers.length !== 16) {
    formErrors.value.cardNumber = '請輸入完整的16位卡號'
    return false
  }
  formErrors.value.cardNumber = ''
  return true
}

/**
 * 驗證到期日
 */
const validateExpiry = () => {
  const expiry = cardInfo.value.expiry
  if (!expiry || expiry.length !== 5) {
    formErrors.value.expiry = '請輸入有效的到期日'
    return false
  }

  const [month, year] = expiry.split('/')
  const currentYear = new Date().getFullYear() % 100
  const currentMonth = new Date().getMonth() + 1

  if (parseInt(month) < 1 || parseInt(month) > 12) {
    formErrors.value.expiry = '月份必須在 01-12 之間'
    return false
  }

  if (parseInt(year) < currentYear || (parseInt(year) === currentYear && parseInt(month) < currentMonth)) {
    formErrors.value.expiry = '卡片已過期'
    return false
  }

  formErrors.value.expiry = ''
  return true
}

/**
 * 驗證 CVV
 */
const validateCvv = () => {
  if (cardInfo.value.cvv.length !== 3) {
    formErrors.value.cvv = '請輸入3位數的安全碼'
    return false
  }
  formErrors.value.cvv = ''
  return true
}

/**
 * 驗證必填欄位
 */
const validateRequiredFields = () => {
  let isValid = true

  if (!billingAddress.value.street.trim()) {
    formErrors.value.street = '請輸入街道地址'
    isValid = false
  } else {
    formErrors.value.street = ''
  }

  if (!billingAddress.value.city.trim()) {
    formErrors.value.city = '請輸入城市'
    isValid = false
  } else {
    formErrors.value.city = ''
  }

  if (!billingAddress.value.zipCode.trim()) {
    formErrors.value.zipCode = '請輸入郵遞區號'
    isValid = false
  } else {
    formErrors.value.zipCode = ''
  }

  return isValid
}

/**
 * 驗證所有表單
 */
const validateAllForms = () => {
  const isCardValid = validateCardNumber()
  const isExpiryValid = validateExpiry()
  const isCvvValid = validateCvv()
  const areFieldsValid = validateRequiredFields()

  return isCardValid && isExpiryValid && isCvvValid && areFieldsValid
}

// ==================== 格式化函數 ====================
/**
 * 格式化信用卡號碼：每 4 位數字加一個空格
 * @param {string} value - 輸入的卡號
 * @returns {string} 格式化後的卡號 (例如: 1234 5678 9012 3456)
 */
const formatCardNumber = (value) => {
  const numbers = value.replace(/\D/g, '')
  const limited = numbers.slice(0, 16)
  const parts = []

  for (let i = 0; i < limited.length; i += 4) {
    parts.push(limited.slice(i, i + 4))
  }

  return parts.join(' ')
}

/**
 * 格式化到期日：自動加入斜線
 * @param {string} value - 輸入的到期日
 * @returns {string} 格式化後的到期日 (例如: 12/25)
 */
const formatExpiry = (value) => {
  const numbers = value.replace(/\D/g, '')

  if (numbers.length <= 2) {
    return numbers
  }

  return numbers.slice(0, 2) + '/' + numbers.slice(2, 4)
}

// ==================== 輸入處理函數 ====================
/**
 * 處理信用卡號碼輸入
 */
const handleCardNumberInput = (event) => {
  cardInfo.value.cardNumber = formatCardNumber(event.target.value)
  if (formErrors.value.cardNumber) {
    validateCardNumber()
  }
}

/**
 * 處理到期日輸入
 */
const handleExpiryInput = (event) => {
  cardInfo.value.expiry = formatExpiry(event.target.value)
  if (formErrors.value.expiry) {
    validateExpiry()
  }
}

/**
 * 處理 CVV 輸入 (只允許 3 位數字)
 */
const handleCvvInput = (event) => {
  const numbers = event.target.value.replace(/\D/g, '')
  cardInfo.value.cvv = numbers.slice(0, 3)
  if (formErrors.value.cvv) {
    validateCvv()
  }
}

// ==================== 計算屬性 ====================
/**
 * 計算應顯示的總金額
 */
const displayAmount = computed(() => {
  return bookingStore.totalPrice.toLocaleString('zh-TW', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  })
})

/**
 * 計算延後付款的收費日期
 */
const laterPaymentDate = computed(() => {
  return bookingStore.refundableDate || '12月23日'
})

// ==================== 步驟控制函數 ====================
/**
 * Step 1 繼續按鈕：完成 Step 1 並進入 Step 2
 */
const handleContinue = () => {
  stepCompleted.value.step1 = true
  currentStep.value = 2
}

/**
 * Step 2 繼續按鈕：完成 Step 2 並進入 Step 3
 */
const handleStep2Continue = () => {
  // 驗證表單
  if (!validateAllForms()) {
    alert('請完整填寫所有必填欄位')
    return
  }

  stepCompleted.value.step2 = true
  currentStep.value = 3
}

/**
 * 返回上一步
 */
const handleBack = () => {
  if (currentStep.value > 1) {
    currentStep.value--

    // 取消當前步驟的完成狀態
    if (currentStep.value === 1) {
      stepCompleted.value.step1 = false
    } else if (currentStep.value === 2) {
      stepCompleted.value.step2 = false
    }
  }
}

/**
 * 最終確認並付款
 */
const handleConfirmPayment = () => {
  // 這裡可以呼叫 bookingStore.createBooking()
  console.log('確認付款', {
    paymentTiming: selectedPaymentTiming.value,
    cardInfo: cardInfo.value,
    billingAddress: billingAddress.value
  })

  // 呼叫後端 API
  // await bookingStore.createBooking()

  alert('訂房成功！')
}
</script>

<template>
  <div class="left-section">

    <!-- ==================== Step 1: 選擇付款時間 ==================== -->
    <div
      class="step-card"
      :class="{
        'active': currentStep === 1,
        'completed': stepCompleted.step1
      }"
    >
      <!-- 步驟標題 -->
      <div class="step-header">
        <h3>1。選擇付款時間</h3>
        <button
          v-if="stepCompleted.step1 && currentStep !== 1"
          type="button"
          class="btn-change"
          @click="stepCompleted.step1 = false; stepCompleted.step2 = false ; currentStep = 1"
        >
          更改
        </button>
      </div>

      <!-- 進行中：顯示完整表單 -->
      <div v-if="currentStep === 1">
        <!-- 立即支付選項 -->
        <div class="payment-option">
          <input
            type="radio"
            id="full"
            name="payment"
            value="full"
            v-model="selectedPaymentTiming"
          >
          <label for="full">
            <div>立即支付 ${{ displayAmount }} TWD</div>
          </label>
        </div>

        <!-- 延後支付選項 -->
        <div class="payment-option">
          <input
            type="radio"
            id="partial"
            name="payment"
            value="partial"
            v-model="selectedPaymentTiming"
          >
          <label for="partial">
            <div>立即支付 $0 TWD</div>
            <small>
              將於 {{ laterPaymentDate }} 收取 ${{ displayAmount }} TWD。無須支付額外費用。
              <a href="#">更多資訊</a>
            </small>
          </label>
        </div>

        <!-- 繼續按鈕 -->
        <button type="button" class="btn-continue" @click="handleContinue">
          繼續
        </button>
      </div>

      <!-- 已完成：顯示摘要 -->
      <div v-else-if="stepCompleted.step1" class="step-summary">
        <p v-if="selectedPaymentTiming === 'full'">
          立即支付 ${{ displayAmount }} TWD
        </p>
        <p v-else>
          已於{{ laterPaymentDate }}收取 ${{ displayAmount }} TWD。無須支付額外費用。
        </p>
      </div>
    </div>

    <!-- ==================== Step 2: 新增付款方式 ==================== -->
    <div
      class="step-card"
      :class="{
        'active': currentStep === 2,
        'completed': stepCompleted.step2,
        'disabled': currentStep < 2
      }"
    >
      <!-- 步驟標題 -->
      <div class="step-header">
        <h3>2。新增付款方式</h3>
        <button
          v-if="stepCompleted.step2 && currentStep !== 2"
          type="button"
          class="btn-change"
          @click="currentStep = 2; stepCompleted.step2 = false"
        >
          更改
        </button>
      </div>

      <!-- 進行中：顯示完整表單 -->
      <div v-if="currentStep === 2" class="payment-form">

        <!-- 付款方式標題 -->
        <div class="payment-method-header">
          <div class="payment-icon"><i class="fa-solid fa-credit-card"></i></div>
          <div>
            <strong>信用卡或簽帳卡</strong>
            <div class="card-brands">
              <img
                src="https://upload.wikimedia.org/wikipedia/commons/4/41/Visa_Logo.png"
                alt="VISA"
                class="card-logo"
              >
              <img
                src="https://upload.wikimedia.org/wikipedia/commons/2/2a/Mastercard-logo.svg"
                alt="Mastercard"
                class="card-logo"
              >
            </div>
          </div>
        </div>

        <!-- 信用卡表單 -->
        <form class="credit-card-form" @submit.prevent>

          <!-- 卡號 -->
          <div class="form-group">
            <label>卡號 <i class="fa-solid fa-lock"></i></label>
            <input
              type="text"
              :value="cardInfo.cardNumber"
              @input="handleCardNumberInput"
              @blur="validateCardNumber"
              placeholder="1234 5678 9012 3456"
              maxlength="19"
              :class="{ 'error': formErrors.cardNumber }"
            >
            <span v-if="formErrors.cardNumber" class="error-message">
              {{ formErrors.cardNumber }}
            </span>
          </div>

          <!-- 到期日 & 安全碼 -->
          <div class="form-row">
            <div class="form-group">
              <label>到期日</label>
              <input
                type="text"
                :value="cardInfo.expiry"
                @input="handleExpiryInput"
                @blur="validateExpiry"
                placeholder="MM/YY"
                maxlength="5"
                :class="{ 'error': formErrors.expiry }"
              >
              <span v-if="formErrors.expiry" class="error-message">
                {{ formErrors.expiry }}
              </span>
            </div>
            <div class="form-group">
              <label>安全碼</label>
              <input
                type="text"
                :value="cardInfo.cvv"
                @input="handleCvvInput"
                @blur="validateCvv"
                placeholder="CVV"
                maxlength="3"
                :class="{ 'error': formErrors.cvv }"
              >
              <span v-if="formErrors.cvv" class="error-message">
                {{ formErrors.cvv }}
              </span>
            </div>
          </div>

          <!-- 帳單地址區塊 -->
          <h4>帳單地址</h4>

          <!-- 街道地址 -->
          <div class="form-group">
            <label>街道地址 <span class="required">*</span></label>
            <input
              type="text"
              v-model="billingAddress.street"
              placeholder="請輸入街道地址"
              :class="{ 'error': formErrors.street }"
            >
            <span v-if="formErrors.street" class="error-message">
              {{ formErrors.street }}
            </span>
          </div>

          <!-- 公寓或套房號碼 -->
          <div class="form-group">
            <label>公寓或套房號碼</label>
            <input
              type="text"
              v-model="billingAddress.apartment"
              placeholder="公寓、套房號碼（選填）"
            >
          </div>

          <!-- 城市 -->
          <div class="form-group">
            <label>城市 <span class="required">*</span></label>
            <input
              type="text"
              v-model="billingAddress.city"
              placeholder="請輸入城市"
              :class="{ 'error': formErrors.city }"
            >
            <span v-if="formErrors.city" class="error-message">
              {{ formErrors.city }}
            </span>
          </div>

          <!-- 省份 & 郵遞區號 -->
          <div class="form-row">
            <div class="form-group">
              <label>省份 / 直轄市 / 州</label>
              <input
                type="text"
                v-model="billingAddress.state"
                placeholder="省份"
              >
            </div>
            <div class="form-group">
              <label>郵遞區號 <span class="required">*</span></label>
              <input
                type="text"
                v-model="billingAddress.zipCode"
                placeholder="郵遞區號"
                :class="{ 'error': formErrors.zipCode }"
              >
              <span v-if="formErrors.zipCode" class="error-message">
                {{ formErrors.zipCode }}
              </span>
            </div>
          </div>

          <!-- 國家 / 地區 -->
          <div class="form-group">
            <label>國家 / 地區</label>
            <select v-model="billingAddress.country">
              <option
                v-for="country in countries"
                :key="country.code"
                :value="country.code"
              >
                {{ country.name }}
              </option>
            </select>
          </div>
        </form>

        <!-- 按鈕組 -->
        <div class="button-group">
          <button type="button" class="btn-back" @click="handleBack">
            返回
          </button>
          <button type="button" class="btn-continue" @click="handleStep2Continue">
            繼續
          </button>
        </div>
      </div>

      <!-- 已完成：顯示摘要 -->
      <div v-else-if="stepCompleted.step2" class="step-summary">
        <div class="summary-icon"><i class="fa-solid fa-credit-card"></i></div>
        <div>
          <p><strong>Credit Card</strong></p>
          <p class="text-muted">•••• {{ cardInfo.cardNumber.slice(-4) }}</p>
        </div>
      </div>
    </div>

    <!-- ==================== Step 3: 查看預訂 ==================== -->
    <div
      class="step-card"
      :class="{
        'active': currentStep === 3,
        'disabled': currentStep < 3
      }"
    >
      <h3>3。查看預訂</h3>

      <!-- 進行中：顯示確認內容 -->
      <div v-if="currentStep === 3" class="check-payment">
        <label>
          <small>
            點選該按鈕，即代表我同意
            <a href="#">《預訂條款》</a>。
          </small>
        </label>

        <!-- 按鈕組 -->
        <div class="button-group">
          <button type="button" class="btn-back" @click="handleBack">
            返回
          </button>
          <button type="button" class="btn-continue" @click="handleConfirmPayment">
            確認並付款
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<style lang="scss" scoped>
/* ==================== 主容器 ==================== */
.left-section {
  display: flex;
  flex-direction: column;
  gap: 30px;
  max-width: 100%;

  @media (max-width: 768px) {
    gap: 16px;
  }
}

/* ==================== 步驟卡片 ==================== */
.step-card {
  border: 1px solid #ddd;
  border-radius: 25px;
  padding: 30px;
  transition: all 0.3s ease;

  @media (max-width: 768px) {
    padding: 20px;
    border-radius: 16px;
  }

  h3 {
    margin-bottom: 16px;
    font-size: 20px;

    @media (max-width: 768px) {
      font-size: 18px;
      margin-bottom: 12px;
    }
  }

  /* 當前進行中的步驟 */
  &.active {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    border-color: #222;
  }

  /* 已完成的步驟 */
  &.completed {
    opacity: 0.7;
    background-color: #f7f7f7;
  }

  /* 未解鎖的步驟 */
  &.disabled {
    opacity: 0.5;
    pointer-events: none;
    background-color: #fafafa;
  }

  /* Step 1 單獨的繼續按鈕樣式 */
  > div > .btn-continue {
    width: 100%;
    padding: 14px;
    background: #222;
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    margin-top: 16px;
    transition: background 0.2s;

    &:hover {
      background: #000;
    }
  }
}

/* ==================== 步驟標題區 ==================== */
.step-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;

  h3 {
    margin-bottom: 0;
  }
}

/* 更改按鈕 */
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

/* ==================== 摘要區域 ==================== */
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
}

/* ==================== 付款選項 ==================== */
.payment-option {
  margin-bottom: 12px;
  padding: 16px;
  border: 1px solid #ddd;
  border-radius: 8px;
  transition: border-color 0.2s;

  &:hover {
    border-color: #bbb;
  }

  input[type="radio"] {
    margin-right: 12px;
  }

  label {
    cursor: pointer;
    display: block;
    width: 100%;

    div {
      font-weight: 600;
      margin-bottom: 4px;
    }

    small {
      color: #666;
      line-height: 1.4;

      a {
        color: #222;
        text-decoration: underline;

        &:hover {
          color: #000;
        }
      }
    }
  }

  @media (max-width: 768px) {
    padding: 12px;
  }
}

/* ==================== 付款表單 ==================== */
.payment-form {
  margin-top: 16px;
}

/* 付款方式標題區塊 */
.payment-method-header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
  padding: 16px;
  background: white;
  border-radius: 8px;
  border: 1px solid #e0e0e0;

  .payment-icon {
    font-size: 24px;
  }

  .card-brands {
    display: flex;
    gap: 8px;
    margin-top: 4px;
  }

  .card-logo {
    height: 20px;
    width: auto;
  }
}

/* 信用卡表單 */
.credit-card-form {
  h4 {
    margin: 24px 0 16px 0;
    color: #333;
    font-size: 18px;
    font-weight: 600;
  }
}

/* 表單欄位組 */
.form-group {
  margin-bottom: 16px;

  label {
    display: block;
    margin-bottom: 6px;
    font-weight: 500;
    color: #333;
    font-size: 14px;

    .required {
      color: #e74c3c;
      margin-left: 4px;
    }
  }

  input,
  select {
    width: 100%;
    padding: 12px;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 16px;
    transition: border-color 0.2s;

    &:focus {
      outline: none;
      border-color: #222;
      box-shadow: 0 0 0 2px rgba(34, 34, 34, 0.1);
    }

    &::placeholder {
      color: #999;
    }

    &.error {
      border-color: #e74c3c;
    }
  }

  select {
    cursor: pointer;
  }

  .error-message {
    display: block;
    color: #e74c3c;
    font-size: 12px;
    margin-top: 4px;
  }
}

.check-payment {
  small {
    color: #666;
    line-height: 1.4;

    a {
      color: #222;
      text-decoration: underline;

      &:hover {
        color: #000;
      }
    }
  }
}

/* 兩欄表單排版 */
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

/* ==================== 按鈕組 ==================== */
.button-group {
  display: flex;
  gap: 12px;
  margin-top: 16px;

  button {
    flex: 1;
    padding: 14px;
    border-radius: 8px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
  }

  /* 返回按鈕 */
  .btn-back {
    background: white;
    color: #222;
    border: 1px solid #222;

    &:hover {
      background: #f7f7f7;
    }
  }

  /* 繼續按鈕 */
  .btn-continue {
    background: #222;
    color: white;
    border: none;

    &:hover {
      background: #000;
    }
  }
}
</style>

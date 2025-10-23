<script setup>
import { ref } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
const bookingStore = useBookingStore()

const currentStep = ref(1)

const stepCompleted = ref({
  step1: false,
  step2: false,
  step3: false
})

const cardInfo = ref({
  cardNumber: '',
  expiry: '',
  cvv: ''
})

const billingAddress = ref({
  street: '',
  apartment: '',
  city: '',
  state: '',
  zipCode: '',
  country: 'TW'
})

const handleContinue = () => {
  stepCompleted.value.step1 = true
  currentStep.value = 2
  console.log('移到 Step 2')
}

const handleStep2Continue = () => {
  stepCompleted.value.step2 = true
  currentStep.value = 3
  console.log('移到 Step 3')
}

const handleBack = () => {
  if (currentStep.value > 1) {
    currentStep.value = currentStep.value - 1
    // 取消當前步驟的完成狀態
    if (currentStep.value === 1) {
      stepCompleted.value.step1 = false
    } else if (currentStep.value === 2) {
      stepCompleted.value.step2 = false
    }
    console.log('返回 Step', currentStep.value)
  }
}
</script>

<template>
  <div class="left-section">
    <!-- 選擇付款時間 -->
    <div class="step-card" :class="{ 'active': currentStep === 1, 'completed': stepCompleted.step1 }">
      <h3>1。選擇付款時間</h3>
      <div class="payment-option">
        <input type="radio" id="full" name="payment" value="full" checked>
        <label for="full">
          <div>立即支付 ${{ bookingStore.totalPrice }} TWD</div>
        </label>
      </div>
      <div class="payment-option">
        <input type="radio" id="partial" name="payment" value="partial">
        <label for="partial">
          <div>立即支付 $0 TWD</div>
          <small>將於{{ bookingStore.refundableDate }}收取 ${{ bookingStore.totalPrice }} TWD。無須支付額外費用。<a href="#">更多資訊</a></small>
        </label>
      </div>
      <button type="button" class="btn-continue" @click="handleContinue">繼續</button>
    </div>

    <!-- 新增付款方式 -->
    <div class="step-card" :class="{ 'active': currentStep === 2, 'completed': stepCompleted.step2, 'disabled': currentStep < 2 }">
      <h3>2。新增付款方式</h3>

      <!-- 只有在 step2 時才顯示內容 -->
      <div v-if="currentStep >= 2" class="payment-form">
        <!-- 付款方式標題 -->
        <div class="payment-method-header">
          <div class="payment-icon">💳</div>
          <div>
            <strong>信用卡或簽帳卡</strong>
            <div class="card-brands">
              <img src="https://upload.wikimedia.org/wikipedia/commons/4/41/Visa_Logo.png" alt="VISA" class="card-logo">
              <img src="https://upload.wikimedia.org/wikipedia/commons/2/2a/Mastercard-logo.svg" alt="Mastercard" class="card-logo">
            </div>
          </div>
        </div>

        <!-- 信用卡表單 -->
        <form class="credit-card-form">
          <!-- 卡號 -->
          <div class="form-group">
            <label>卡號 🔒</label>
            <input
              type="text"
              v-model="cardInfo.cardNumber"
              placeholder="1234 5678 9012 3456"
              maxlength="19"
            >
          </div>

          <!-- 到期日 & 安全碼 -->
          <div class="form-row">
            <div class="form-group">
              <label>到期日</label>
              <input
                type="text"
                v-model="cardInfo.expiry"
                placeholder="MM/YY"
                maxlength="5"
              >
            </div>
            <div class="form-group">
              <label>安全碼</label>
              <input
                type="text"
                v-model="cardInfo.cvv"
                placeholder="CVV"
                maxlength="3"
              >
            </div>
          </div>

          <!-- 帳單地址 -->
          <h4>帳單地址</h4>

          <div class="form-group">
            <label>街道地址</label>
            <input type="text" v-model="billingAddress.street" placeholder="請輸入街道地址">
          </div>

          <div class="form-group">
            <label>公寓或套房號碼</label>
            <input type="text" v-model="billingAddress.apartment" placeholder="公寓、套房號碼（選填）">
          </div>

          <div class="form-group">
            <label>城市</label>
            <input type="text" v-model="billingAddress.city" placeholder="請輸入城市">
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>省份 / 直轄市 / 州</label>
              <input type="text" v-model="billingAddress.state" placeholder="省份">
            </div>
            <div class="form-group">
              <label>郵遞區號</label>
              <input type="text" v-model="billingAddress.zipCode" placeholder="郵遞區號">
            </div>
          </div>

          <div class="form-group">
            <label>國家 / 地區</label>
            <select v-model="billingAddress.country">
              <option value="TW">台灣</option>
              <option value="US">美國</option>
              <option value="JP">日本</option>
              <option value="KR">韓國</option>
            </select>
          </div>
        </form>
        <div class="button-group">
          <button type="button" class="btn-back" @click="handleBack">返回</button>
          <button type="button" class="btn-continue" @click="handleStep2Continue">繼續</button>
        </div>
      </div>
    </div>

    <!-- 查看預訂 -->
    <div class="step-card" :class="{ 'active': currentStep === 3, 'disabled': currentStep < 3 }">
      <h3>3。查看預訂</h3>

      <!-- 只有在 step3 時才顯示內容 -->
      <div v-if="currentStep >= 3">
        <label>
          <small>點選該按鈕，即代表我同意《預訂條款》。</small>
        </label>
        <div class="button-group">
          <button type="button" class="btn-back" @click="handleBack">返回</button>
          <button type="button" class="btn-continue">確認並付款</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.left-section {
  display: flex;
  flex-direction: column;
  gap: 24px;
  max-width: 100%;

  @media (max-width: 768px) {
    gap: 16px;
  }
}

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

  // 當前進行中的步驟 - 加陰影
  &.active {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    border-color: #222;
  }

  // 已完成的步驟
  &.completed {
    opacity: 0.7;
    background-color: #f7f7f7;
  }

  // 未解鎖的步驟 - 變灰色
  &.disabled {
    opacity: 0.5;
    pointer-events: none;
    background-color: #fafafa;
  }

  .btn-continue {
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

.payment-form {
  margin-top: 16px;
  padding: 16px;
  background: #f9f9f9;
  border-radius: 8px;
}

.payment-method-header {
  display: flex;
  align-items: center;
  gap: 12px;
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

.credit-card-form {
  .form-group {
    margin-bottom: 16px;

    label {
      display: block;
      margin-bottom: 6px;
      font-weight: 500;
      color: #333;
    }

    input, select {
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
    }

    select {
      cursor: pointer;
    }
  }

  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;

    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
  }

  h4 {
    margin: 24px 0 16px 0;
    color: #333;
    font-size: 18px;
    font-weight: 600;
  }
}

// 按鈕群組樣式
.button-group {
  display: flex;
  gap: 12px;
  margin-top: 16px;

  .btn-back {
    flex: 1;
    padding: 14px;
    background: white;
    color: #222;
    border: 1px solid #222;
    border-radius: 8px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;

    &:hover {
      background: #f7f7f7;
    }
  }

  .btn-continue {
    flex: 1;
    margin-top: 0;
  }
}
</style>

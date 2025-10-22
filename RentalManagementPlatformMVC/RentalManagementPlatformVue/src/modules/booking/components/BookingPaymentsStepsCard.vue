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
          <div>立即支付 ${{ bookingStore.totalPrice.toFixed(2) }} TWD</div>
        </label>
      </div>
      <div class="payment-option">
        <input type="radio" id="partial" name="payment" value="partial">
        <label for="partial">
          <div>立即支付 $0 TWD</div>
          <small>已於12月22日收取 ${{ bookingStore.totalPrice.toFixed(2) }} TWD。無須支付額外費用。<a href="#">更多資訊</a></small>
        </label>
      </div>
      <button type="button" class="btn-continue" @click="handleContinue">繼續</button>
    </div>

    <!-- 新增付款方式 -->
    <div class="step-card" :class="{ 'active': currentStep === 2, 'completed': stepCompleted.step2, 'disabled': currentStep < 2 }">
      <h3>2。新增付款方式</h3>

      <!-- 只有在 step2 時才顯示內容 -->
      <div v-if="currentStep >= 2" class="payment-form">
        <p>信用卡資訊即將在這裡顯示...</p>
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
}

.step-card {
  border: 1px solid #ddd;
  border-radius: 30px;
  padding: 30px;
  transition: all 0.3s ease;

  h3 {
    margin-bottom: 16px;
    font-size: 20px;
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

  input[type="radio"] {
    margin-right: 12px;
  }
}

.payment-form {
  margin-top: 16px;
  padding: 16px;
  background: #f9f9f9;
  border-radius: 8px;
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

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useBookingStore } from '@/stores/bookingStore';
import { useRouter } from 'vue-router';
import { getData } from 'country-list';

// ==================== Props ====================
const props = defineProps({
  totalPrice: {
    type: Number,
    required: true
  }
});

// ==================== 狀態管理 ====================
const bookingStore = useBookingStore();
const router = useRouter();

// 當前進行的步驟 (1, 2, 3)
const currentStep = ref(1);

// 每個步驟的完成狀態
const stepCompleted = ref({
  step1: false,
  step2: false,
  step3: false
});

// 用戶選擇的付款時間 ('full' = 立即支付, 'partial' = 延後支付)
const selectedPaymentTiming = ref('full');

// ==================== 表單資料 ====================
// 聯絡資訊
const billingInfo = ref({
  name: '',
  email: '',
  phone: '',
  notes: ''
});

// 帳單地址
const billingAddress = ref({
  street: '',
  apartment: '',
  city: '',
  state: '',
  zipCode: '',
  country: 'TW'
});

// 國家列表 (從 country-list 套件獲取)
const countries = ref(
  getData().map(country => ({
    code: country.code,
    name: country.name
  }))
);

// ==================== 工具函數 ====================
/**
 * 格式化日期
 * @param {string} dateStr - ISO 日期字串
 * @returns {string} 格式化後的日期
 */
const formatDate = (dateStr) => {
  const date = new Date(dateStr);
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

// ==================== 步驟控制函數 ====================
/**
 * Step 1 繼續按鈕：完成 Step 1 並進入 Step 2
 */
const handleContinue = () => {
  stepCompleted.value.step1 = true
  currentStep.value = 2
};

/**
 * Step 2 繼續按鈕：完成 Step 2 並進入 Step 3
 */
const handleStep2Continue = () => {
  // 簡單驗證
  if (!billingInfo.value.name || !billingInfo.value.email || !billingInfo.value.phone) {
    alert('請填寫完整的聯絡資訊');
    return;
  };

  if (!billingAddress.value.street || !billingAddress.value.city || !billingAddress.value.zipCode) {
    alert('請填寫完整的帳單地址');
    return;
  };

  stepCompleted.value.step2 = true;
  currentStep.value = 3;
};

/**
 * 返回上一步
 */
const handleBack = () => {
  if (currentStep.value > 1) {
    currentStep.value--;

    // 取消當前步驟的完成狀態
    if (currentStep.value === 1) {
      stepCompleted.value.step1 = false;
    } else if (currentStep.value === 2) {
      stepCompleted.value.step2 = false;
    };
  };
};

/**
 * 確認並付款
 */
const handleConfirmPayment = async () => {
  try {
    if (!bookingStore.hasBookingDraft) {
      alert('訂房資料不完整，請先載入測試資料');
      return;
    };

    console.log('準備送出訂單...');

    const paymentData = {
      finalAmount: props.totalPrice,
      paymentTiming: selectedPaymentTiming.value,
      billingInfo: {
        name: billingInfo.value.name,
        email: billingInfo.value.email,
        phone: billingInfo.value.phone,
        notes: billingInfo.value.notes || ''
      },
      billingAddress: {
        country: billingAddress.value.country,
        street: billingAddress.value.street,
        apartment: billingAddress.value.apartment || '',
        city: billingAddress.value.city,
        state: billingAddress.value.state || '',
        zipCode: billingAddress.value.zipCode
      }
    };

    // 呼叫 Store 的方法建立訂單
    const result = await bookingStore.createBooking(paymentData);

    // 判斷後端回傳的結果
    if (result.paymentRequired) {
      // ----- 情況 1：選擇立即付款 -----
      console.log('訂單建立成功，選擇立即付款！');
      console.log('訂單編號：', result.orderNumber);

      // 將綠界付款表單插入頁面並自動提交
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = result.ecpayFormHtml;
      document.body.appendChild(tempDiv);
      console.log('綠界表單已插入 DOM');

      const form = tempDiv.querySelector('form');
      if (form) {
        console.log('找到綠界表單，準備提交...');
        // 延遲提交以確保表單已完全載入
        setTimeout(() => {
          form.submit();
          console.log('表單已提交！');
        }, 500);
      } else {
        console.error('找不到綠界付款表單！');
        alert('無法找到付款表單，請聯繫客服');
      };

    } else {
      // ----- 情況 2：選擇延後付款 -----
      console.log('訂單建立成功，選擇延後付款！');
      console.log('訂單編號：', result.orderNumber);
      alert('訂單已成功建立！您選擇了延後付款，頁面將跳轉至「我的訂單」。');

      // 跳轉到「我的訂單」頁面
      await router.push('/booking/mybookings');

      // 清除 booking store 中的草稿資料
      bookingStore.clearBookingDraft();
    };

  } catch (error) {
    console.error('建立訂單失敗', error);

    if (error.response) {
      const errorMessage = error.response.data?.message || '訂單建立失敗';
      alert(`錯誤：${errorMessage}`);
    } else if (error.request) {
      alert('網路連線失敗，請檢查網路後再試');
    } else {
      alert('訂單建立失敗，請稍後再試');
    };
  };
};

// ==================== 開發測試：自動載入資料 ====================
onMounted(() => {
  // 設定聯絡資訊
  billingInfo.value = {
    name: '王小明',
    email: 'test@example.com',
    phone: '0912-345-678',
    notes: '提早入住'
  };

  // 設定帳單地址
  billingAddress.value = {
    country: 'TW',
    street: '信義路五段7號',
    apartment: '10樓之1',
    city: '台北市',
    state: '台北市',
    zipCode: '110'
  };

  console.log('測試房客資料已載入');
  console.log('billingInfo:', billingInfo.value);
  console.log('billingAddress:', billingAddress.value);
});
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
          @click="currentStep = 1; stepCompleted.step1 = false; stepCompleted.step2 = false"
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
            <div>立即支付 ${{ totalPrice.toFixed(2) }} TWD</div>
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
              將於 {{ bookingStore.refundableDate }} 收取 ${{ totalPrice.toFixed(2) }} TWD。無須支付額外費用。
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
          立即支付 ${{ totalPrice.toFixed(2) }} TWD
        </p>
        <p v-else>
          已於12月23日收取 ${{ totalPrice.toFixed(2) }} TWD。無須支付額外費用。
        </p>
      </div>
    </div>

    <!-- ==================== Step 2: 付款資訊 ==================== -->
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
        <h3>2。付款資訊</h3>
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

        <!-- 付款方式說明 -->
        <div class="payment-method-header">
          <div class="payment-icon"><i class="fa-solid fa-credit-card"></i></div>
          <div>
            <strong>信用卡付款</strong>
            <div class="payment-description">
              <p class="text-muted">
                點擊「繼續」後，您將被導向綠界安全付款頁面完成信用卡付款
              </p>
              <div class="security-badges">
                <span class="badge"><i class="fa-solid fa-lock"></i> SSL 安全加密</span>
                <span class="badge"><i class="fa-solid fa-check"></i> PCI DSS 認證</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 支援的信用卡 -->
        <div class="supported-cards">
          <span class="label">支援卡別：</span>
          <div class="card-logos">
            <img src="https://upload.wikimedia.org/wikipedia/commons/4/41/Visa_Logo.png" alt="VISA">
            <img src="https://upload.wikimedia.org/wikipedia/commons/2/2a/Mastercard-logo.svg" alt="Mastercard">
            <img src="https://upload.wikimedia.org/wikipedia/commons/f/fa/American_Express_logo_%282018%29.svg" alt="AMEX">
            <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/4/40/JCB_logo.svg/320px-JCB_logo.svg.png" alt="JCB">
          </div>
        </div>

        <!-- 表單內容 -->
        <form class="billing-form">

          <!-- 聯絡資訊區塊 -->
          <h4>
            <i class="fa-solid fa-user"></i>
            聯絡資訊
          </h4>

          <div class="form-group">
            <label>姓名 <span class="required">*</span></label>
            <input
              type="text"
              v-model="billingInfo.name"
              placeholder="請輸入姓名"
              required
            >
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Email <span class="required">*</span></label>
              <input
                type="email"
                v-model="billingInfo.email"
                placeholder="example@email.com"
                required
              >
            </div>
            <div class="form-group">
              <label>電話 <span class="required">*</span></label>
              <input
                type="tel"
                v-model="billingInfo.phone"
                placeholder="0912-345-678"
                required
              >
            </div>
          </div>

          <!-- 帳單地址區塊 -->
          <h4>
            <i class="fa-solid fa-location-dot"></i>
            帳單地址
          </h4>

          <div class="form-group">
            <label>國家 / 地區 <span class="required">*</span></label>
            <select v-model="billingAddress.country" required>
              <option
                v-for="country in countries"
                :key="country.code"
                :value="country.code"
              >
                {{ country.name }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>城市 <span class="required">*</span></label>
            <input
              type="text"
              v-model="billingAddress.city"
              placeholder="請輸入城市"
              required
            >
          </div>

          <div class="form-group">
            <label>街道地址 <span class="required">*</span></label>
            <input
              type="text"
              v-model="billingAddress.street"
              placeholder="請輸入街道地址"
              required
            >
          </div>

          <div class="form-group">
            <label>公寓或套房號碼</label>
            <input
              type="text"
              v-model="billingAddress.apartment"
              placeholder="公寓、套房號碼（選填）"
            >
          </div>

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
                required
              >
            </div>
          </div>

          <!-- 特殊需求 -->
          <div class="form-group">
            <label>
              <i class="fa-solid fa-comment"></i>
              特殊需求或備註（選填）
            </label>
            <textarea
              v-model="billingInfo.notes"
              placeholder="例如：提早入住、加床服務等"
              rows="3"
              style="resize: none;"
            ></textarea>
          </div>
        </form>

        <!-- 重要提醒 -->
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

        <!-- 取消政策提醒 -->
        <div class="cancellation-reminder">
          <i class="fa-solid fa-shield-halved"></i>
          <span>
            <strong>取消政策：</strong>
            {{ bookingStore.refundableDate }} 前可免費取消
          </span>
        </div>

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
        <div class="summary-icon"><i class="fa-solid fa-check"></i></div>
        <div>
          <p><strong>{{ billingInfo.name }}</strong></p>
          <p class="text-muted">{{ billingInfo.email }}</p>
          <p class="text-muted">{{ billingAddress.city }}, {{ billingAddress.country }}</p>
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
      <div v-if="currentStep === 3">
        <label>
          <small>
            點選該按鈕,即代表我同意
            <a href="#">《預訂條款》</a>。
          </small>
        </label>

        <!-- 按鈕組 -->
        <div class="button-group">
          <button type="button" class="btn-back" @click="handleBack" :disabled="bookingStore.isLoading">
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
  color: #4caf50;
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
    display: inline-block;
    padding: 4px 12px;
    background: #e8f5e9;
    color: #2e7d32;
    border-radius: 16px;
    font-size: 12px;
    font-weight: 500;
  }
}

/* 支援的信用卡 */
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

/* 表單標題 */
.billing-form h4 {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 28px 0 16px 0;
  padding-bottom: 12px;
  border-bottom: 2px solid #f0f0f0;
  color: #333;
  font-size: 18px;
  font-weight: 600;

  i {
    color: #666;
  }
}

/* 必填標記 */
.required {
  color: #e53e3e;
  margin-left: 4px;
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

    i {
      margin-right: 4px;
      color: #666;
    }
  }

  input,
  select,
  textarea {
    width: 100%;
    padding: 12px;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 16px;
    transition: border-color 0.2s;
    font-family: inherit;

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

  textarea {
    resize: vertical;
    min-height: 80px;
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

/* 資訊提示框 */
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

/* 取消政策提醒 */
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
}

/* 按鈕內的 loading spinner */
.loading-spinner-btn {
  width: 16px;
  height: 16px;
  border: 2px solid transparent;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>

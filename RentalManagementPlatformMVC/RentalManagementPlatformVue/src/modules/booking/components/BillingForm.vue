<script setup>
import { ref, onMounted } from 'vue';
import { getData } from 'country-list';
import { useBookingValidation } from '@/composables/useBookingValidation';

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({
      billingInfo: {
        name: '',
        email: '',
        phone: '',
        notes: ''
      },
      billingAddress: {
        street: '',
        apartment: '',
        city: '',
        state: '',
        zipCode: '',
        country: 'TW'
      }
    })
  }
});

const emit = defineEmits(['update:modelValue', 'validation-change']);

const { validateBillingInfo, validateBillingAddress } = useBookingValidation();

// 國家列表
const countries = ref(
  getData().map(country => ({
    code: country.code,
    name: country.name
  }))
);

// 本地資料
const localData = ref({ ...props.modelValue });

// 驗證結果
const validationResults = ref({
  billingInfo: { isValid: false, errors: {} },
  billingAddress: { isValid: false, errors: {} }
});

// 監聽資料變更並觸發驗證
const updateAndValidate = () => {
  // 驗證聯絡資訊
  validationResults.value.billingInfo = validateBillingInfo(localData.value.billingInfo);

  // 驗證帳單地址
  validationResults.value.billingAddress = validateBillingAddress(localData.value.billingAddress);

  // 發送更新事件
  emit('update:modelValue', localData.value);

  // 發送驗證狀態
  const isOverallValid = validationResults.value.billingInfo.isValid && validationResults.value.billingAddress.isValid;

  emit('validation-change', {
    isValid: isOverallValid,
    errors: {
      ...validationResults.value.billingInfo.errors,
      ...validationResults.value.billingAddress.errors
    }
  });
};

// 暴露驗證方法
const validate = () => {
  updateAndValidate();
  return validationResults.value.billingInfo.isValid && validationResults.value.billingAddress.isValid;
};

defineExpose({
  validate
});

onMounted(() => {
  localData.value = {
    billingInfo: {
      name: '李文志',
      email: 'aa39275814@gmail.com',
      phone: '0916-315-915',
      notes: '提早入住'
    },
    billingAddress: {
      country: 'TW',
      street: '信義路五段7號',
      apartment: '10樓之1',
      city: '臺北市信義區',
      state: '臺灣省',
      zipCode: '110'
    }
  };
  updateAndValidate();
});
</script>

<template>
  <form class="billing-form">
    <h4><i class="fa-solid fa-user"></i> 聯絡資訊</h4>

    <div class="form-group">
      <label>姓名 <span class="required">*</span></label>
      <input
        type="text"
        v-model="localData.billingInfo.name"
        @input="updateAndValidate"
        placeholder="請輸入姓名"
        :class="{ 'is-invalid': validationResults.billingInfo.errors.name }"
        required
      >
      <div v-if="validationResults.billingInfo.errors.name" class="invalid-feedback">
        {{ validationResults.billingInfo.errors.name }}
      </div>
    </div>

    <div class="form-row">
      <div class="form-group">
        <label>Email <span class="required">*</span></label>
        <input
          type="email"
          v-model="localData.billingInfo.email"
          @input="updateAndValidate"
          placeholder="example@email.com"
          :class="{ 'is-invalid': validationResults.billingInfo.errors.email }"
          required
        >
        <div v-if="validationResults.billingInfo.errors.email" class="invalid-feedback">
          {{ validationResults.billingInfo.errors.email }}
        </div>
      </div>
      <div class="form-group">
        <label>電話 <span class="required">*</span></label>
        <input
          type="tel"
          v-model="localData.billingInfo.phone"
          @input="updateAndValidate"
          placeholder="0912-345-678"
          :class="{ 'is-invalid': validationResults.billingInfo.errors.phone }"
          required
        >
        <div v-if="validationResults.billingInfo.errors.phone" class="invalid-feedback">
          {{ validationResults.billingInfo.errors.phone }}
        </div>
      </div>
    </div>

    <h4><i class="fa-solid fa-location-dot"></i> 帳單地址</h4>

    <div class="form-group">
      <label>國家 / 地區 <span class="required">*</span></label>
      <select
        v-model="localData.billingAddress.country"
        @change="updateAndValidate"
        :class="{ 'is-invalid': validationResults.billingAddress.errors.country }"
        required
      >
        <option v-for="country in countries" :key="country.code" :value="country.code">
          {{ country.name }}
        </option>
      </select>
      <div v-if="validationResults.billingAddress.errors.country" class="invalid-feedback">
        {{ validationResults.billingAddress.errors.country }}
      </div>
    </div>

    <div class="form-group">
      <label>城市 <span class="required">*</span></label>
      <input
        type="text"
        v-model="localData.billingAddress.city"
        @input="updateAndValidate"
        placeholder="請輸入城市"
        :class="{ 'is-invalid': validationResults.billingAddress.errors.city }"
        required
      >
      <div v-if="validationResults.billingAddress.errors.city" class="invalid-feedback">
        {{ validationResults.billingAddress.errors.city }}
      </div>
    </div>

    <div class="form-group">
      <label>街道地址 <span class="required">*</span></label>
      <input
        type="text"
        v-model="localData.billingAddress.street"
        @input="updateAndValidate"
        placeholder="請輸入街道地址"
        :class="{ 'is-invalid': validationResults.billingAddress.errors.street }"
        required
      >
      <div v-if="validationResults.billingAddress.errors.street" class="invalid-feedback">
        {{ validationResults.billingAddress.errors.street }}
      </div>
    </div>

    <div class="form-group">
      <label>公寓或套房號碼</label>
      <input
        type="text"
        v-model="localData.billingAddress.apartment"
        @input="updateAndValidate"
        placeholder="公寓、套房號碼（選填）"
      >
    </div>

    <div class="form-row">
      <div class="form-group">
        <label>省份 / 直轄市 / 州</label>
        <input
          type="text"
          v-model="localData.billingAddress.state"
          @input="updateAndValidate"
          placeholder="省份"
        >
      </div>
      <div class="form-group">
        <label>郵遞區號 <span class="required">*</span></label>
        <input
          type="text"
          v-model="localData.billingAddress.zipCode"
          @input="updateAndValidate"
          placeholder="郵遞區號"
          :class="{ 'is-invalid': validationResults.billingAddress.errors.zipCode }"
          required
        >
        <div v-if="validationResults.billingAddress.errors.zipCode" class="invalid-feedback">
          {{ validationResults.billingAddress.errors.zipCode }}
        </div>
      </div>
    </div>

    <div class="form-group">
      <label><i class="fa-solid fa-comment"></i> 特殊需求或備註（選填）</label>
      <textarea
        v-model="localData.billingInfo.notes"
        @input="updateAndValidate"
        placeholder="例如：提早入住、加床服務等"
        rows="3"
      ></textarea>
    </div>
  </form>
</template>

<style lang="scss" scoped>
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

.required {
  color: #e53e3e;
  margin-left: 4px;
}

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

  input, select, textarea {
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

    &.is-invalid {
      border-color: #dc3545;
      box-shadow: 0 0 0 2px rgba(220, 53, 69, 0.1);
    }
  }

  select {
    cursor: pointer;
  }

  textarea {
    resize: vertical;
    min-height: 80px;
  }

  .invalid-feedback {
    color: #dc3545;
    font-size: 14px;
    margin-top: 4px;
    display: block;
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
</style>

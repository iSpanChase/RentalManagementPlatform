<template>
  <Form @submit="handleSubmit" :initial-values="initialData" v-slot="{ isSubmitting, values }">
    <div class="mb-3">
      <label for="title" class="form-label">房源標題</label>
      <Field name="title" type="text" class="form-control" :rules="isRequired" />
      <ErrorMessage name="title" class="text-danger" />
    </div>

    <div class="mb-3">
      <label for="description" class="form-label">房源描述</label>
      <Field name="description" as="textarea" class="form-control" :rules="isRequired" rows="5" />
      <ErrorMessage name="description" class="text-danger" />
    </div>

    <!-- Address Fields -->
    <div class="row">
        <div class="col-md-6 mb-3">
            <label for="cityId" class="form-label">城市</label>
            <template v-if="mode === 'create'">
                <Field
                    name="cityId"
                    as="select"
                    class="form-select"
                    :rules="isRequired"
                    @change="handleCityChange($event.target.value)"
                >
                    <option value="" disabled>請選擇城市</option>
                    <option v-for="city in cities" :key="city.cityId" :value="city.cityId">
                        {{ city.cityName }}
                    </option>
                </Field>
                <ErrorMessage name="cityId" class="text-danger" />
            </template>
            <template v-else>
                <input
                    type="text"
                    class="form-control"
                    :value="initialData?.cityName ?? ''"
                    disabled
                />
            </template>
        </div>
        <div class="col-md-6 mb-3">
            <label for="districtId" class="form-label">區域</label>
            <template v-if="mode === 'create'">
                <Field
                    name="districtId"
                    as="select"
                    class="form-select"
                    :disabled="!values.cityId || districtsLoading"
                    :rules="isRequired"
                >
                    <option value="" disabled>請先選擇城市</option>
                    <option v-if="districtsLoading" value="" disabled>載入中...</option>
                    <option v-for="district in districts" :key="district.districtId" :value="district.districtId">
                        {{ district.districtName }}
                    </option>
                </Field>
                <ErrorMessage name="districtId" class="text-danger" />
            </template>
            <template v-else>
                <input
                    type="text"
                    class="form-control"
                    :value="initialData?.districtName ?? ''"
                    disabled
                />
            </template>
        </div>
    </div>
    <div class="mb-3">
        <label for="street" class="form-label">街道地址</label>
        <template v-if="mode === 'create'">
            <Field name="street" type="text" class="form-control" :rules="isRequired" />
            <ErrorMessage name="street" class="text-danger" />
        </template>
        <template v-else>
            <input
                type="text"
                class="form-control"
                :value="initialData?.addressLine ?? initialData?.street ?? ''"
                disabled
            />
        </template>
    </div>

    <div class="row">
      <div class="col-md-6 mb-3">
        <label for="pricePerNight" class="form-label">每晚價格</label>
        <Field name="pricePerNight" type="number" class="form-control" :rules="isPositiveNumber" />
        <ErrorMessage name="pricePerNight" class="text-danger" />
      </div>
      <div class="col-md-6 mb-3">
        <label for="maxGuests" class="form-label">最多入住人數</label>
        <Field name="maxGuests" type="number" class="form-control" :rules="isPositiveNumber" />
        <ErrorMessage name="maxGuests" class="text-danger" />
      </div>
    </div>

    <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
      <span v-if="isSubmitting" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
      {{ isSubmitting ? '儲存中...' : '下一步' }}
    </button>
  </Form>
</template>

<script setup>
import { Form, Field, ErrorMessage } from 'vee-validate';

// VeeValidate validation rules
const isRequired = value => (value ? true : '此欄位為必填');
const isPositiveNumber = value => (value > 0 ? true : '必須大於 0');

// Component props and emits
const props = defineProps({
  initialData: {
    type: Object,
    default: () => ({})
  },
  cities: {
    type: Array,
    required: true
  },
  districts: {
    type: Array,
    required: true
  },
  districtsLoading: {
    type: Boolean,
    default: false
  },
  mode: {
    type: String,
    default: 'create' // 'create' or 'edit'
  }
});

const emit = defineEmits(['submit', 'city-changed']);

// Handle form submission
function handleSubmit(values) {
  emit('submit', values);
}

function handleCityChange(cityId) {
    emit('city-changed', cityId)
}

</script>

<style scoped>
.text-danger {
  color: #dc3545;
  font-size: 0.875em;
  margin-top: 0.25rem;
}
</style>

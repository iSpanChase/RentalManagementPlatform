<script setup lang="ts">
import { computed, ref, watch } from 'vue'

interface Props {
  /** 頭像 URL */
  avatarUrl?: string | null
  /** 使用者名稱 (用於生成縮寫) */
  displayName?: string | null
  /** 頭像大小 (像素) */
  size?: number
  /** 更新時間 (用於緩存破解) */
  updatedAt?: string | null
  /** 額外的 CSS class */
  class?: string
  /** 是否顯示邊框 */
  showBorder?: boolean
  /** 邊框顏色 */
  borderColor?: string
}

const props = withDefaults(defineProps<Props>(), {
  avatarUrl: '',
  displayName: '訪客',
  size: 40,
  updatedAt: '',
  class: '',
  showBorder: true,
  borderColor: '#EBEBEB'
})

/** 載入失敗標記 */
const imageFailed = ref(false)

/** 處理頭像載入錯誤 */
const onImageError = () => {
  imageFailed.value = true
}

/** 清理失敗標記，重新嘗試載入 */
watch(() => props.avatarUrl, () => {
  imageFailed.value = false
})

/** 帶版本參數的頭像 URL (避免緩存) */
const versionedAvatarUrl = computed(() => {
  const url = props.avatarUrl
  if (!url) return ''

  const version = props.updatedAt
    ? new Date(props.updatedAt).getTime()
    : Date.now()

  return url.includes('?') ? `${url}&v=${version}` : `${url}?v=${version}`
})

/** 生成使用者名稱縮寫 */
const userInitials = computed(() => {
  const name = (props.displayName || '').trim()
  if (!name) return 'U'

  const parts = name.split(/\s+/)
  const first = parts[0]?.[0] ?? name[0] ?? ''
  const last = parts.length >= 2 ? (parts[parts.length - 1]?.[0] ?? '') : ''
  const combo = (first + last).toUpperCase()

  return combo || (name[0]?.toUpperCase() ?? 'U')
})

/** 快取破解 key */
const avatarKey = computed(() => {
  const url = props.avatarUrl ?? ''
  const time = props.updatedAt ?? ''
  return `${url}-${time}`
})

/** 動態樣式 */
const avatarStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size}px`,
  border: props.showBorder ? `2px solid ${props.borderColor}` : 'none',
  fontSize: `${Math.max(12, props.size * 0.3)}px`
}))
</script>

<template>
  <div
    class="avatar-display"
    :class="props.class"
    :style="avatarStyle"
  >
    <img
      v-if="versionedAvatarUrl && !imageFailed"
      :key="avatarKey"
      :src="versionedAvatarUrl"
      :alt="displayName"
      class="avatar-image"
      @error="onImageError"
    />
    <span v-else class="avatar-initials">
      {{ userInitials }}
    </span>
  </div>
</template>

<style lang="scss" scoped>
.avatar-display {
  background: #BE9A78;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 600;
  overflow: hidden;
  position: relative;
  flex-shrink: 0;

  .avatar-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 50%;
  }

  .avatar-initials {
    font-weight: 600;
    line-height: 1;
  }
}
</style>
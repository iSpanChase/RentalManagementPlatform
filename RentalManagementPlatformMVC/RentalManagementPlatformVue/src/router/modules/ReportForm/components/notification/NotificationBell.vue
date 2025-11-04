<template>
    <div class="dropdown notification-bell">
    <!-- 鈴鐺按鈕，用來觸發下拉選單 -->
    <button
        class="btn btn-secondary dropdown-toggle"
        type="button"
        id="notificationDropdown"
        data-bs-toggle="dropdown"
        aria-expanded="false"
        @click="handleDropdownOpen"
    >
        <!-- 鈴鐺 SVG 圖示 -->
        <FontAwesomeIcon :icon="faBell" />

        <!-- 未讀紅點，當有未讀訊息時顯示 -->
        <span v-if="notificationStore.hasUnread" class="red-dot"></span>
    </button>

    <!-- 下拉通知列表 -->
    <ul class="dropdown-menu" aria-labelledby="notificationDropdown">
        <li v-if="notificationStore.notifications.length === 0" class="dropdown-item text-muted">
        沒有任何通知
        </li>
        <li v-for="notification in notificationStore.notifications" :key="notification.id">
        <div class="dropdown-item d-flex justify-content-between align-items-start">
            <span>{{ notification.message }}</span>
            <!-- 關閉按鈕 -->
            <button class="btn-close ms-2" @click.stop="handleRemove(notification.id)"></button>
        </div>
        </li>
    </ul>
    </div>
</template>

<script setup lang="ts">
    import { faBell } from '@fortawesome/free-solid-svg-icons'
    import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
    import { useNotificationStore } from '@/stores/notificationStore';

    // 1. 獲取 Pinia store 的實例
    // 呼叫hook，使元件與全域狀態連接
    const notificationStore = useNotificationStore();

    // 2. 處理下拉選單打開事件
    // 當使用者點擊鈴鐺時，呼叫 store 中的 markAsRead action 使紅點就會消失
    function handleDropdownOpen() {
        notificationStore.markAsRead();
    }

    // 3. 處理移除通知事件
    // 當使用者點擊 'x' 按鈕時，呼叫 store 中的 removeNotification action 傳入該通知的 ID
    function handleRemove(id: number) {
        notificationStore.removeNotification(id);
    }
</script>

<style scoped>
    .notification-bell {
        position: relative;
    }

    .red-dot {
        position: absolute;
        top: 5px;
        right: 5px;
        width: 8px;
        height: 8px;
        background-color: red;
        border-radius: 50%;
        border: 1px solid white;
    }

    .dropdown-menu {
        /* 讓列表可以容納多則訊息並滾動 */
        max-height: 400px;
        overflow-y: auto;
    }

    .dropdown-item {
        /* 讓文字可以自動換行 */
        white-space: normal;
    }
</style>

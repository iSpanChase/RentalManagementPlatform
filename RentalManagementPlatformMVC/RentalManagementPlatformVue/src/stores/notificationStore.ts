// src/stores/notificationStore.ts
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

// 定義單一通知的結構
export interface Notification {
    id: number;
    message: string;
}

// 使用 defineStore 來定義一個 store
// 第一個參數 'notifications' 是這個 store 的唯一 ID
export const useNotificationStore = defineStore('notifications', () => {
    // --- State (狀態) ---
    // 使用 ref() 來定義響應式的狀態屬性，等同於 Pinia 的 state
    const notifications = ref<Notification[]>([]);
    const hasUnread = ref<boolean>(false);

    // --- Actions (方法) ---
    // Actions 是可以修改 state 的方法

    // 從 localStorage 載入通知
    function loadFromLocalStorage() {
        const storedNotifications = localStorage.getItem('notifications');
        if (storedNotifications) {
            notifications.value = JSON.parse(storedNotifications);
        }
    }

    // 新增一則通知
    function addNotification(message: string) {
        const newNotification: Notification = {
            id: Date.now(), // 使用時間戳作為唯一 ID
            message: message,
        };
        notifications.value.unshift(newNotification); // 加到列表最前面
        hasUnread.value = true; // 標記為有未讀訊息

        // 同步到 localStorage
        localStorage.setItem('notifications', JSON.stringify(notifications.value));
    }

    // 移除一則通知
    function removeNotification(id: number) {
        notifications.value = notifications.value.filter(n => n.id !== id);

        // 同步到 localStorage
        localStorage.setItem('notifications', JSON.stringify(notifications.value));
    }

    // 標記為已讀 (清除紅點)
    function markAsRead() {
        hasUnread.value = false;
    }

    // --- Getters (計算屬性) ---
    // Getters 是基於 state 的計算屬性，等同於 Pinia 的 getters
    const unreadCount = computed(() => notifications.value.length);

    // 最後，回傳所有需要在元件中使用的 state、actions 和 getters
    return {
        notifications,
        hasUnread,
        unreadCount,
        loadFromLocalStorage,
        addNotification,
        removeNotification,
        markAsRead,
    };
});
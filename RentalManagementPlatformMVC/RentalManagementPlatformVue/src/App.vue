<script setup lang="ts">
import { onMounted } from 'vue';
import { useAppToggle } from './composables/useAppToggle';
import TheFooter from './components/TheFooter.vue';
import TheHeader from './components/TheHeader.vue';
import ThePreloader from './components/ThePreloader.vue';
import TheHiddenSidebar from './components/TheHiddenSidebar.vue';
import SearchPopup from './components/SearchPopup.vue';
import ScrollToTopButton from './components/ScrollToTopButton.vue';
import TempNavComponent from './components/TempNavComponent.vue';
import { startConnection, registerWarningHandler } from './modules/ReportForm/api/notificationService.ts';
import NotificationBell from '@/modules/ReportForm/components/notification/NotificationBell.vue'
import { useNotificationStore } from '@/stores/notificationStore';
import ChatFloating from './components/ChatFloating.vue'
import { useAuthStore } from '@/stores/auth';

const { isSearchPopupOpen, isSidebarOpen, isMobileMenuOpen, handleToggleSearch, handleToggleSidebar, handleToggleMobileMenu } = useAppToggle();
const notificationStore = useNotificationStore(); // 獲取 store 實例

//SignalR 獲取通知
onMounted(async () => {
    // 1. 應用程式啟動時，立即從 localStorage 載入通知
    notificationStore.loadFromLocalStorage();

    // 2. 獲取登入者ID
    const authStore = useAuthStore();
    const currentUserId = authStore.state.profile?.userId;

    if (currentUserId) {
        // 3. 開始 SignalR 連線
        try {
            const userId = 47;
            await startConnection(userId);
            console.log("SignalR 連線成功，並開始監聽通知...");

            // 3. 註冊 SignalR 處理器，收到訊息時呼叫 store 的 action
            registerWarningHandler((message: string) => {
                console.log(`收到新通知: ${message}`);
                notificationStore.addNotification(message);
            });
        }
        catch (err) {
            console.error("SignalR 連線失敗: ", err);
        }
    }
    else{
        console.log("使用者未登入，跳過 SignalR 連線。");
    }
});
</script>

<template>
    <div class="page-wrapper" :class="{'mobile-menu-visible': isMobileMenuOpen}">

        <!-- 通知小鈴鐺 -->
        <div class="global-notification-bell">
            <NotificationBell />
        </div>


        <ThePreloader />


        <!-- <TheHeader
            :isSearchPopupOpen="isSearchPopupOpen"
            :isSidebarOpen="isSidebarOpen"
            :isMobileMenuOpen="isMobileMenuOpen"
            @toggle-search="handleToggleSearch"
            @toggle-sidebar="handleToggleSidebar"
            @toggle-mobile-menu="handleToggleMobileMenu"
        /> -->

        <!-- <TheHiddenSidebar v-if="isSidebarOpen" @close-sidebar="handleToggleSidebar" />
        <SearchPopup v-if="isSearchPopupOpen" @close-search="handleToggleSearch" />
        <TheMobileMenu v-if="isMobileMenuOpen" @close-mobile-menu="handleToggleMobileMenu" /> -->

        <!-- <TempNavComponent></TempNavComponent> -->
        <!-- <RouterLink class="col-2" to="/">Home</RouterLink> -->
        <RouterView />

        <!-- <TheFooter /> -->

    </div>
     <ChatFloating />
    <ScrollToTopButton />
</template>

<style scoped>
/* 通知鈴鐺固定在右下角 */
.global-notification-bell {
  position: fixed;
  bottom: 30px;
  left: 30px;
  z-index: 1000;
  transition: all 0.3s ease;
}

@media (max-width: 768px) {
  .global-notification-bell {
    bottom: 20px;
    right: 20px;
  }
}

@media (max-width: 480px) {
  .global-notification-bell {
    bottom: 80px;
    right: 15px;
  }
}
</style>
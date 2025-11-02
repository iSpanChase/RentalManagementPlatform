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
import { startConnection, registerWarningHandler } from './router/modules/ReportForm/api/notificationService.ts';

const { isSearchPopupOpen, isSidebarOpen, isMobileMenuOpen, handleToggleSearch, handleToggleSidebar, handleToggleMobileMenu } = useAppToggle();

//SignalR 獲取通知
onMounted(async () => {
    try {
        const userId = 47;
        await startConnection(userId);

        // 註冊一個處理器，當收到訊息時，用 alert 彈窗顯示
        registerWarningHandler((message: string) => {
            alert(`[即時通知]\n---------------------------------\n${message}`);
        });
    }
    catch (err) {
        console.error("SignalR 連線失敗: ", err);
    }
});
</script>

<template>
    <div class="page-wrapper" :class="{'mobile-menu-visible': isMobileMenuOpen}">

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

    <ScrollToTopButton />
</template>

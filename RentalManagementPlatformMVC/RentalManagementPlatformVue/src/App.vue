<script setup lang="ts">
import { useAppToggle } from './composables/useAppToggle';
import TheFooter from './components/TheFooter.vue';
import TheHeader from './components/TheHeader.vue';
import ThePreloader from './components/ThePreloader.vue';
import TheHiddenSidebar from './components/TheHiddenSidebar.vue';
import SearchPopup from './components/SearchPopup.vue';
import ScrollToTopButton from './components/ScrollToTopButton.vue';
import TempNavComponent from './components/TempNavComponent.vue';


const { isSearchPopupOpen, isSidebarOpen, isMobileMenuOpen, handleToggleSearch, handleToggleSidebar, handleToggleMobileMenu } = useAppToggle();
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

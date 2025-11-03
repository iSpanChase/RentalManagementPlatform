<script setup lang="ts">
import { useWindowScroll } from '@vueuse/core';
import { computed } from 'vue';

const { y } = useWindowScroll(); // Get current scroll position

// Show button when scrolled down more than 200px
const showButton = computed(() => y.value > 200);

// Smoothly scroll back to the top
const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
};
</script>

<template>
    <!--Scroll to top-->
    <div class="scroll-to-top scroll-to-target" v-show="showButton" @click="scrollToTop"><span class="fas fa-arrow-up"></span></div>
</template>

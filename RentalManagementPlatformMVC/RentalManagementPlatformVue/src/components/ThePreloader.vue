<script setup lang="ts">
import { ref, onMounted } from 'vue';

const showPreloader = ref(true);

onMounted(() => {
    // Simulate loading time or wait for other assets
    setTimeout(() => {
        showPreloader.value = false;
    }, 500); // Hide after 500ms
});
</script>

<template>
    <!-- preloader -->
    <div id="preloader" class="preloader" v-if="showPreloader">
        <div class="animation-preloader">
            <div class="spinner">
            </div>
        </div>
        <div class="loader">
            <div class="row">
                <div class="col-3 loader-section section-left">
                    <div class="bg"></div>
                </div>
                <div class="col-3 loader-section section-left">
                    <div class="bg"></div>
                </div>
                <div class="col-3 loader-section section-right">
                    <div class="bg"></div>
                </div>
                <div class="col-3 loader-section section-right">
                    <div class="bg"></div>
                </div>
            </div>
        </div>
    </div>
</template>
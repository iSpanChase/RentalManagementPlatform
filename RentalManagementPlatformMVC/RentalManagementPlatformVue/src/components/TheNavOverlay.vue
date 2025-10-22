<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useMouse, useEventListener, useElementHover } from '@vueuse/core'; // Added useEventListener, useElementHover

const { x, y } = useMouse(); // Get global mouse coordinates

// Cursor position (direct follow)
const cursorX = ref(0);
const cursorY = ref(0);

// Follower position (lagged follow)
const followerX = ref(0);
const followerY = ref(0);

// Update cursor position directly
watch([x, y], ([newX, newY]) => {
    cursorX.value = newX;
    cursorY.value = newY;
});

// Update follower position with easing (lag effect)
const easeFactor = 0.15; // Adjust for desired lag

watch([x, y], ([newX, newY]) => {
    followerX.value += (newX - followerX.value) * easeFactor;
    followerY.value += (newY - followerY.value) * easeFactor;
});

// Hover states
const isHoveringInteractive = ref(false); // For buttons, links
const navOverlayRef = ref<HTMLElement | null>(null); // Ref for the nav-overlay div
const isHoveringOverlay = useElementHover(navOverlayRef); // For the nav-overlay itself

// Global event listeners for interactive elements
useEventListener(document, 'mouseenter', (e) => {
    const target = e.target as HTMLElement;
    if (target.matches('button, a')) {
        isHoveringInteractive.value = true;
    }
}, true); // Use capture phase

useEventListener(document, 'mouseleave', (e) => {
    const target = e.target as HTMLElement;
    if (target.matches('button, a')) {
        isHoveringInteractive.value = false;
    }
}, true); // Use capture phase

// Styles for cursor and follower
const cursorStyle = computed(() => ({
    transform: `translate3d(${cursorX.value}px, ${cursorY.value}px, 0)`,
}));

const followerStyle = computed(() => ({
    transform: `translate3d(${followerX.value - 22}px, ${followerY.value - 22}px, 0)`,
}));

// Dynamic classes for cursor and follower
const cursorClass = computed(() => ({
    'active': isHoveringInteractive.value,
    'close-cursor': isHoveringOverlay.value,
}));

const followerClass = computed(() => ({
    'active': isHoveringInteractive.value,
    'close-cursor': isHoveringOverlay.value,
}));
</script>

<template>
    <div ref="navOverlayRef" class="nav-overlay">
        <div class="cursor" :style="cursorStyle" :class="cursorClass"></div>
        <div class="cursor-follower" :style="followerStyle" :class="followerClass"></div>
    </div>
</template>
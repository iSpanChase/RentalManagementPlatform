import { ref, onMounted, onUnmounted } from 'vue';
import { useIntersectionObserver } from '@vueuse/core';

export function useWowAnimation(elementRef: Ref<HTMLElement | null>, options?: IntersectionObserverInit) {
    const isVisible = ref(false);
    const hasAnimated = ref(false); // To ensure animation only plays once

    const { stop } = useIntersectionObserver(
        elementRef,
        ([{ isIntersecting }]) => {
            if (isIntersecting && !hasAnimated.value) {
                isVisible.value = true;
                hasAnimated.value = true; // Mark as animated
                stop(); // Stop observing after animation is triggered once
            } else if (!isIntersecting && hasAnimated.value) {
                // Optionally reset isVisible if you want animation to replay on scroll up/down
                // isVisible.value = false;
                // hasAnimated.value = false;
            }
        },
        options || { threshold: 0.1 } // Default options, can be customized
    );

    return { isVisible };
}

import { ref } from 'vue';
import type { Ref } from 'vue';
import { useIntersectionObserver, type UseIntersectionObserverOptions } from '@vueuse/core';

type WowAnimationOptions = Pick<IntersectionObserverInit, 'rootMargin' | 'threshold'>;

export function useWowAnimation(elementRef: Ref<HTMLElement | null>, options?: WowAnimationOptions) {
    const isVisible = ref(false);
    const hasAnimated = ref(false); // To ensure animation only plays once

    const observerOptions: UseIntersectionObserverOptions = {
        threshold: options?.threshold ?? 0.1,
        rootMargin: options?.rootMargin,
    };

    const { stop } = useIntersectionObserver(
        elementRef,
        (entries) => {
            const entry = entries[0];
            if (!entry) {
                return;
            }

            const { isIntersecting } = entry;

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
        observerOptions
    );

    return { isVisible };
}

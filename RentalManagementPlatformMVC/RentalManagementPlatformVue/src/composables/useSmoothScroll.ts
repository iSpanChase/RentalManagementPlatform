import { useScroll } from '@vueuse/core';
import { unref } from 'vue';

export function useSmoothScroll() {
    const { scrollTo } = useScroll(window);

    const scrollToTarget = (event: MouseEvent, targetSelector?: string) => {
        event.preventDefault();
        const target = targetSelector || (event.currentTarget as HTMLAnchorElement)?.hash;

        if (target) {
            const el = document.querySelector(target);
            if (el) {
                scrollTo({ top: el.offsetTop, behavior: 'smooth' });
            } else {
                console.warn(`Scroll target element not found: ${target}`);
            }
        }
    };

    return { scrollToTarget };
}

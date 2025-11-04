export function useSmoothScroll() {
    const scrollToTarget = (event: MouseEvent, targetSelector?: string) => {
        event.preventDefault();
        const target = targetSelector || (event.currentTarget as HTMLAnchorElement | null)?.hash;

        if (!target) {
            return;
        }

        const el = document.querySelector(target);
        if (!(el instanceof HTMLElement)) {
            console.warn(`Scroll target element not found: ${target}`);
            return;
        }

        window.scrollTo({ top: el.offsetTop, behavior: 'smooth' });
    };

    return { scrollToTarget };
}

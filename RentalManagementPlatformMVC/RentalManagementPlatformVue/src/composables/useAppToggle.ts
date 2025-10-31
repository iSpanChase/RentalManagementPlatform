import { useToggle } from '@vueuse/core';

export function useAppToggle() {
    const [isSearchPopupOpen, toggleSearchPopup] = useToggle(false);
    const [isSidebarOpen, toggleSidebar] = useToggle(false);
    const [isMobileMenuOpen, toggleMobileMenu] = useToggle(false); // Added mobile menu state

    const handleToggleSearch = () => {
        toggleSearchPopup();
        if (isSidebarOpen.value) {
            toggleSidebar(false); // Close sidebar if search opens
        }
        if (isMobileMenuOpen.value) {
            toggleMobileMenu(false); // Close mobile menu if search opens
        }
    };

    const handleToggleSidebar = () => {
        toggleSidebar();
        if (isSearchPopupOpen.value) {
            toggleSearchPopup(false); // Close search if sidebar opens
        }
        if (isMobileMenuOpen.value) {
            toggleMobileMenu(false); // Close mobile menu if sidebar opens
        }
    };

    const handleToggleMobileMenu = () => { // Added mobile menu toggle handler
        toggleMobileMenu();
        if (isSearchPopupOpen.value) {
            toggleSearchPopup(false); // Close search if mobile menu opens
        }
        if (isSidebarOpen.value) {
            toggleSidebar(false); // Close sidebar if mobile menu opens
        }
    };

    return {
        isSearchPopupOpen,
        isSidebarOpen,
        isMobileMenuOpen, // Export mobile menu state
        handleToggleSearch,
        handleToggleSidebar,
        handleToggleMobileMenu, // Export mobile menu toggle handler
    };
}

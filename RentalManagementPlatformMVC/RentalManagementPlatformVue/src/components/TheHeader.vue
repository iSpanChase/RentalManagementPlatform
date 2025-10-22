<script setup lang="ts">
import { ref, computed } from 'vue';
import { onClickOutside, useWindowScroll } from '@vueuse/core';
import TheNavOverlay from './TheNavOverlay.vue';
import TheMobileMenu from './TheMobileMenu.vue';
import { useRoute } from 'vue-router'; // Added

const props = defineProps<{
    isSearchPopupOpen: boolean;
    isSidebarOpen: boolean;
    isMobileMenuOpen: boolean; // Added
}>();

const emit = defineEmits(['toggle-search', 'toggle-sidebar', 'toggle-mobile-menu']); // Added

const handleSearchToggle = () => {
    emit('toggle-search');
};

const handleSidebarToggle = () => {
    emit('toggle-sidebar');
};

// Dropdown logic
const activeDropdown = ref<number | null>(null); // Tracks the index of the open dropdown
const navRef = ref<HTMLElement | null>(null); // Reference to the main navigation element

const toggleDropdown = (index: number) => {
    if (activeDropdown.value === index) {
        activeDropdown.value = null; // Close if already open
    } else {
        activeDropdown.value = index; // Open new dropdown
    }
};

const closeAllDropdowns = () => {
    activeDropdown.value = null;
};

// Close dropdowns when clicking outside the navigation
onClickOutside(navRef, closeAllDropdowns);

// Sticky Header Logic
const { y } = useWindowScroll();
const isSticky = computed(() => y.value > 100); // Original threshold was 100px

// Dynamic Current Menu Class Logic for Dropdowns
const route = useRoute();

const isDropdownActive = (paths: string[]) => {
    return computed(() => {
        return paths.some(path => route.path.startsWith(path));
    });
};

const isHomeDropdownActive = isDropdownActive(['/', '/index-2', '/index-3']); // Assuming these are sub-pages for Home
const isRoomsDropdownActive = isDropdownActive(['/room-grid', '/room-list', '/room-details']);
const isPagesDropdownActive = isDropdownActive(['/services', '/restaurant', '/gallery', '/offers', '/menu', '/places']);
const isBlogDropdownActive = isDropdownActive(['/blog', '/blog-details']);
</script>

<template>
    <!-- Main Header -->
    <header class="main-header header-style-one style-two" :class="{'fixed-header': isSticky}">
        <div class="header-top">
            <div class="auto-container">
                <div class="wrapper-box box-style-one">
                    <div class="left-column">
                        <ul class="contact-info box-style-two">
                            <li><a href="mailto:info@webmail.com">info@webmail.com</a></li>
                            <li>|</li>
                            <li><a href="tel:09806764956">098-067-649-56</a></li>
                        </ul>
                    </div>
                    <div class="right-column box-style-two">
                        <div class="login"><a href="#">Login</a></div>
                        <ul class="social-icon box-style-two">
                            <li><a href="#"> fb.</a></li>
                            <li><a href="#"> tw.</a></li>
                            <li><a href="#"> be.</a></li>
                            <li><a href="#"> yu.</a></li>
                            <li><a href="#"> ln.</a></li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
        <div class="auto-container">
            <div class="text-center">
                <!--Logo-->
                <div class="logo-box main-logo">
                    <div class="logo"><a href="index.html"><img src="../assets/images/logo.png" alt=""></a></div>
                </div>
            </div>
        </div>
        <!-- Header Upper -->
        <div class="header-upper">
            <div class="auto-container">
                <div class="inner-container">

                    <!--Nav Box-->
                    <div class="nav-outer">
                        <!--Mobile Navigation Toggler-->
                        <div class="mobile-nav-toggler" @click="emit('toggle-mobile-menu')"><img src="../assets/images/icons/icon-bar.png" alt=""></div>

                        <!-- Main Menu -->
                        <nav ref="navRef" class="main-menu navbar-expand-md navbar-light">
                            <div class="collapse navbar-collapse show clearfix" id="navbarSupportedContent">
                                <ul class="navigation">
                                    <li class="dropdown" :class="{'current': isHomeDropdownActive.value}"><a href="index.html" @click.prevent="toggleDropdown(0)">Home</a>
                                        <ul v-show="activeDropdown === 0">
                                            <li><a href="index.html">Home One</a></li>
                                            <li><a href="index-2.html">Home Two</a></li>
                                            <li><a href="index-3.html">Home Three</a></li>
                                        </ul>
                                    </li>
                                    <li><RouterLink to="/about" active-class="current">About Us </RouterLink></li>
                                    <li class="dropdown" :class="{'current': isRoomsDropdownActive.value}"><a href="#" @click.prevent="toggleDropdown(1)">Rooms</a>
                                        <ul v-show="activeDropdown === 1">
                                            <li><a href="room-grid.html">Room Grid Style</a>
                                            </li>
                                            <li><a href="room-list.html">Room List Style</a></li>
                                            <li><a href="room-details.html">Room Details</a></li>
                                        </ul>
                                    </li>
                                    <li class="dropdown" :class="{'current': isPagesDropdownActive.value}"><a href="#" @click.prevent="toggleDropdown(2)">Pages</a>
                                        <ul v-show="activeDropdown === 2">
                                            <li><a href="services.html">Services</a></li>
                                            <li><a href="restaurant.html">Restaurant</a></li>
                                            <li><a href="gallery.html">Gallery</a></li>
                                            <li><a href="offers.html">Offers</a></li>
                                            <li><a href="menu.html">Menu</a></li>
                                            <li><a href="places.html">Places</a></li>
                                        </ul>
                                    </li>
                                    <li class="dropdown" :class="{'current': isBlogDropdownActive.value}"><a href="#" @click.prevent="toggleDropdown(3)">Blog</a>
                                        <ul v-show="activeDropdown === 3">
                                            <li><a href="blog.html">Blog</a></li>
                                            <li><a href="blog-details.html">Blog Details</a></li>
                                        </ul>
                                    </li>
                                    <li><RouterLink to="/contact" active-class="current">Contact</RouterLink></li>
                                </ul>
                            </div>
                        </nav>
                    </div>
                    <div class="right-column">
                        <div class="search-toggler" @click="handleSearchToggle"><i class="far fa-search"></i></div>
                                                    <div class="menu-bar sidemenu-nav-toggler" @click="handleSidebarToggle"><img src="../assets/images/icons/icon-bar3.png" alt=""></div>                    </div>
                </div>
            </div>
        </div>
        <!--End Header Upper-->

        <!-- Sticky Header  -->
        <div class="sticky-header" :class="{'animated slideInDown': isSticky}">
            <div class="header-upper">
                <div class="auto-container">
                    <div class="inner-container">

                        <!--Nav Box-->
                        <div class="nav-outer">
                            <!--Mobile Navigation Toggler-->
                            <div class="mobile-nav-toggler"><img src="../assets/images/icons/icon-bar.png" alt="">
                            </div>

                            <!-- Main Menu -->
                            <nav class="main-menu navbar-expand-md navbar-light">
                            </nav>
                        </div>
                        <div class="right-column">
                            <div class="search-toggler" @click="handleSearchToggle"><i class="far fa-search"></i></div>
                            <div class="menu-bar sidemenu-nav-toggler"><img
                                    src="../assets/images/icons/icon-bar3.png" alt=""></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- End Sticky Menu -->

        <TheMobileMenu />

        <TheNavOverlay />
    </header>
</template>
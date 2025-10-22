<script setup lang="ts">
import { ref } from 'vue';
import { RouterLink } from 'vue-router'; // Added

const props = defineProps<{
    isMobileMenuOpen: boolean;
}>();

const emit = defineEmits(['close-mobile-menu']);

const closeMobileMenu = () => {
    emit('close-mobile-menu');
};

// Mobile Dropdown logic
const activeMobileDropdown = ref<number | null>(null);

const toggleMobileDropdown = (index: number) => {
    if (activeMobileDropdown.value === index) {
        activeMobileDropdown.value = null;
    } else {
        activeMobileDropdown.value = index;
    }
};
</script>

<template>
    <!-- Mobile Menu  -->
    <div class="mobile-menu" v-if="isMobileMenuOpen">
        <div class="menu-backdrop" @click="closeMobileMenu"></div>
        <div class="close-btn" @click="closeMobileMenu"><i class="icon far fa-times"></i></div>

        <nav class="menu-box">
            <div class="nav-logo"><a href="index.html"><img src="../assets/images/logo-light.png" alt=""
                        title=""></a></div>
            <div class="menu-outer">
                <ul class="navigation">
                    <li class="dropdown"><RouterLink to="/" @click.prevent="toggleMobileDropdown(0)" active-class="current" exact-active-class="current">Home</RouterLink>
                        <ul v-show="activeMobileDropdown === 0">
                            <li><RouterLink to="/" active-class="current" exact-active-class="current">Home One</RouterLink></li>
                            <li><RouterLink to="/index-2" active-class="current">Home Two</RouterLink></li>
                            <li><RouterLink to="/index-3" active-class="current">Home Three</RouterLink></li>
                        </ul>
                    </li>
                    <li><RouterLink to="/about" active-class="current">About Us </RouterLink></li>
                    <li class="dropdown"><a href="#" @click.prevent="toggleMobileDropdown(1)">Rooms</a>
                        <ul v-show="activeMobileDropdown === 1">
                            <li><RouterLink to="/room-grid" active-class="current">Room Grid Style</RouterLink>
                            </li>
                            <li><RouterLink to="/room-list" active-class="current">Room List Style</RouterLink></li>
                            <li><RouterLink to="/room-details" active-class="current">Room Details</RouterLink></li>
                        </ul>
                    </li>
                    <li class="dropdown"><a href="#" @click.prevent="toggleMobileDropdown(2)">Pages</a>
                        <ul v-show="activeMobileDropdown === 2">
                            <li><RouterLink to="/services" active-class="current">Services</RouterLink></li>
                            <li><RouterLink to="/restaurant" active-class="current">Restaurant</RouterLink></li>
                            <li><RouterLink to="/gallery" active-class="current">Gallery</RouterLink></li>
                            <li><RouterLink to="/offers" active-class="current">Offers</RouterLink></li>
                            <li><RouterLink to="/menu" active-class="current">Menu</RouterLink></li>
                            <li><RouterLink to="/places" active-class="current">Places</RouterLink></li>
                        </ul>
                    </li>
                    <li class="dropdown"><a href="#" @click.prevent="toggleMobileDropdown(3)">Blog</a>
                        <ul v-show="activeMobileDropdown === 3">
                            <li><RouterLink to="/blog" active-class="current">Blog</RouterLink></li>
                            <li><RouterLink to="/blog-details" active-class="current">Blog Details</RouterLink></li>
                        </ul>
                    </li>
                    <li><RouterLink to="/contact" active-class="current">Contact</RouterLink></li>
                </ul>
            </div>
            <!--Social Links-->
            <div class="social-links">
                <ul class="clearfix">
                    <li><a href="#"><span class="fab fa-twitter"></span></a></li>
                    <li><a href="#"><span class="fab fa-facebook-square"></span></a></li>
                    <li><a href="#"><span class="fab fa-pinterest-p"></span></a></li>
                    <li><a href="#"><span class="fab fa-instagram"></span></a></li>
                    <li><a href="#"><span class="fab fa-youtube"></span></a></li>
                </ul>
            </div>
        </nav>
    </div><!-- End Mobile Menu -->
</template>
<script setup lang="ts">
import { ref, computed } from 'vue';
import { VuePhotoswipe, VuePhotoswipeGallery } from 'vue-photoswipe'; // Changed import
import 'photoswipe/dist/photoswipe.css'; // Import PhotoSwipe CSS

const emit = defineEmits(['close-sidebar']);

const closeSidebar = () => {
    emit('close-sidebar');
};

// Lightbox logic adapted for VuePhotoswipe
// PhotoSwipe expects an array of objects with src, w, h
// For now, we'll use dummy w and h, as we don't have actual image dimensions
const rawImages = ref([
    '../assets/images/news/news-ins-2.jpg',
    '../assets/images/news/news-ins-3.jpg',
    '../assets/images/news/news-ins-4.jpg',
    '../assets/images/news/news-ins-5.jpg',
    '../assets/images/news/news-ins-6.jpg',
    '../assets/images/news/news-ins-7.jpg',
    '../assets/images/news/news-ins-8.jpg',
    '../assets/images/news/news-ins-9.jpg',
    '../assets/images/news/news-ins.jpg',
]);

const images = computed(() => rawImages.value.map(src => ({
    src,
    w: 1200, // Dummy width, ideally fetch actual dimensions
    h: 900,  // Dummy height, ideally fetch actual dimensions
    alt: 'Instagram Image',
})));

const pswpGallery = ref(null); // Ref to the gallery component

const showImg = (index: number) => {
    if (pswpGallery.value) {
        (pswpGallery.value as any).open(index); // Cast to any to access open method
    }
};
</script>

<template>
    <!-- Hidden Sidebar -->
    <section class="hidden-sidebar close-sidebar">
        <div class="wrapper-box">
            <div class="content-wrapper">
                <div class="hidden-sidebar-close" @click="closeSidebar"><span class="flaticon-remove"></span></div>
                <div class="about-widget widget">
                    <div class="logo"><img src="../assets/images/logo-light.png" alt=""></div>
                    <div class="text">We Have Over 40 Payment Ways for Locking the Lowest Room Rates. No Credit Card
                        Needed! Read Reviews from Verified Guests.</div>
                </div>
                <div class="instagram-widget widget">
                    <h4>Instagram Feeds</h4>
                    <div class="inner-box">
                        <div class="wrapper-box">
                            <div class="image" v-for="(img, index) in images" :key="index">
                                <img :src="img" alt="">
                                <div class="overlay-link">
                                    <a href="#" class="lightbox-image" @click.prevent="showImg(index)">
                                        <span class="fa fa-plus"></span>
                                    </a>
                                </div>
                            </div>
                        </div><!-- /.gallery-wrapper -->
                        <VuePhotoswipeGallery :items="images" ref="pswpGallery" />
                    </div>
                </div>
                <div class="widget contact-widget">
                    <h4>Get In Touch</h4>
                    <div class="text">Welcome to Alloggio, where comfort
                        is everything.</div>
                    <ul>
                        <li><i class="fal fa-phone"></i><a href="tel:90809875769">908-098-757-69</a></li>
                        <li><i class="fal fa-envelope"></i><a href="mailto:info@webmail.com">info@webmail.com</a>
                        </li>
                        <li><i class="fal fa-map-marker-alt"></i> 13/A, Miranda City Hall, NYC</li>
                    </ul>
                </div>
            </div>
        </div>
    </section>
</template>
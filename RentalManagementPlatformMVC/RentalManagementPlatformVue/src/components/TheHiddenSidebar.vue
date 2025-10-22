<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import PhotoSwipeLightbox from 'photoswipe/lightbox'; // Import PhotoSwipeLightbox
import 'photoswipe/photoswipe.css'; // Import PhotoSwipe CSS

const emit = defineEmits(['close-sidebar']);

const closeSidebar = () => {
    emit('close-sidebar');
};

// Lightbox logic adapted for direct PhotoSwipe
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

// PhotoSwipe expects an array of objects with src, w, h
// For now, we'll use dummy w and h, as we don't have actual image dimensions
const images = computed(() => rawImages.value.map(src => ({
    src,
    width: 1200, // PhotoSwipe uses 'width' and 'height'
    height: 900,
    alt: 'Instagram Image',
})));

let lightbox: PhotoSwipeLightbox | null = null;

onMounted(() => {
    lightbox = new PhotoSwipeLightbox({
        gallery: '#instagram-gallery', // ID of the gallery container
        children: '.image a', // Selector for gallery items
        pswpModule: () => import('photoswipe'),
    });
    lightbox.init();
});

onUnmounted(() => {
    if (lightbox) {
        lightbox.destroy();
        lightbox = null;
    }
});

// showImg now just triggers the click on the element
const showImg = (index: number) => {
    // PhotoSwipeLightbox handles opening on click of children selector
    // We just need to ensure the click is propagated or trigger it
    // For now, the template's a tag will handle the click, PhotoSwipeLightbox will intercept
    // If we need programmatic open, we'd use lightbox.loadAndOpen(index)
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
                        <div class="wrapper-box" id="instagram-gallery">
                            <div class="image" v-for="(img, index) in images" :key="index">
                                <img :src="img.src" :alt="img.alt">
                                <div class="overlay-link">
                                    <a :href="img.src" class="lightbox-image" :data-pswp-width="img.width" :data-pswp-height="img.height" :data-pswp-srcset="img.src" @click.prevent="showImg(index)">
                                        <span class="fa fa-plus"></span>
                                    </a>
                                </div>
                            </div>
                        </div><!-- /.gallery-wrapper -->
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
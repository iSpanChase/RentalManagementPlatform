<script setup lang="ts">
import { ref } from 'vue';
import VueEasyLightbox from 'vue-easy-lightbox';

const emit = defineEmits(['close-sidebar']);

const closeSidebar = () => {
    emit('close-sidebar');
};

// Lightbox logic
const images = ref([
    '../assets/images/news/news-ins-2.jpg',
    './assets/images/news/news-ins-3.jpg',
    './assets/images/news/news-ins-4.jpg',
    './assets/images/news/news-ins-5.jpg',
    './assets/images/news/news-ins-6.jpg',
    './assets/images/news/news-ins-7.jpg',
    './assets/images/news/news-ins-8.jpg',
    './assets/images/news/news-ins-9.jpg',
    './assets/images/news/news-ins.jpg',
]);
const visibleRef = ref(false);
const indexRef = ref(0); // default 0

const onHide = () => (visibleRef.value = false);

const showImg = (index: number) => {
    indexRef.value = index;
    visibleRef.value = true;
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
                        <VueEasyLightbox
                            :visible="visibleRef"
                            :imgs="images"
                            :index="indexRef"
                            @hide="onHide"
                        ></VueEasyLightbox>
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
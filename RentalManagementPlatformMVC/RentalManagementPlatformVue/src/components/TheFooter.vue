<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue';
import { Autoplay, Navigation, Pagination } from 'swiper/modules';
import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';

const swiperModules = [Autoplay, Navigation, Pagination];

const swiperOptions = {
    loop: true,
    centeredSlides: false,
    spaceBetween: 30,
    autoHeight: true,
    lazy: true,
    navigation: true, // Enable navigation arrows
    pagination: {
        clickable: true, // Enable pagination dots
    },
    autoplay: {
        delay: 6000,
        disableOnInteraction: false,
    },
    speed: 1000,
    breakpoints: {
        0: {
            slidesPerView: 2,
        },
        480: {
            slidesPerView: 3,
        },
        600: {
            slidesPerView: 3,
        },
        768: {
            slidesPerView: 4,
        },
        992: {
            slidesPerView: 4,
        },
        1200: {
            slidesPerView: 4,
        },
    },
};
</script>

<template>
    <!--footer section  -->
    <footer class="main-footer style-two" style="background-image: url(../assets/images/background/bg-1.jpg);">
        <div class="auto-container">
            <div class="widget-wrapper">
                <div class="row">
                    <div class="col-lg-3 col-md-6">
                        <div class="footer-logo"><img src="../assets/images/footer-logo.png" alt=""></div>
                    </div>
                    <div class="column col-lg-3 col-md-6">
                        <div class="widget links-widget">
                            <h4>Pages</h4>
                            <div class="row">
                                <div class="col-sm-6">
                                    <ul>
                                        <li><a href="#">About</a></li>
                                        <li><a href="#">Approach</a></li>
                                        <li><a href="#">People</a></li>
                                        <li><a href="#">Reviews</a></li>
                                        <li><a href="#">Insights</a></li>
                                    </ul>
                                </div>
                                <div class="col-sm-6">
                                    <ul>
                                        <li><a href="#">Services</a></li>
                                        <li><a href="#">Our Menus</a></li>
                                        <li><a href="#">Shop</a></li>
                                        <li><a href="#">Contacts</a></li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-3 col-md-6">
                        <div class="widget text-widget">
                            <h4>Our Awards</h4>
                            <div class="text">When your people get up every day <br> wanting to come to work.</div>
                            <div class="row">
                                <div class="col-sm-4">
                                    <div class="logo"><img src="./assets/images/resource/image-15.png" alt=""></div>
                                </div>
                                <div class="col-sm-4">
                                    <div class="logo"><img src="./assets/images/resource/image-16.png" alt=""></div>
                                </div>
                                <div class="col-sm-4">
                                    <div class="logo"><img src="./assets/images/resource/image-17.png" alt=""></div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-3 col-md-6">
                        <div class="widget contact-widget">
                            <h4>Get In Touch</h4>
                            <div class="text">Welcome to Alloggio, where comfort
                                is everything.</div>
                            <ul>
                                <li><i class="fal fa-phone"></i><a href="tel:90809875769">908-098-757-69</a></li>
                                <li><i class="fal fa-envelope"></i><a
                                        href="mailto:info@webmail.com">info@webmail.com</a></li>
                                <li><i class="fal fa-map-marker-alt"></i> 13/A, Miranda City Hall, NYC</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Footer Bottom -->
        <div class="footer-bottom">
            <div class="auto-container">
                <div class="wrapper-box">
                    <div class="copyright">
                        <div class="text">Copyright Kingho. All right reserved.</div>
                    </div>
                    <div class="brand-logo">
                        <swiper
                            :modules="swiperModules"
                            :loop="swiperOptions.loop"
                            :centeredSlides="swiperOptions.centeredSlides"
                            :spaceBetween="swiperOptions.spaceBetween"
                            :autoHeight="swiperOptions.autoHeight"
                            :lazy="swiperOptions.lazy"
                            :navigation="swiperOptions.navigation"
                            :pagination="swiperOptions.pagination"
                            :autoplay="swiperOptions.autoplay"
                            :speed="swiperOptions.speed"
                            :breakpoints="swiperOptions.breakpoints"
                            class="theme_carousel"
                        >
                            <swiper-slide class="logo"><img src="./assets/images/resource/image-18.png" alt=""></swiper-slide>
                            <swiper-slide class="logo"><img src="./assets/images/resource/image-19.png" alt=""></swiper-slide>
                            <swiper-slide class="logo"><img src="./assets/images/resource/image-20.png" alt=""></swiper-slide>
                            <swiper-slide class="logo"><img src="./assets/images/resource/image-21.png" alt=""></swiper-slide>
                        </swiper>
                    </div>
                </div>
            </div>
        </div>
    </footer>
</template>

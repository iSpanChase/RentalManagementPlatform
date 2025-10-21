<script setup lang="ts">
import TempNavComponent from './components/TempNavComponent.vue';
import './assets/js/jquery.js';
import './assets/js/bootstrap.bundle.min.js';
import './assets/js/jquery.fancybox.js';
import './assets/js/isotope.js';
import './assets/js/owl.js';
import './assets/js/appear.js';
import './assets/js/wow.js';
import './assets/js/scrollbar.js';
import './assets/js/TweenMax.min.js';
import './assets/js/swiper.min.js';
import './assets/js/jquery.ajaxchimp.min.js';
import './assets/js/parallax-scroll.js';
import './assets/js/jquery-ui-1.9.2.custom.min.js';
import './assets/js/jquery.nice-select.min.js';
import './assets/js/active.js';
</script>

<template>
  <div class="page-wrapper">
    <!-- preloader -->
    <div id="preloader" class="preloader">
      <div class="animation-preloader">
        <div class="spinner"></div>
      </div>
      <div class="loader">
        <div class="row">
          <div class="col-3 loader-section section-left">
            <div class="bg"></div>
          </div>
          <div class="col-3 loader-section section-left">
            <div class="bg"></div>
          </div>
          <div class="col-3 loader-section section-right">
            <div class="bg"></div>
          </div>
          <div class="col-3 loader-section section-right">
            <div class="bg"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- <RouterLink class="col-2" to="/">Home</RouterLink> -->
    <RouterView />
  </div>
  <!--End pagewrapper-->

  <!--Scroll to top-->
  <div class="scroll-to-top scroll-to-target" data-target="html">
    <span class="fas fa-arrow-up"></span>
  </div>
</template>

<script setup>
import MainHeader from '@/components/layout/MainHeader.vue';
import MainFooter from '@/components/layout/MainFooter.vue';
</script>

<template>
  <div>
    <!-- 1. Header -->
    <MainHeader />

    <!-- 2. Hidden Sidebar -->
    <section class="hidden-sidebar close-sidebar">
      <div class="wrapper-box">
        <div class="content-wrapper">
          <div class="hidden-sidebar-close"><span class="flaticon-remove"></span></div>
          <div class="about-widget widget">
            <div class="logo"><img src="../assets/images/logo-light.png" alt="" /></div>
            <div class="text">
              We Have Over 40 Payment Ways for Locking the Lowest Room Rates. No Credit Card Needed!
              Read Reviews from Verified Guests.
            </div>
          </div>
          <div class="instagram-widget widget">
            <h4>Instagram Feeds</h4>
            <div class="inner-box">
              <div class="wrapper-box">
                <div class="image">
                  <img src="../assets/images/news/news-ins-2.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="../assets/images/gallery/instagram-1.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="widget contact-widget">
            <h4>Get In Touch</h4>
            <div class="text">Welcome to Alloggio, where comfort is everything.</div>
            <ul>
              <li><i class="fal fa-phone"></i><a href="tel:90809875769">908-098-757-69</a></li>
              <li>
                <i class="fal fa-envelope"></i
                ><a href="mailto:info@webmail.com">info@webmail.com</a>
              </li>
              <li><i class="fal fa-map-marker-alt"></i> 13/A, Miranda City Hall, NYC</li>
            </ul>
          </div>
        </div>
      </div>
    </section>

    <!-- 3. Search Popup -->
    <div id="search-popup" class="search-popup">
      <div class="close-search theme-btn"><i class="far fa-times"></i></div>
      <div class="popup-inner">
        <div class="overlay-layer"></div>
        <div class="search-form">
          <form method="post" action="index.html">
            <div class="form-group">
              <fieldset>
                <input
                  type="search"
                  class="form-control"
                  name="search-input"
                  value=""
                  placeholder="Type & Enter"
                  required
                />
              </fieldset>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- 4. Main Content -->
    <RouterView />

    <!-- 5. Footer -->
    <MainFooter />
  </div>
</template>

<style scoped>
</style>

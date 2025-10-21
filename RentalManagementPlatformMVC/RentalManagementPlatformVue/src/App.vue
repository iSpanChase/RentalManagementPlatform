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

    <!-- Main Header -->
    <header class="main-header header-style-one style-two">
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
            <div class="logo">
              <a href="index.html"><img src="./assets/images/logo.png" alt="" /></a>
            </div>
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
              <div class="mobile-nav-toggler">
                <img src="./assets/images/icons/icon-bar.png" alt="" />
              </div>

              <!-- Main Menu -->
              <nav class="main-menu navbar-expand-md navbar-light">
                <div class="collapse navbar-collapse show clearfix" id="navbarSupportedContent">
                  <ul class="navigation">
                    <li class="dropdown">
                      <a href="index.html">Home</a>
                      <ul>
                        <li><a href="index.html">Home One</a></li>
                        <li><a href="index-2.html">Home Two</a></li>
                        <li><a href="index-3.html">Home Three</a></li>
                      </ul>
                    </li>
                    <li><a href="about.html">About Us </a></li>
                    <li class="dropdown">
                      <a href="#">Rooms</a>
                      <ul>
                        <li><a href="room-grid.html">Room Grid Style</a></li>
                        <li><a href="room-list.html">Room List Style</a></li>
                        <li><a href="room-details.html">Room Details</a></li>
                      </ul>
                    </li>
                    <li class="dropdown">
                      <a href="#">Pages</a>
                      <ul>
                        <li><a href="services.html">Services</a></li>
                        <li><a href="restaurant.html">Restaurant</a></li>
                        <li><a href="gallery.html">Gallery</a></li>
                        <li><a href="offers.html">Offers</a></li>
                        <li><a href="menu.html">Menu</a></li>
                        <li><a href="places.html">Places</a></li>
                      </ul>
                    </li>
                    <li class="dropdown">
                      <a href="#">Blog</a>
                      <ul>
                        <li><a href="blog.html">Blog</a></li>
                        <li><a href="blog-details.html">Blog Details</a></li>
                      </ul>
                    </li>
                    <li><a href="contact.html">Contact</a></li>
                  </ul>
                </div>
              </nav>
            </div>
            <div class="right-column">
              <div class="search-toggler"><i class="far fa-search"></i></div>
              <div class="menu-bar sidemenu-nav-toggler">
                <img src="./assets/images/icons/icon-bar3.png" alt="" />
              </div>
            </div>
          </div>
        </div>
      </div>
      <!--End Header Upper-->

      <!-- Sticky Header  -->
      <div class="sticky-header">
        <div class="header-upper">
          <div class="auto-container">
            <div class="inner-container">
              <!--Nav Box-->
              <div class="nav-outer">
                <!--Mobile Navigation Toggler-->
                <div class="mobile-nav-toggler">
                  <img src="./assets/images/icons/icon-bar.png" alt="" />
                </div>

                <!-- Main Menu -->
                <nav class="main-menu navbar-expand-md navbar-light"></nav>
              </div>
              <div class="right-column">
                <div class="search-toggler"><i class="far fa-search"></i></div>
                <div class="menu-bar sidemenu-nav-toggler">
                  <img src="./assets/images/icons/icon-bar3.png" alt="" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- End Sticky Menu -->

      <!-- Mobile Menu  -->
      <div class="mobile-menu">
        <div class="menu-backdrop"></div>
        <div class="close-btn"><i class="icon far fa-times"></i></div>

        <nav class="menu-box">
          <div class="nav-logo">
            <a href="index.html"><img src="./assets/images/logo-light.png" alt="" title="" /></a>
          </div>
          <div class="menu-outer">
            <!--Here Menu Will Come Automatically Via Javascript / Same Menu as in Header-->
          </div>
          <!--Social Links-->
          <div class="social-links">
            <ul class="clearfix">
              <li>
                <a href="#"><span class="fab fa-twitter"></span></a>
              </li>
              <li>
                <a href="#"><span class="fab fa-facebook-square"></span></a>
              </li>
              <li>
                <a href="#"><span class="fab fa-pinterest-p"></span></a>
              </li>
              <li>
                <a href="#"><span class="fab fa-instagram"></span></a>
              </li>
              <li>
                <a href="#"><span class="fab fa-youtube"></span></a>
              </li>
            </ul>
          </div>
        </nav>
      </div>
      <!-- End Mobile Menu -->

      <div class="nav-overlay">
        <div class="cursor"></div>
        <div class="cursor-follower"></div>
      </div>
    </header>
    <!-- End Main Header -->

    <!-- Hidden Sidebar -->
    <section class="hidden-sidebar close-sidebar">
      <div class="wrapper-box">
        <div class="content-wrapper">
          <div class="hidden-sidebar-close"><span class="flaticon-remove"></span></div>
          <div class="about-widget widget">
            <div class="logo"><img src="./assets/images/logo-light.png" alt="" /></div>
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
                  <img src="./assets/images/news/news-ins-2.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-1.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-3.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-2.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-4.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-3.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-5.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-4.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-6.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-5.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-7.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-6.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-8.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-7.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins-9.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-8.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
                <div class="image">
                  <img src="./assets/images/news/news-ins.jpg" alt="" />
                  <div class="overlay-link">
                    <a
                      href="./assets/images/gallery/instagram-1.jpg"
                      class="lightbox-image"
                      data-fancybox="gallery"
                      ><span class="fa fa-plus"></span
                    ></a>
                  </div>
                </div>
              </div>
              <!-- /.gallery-wrapper -->
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
    <!--Search Popup-->
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

    <TempNavComponent></TempNavComponent>
    <RouterLink class="col-2" to="/">Home</RouterLink>
    <RouterView />

    <!--footer section  -->
    <footer
      class="main-footer style-two"
      style="background-image: url(assets/images/background/bg-1.jpg)"
    >
      <div class="auto-container">
        <div class="widget-wrapper">
          <div class="row">
            <div class="col-lg-3 col-md-6">
              <div class="footer-logo"><img src="./assets/images/footer-logo.png" alt="" /></div>
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
                <div class="text">
                  When your people get up every day <br />
                  wanting to come to work.
                </div>
                <div class="row">
                  <div class="col-sm-4">
                    <div class="logo">
                      <img src="./assets/images/resource/image-15.png" alt="" />
                    </div>
                  </div>
                  <div class="col-sm-4">
                    <div class="logo">
                      <img src="./assets/images/resource/image-16.png" alt="" />
                    </div>
                  </div>
                  <div class="col-sm-4">
                    <div class="logo">
                      <img src="./assets/images/resource/image-17.png" alt="" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-3 col-md-6">
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
              <div
                class="theme_carousel owl-theme owl-carousel"
                data-options='{"loop": true, "center": false, "margin": 30, "autoheight":true, "lazyload":true, "nav": true, "dots": true, "autoplay": true, "autoplayTimeout": 6000, "smartSpeed": 1000, "responsive":{ "0" :{ "items": "2" }, "480" :{ "items" : "3" }, "600" :{ "items" : "3" }, "768" :{ "items" : "4" } , "992":{ "items" : "4" }, "1200":{ "items" : "4" }}}'
              >
                <div class="logo"><img src="./assets/images/resource/image-18.png" alt="" /></div>
                <div class="logo"><img src="./assets/images/resource/image-19.png" alt="" /></div>
                <div class="logo"><img src="./assets/images/resource/image-20.png" alt="" /></div>
                <div class="logo"><img src="./assets/images/resource/image-21.png" alt="" /></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </footer>
  </div>
  <!--End pagewrapper-->

  <!--Scroll to top-->
  <div class="scroll-to-top scroll-to-target" data-target="html">
    <span class="fas fa-arrow-up"></span>
  </div>
</template>

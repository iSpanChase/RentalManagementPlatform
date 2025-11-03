<script setup>
</script>

<template>
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
              <a href="index.html"><img src="../../assets/images/logo.png" alt="" /></a>
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
                <img src="../../assets/images/icons/icon-bar.png" alt="" />
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
                    <li><router-link to="/hosting/rooms">My Rooms</router-link></li>
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
                <img src="../../assets/images/icons/icon-bar3.png" alt="" />
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
                  <img src="../../assets/images/icons/icon-bar.png" alt="" />
                </div>

                <!-- Main Menu -->
                <nav class="main-menu navbar-expand-md navbar-light"></nav>
              </div>
              <div class="right-column">
                <div class="search-toggler"><i class="far fa-search"></i></div>
                <div class="menu-bar sidemenu-nav-toggler">
                  <img src="../../assets/images/icons/icon-bar3.png" alt="" />
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
            <a href="index.html"><img src="../../assets/images/logo-light.png" alt="" title="" /></a>
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
</template>

<style scoped>
</style>

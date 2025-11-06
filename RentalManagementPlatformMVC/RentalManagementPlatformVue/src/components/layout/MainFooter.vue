<script setup>
import { ref } from 'vue'

const currentYear = new Date().getFullYear()

// 快速連結數據
const quickLinks = ref([
  { name: '房源搜尋', to: '/index' },
  { name: '房源推薦', to: '/recommendations' },
  { name: 'FAQ', to: '#' }
])

// 客戶支援數據
const supportLinks = ref([
  { name: '常見問題', href: '#' },
  { name: '訂房指南', href: '#' },
  { name: '取消政策', href: '#' },
  { name: '安全須知', href: '#' }
])

// 聯繫資訊數據
const contactInfo = ref([
  { icon: 'fas fa-phone', text: '+886-2-1234-5678', href: 'tel:+886-2-1234-5678', type: 'tel' },
  { icon: 'fas fa-envelope', text: 'info@airnest.com', href: 'mailto:info@airnest.com', type: 'email' },
  { icon: 'fas fa-map-marker-alt', text: '台北市信義區信義路五段7號', type: 'address' }
])

// 社交媒體連結
const socialLinks = ref([
  { icon: 'fab fa-facebook-f', href: '#', label: 'Facebook' },
  { icon: 'fab fa-instagram', href: '#', label: 'Instagram' },
  { icon: 'fab fa-line', href: '#', label: 'Line' }
])
</script>

<template>
  <footer class="main-footer" role="contentinfo">
    <!-- Main Footer Content -->
    <div class="footer-main">
      <div class="container">
        <div class="footer-grid">
          <!-- Company Info -->
          <div class="footer-section">
            <div class="footer-logo">
              <img src="../../assets/images/AirNest_Logo.png" alt="AirNest Logo" width="180" height="50">
            </div>
            <p class="company-description">
              歡迎來到 AirNest，我們致力於為您提供舒適、安全的住宿體驗。
            </p>
            <div class="social-links">
              <a
                v-for="social in socialLinks"
                :key="social.label"
                :href="social.href"
                class="social-link"
                target="_blank"
                rel="noopener noreferrer"
                :aria-label="`Follow us on ${social.label}`"
              >
                <i :class="social.icon"></i>
              </a>
            </div>
          </div>

          <!-- Quick Links -->
          <div class="footer-section">
            <h4 class="footer-title">快速連結</h4>
            <ul class="footer-links">
              <li v-for="link in quickLinks" :key="link.name">
                <router-link :to="link.to" class="footer-link" exact>{{ link.name }}</router-link>
              </li>
            </ul>
          </div>

          <!-- Support -->
           <!-- 
          <div class="footer-section">
            <h4 class="footer-title">客戶支援</h4>
            <ul class="footer-links">
              <li v-for="link in supportLinks" :key="link.name">
                <a :href="link.href" class="footer-link">{{ link.name }}</a>
              </li>
            </ul>
          </div>
            -->
          
          <!-- Contact -->
          <div class="footer-section">
            <h4 class="footer-title">聯繫資訊</h4>
            <div class="contact-info">
              <div v-for="contact in contactInfo" :key="contact.text" class="contact-item">
                <i :class="contact.icon"></i>
                <a v-if="contact.href" :href="contact.href" class="contact-link">{{ contact.text }}</a>
                <span v-else>{{ contact.text }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>

<style lang="scss" scoped>
$primary-color: #D7BDA7;
$secondary-color: #C4A88A;
$text-color: rgba(255, 255, 255, 0.9);
$hover-color: #ffffff;
$divider-color: rgba(255, 255, 255, 0.3);

.main-footer {
  background: linear-gradient(135deg, #BE9A78 0%, #A08268 100%);
  color: $text-color;
  margin-top: auto;
  font-size: 14px;
  padding: 0;
  margin: 0;
}

.footer-main {
  padding: 1.5rem 0 1.5rem;

  .container {
    max-width: 100%;
    margin: 0 auto;
    padding: 0 40px;
    width: 100%; 
  }

  .footer-grid {
    display: grid;
    grid-template-columns: 2fr 1fr 1fr 1.5fr;
    gap: 2rem;
  }
}

.footer-section {
  .footer-title {
    font-size: 1.125rem;
    font-weight: 600;
    margin-bottom: 1rem;
    color: $hover-color;
    position: relative;

    &::after {
      content: '';
      position: absolute;
      bottom: -0.25rem;
      left: 0;
      width: 1.875rem;
      height: 2px;
      background: $divider-color;
    }
  }
}

.footer-logo {
  margin-bottom: 0.75rem;

  img {
    height: 50px;
    max-width: 180px;
    object-fit: contain;
  }
}

.company-description {
  line-height: 1.6;
  margin: 0;
}

.footer-links {
  list-style: none;
  padding: 0;
  margin: 0;

  li {
    margin-bottom: 0.5rem;
  }

  .footer-link {
    color: $text-color;
    text-decoration: none;
    transition: color 0.2s ease;

    &:hover,
    &:focus {
      color: $hover-color;
      outline: none;
    }
  }
}

.social-links {
  display: flex;
  gap: 0.75rem;
  margin-top: 1rem;

  .social-link {
    color: $text-color;
    font-size: 1.25rem;
    transition: color 0.2s ease;

    &:hover,
    &:focus {
      color: $hover-color;
      outline: none;
    }
  }
}

.contact-info {
  .contact-item {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin-bottom: 0.75rem;

    i {
      color: $divider-color;
      width: 1rem;
      font-size: 0.875rem;
    }

    .contact-link {
      color: $text-color;
      text-decoration: none;
      transition: color 0.2s ease;

      &:hover,
      &:focus {
        color: $hover-color;
        outline: none;
      }
    }

    span {
      color: $text-color;
    }
  }
}

@media (min-width: 1920px) {
  .footer-main .container {
    padding: 0 60px;
  }
}

@media (max-width: 1024px) {
  .footer-main {
    .container {
      padding: 0 30px; // 調整平板的 padding
    }

    .footer-grid {
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
  }
}

@media (max-width: 768px) {
  .footer-main {
    padding: 2rem 0 1.25rem;

    .container {
      padding: 0 20px; // 調整手機的 padding
    }

    .footer-grid {
      grid-template-columns: 1fr;
      text-align: center;
    }

    .footer-title::after {
      left: 50%;
      transform: translateX(-50%);
    }
  }

  .social-links {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .footer-main {
    padding: 1.5rem 0 1rem;

    .container {
      padding: 0 15px;
    }
  }
}
</style>

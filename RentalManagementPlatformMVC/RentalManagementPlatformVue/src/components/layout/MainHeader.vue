<script setup>
import { computed } from 'vue'
import { useWindowScroll } from '@vueuse/core'

const { y } = useWindowScroll()
const isSticky = computed(() => y.value > 100)
</script>

<template>
  <header class="main-header">
    <div class="container">
      <div class="header-flex">
        <!-- Logo -->
        <router-link to="/" class="logo-link">
          <img src="../../assets/images/AirNest_Logo.png" alt="AirNest Logo" class="logo">
        </router-link>

          <!-- Navigation
          <nav class="main-nav">
            <ul class="nav-menu">
              <li><router-link to="/" class="nav-link" active-class="active">首頁</router-link></li>
              <li><router-link to="/about" class="nav-link" active-class="active">關於我們</router-link></li>
              <li><router-link to="/rooms" class="nav-link" active-class="active">房源</router-link></li>
              <li><router-link to="/services" class="nav-link" active-class="active">服務</router-link></li>
              <li><router-link to="/contact" class="nav-link" active-class="active">聯繫我們</router-link></li>
            </ul>
          </nav> -->

        <!-- Right Actions -->
        <div class="right-actions">
          <a href="#" class="login-link">
            <i class="fas fa-user"></i>
            登入
          </a>
          <button class="search-btn">
            <i class="fas fa-search"></i>
          </button>
          <button class="mobile-menu-btn">
            <span></span>
            <span></span>
            <span></span>
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<style lang="scss" scoped>
.main-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%);
  color: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  padding: 12px 0;

  .container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 20px;
  }

  .header-flex {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
}

.logo-link {
  display: block;
  transition: transform 0.2s ease;

  &:hover {
    transform: scale(1.05);
  }

  .logo {
    height: 48px;
    max-width: 180px;
    object-fit: contain;
  }
}

.main-nav {
  .nav-menu {
    display: flex;
    list-style: none;
    margin: 0;
    padding: 0;
    gap: 30px;

    .nav-link {
      color: rgba(255, 255, 255, 0.9);
      text-decoration: none;
      font-weight: 500;
      font-size: 16px;
      padding: 8px 0;
      transition: all 0.2s ease;
      position: relative;

      &:hover,
      &.active {
        color: #BE9A78;
      }

      &::after {
        content: '';
        position: absolute;
        bottom: -2px;
        left: 0;
        width: 0;
        height: 2px;
        background: #BE9A78;
        transition: width 0.3s ease;
      }

      &:hover::after,
      &.active::after {
        width: 100%;
      }
    }
  }
}

.right-actions {
  display: flex;
  align-items: center;
  gap: 15px;

  .login-link {
    color: rgba(255, 255, 255, 0.9);
    text-decoration: none;
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    border-radius: 15px;
    transition: all 0.2s ease;
    font-size: 14px;

    &:hover {
      background: rgba(255, 255, 255, 0.1);
      color: white;
    }

    i {
      font-size: 12px;
    }
  }

  .search-btn {
    width: 36px;
    height: 36px;
    border: none;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s ease;
    color: rgba(255, 255, 255, 0.9);

    &:hover {
      background: #BE9A78;
      color: white;
    }

    i {
      font-size: 14px;
    }
  }

  .mobile-menu-btn {
    display: none;
    flex-direction: column;
    width: 24px;
    height: 20px;
    background: transparent;
    border: none;
    cursor: pointer;
    gap: 4px;
    align-items: center;
    justify-content: center;

    span {
      width: 20px;
      height: 2px;
      background: rgba(255, 255, 255, 0.9);
      border-radius: 1px;
      transition: all 0.3s ease;
    }

    &:hover span {
      background: #BE9A78;
    }
  }
}

@media (max-width: 1024px) {
  .main-nav {
    display: none;
  }

  .right-actions {
    .mobile-menu-btn {
      display: flex;
    }
  }
}

@media (max-width: 768px) {
  .main-header {
    padding: 10px 0;

    .container {
      padding: 0 15px;
    }
  }

  .logo-link .logo {
    height: 40px;
  }

  .right-actions {
    gap: 10px;

    .login-link {
      font-size: 12px;
      padding: 4px 8px;
    }

    .search-btn {
      width: 32px;
      height: 32px;
    }
  }
}

@media (max-width: 480px) {
  .main-header {
    .header-flex {
      gap: 10px;
    }
  }

  .right-actions {
    .login-link {
      span {
        display: none;
      }
    }
  }
}
</style>

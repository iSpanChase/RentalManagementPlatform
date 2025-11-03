<script setup>
let thisYear = new Date().getFullYear()
</script>

<template>
  <div class="booking-layout">
    <header class="booking-header">
      <div class="header-content">
        <router-link to="/" class="logo-link">
          <img src="../../../assets/images/AirNest_Logo.png" alt="AirNest Logo">
        </router-link>

        <nav class="header-nav">
          <router-link to="/help" class="nav-link">
            <i class="fas fa-question-circle"></i>
            需要協助
          </router-link>
        </nav>
      </div>
    </header>

    <main class="booking-main">
      <div class="main-container">
        <router-view />
      </div>
    </main>

    <footer class="booking-footer">
      <div class="footer-content">
        <div class="footer-left">
          <p>© {{ thisYear }} AirNest</p>
          <span class="separator">·</span>
          <nav class="footer-nav">
            <a href="#" class="footer-link">隱私政策</a>
            <span class="separator">·</span>
            <a href="#" class="footer-link">服務條款</a>
            <span class="separator">·</span>
            <a href="#" class="footer-link">公司資訊</a>
          </nav>
        </div>

        <div class="footer-right">
          <div class="support-info">
            <i class="fas fa-phone"></i>
            <span>客服專線：(02) 1234-5678</span>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<style lang="scss" scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-weight: normal;
}

.booking-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
}

.booking-header {
  background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%);
  color: white;
  padding: 0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;

  .header-content {
    max-width: 100%; // 從 1200px 改為 1600px
    margin: 0 auto;
    padding: 10px 40px; // 從 20px 改為 40px
    width: 100%; // 新增
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .logo-link {
    transition: transform 0.2s ease;

    img {
      height: 3rem;
      max-width: 200px;
      object-fit: contain;
    }
  }

  .header-nav {
    display: flex;
    align-items: center;
    gap: 20px;

    .nav-link {
      display: flex;
      align-items: center;
      gap: 8px;
      color: white;
      text-decoration: none;
      padding: 8px 16px;
      border-radius: 20px;
      transition: all 0.2s ease;
      font-size: 14px;

      &:hover {
        background: rgba(255, 255, 255, 0.1);
        color: #f0f0f0;
      }

      i {
        font-size: 16px;
      }
    }
  }
}

.booking-main {
  flex: 1;
  padding: 0;

  .main-container {
    max-width: 1600px; // 從 1200px 改為 1600px
    margin: 0 auto;
    padding: 30px 40px; // 從 20px 改為 40px
    width: 100%; // 新增
    min-height: calc(100vh - 200px);
  }
}

.booking-footer {
  background: linear-gradient(135deg, #BE9A78 0%, #A08268 100%);
  color: white;
  padding: 20px 0;
  margin-top: auto;

  .footer-content {
    max-width: 100%;
    margin: 0 auto;
    padding: 0 40px;
    width: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 20px;
  }

  .footer-left {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;

    p {
      margin: 0;
      font-weight: 500;
    }

    .separator {
      margin: 0 5px;
      opacity: 0.7;
    }

    .footer-nav {
      display: flex;
      align-items: center;
      gap: 5px;

      .footer-link {
        color: white;
        text-decoration: none;
        font-size: 14px;
        transition: color 0.2s ease;

        &:hover {
          color: rgba(255, 255, 255, 0.8);
          text-decoration: underline;
        }
      }
    }
  }

  .footer-right {
    .support-info {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      color: rgba(255, 255, 255, 0.9);

      i {
        font-size: 16px;
      }
    }
  }
}

@media (min-width: 1920px) {
  .booking-header .header-content,
  .booking-main .main-container,
  .booking-footer .footer-content {
    padding-left: 60px;
    padding-right: 60px;
  }
}

@media (max-width: 768px) {
  .booking-header {
    .header-content {
      padding: 12px 20px;
      flex-direction: column;
      gap: 15px;

      .logo-link img {
        height: 2.5rem;
      }
    }

    .header-nav {
      justify-content: center;
    }
  }

  .booking-main {
    .main-container {
      padding: 20px 20px;
    }
  }

  .booking-footer {
    .footer-content {
      padding: 0 20px;
      flex-direction: column;
      text-align: center;
      gap: 15px;

      .footer-left {
        justify-content: center;
        flex-direction: column;
        gap: 10px;

        .footer-nav {
          justify-content: center;
        }
      }
    }
  }
}

@media (max-width: 480px) {
  .booking-header {
    .header-content {
      padding: 12px 15px;
    }

    .header-nav .nav-link {
      font-size: 12px;
      padding: 6px 12px;
    }
  }

  .booking-main .main-container {
    padding: 20px 15px;
  }

  .booking-footer {
    .footer-content {
      padding: 0 15px;
    }

    .footer-left {
      .footer-nav {
        flex-direction: column;
        gap: 8px;

        .separator {
          display: none;
        }
      }
    }
  }
}
</style>

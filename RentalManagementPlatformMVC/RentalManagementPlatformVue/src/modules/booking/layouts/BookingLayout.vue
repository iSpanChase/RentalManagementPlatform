<template>
  <div class="booking-layout">
    <!-- 訂單專用的導航 -->
    <nav class="booking-nav">
      <div class="nav-content">
        <h2>訂單管理系統</h2>
        <div class="nav-links">
          <router-link to="/booking/list">我的訂單</router-link>
          <router-link to="/">返回首頁</router-link>
        </div>
      </div>
    </nav>

    <!-- 主要內容（訂單頁面會顯示在這裡） -->
    <main class="booking-main">
      <router-view />
    </main>

    <!-- 訂單專用的頁尾 -->
    <footer class="booking-footer">
      <p>© 2025 訂單管理系統</p>
    </footer>
  </div>
</template>

<style scoped>
.booking-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.booking-nav {
  background: #2c3e50;
  color: white;
  padding: 20px;
}

.nav-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-links {
  display: flex;
  gap: 20px;
}

.nav-links a {
  color: white;
  text-decoration: none;
}

.booking-main {
  flex: 1;
  padding: 40px 20px;
}

.booking-footer {
  background: #34495e;
  color: white;
  text-align: center;
  padding: 20px;
}
</style>

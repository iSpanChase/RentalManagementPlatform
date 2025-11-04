<script setup>
let thisYear = new Date().getFullYear()
</script>

<template>
  <div class="booking-layout">
    <header class="booking-header">
      <router-link to="/">
        <img src="@/assets/images/logo.png" alt="logo">
      </router-link>
    </header>

    <main class="booking-main">
      <router-view />
    </main>

    <footer class="booking-footer">
      <p>© {{ thisYear }} KING HOTEL</p>
      <span class="separator">·</span>
      <nav class="footer-nav">
        <a href="#">隱私</a>
        <span class="separator">·</span>
        <a href="#">相關條款</a>
      </nav>
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
}

.booking-header {
  background: #222222;
  color: white;
  padding: 10px 20px;
  display: flex;
  align-items: center;
  gap: 20px;

  img {
    height: 2.5rem;
    margin-left: 1rem;
  }
}

.booking-main {
  flex: 1;
  padding: 20px 20px;
}

.booking-footer {
  background: #BE9A78;
  color: white;
  text-align: center;
  padding: 20px;
  display: flex;
  justify-content: flex-start;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;

  p {
    margin: 0;
  }

  .separator {
    margin: 0 5px;
  }

  .footer-nav {
    display: flex;
    align-items: center;
    gap: 5px;

    a {
      color: white;
      text-decoration: none;
    }
  }
}
</style>

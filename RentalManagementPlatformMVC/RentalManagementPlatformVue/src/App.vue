<script setup lang="ts">
import TempNavComponent from './components/TempNavComponent.vue';
</script>

<template>
  <TempNavComponent></TempNavComponent>
  <RouterLink class="col-2" to="/">Home</RouterLink>
  <h1>測試</h1>
  <RouterView />
</template>

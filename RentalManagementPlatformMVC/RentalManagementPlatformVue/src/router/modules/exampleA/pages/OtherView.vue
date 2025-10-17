<!-- src/modules/exampleA/pages/OtherView.vue -->
<template>
<div>
<h1>Example A Other</h1>
<router-link :to="{ name: 'ExampleA.IndexView' }">Index</router-link>
<router-view />
</div>
</template>


<script setup>
// setup script
</script>
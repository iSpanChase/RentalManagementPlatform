<!-- src/modules/exampleA/pages/IndexView.vue -->
<template>
<div>
<h1>Example A Index</h1>
<router-link :to="{ name: 'ExampleA.OtherView' }">Other</router-link>
<router-view />
</div>
</template>


<script setup>
// setup script
</script>
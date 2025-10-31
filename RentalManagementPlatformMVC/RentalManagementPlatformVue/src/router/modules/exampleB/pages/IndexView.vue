<!-- src/modules/exampleB/pages/IndexView.vue -->
<template>
<div>
<h1>Example B Index</h1>
<router-link :to="{ name: 'ExampleB.OtherView' }">Other</router-link>
<router-view />
</div>
</template>


<script setup>
// setup script
</script>
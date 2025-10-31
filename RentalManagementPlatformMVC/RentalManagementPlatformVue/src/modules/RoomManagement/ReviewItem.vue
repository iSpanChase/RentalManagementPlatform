<template>
  <div class="review-item">
    <div class="review-header">
      <strong>{{ review.reviewerName }}</strong>
      <span>{{ new Date(review.createdAt).toLocaleDateString() }}</span>
    </div>
    <div class="rating">
      <span v-for="n in 5" :key="n">{{ n <= review.rating ? '★' : '☆' }}</span>
    </div>
    <p class="comment">{{ review.comment }}</p>
  </div>
</template>

<script setup>
defineProps({
  review: {
    type: Object,
    required: true,
  },
});
</script>

<style scoped>
.review-item {
  border-bottom: 1px solid #eee;
  padding: 1rem 0;
}
.review-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}
.rating {
  color: #f5a623;
  margin-bottom: 0.5rem;
}
.comment {
  white-space: pre-wrap;
}
</style>

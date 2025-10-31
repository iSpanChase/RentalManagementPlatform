<template>
  <div class="review-list">
    <h3>Reviews</h3>
    <div v-if="reviews && reviews.length">
      <ReviewItem v-for="review in reviews" :key="review.reviewId" :review="review" />
    </div>
    <div v-else>
      <p>No reviews yet.</p>
    </div>
  </div>
</template>

<script setup>
import ReviewItem from './ReviewItem.vue';

defineProps({
  reviews: {
    type: Array,
    required: true,
  },
});
</script>

<style scoped>
.review-list h3 {
  margin-bottom: 1rem;
}
</style>

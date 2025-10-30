<template>
  <div class="review-form">
    <h3>Leave a Review</h3>
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label>Rating</label>
        <div class="star-rating">
          <span v-for="n in 5" :key="n" @click="setRating(n)">{{ n <= rating ? '★' : '☆' }}</span>
        </div>
      </div>
      <div class="form-group">
        <label for="comment">Comment</label>
        <textarea id="comment" v-model="comment" rows="4"></textarea>
      </div>
      <button type="submit" :disabled="isSubmitting">Submit Review</button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const rating = ref(0);
const comment = ref('');
const isSubmitting = ref(false);

const emit = defineEmits(['submit-review']);

const setRating = (newRating) => {
  rating.value = newRating;
};

const handleSubmit = () => {
  if (rating.value === 0 || !comment.value) {
    alert('Please provide a rating and a comment.');
    return;
  }
  isSubmitting.value = true;
  emit('submit-review', { rating: rating.value, comment: comment.value });
  // Reset form after submission is handled by the parent
};

// Expose a method to parent to reset form
defineExpose({
  resetForm: () => {
    rating.value = 0;
    comment.value = '';
    isSubmitting.value = false;
  }
});
</script>

<style scoped>
.review-form h3 {
  margin-bottom: 1rem;
}
.form-group {
  margin-bottom: 1rem;
}
.star-rating span {
  cursor: pointer;
  font-size: 1.5rem;
  color: #f5a623;
}
textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}
button {
  padding: 0.5rem 1rem;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
button:disabled {
  background-color: #ccc;
}
</style>

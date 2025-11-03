<script setup>
const props = defineProps({
  title: {
    type: String,
    required: true
  },
  description: {
    type: String,
    required: true
  },
  buttonText: {
    type: String,
    default: ''
  },
  buttonTo: {
    type: String,
    default: ''
  },
  buttonColor: {
    type: String,
    default: 'primary'
  }
});
</script>

<template>
  <div class="empty-state">
    <div class="empty-content">
      <slot name="icon">
        <i class="fa-solid fa-inbox empty-icon"></i>
      </slot>
      <h3>{{ title }}</h3>
      <p>{{ description }}</p>
      <router-link v-if="buttonText && buttonTo" :to="buttonTo">
        <button :class="['btn', `btn-${buttonColor}`]">{{ buttonText }}</button>
      </router-link>
      <slot name="actions"></slot>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.empty-state {
  text-align: center;
  padding: 50px 20px;
  background: white;
  border-radius: 16px;
  border: 1px solid #ebebeb;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);

  .empty-content {
    max-width: 400px;
    margin: 0 auto;
  }

  .empty-icon {
    font-size: 48px;
    color: #c0c0c0;
    margin-bottom: 20px;
  }

  h3 {
    font-size: 20px;
    font-weight: 600;
    color: #222;
    margin-bottom: 8px;
  }

  p {
    font-size: 16px;
    color: #717171;
    margin-bottom: 24px;
    line-height: 1.5;
  }

  .btn {
    padding: 12px 24px;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s;
    text-decoration: none;
    display: inline-block;

    &.btn-primary {
      background-color: #008489;
      color: white;

      &:hover {
        background-color: darken(#008489, 10%);
      }
    }

    &.btn-secondary {
      background-color: #f7a800;
      color: #222;

      &:hover {
        background-color: darken(#f7a800, 10%);
      }
    }
  }
}
</style>
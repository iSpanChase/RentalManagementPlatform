<template>
  <div class="base-card-component">
    <div class="card-header">
      <div class="title">
        <h3>{{ title }}</h3>
        <p v-if="subtitle">{{ subtitle }}</p>
      </div>
      <div class="actions">
        <slot name="header-extra"></slot>
        <button v-for="a in actions" :key="a.key" @click="emitAction(a.key)">{{ a.label }}</button>
        <button v-if="collapsible" @click="collapsed = !collapsed">
          <font-awesome-icon v-if="collapsed" :icon="['fas','chevron-down']" />
          <font-awesome-icon v-else :icon="['fas','chevron-up']" />
        </button>
      </div>
    </div>

    <div v-if="loading" class="card-loading">Loading...</div>

    <div v-else v-show="!collapsed" class="card-body">
      <slot></slot>
    </div>

    <div class="card-footer">
      <slot name="footer"></slot>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
defineProps({
  title: String,
  subtitle: String,
  loading: { type: Boolean, default: false },
  actions: { type: Array, default: () => [] },
  collapsible: { type: Boolean, default: true },
});
const collapsed = ref(false);
const emit = defineEmits(['update:config', 'remove', 'clicked']);
function emitAction(key){ emit('clicked', key); }
</script>

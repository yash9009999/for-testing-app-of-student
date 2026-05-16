<template>
  <div class="toasts" aria-live="polite" aria-atomic="true">
    <div v-for="toast in state.toasts" :key="toast.id" :class="['toast', toast.type]">
      <div class="toast-header">
        <strong class="toast-title">{{ toast.title }}</strong>
        <button class="toast-close" @click="remove(toast.id)" aria-label="Close">×</button>
      </div>
      <div class="toast-body">{{ toast.message }}</div>
    </div>
  </div>
</template>

<script>
import toastService from '../services/toastService';

export default {
  name: 'Toasts',
  setup() {
    return { state: toastService.state, remove: toastService.removeToast };
  }
}
</script>

<style scoped>
.toasts {
  position: fixed;
  right: 1rem;
  bottom: 1rem;
  z-index: 1100;
  display: flex;
  flex-direction: column-reverse;
  align-items: flex-end;
  gap: 0.5rem;
  max-width: 360px;
}
.toast {
  border-radius: 8px;
  padding: 0.6rem 0.9rem;
  box-shadow: 0 6px 18px rgba(0,0,0,0.15);
  background: var(--color-background-soft);
  color: var(--color-text);
  border-left: 6px solid transparent;
  width: 100%;
  box-sizing: border-box;
  border: 1px solid var(--color-border);
}
.toast.success {
  border-left-color: var(--color-primary);
  background: var(--color-background-soft);
}
.toast.error { 
  border-left-color: #dc3545;
  background: var(--color-background-soft);
}
.toast.info { 
  border-left-color: #17a2b8;
  background: var(--color-background-soft);
}
.toast-header { 
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.toast-title { 
  font-weight: 700;
  font-size: 0.95rem;
  color: var(--color-heading);
}
.toast-close { 
  background: none;
  border: 0;
  font-size: 1.1rem;
  line-height: 1;
  cursor: pointer;
  color: var(--color-text);
  opacity: 0.7;
  transition: opacity 0.2s;
}
.toast-close:hover {
  opacity: 1;
}
.toast-body { 
  font-size: 0.9rem;
  margin-top: 0.35rem;
  color: var(--color-text);
}
</style>

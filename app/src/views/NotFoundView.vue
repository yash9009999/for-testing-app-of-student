<template>
  <div class="not-found">
    <div class="not-found-card">
      <div class="hero">
        <img :src="imageUrl" alt="Dropped ice cream cone" class="hero-image" />
        <div class="hero-copy">
          <p class="eyebrow">404 - Page Not Found</p>
          <h1>Sorry! We couldn't find what you were looking for...</h1>
          <p class="hint">Check the URL for typos, refresh the page, or try again later.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'NotFound',
  data() {
    return {
      imageUrl: new URL('../assets/ice-cream-wrong.png', import.meta.url).href
    };
  }
};
</script>

<style scoped>
.not-found {
  padding: 1.25rem 1.5rem;
  background: var(--color-background);
  box-sizing: border-box;
}

.not-found-card {
  width: 100%;
  max-width: 880px;
  margin: 0 auto;
  padding: 1.4rem;
  border-radius: 16px;
  border: 1px solid var(--color-border);
  background: var(--color-background-soft);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.hero {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  align-items: center;
  text-align: center;
}

.hero-image {
  width: 150px;
  height: auto;
  border-radius: 14px;
  max-width: 100%;
}

.hero-copy h1 {
  margin: 0.35rem 0 0.5rem;
  font-size: 2rem;
  font-weight: 800;
  color: var(--color-heading);
}

.not-found-subtitle {
  margin: 0;
  font-size: 1rem;
  color: var(--color-text);
  opacity: 0.9;
  line-height: 1.5;
}

.eyebrow {
  margin: 0;
  font-weight: 800;
  letter-spacing: 2px;
  text-transform: uppercase;
  color: var(--color-heading);
  opacity: 0.65;
}

.hint {
  margin: 0.75rem 0 0;
  color: var(--color-text);
  opacity: 0.75;
  font-size: 0.95rem;
}
</style>
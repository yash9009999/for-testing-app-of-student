<template>
  <header>
    <img :alt="logoAlt" class="logo" :src="currentLogo" @click="swapLogo" width="125" height="125" />

    <div class="wrapper">
      <Hero title="Scoops2Go!" :link="currentLink" :description="currentDescription"/>

      <nav>
        <RouterLink to="/">Home</RouterLink>
        <RouterLink to="/scoops-builder">🪄 Build Your Treat!</RouterLink>
      </nav>
    </div>

    <div class="toolbar">
      <ThemeToggle />
      <span class="toolbar-separator">|</span>
      <RouterLink to="/scoops-builder" class="lookup-link">🪄 Product Browsing & Treat Creation</RouterLink>
      <span class="toolbar-separator">|</span>
      <RouterLink :to="{ name: 'cart', query: { cart: JSON.stringify(cart) } }" class="lookup-link">
        <span>🛒 Basket and Checkout </span>
        <span class="cart-count" v-if="cart.length">{{ cart.length }}</span>
        <small class="nav-basket-number" v-if="orderId">(#{{ orderId }})</small>
      </RouterLink>
      <span class="toolbar-separator">|</span>
      <RouterLink to="/order-tracking" class="lookup-link">🔎 Order Tracking</RouterLink>
      <span class="toolbar-separator">|</span>
      <template v-if="isUserAuthenticated">
        <RouterLink to="/user" class="lookup-link">👤 My Profile</RouterLink>
        <span class="toolbar-separator">|</span>
        <button @click="handleLogout" class="logout-button">🚪 Logout</button>
      </template>
      <template v-else>
        <RouterLink to="/login" class="lookup-link">🔓 Login</RouterLink>
        <span class="toolbar-separator">|</span>
        <RouterLink to="/register" class="lookup-link">✍️ Register</RouterLink>
      </template>
    </div>
  </header>

  <Toasts />

  <div class="content">
    <RouterView 
      @iceCreamCreated="addToCart" 
      @update:cart="updateCart" 
      @orderUpdated="onOrderUpdated" 
      @auth-changed="onAuthChanged"
      :cart="cart" 
    />
  </div>
</template>

<script>
import { RouterLink, RouterView } from 'vue-router'
import Hero from './components/Hero.vue'
import Toasts from './components/Toasts.vue'
import ThemeToggle from './components/ThemeToggle.vue'
import { isAuthenticated, clearAuth } from './services/authService'

export default {
  name: 'App',
  components: {
    RouterLink,
    RouterView,
    Hero,
    Toasts,
    ThemeToggle
  },
  data() {
    return {
      logos: [
        { src: '', alt: 'Icecream (Classic) Logo' },
        { src: '', alt: 'Ice cream (Swirl) Logo' },
        { src: '', alt: 'Ice cream (Cup) Logo' },
        { src: '', alt: 'Ice cream (Cone) Logo' },
        { src: '', alt: 'Ice cream (Pop) Logo' }
      ],
      descriptions: [
        'Ice cream icons created by Freepik.',
        'Ice cream icons created by Freepik.',
        'Ice cream icons created by Freepik.',
        'Ice cream icons created by Freepik.',
        'Ice cream icons created by Freepik.'
      ],
      links: [
        'https://www.flaticon.com/free-icons/ice-cream',
        'https://www.flaticon.com/free-icons/ice-cream',
        'https://www.flaticon.com/free-icons/ice-cream',
        'https://www.flaticon.com/free-icons/ice-cream',
        'https://www.flaticon.com/free-icons/ice-cream'
      ],
      currentLogoIndex: 0,
      selectedFlavour: null,
      selectedTopping: null,
      selectedCone: null,
      cart: []
      ,
      orderId: null,
      authStateKey: 0
    }
  },
  created() {
    this.loadLogos();
    try { this.orderId = localStorage.getItem('s2g_orderId'); } catch(e) { this.orderId = null; }
  },
  computed: {
    isUserAuthenticated() {
      // Include authStateKey in computed to trigger re-evaluation when auth changes
      void this.authStateKey;
      return isAuthenticated();
    },
    currentLogo() {
      return this.logos[this.currentLogoIndex].src;
    },
    logoAlt() {
      return this.logos[this.currentLogoIndex].alt;
    },
    currentDescription() {
      return this.descriptions[this.currentLogoIndex];
    },
    currentLink() {
      return this.links[this.currentLogoIndex];
    }
  },
  methods: {
    async loadLogos() {
      this.logos[0].src = (await import('./assets/ice-cream-classic.png')).default;
      this.logos[1].src = (await import('./assets/ice-cream-swirl.png')).default;
      this.logos[2].src = (await import('./assets/ice-cream-cup.png')).default;
      this.logos[3].src = (await import('./assets/ice-cream-cone.png')).default;
      this.logos[4].src = (await import('./assets/ice-cream-pop.png')).default;
    },
    swapLogo() {
      this.currentLogoIndex = (this.currentLogoIndex + 1) % this.logos.length;
    },
    onOrderUpdated(order) {
      try {
        this.orderId = order && order.id ? order.id : localStorage.getItem('s2g_orderId');
        // if the order was cleared (e.g. after successful checkout), also clear the in-memory cart
        if (!order) this.cart = [];
      } catch(e) { this.orderId = null; }
    },
    handleLogout() {
      clearAuth();
      this.authStateKey++;
      this.$router.push('/');
    },
    onAuthChanged() {
      // Increment key to trigger reactive updates when auth state changes
      this.authStateKey++;
    }
  }
}
</script>

<style scoped>
header {
  line-height: 1.5;
  max-height: 100vh;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

nav {
  width: 100%;
  font-size: 12px;
  text-align: center;
  margin-top: 2rem;
}

nav a.router-link-exact-active {
  color: var(--color-text);
}

nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
  color: var(--color-text);
  text-decoration: none;
  transition: background-color 0.2s, color 0.2s, border-color 0.2s;
}

nav a:first-of-type {
  border: 0;
}

nav a:hover {
  background-color: var(--color-primary-light);
  border-color: var(--color-border-hover);
  color: var(--color-text);
}

.toolbar {
  position: absolute;
  top: 1rem;
  right: 1rem;
  display: flex;
  align-items: center;
}

.toolbar-separator {
  margin: 0 0.5rem;
  color: var(--color-text);
}

.cart-link {
  position: relative;
  display: flex;
  align-items: center;
  text-decoration: none;
  color: var(--color-text);
}

.cart-icon {
  font-size: 1.5rem;
}

.cart-count {
  position: absolute;
  top: -0.5rem;
  right: -0.5rem;
  background-color: var(--color-primary);
  color: #fff;
  border-radius: 50%;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.nav-basket-number {
  margin-left: 0.5rem;
  background: var(--color-background-mute);
  color: var(--color-text);
  padding: 0.08rem 0.4rem;
  border-radius: 6px;
  font-size: 0.75rem;
}

.profile-link {
  text-decoration: none;
  color: var(--color-text);
  font-size: 1rem;
}

.login-link {
  text-decoration: none;
  color: var(--color-text);
  font-size: 1rem;
}

.lookup-link {
  text-decoration: none;
  color: var(--color-text);
  display: flex;
  align-items: center;
}

.logout-button {
  background: none;
  border: none;
  color: var(--color-text);
  text-decoration: none;
  cursor: pointer;
  font-size: 1rem;
  padding: 0;
  transition: opacity 0.2s;
}

.logout-button:hover {
  opacity: 0.7;
}

.content {
  overflow-y: auto;
  height: calc(100vh - 200px);
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }

  nav {
    text-align: left;
    margin-left: -1rem;
    font-size: 1rem;

    padding: 1rem 0;
    margin-top: 1rem;
  }
}
</style>

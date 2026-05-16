<template>
  <div class="auth-callback">
    <div v-if="loading" class="loading">
      <p>Processing authentication...</p>
    </div>
    <div v-else-if="error" class="error">
      <p>Authentication failed: {{ error }}</p>
      <router-link to="/">Return to home</router-link>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { exchangeAuthorizationCode, storeAuthToken, consumeOAuthState } from '../services/authService';

const router = useRouter();
const route = useRoute();
const loading = ref(true);
const error = ref(null);

onMounted(async () => {
  try {
    const code = route.query.code;
    const state = route.query.state;

    if (!code) {
      error.value = 'No authorization code received';
      loading.value = false;
      return;
    }

    if (!consumeOAuthState(state)) {
      error.value = 'Invalid OAuth state — please sign in again';
      loading.value = false;
      return;
    }

    const response = await exchangeAuthorizationCode(code);

    if (response.success && response.token && response.userId) {
      storeAuthToken(response.token, response.userId);
      window.dispatchEvent(new CustomEvent('auth-changed'));

      const redirect = sessionStorage.getItem('auth-redirect') || '/';
      sessionStorage.removeItem('auth-redirect');
      router.push(redirect);
    } else {
      error.value = response.message || 'Authentication failed';
      loading.value = false;
    }
  } catch (err) {
    error.value = err.message || 'An error occurred during authentication';
    loading.value = false;
  }
});
</script>

<style scoped>
.auth-callback {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
}

.loading {
  text-align: center;
  font-size: 18px;
  color: #333;
}

.error {
  text-align: center;
  color: #d32f2f;
  font-size: 16px;
}

.error a {
  display: inline-block;
  margin-top: 20px;
  padding: 10px 20px;
  background-color: #007bff;
  color: white;
  text-decoration: none;
  border-radius: 4px;
}

.error a:hover {
  background-color: #0056b3;
}
</style>

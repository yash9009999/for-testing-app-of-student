<template>
  <div class="auth-view">
    <div class="auth-container">
      <div class="auth-card">
        <div class="auth-header">
          <h1>Welcome Back</h1>
          <p class="auth-subtitle">Sign in to your Scoops2Go account</p>
        </div>

        <form @submit.prevent="handleLogin" class="auth-form">
          <div class="form-group">
            <label for="username">Username</label>
            <input
              id="username"
              v-model="form.username"
              type="text"
              placeholder="Your username"
              class="form-input"
              required
            />
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              id="password"
              v-model="form.password"
              type="password"
              placeholder="Enter password"
              class="form-input"
              required
            />
          </div>

          <div v-if="error" class="validation-box validation-box--warning">
            <div class="validation-box__icon" aria-hidden="true">⚠️</div>
            <div>
              <strong class="validation-box__title">Error</strong>
              <p class="validation-box__message">{{ error }}</p>
            </div>
          </div>

          <button
            type="submit"
            :disabled="isLoading"
            class="green-button form-button"
          >
            {{ isLoading ? 'Signing In…' : 'Sign In' }}
          </button>
        </form>

        <div class="auth-footer">
          <p>Don't have an account?
            <router-link to="/register" class="auth-link">Create One</router-link>
          </p>
        </div>
      </div>

      <div class="auth-accent" style="--accent-color: rgba(140, 200, 158, 0.15)"></div>
    </div>
  </div>
</template>

<script>
import { login, storeAuthToken } from '../services/authService';
import toastService from '../services/toastService';

export default {
  name: 'LoginView',
  data() {
    return {
      form: {
        username: '',
        password: ''
      },
      isLoading: false,
      error: null
    };
  },
  methods: {
    async handleLogin() {
      this.isLoading = true;
      this.error = null;

      try {
        const data = await login(this.form.username, this.form.password);

        // Store token and userId
        storeAuthToken(data.token, data.userId);

        toastService.success(`Welcome back, ${this.form.username}!`, 'Signed In');
        this.$emit('auth-changed');
        this.$router.push('/');
      } catch (err) {
        this.error = err.message || 'An error occurred during login';
      } finally {
        this.isLoading = false;
      }
    }
  }
};
</script>

<style scoped>
.auth-view {
  padding: 2rem;
  min-height: 100vh;
  background: var(--color-background);
  display: flex;
  align-items: center;
  justify-content: center;
}

.auth-container {
  position: relative;
  width: 100%;
  max-width: 450px;
}

.auth-card {
  padding: 2.5rem;
  border-radius: 16px;
  border: 1px solid var(--color-border);
  background: var(--color-background-soft);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(4px);
}

.auth-header {
  text-align: center;
  margin-bottom: 2rem;
}

.auth-header h1 {
  margin: 0;
  font-size: 1.8rem;
  font-weight: 800;
  color: var(--color-heading);
}

.auth-subtitle {
  margin: 0.5rem 0 0;
  font-size: 0.95rem;
  color: var(--color-text);
  opacity: 0.8;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  margin-bottom: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--color-text);
}

.form-input {
  padding: 0.75rem 1rem;
  border-radius: 10px;
  border: 1px solid var(--color-border);
  background: var(--color-background);
  color: var(--color-text);
  font-size: 1rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: rgba(140, 200, 158, 0.6);
  box-shadow: 0 0 0 3px rgba(140, 200, 158, 0.15);
}

.form-input::placeholder {
  color: var(--color-text);
  opacity: 0.5;
}

.validation-box {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;
  padding: 0.8rem 1rem;
  border-radius: 10px;
  border: 1px solid rgba(238, 170, 60, 0.35);
  background-color: rgba(238, 170, 60, 0.12);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
}

.validation-box__icon {
  font-size: 1.05rem;
  line-height: 1.2;
}

.validation-box__title {
  font-weight: 800;
  color: var(--color-heading);
  display: block;
  margin-bottom: 0.15rem;
}

.validation-box__message {
  margin: 0;
  color: var(--color-text);
  font-size: 0.9rem;
}

.form-button {
  margin-top: 0.5rem;
  padding: 0.85rem 1.5rem;
  font-size: 1rem;
  font-weight: 700;
}

.form-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.auth-footer {
  text-align: center;
  padding-top: 1.5rem;
  border-top: 1px solid var(--color-border);
}

.auth-footer p {
  margin: 0;
  color: var(--color-text);
  font-size: 0.9rem;
}

.auth-link {
  font-weight: 700;
  color: var(--color-primary);
  text-decoration: none;
  transition: opacity 0.2s;
}

.auth-link:hover {
  opacity: 0.8;
}

.auth-accent {
  position: absolute;
  top: -10%;
  right: -10%;
  width: 300px;
  height: 300px;
  border-radius: 50%;
  background: var(--accent-color);
  filter: blur(40px);
  pointer-events: none;
}

@media (max-width: 600px) {
  .auth-card {
    padding: 2rem;
  }

  .auth-header h1 {
    font-size: 1.5rem;
  }

  .auth-accent {
    display: none;
  }
}
</style>

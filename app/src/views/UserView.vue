<template>
  <div class="user-view">
    <div class="user-container">
      <div class="user-card">
        <div class="user-header">
          <h1>User Profile</h1>
          <p class="user-subtitle">View and update your account information</p>
        </div>

        <div v-if="isLoading" class="loading-state">
          <p>Loading user information...</p>
        </div>

        <div v-else-if="error" class="validation-box validation-box--warning">
          <div class="validation-box__icon" aria-hidden="true">⚠️</div>
          <div>
            <strong class="validation-box__title">Error</strong>
            <p class="validation-box__message">{{ error }}</p>
          </div>
        </div>

        <form v-else @submit.prevent="handleUpdate" class="user-form">
          <div class="form-group">
            <label for="userId">User ID</label>
            <input
              id="userId"
              v-model="user.id"
              type="text"
              class="form-input"
              disabled
              readonly
            />
          </div>

          <div class="form-group">
            <label for="username">Username</label>
            <input
              id="username"
              v-model="form.username"
              type="text"
              placeholder="Your username"
              class="form-input"
              :disabled="isSaving"
              required
            />
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              v-model="form.email"
              type="email"
              placeholder="your.email@example.com"
              class="form-input"
              :disabled="isSaving"
              required
            />
          </div>

          <div class="form-group">
            <label for="fullName">Full Name</label>
            <input
              id="fullName"
              v-model="form.fullName"
              type="text"
              placeholder="Your full name"
              class="form-input"
              :disabled="isSaving"
              required
            />
          </div>

          <div class="form-group">
            <label for="favouriteFlavour">Favourite Flavour</label>
            <input
              id="favouriteFlavour"
              v-model="form.favouriteFlavour"
              type="text"
              placeholder="e.g. Mint Chocolate Chip"
              class="form-input"
              :disabled="isSaving"
            />
          </div>

          <div class="form-group">
            <label for="createdAt">Member Since</label>
            <input
              id="createdAt"
              :value="formattedCreatedAt"
              type="text"
              class="form-input"
              disabled
              readonly
            />
          </div>

          <div v-if="updateError" class="validation-box validation-box--warning">
            <div class="validation-box__icon" aria-hidden="true">⚠️</div>
            <div>
              <strong class="validation-box__title">Update Error</strong>
              <p class="validation-box__message">{{ updateError }}</p>
            </div>
          </div>

          <div v-if="updateSuccess" class="validation-box validation-box--success">
            <div class="validation-box__icon" aria-hidden="true">✓</div>
            <div>
              <strong class="validation-box__title">Success</strong>
              <p class="validation-box__message">Your profile has been updated successfully!</p>
            </div>
          </div>

          <div class="promo-inline">
            <div class="promo-inline__header">
              <div>
                <h2>Available promo codes</h2>
                <p>Tap a code to view its details and conditions.</p>
              </div>
            </div>
            <div class="promo-chip-list">
              <button
                v-for="promo in promotionalCodes"
                :key="promo.code"
                type="button"
                class="promo-chip"
                @click="openPromo(promo)"
              >
                <span class="promo-chip__text">{{ promo.code }}</span>
              </button>
            </div>

            <div v-if="showPromoModal" class="promo-modal" @click.self="closePromo">
              <div class="promo-modal__card" role="dialog" aria-modal="true" :aria-label="selectedPromo?.code">
                <div class="promo-modal__header">
                  <div>
                    <p class="promo-modal__label">Promotional code</p>
                    <h3 class="promo-modal__code">{{ selectedPromo?.code }}</h3>
                  </div>
                  <button type="button" class="promo-modal__close" @click="closePromo" aria-label="Close">×</button>
                </div>
                <p class="promo-modal__desc">{{ selectedPromo?.description }}</p>
                <p class="promo-modal__condition"><strong>Condition:</strong> {{ selectedPromo?.condition }}</p>
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button
              type="button"
              @click="resetForm"
              :disabled="isSaving"
              class="secondary-button"
            >
              Reset
            </button>
            <button
              type="submit"
              :disabled="isSaving || !hasChanges"
              class="green-button"
            >
              {{ isSaving ? 'Saving...' : 'Update Profile' }}
            </button>
          </div>
        </form>
      </div>

      <div class="user-accent" style="--accent-color: rgba(140, 200, 158, 0.15)"></div>
    </div>
  </div>
</template>

<script>
import { fetchUserById, updateUser } from '../services/userService';
import { getUserId, isAuthenticated } from '../services/authService';
import toastService from '../services/toastService';

export default {
  name: 'UserView',
  data() {
    return {
      user: null,
      form: {
        username: '',
        email: '',
        fullName: '',
        favouriteFlavour: ''
      },
      isLoading: true,
      isSaving: false,
      error: null,
      updateError: null,
      updateSuccess: false,
      promotionalCodes: [
        {
          code: 'LuckyForSome',
          name: 'Lucky For Some',
          description: '13% discount on your entire order',
          condition: 'Minimum order total: £13.00'
        },
        {
          code: 'MegaMelt100',
          name: 'Mega Melt 100',
          description: '£20 discount on your order',
          condition: 'Minimum order total: £100.00'
        },
        {
          code: 'Frozen40',
          name: 'Frozen 40',
          description: 'Pay 60% of order total (40% off)',
          condition: 'Minimum order total: £40.00 AND 4 or more treats in basket'
        },
        {
          code: 'TripleTreat3',
          name: 'Triple Treat 3',
          description: '£3 discount when you add 3+ treats',
          condition: '3 or more treats in basket'
        },
        {
          code: 'ScoopThereItIs!',
          name: 'Scoop There It Is!',
          description: '£1 discount on any order',
          condition: 'No minimum order required'
        }
      ],
      showPromoModal: false,
      selectedPromo: null
    };
  },
  computed: {
    formattedCreatedAt() {
      if (!this.user?.createdAt) return '';
      return new Date(this.user.createdAt).toLocaleDateString('en-GB', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    },
    hasChanges() {
      if (!this.user) return false;
      return (
        this.form.username !== this.user.username ||
        this.form.email !== this.user.email ||
        this.form.fullName !== this.user.fullName ||
        this.form.favouriteFlavour !== (this.user.favouriteFlavour || '')
      );
    }
  },
  async created() {
    await this.loadUser();
  },
  methods: {
    async loadUser() {
      this.isLoading = true;
      this.error = null;

      if (!isAuthenticated()) {
        this.error = 'You must be logged in to view this page';
        this.isLoading = false;
        return;
      }

      const userId = getUserId();
      if (!userId) {
        this.error = 'User ID not found. Please log in again.';
        this.isLoading = false;
        return;
      }

      try {
        this.user = await fetchUserById(userId);
        this.form.username = this.user.username;
        this.form.email = this.user.email;
        this.form.fullName = this.user.fullName;
        this.form.favouriteFlavour = this.user.favouriteFlavour || '';
      } catch (err) {
        this.error = err.message || 'Failed to load user information';
      } finally {
        this.isLoading = false;
      }
    },
    async handleUpdate() {
      this.isSaving = true;
      this.updateError = null;
      this.updateSuccess = false;

      const userId = getUserId();

      try {
        const updatedUser = await updateUser(userId, {
          username: this.form.username,
          email: this.form.email,
          fullName: this.form.fullName,
          favouriteFlavour: this.form.favouriteFlavour
        });

        this.user = updatedUser;
        this.updateSuccess = true;
        toastService.success('Your profile has been updated', 'Profile Updated');

        // Clear success message after 3 seconds
        setTimeout(() => {
          this.updateSuccess = false;
        }, 3000);
      } catch (err) {
        this.updateError = err.message || 'Failed to update user information';
      } finally {
        this.isSaving = false;
      }
    },
    resetForm() {
      if (this.user) {
        this.form.username = this.user.username;
        this.form.email = this.user.email;
        this.form.fullName = this.user.fullName;
        this.form.favouriteFlavour = this.user.favouriteFlavour || '';
        this.updateError = null;
        this.updateSuccess = false;
      }
    },
    openPromo(promo) {
      this.selectedPromo = promo;
      this.showPromoModal = true;
    },
    closePromo() {
      this.showPromoModal = false;
      this.selectedPromo = null;
    }
  }
};
</script>

<style scoped>
.user-view {
  min-height: 60vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
  padding: 2rem 1rem 2.5rem;
}

.user-container {
  position: relative;
  width: 100%;
  max-width: 680px;
  margin: 0 auto;
}

.tiles-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  width: 100%;
  align-items: center;
}

.user-card {
  background: var(--color-background-soft);
  border-radius: 12px;
  padding: 2.5rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  position: relative;
  z-index: 1;
}

.user-header {
  text-align: center;
  margin-bottom: 2rem;
}

.user-header h1 {
  font-size: 2rem;
  margin-bottom: 0.5rem;
  color: var(--color-heading);
}

.user-subtitle {
  color: var(--color-text);
  opacity: 0.8;
  font-size: 0.95rem;
}

.loading-state {
  text-align: center;
  padding: 2rem;
  color: var(--color-text);
}

.user-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 600;
  color: var(--color-heading);
  font-size: 0.95rem;
}

.form-input {
  padding: 0.75rem;
  border: 2px solid var(--color-border);
  border-radius: 8px;
  font-size: 1rem;
  background: var(--color-background);
  color: var(--color-text);
  transition: border-color 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-input:disabled {
  background: var(--color-background-mute);
  cursor: not-allowed;
  opacity: 0.7;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 0.5rem;
}

.green-button,
.secondary-button {
  flex: 1;
  padding: 0.875rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.green-button {
  background: var(--color-primary);
  color: white;
}

.green-button:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
}

.green-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.secondary-button {
  background: var(--color-background-mute);
  color: var(--color-text);
  border: 2px solid var(--color-border);
}

.secondary-button:hover:not(:disabled) {
  background: var(--color-background-soft);
}

.secondary-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.validation-box {
  display: flex;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 8px;
  background: rgba(255, 193, 7, 0.1);
  border: 2px solid rgba(255, 193, 7, 0.3);
}

.validation-box--warning {
  background: rgba(255, 193, 7, 0.1);
  border-color: rgba(255, 193, 7, 0.3);
}

.validation-box--success {
  background: rgba(76, 175, 80, 0.1);
  border-color: rgba(76, 175, 80, 0.3);
}

.validation-box__icon {
  font-size: 1.25rem;
  flex-shrink: 0;
}

.validation-box__title {
  display: block;
  margin-bottom: 0.25rem;
  color: var(--color-heading);
}

.validation-box__message {
  margin: 0;
  color: var(--color-text);
  font-size: 0.9rem;
}

.user-accent {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 120%;
  height: 120%;
  background: var(--accent-color);
  border-radius: 50%;
  filter: blur(80px);
  z-index: 0;
  pointer-events: none;
}

/* Promotional Codes Styles */

.promo-inline {
  margin-top: 1.2rem;
  padding: 0.85rem 0.95rem;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  background: var(--color-background-soft);
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.promo-inline__header {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.promo-inline__eyebrow {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 0.72rem;
  color: var(--color-text);
  opacity: 0.65;
}

.promo-inline__header h2 {
  margin: 0;
  font-size: 1.05rem;
  color: var(--color-heading);
}

.promo-inline__header p {
  margin: 0;
  color: var(--color-text);
  opacity: 0.82;
  font-size: 0.9rem;
}

.promo-chip-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.promo-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0.26rem 0.5rem;
  border: 1px solid var(--color-border);
  border-radius: 999px;
  background: var(--color-background-mute);
  color: var(--color-text);
  cursor: pointer;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease, background 0.2s ease;
  box-shadow: inset 0 1px 0 rgba(255,255,255,0.4);
}

.promo-chip:hover {
  border-color: rgba(86, 197, 110, 0.6);
  box-shadow: 0 1px 6px rgba(0,0,0,0.06);
  transform: translateY(-0.5px);
  background: rgba(163, 230, 175, 0.16);
}

.promo-chip__text {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-weight: 700;
  color: var(--color-heading);
  font-size: 0.86rem;
}

.promo-modal {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 30;
}

.promo-modal__card {
  background: var(--color-background);
  border-radius: 12px;
  border: 1px solid var(--color-border);
  box-shadow: 0 10px 30px rgba(0,0,0,0.18);
  max-width: 420px;
  width: 100%;
  padding: 1.5rem;
}

.promo-modal__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.promo-modal__label {
  margin: 0;
  font-size: 0.85rem;
  color: var(--color-text);
  opacity: 0.75;
}

.promo-modal__code {
  margin: 0.15rem 0 0 0;
  font-size: 1.35rem;
  color: var(--color-heading);
}

.promo-modal__desc {
  margin: 0 0 0.6rem 0;
  color: var(--color-text);
  font-size: 1rem;
}

.promo-modal__condition {
  margin: 0;
  color: var(--color-text);
  opacity: 0.85;
  font-size: 0.95rem;
}

.promo-modal__close {
  border: none;
  background: var(--color-background-mute);
  color: var(--color-heading);
  border-radius: 999px;
  width: 32px;
  height: 32px;
  font-size: 1.2rem;
  cursor: pointer;
  border: 1px solid var(--color-border);
  line-height: 1;
}

.promo-modal__close:hover {
  background: var(--color-background-soft);
}

@media (max-width: 768px) {
  .promo-codes-card {
    padding: 1.25rem;
  }

  .promo-codes-header h2 {
    font-size: 1.3rem;
  }

  .promo-codes-grid {
    grid-template-columns: 1fr;
  }
}
</style>

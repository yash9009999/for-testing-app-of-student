<template>
  <div class="scoops-builder-view">
    <div class="tiles-container">
      <div class="builder-top">
        <div class="selection-details">
          <div class="selection-tile selection-summary">
            <div class="summary-header">
              <div>
                <p class="summary-label">Current treat</p>
                <p class="summary-total">£{{ treatTotalPrice.toFixed(2) }}</p>
              </div>
              <div class="summary-counts">
                <span class="badge">{{ selectedCone ? '1 cone' : 'No cone' }}</span>
                <span class="badge">{{ selectedFlavours.length }} flavour{{ selectedFlavours.length === 1 ? '' : 's' }}</span>
                <span class="badge">{{ selectedToppings.length }} topping{{ selectedToppings.length === 1 ? '' : 's' }}</span>
              </div>
            </div>

            <div class="composition">
              <div class="chip-group">
                <p class="chip-group-title">Cone</p>
                <div class="chip-row">
                  <span class="chip cone-chip" v-if="selectedCone">{{ selectedCone.productName }}</span>
                  <span class="chip chip-empty cone-chip" v-else>Not selected</span>
                </div>
              </div>

              <div class="chip-group">
                <p class="chip-group-title">Flavours</p>
                <div class="chip-row">
                    <span v-if="!selectedFlavours.length" class="chip chip-empty flavour-chip">None</span>
                    <span v-for="flavour in selectedFlavours" :key="flavour.productId" class="chip flavour-chip">{{ flavour.productName }}</span>
                </div>
              </div>

              <div class="chip-group">
                <p class="chip-group-title">Toppings</p>
                <div class="chip-row">
                    <span v-if="!selectedToppings.length" class="chip chip-empty topping-chip">None</span>
                    <span v-for="topping in selectedToppings" :key="topping.productId" class="chip topping-chip">{{ topping.productName }}</span>
                </div>
              </div>
            </div>
            <div class="selection-actions">
              <button class="green-button" @click="generateRandomTreat" title="Generate a random treat combination">
                🎲 Random Treat
              </button>
              <button
                class="green-button"
                :class="{ 'disabled-button': !treatIsValid }"
                :disabled="!treatIsValid"
                :title="!treatIsValid ? validationReason : 'Add selected items to basket'"
                :aria-disabled="!treatIsValid"
                @click="addToCart"
              >
                {{ treatIsValid ? 'Add to Basket' : 'Add to Basket (disabled)'}}
              </button>
            </div>
            <div v-if="!treatIsValid" class="validation-box validation-box--warning" role="status" aria-live="polite">
              <div>
                <strong class="validation-box__title">Action needed:</strong>
                <p class="validation-box__message">
                  <span v-if="!selectedCone">Select 1 cone.</span>
                  <span v-else-if="selectedFlavours.length < 1">Select at least 1 flavour.</span>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="tile gradient-purple selection-section selection-panel">
        <h2 class="selection-panel-title">Select Cone (required)</h2>
        <div class="cone-list">
          <div class="cone-item product-card cone-card" v-for="cone in cones" :key="cone.productName">
            <div class="product-row">
              <input type="radio" name="cone" :checked="selectedCone && selectedCone.productId === cone.productId" @change="selectCone(cone)" />
              <span class="product-name">{{ cone.productName }}</span>
              <span class="product-price badge">£{{ cone.price.toFixed(2) }}</span>
            </div>
            <p class="product-description">{{ cone.description }}</p>
            <p class="product-ingredients" v-if="cone.ingredients && cone.ingredients.length">
              Ingredients:
              <span v-for="(obj, idx) in getIngredientObjects(cone.ingredients)" :key="obj.text + idx">
                <strong v-if="obj.isAllergen">{{ obj.text }}</strong><span v-else>{{ obj.text }}</span><span v-if="idx !== cone.ingredients.length - 1">, </span>
              </span>
            </p>
          </div>
        </div>
      </div>
      <div class="tile gradient-pink selection-section selection-panel">
        <h2 class="selection-panel-title">Select Flavours</h2>
        <div class="flavour-list">
          <div class="flavour-item product-card flavour-card" v-for="flavour in flavours" :key="flavour.productName">
            <div class="product-row">
              <input type="checkbox" :checked="selectedFlavours.some(f => f.productId === flavour.productId)" @change="selectFlavour(flavour)" />
              <span class="product-name">{{ flavour.productName }}</span>
              <span class="product-price badge">£{{ flavour.price.toFixed(2) }}</span>
            </div>
            <p class="product-description">{{ flavour.description }}</p>
            <p class="product-ingredients" v-if="flavour.ingredients && flavour.ingredients.length">
              Ingredients:
              <span v-for="(obj, idx) in getIngredientObjects(flavour.ingredients)" :key="obj.text + idx">
                <strong v-if="obj.isAllergen">{{ obj.text }}</strong><span v-else>{{ obj.text }}</span><span v-if="idx !== flavour.ingredients.length - 1">, </span>
              </span>
            </p>
          </div>
        </div>
      </div>
      <div class="tile gradient-green selection-section selection-panel">
        <h2 class="selection-panel-title">Select Toppings</h2>
        <div class="topping-list">
          <div class="topping-item product-card topping-card" v-for="topping in toppings" :key="topping.productName">
            <div class="product-row">
              <input type="checkbox" :checked="selectedToppings.some(t => t.productId === topping.productId)" @change="selectTopping(topping)" />
              <span class="product-name">{{ topping.productName }}</span>
              <span class="product-price badge">£{{ topping.price.toFixed(2) }}</span>
            </div>
            <p class="product-description">{{ topping.description }}</p>
            <p class="product-ingredients" v-if="topping.ingredients && topping.ingredients.length">
              Ingredients:
              <span v-for="(obj, idx) in getIngredientObjects(topping.ingredients)" :key="obj.text + idx">
                <strong v-if="obj.isAllergen">{{ obj.text }}</strong><span v-else>{{ obj.text }}</span><span v-if="idx !== topping.ingredients.length - 1">, </span>
              </span>
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { fetchProducts } from '../services/productService';
import { createOrder, updateOrder, getOrder } from '../services/orderService';
import toastService from '../services/toastService';
import { RouterLink } from 'vue-router';

export default {
  name: 'ScoopsBuilderView',
  components: {
    RouterLink
  },
  data() {
    return {
      products: [],
      flavours: [],
      toppings: [],
      cones: [],
  selectedFlavours: [],
  selectedToppings: [],
  selectedCone: null,
      cart: [],
      ukAllergens: [
        'celery', 'cereals containing gluten', 'crustaceans', "eggs", 'fish',
        'lupin', 'milk', 'molluscs', 'mustard', 'nuts',
        'peanuts', 'sesame seeds', 'soya', 'sulphur dioxide'
      ]
    }
  },
  async created() {
    await this.loadProducts();
  },
  methods: {
    async loadProducts() {
      try {
        this.products = await fetchProducts();
        this.flavours = this.products.filter(p => p.type === 'FLAVOR');
        this.toppings = this.products.filter(p => p.type === 'TOPPING');
        this.cones = this.products.filter(p => p.type === 'CONE');
      } catch (error) {
        console.error('Error loading products:', error);
      }
    },
    getIngredientObjects(ingredients) {
      // Returns [{ text, isAllergen }] for each ingredient
      return ingredients.map(ingredient => {
        const ingredientNorm = ingredient.trim().toLowerCase();
        const isAllergen = this.ukAllergens.some(allergen => {
          const allergenNorm = allergen.trim().toLowerCase();
          return ingredientNorm === allergenNorm;
        });
        return { text: ingredient, isAllergen };
      });
    },
    selectFlavour(flavour) {
      const idx = this.selectedFlavours.findIndex(f => f.productId === flavour.productId);
      if (idx === -1) {
        this.selectedFlavours.push(flavour);
      } else {
        this.selectedFlavours.splice(idx, 1);
      }
    },
    selectTopping(topping) {
      const idx = this.selectedToppings.findIndex(t => t.productId === topping.productId);
      if (idx === -1) {
        this.selectedToppings.push(topping);
      } else {
        this.selectedToppings.splice(idx, 1);
      }
    },
    selectCone(cone) {
      this.selectedCone = cone;
    },
    addToCart() {
      if (!this.treatIsValid) return;
      // Build a Treat payload: name, quantity, products (by id)
      const products = [];
      // cone + flavours + toppings
      if (this.selectedCone) products.push(this.selectedCone);
      this.selectedFlavours.forEach(f => products.push(f));
      this.selectedToppings.forEach(t => products.push(t));

      // Build order payload
      const orderId = localStorage.getItem('s2g_orderId');
      const orderPayload = {
        promotion: null,
        basketItems: [{products: products}]
      };

      // If there's an existing order, we'll PUT with updated basket (append treat)
      const doRequest = async () => {
        try {
          if (!orderId) {
            //orderPayload.basketItems = products; //[treatPayload];
            console.log("basketItems:" + JSON.stringify(orderPayload.basketItems));
            const created = await createOrder(orderPayload);
            localStorage.setItem('s2g_orderId', created.orderId);
            this.$emit('orderUpdated', created);
            // toast success for creation
            toastService.success(`Order ${created.orderId} created and item added to basket`, 'Order Created');
          } else {
            // Fetch current order, append the new treat, then PUT the full order
            try {
              const existing = await getOrder(parseInt(orderId, 10));
              if (!existing || !existing.basketItems) existing.basketItems = [];
              existing.basketItems.push({products: products});
              const updated = await updateOrder(existing);
              this.$emit('orderUpdated', updated);
              // toast success for update
              toastService.success(`Order ${updated.orderId} updated — item added to basket`, 'Order Updated');
            } catch (fetchErr) {
              // If fetching fails (order missing), fallback to creating a new order
              console.warn('Fetching existing order failed, creating new order', fetchErr);
              //orderPayload.basketItems = products;
              const created = await createOrder(orderPayload);
              localStorage.setItem('s2g_orderId', created.orderId);
              this.$emit('orderUpdated', created);
              toastService.success(`Order ${created.orderId} created and item added to basket`, 'Order Created');
            }
          }

          // clear selection
          this.selectedFlavours = [];
          this.selectedToppings = [];
          this.selectedCone = null;
        } catch (err) {
          console.error('Order request failed', err);
          const msg = err && err.message ? err.message : 'Unknown error';
          toastService.error(`Order request failed: ${msg}`, 'Order Error');
        }
      };
      doRequest();
    },
    generateRandomTreat() {
      // Pick a random cone
      if (this.cones.length > 0) {
        const randomConeIdx = Math.floor(Math.random() * this.cones.length);
        this.selectedCone = this.cones[randomConeIdx];
      }

      // Pick 1-3 random flavours
      if (this.flavours.length > 0) {
        const numFlavours = Math.floor(Math.random() * 3) + 1; // 1 to 3
        this.selectedFlavours = [];
        const flavourIndices = new Set();
        while (flavourIndices.size < Math.min(numFlavours, this.flavours.length)) {
          flavourIndices.add(Math.floor(Math.random() * this.flavours.length));
        }
        flavourIndices.forEach(idx => this.selectedFlavours.push(this.flavours[idx]));
      }

      // Pick 0-2 random toppings
      if (this.toppings.length > 0) {
        const numToppings = Math.floor(Math.random() * 3); // 0 to 2
        this.selectedToppings = [];
        const toppingIndices = new Set();
        while (toppingIndices.size < Math.min(numToppings, this.toppings.length)) {
          toppingIndices.add(Math.floor(Math.random() * this.toppings.length));
        }
        toppingIndices.forEach(idx => this.selectedToppings.push(this.toppings[idx]));
      }

      toastService.success('Random treat generated! Feel free to adjust or add to basket.', 'Random Treat');
    }
  }
  ,
  computed: {
    treatIsValid() {
      return (
        this.selectedCone &&
        this.selectedFlavours.length >= 1 &&
        // upper limits removed: allow more than 3 flavours and more than 5 toppings
        true
      );
    },
    treatTotalPrice() {
      let total = 0;
      if (this.selectedCone) total += this.selectedCone.price;
      total += this.selectedFlavours.reduce((s, f) => s + (f.price || 0), 0);
      total += this.selectedToppings.reduce((s, t) => s + (t.price || 0), 0);
      return total;
    }
    ,
    validationReason() {
      if (!this.selectedCone) return 'Please select a cone.';
      if (this.selectedFlavours.length < 1) return 'Select at least 1 flavour.';
      // upper limits removed
      return '';
    }
  }
}
</script>

<style scoped>
.scoops-builder-view {
  padding: 2rem;
  text-align: center;
}

.tiles-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.builder-top {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.selection-details {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  position: relative;
}

.selection-summary {
  border-radius: 10px;
  border: 1px solid var(--color-border);
  background: var(--color-background-soft);
  box-shadow: 0 2px 6px rgba(0,0,0,0.08);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  text-align: left;
}

.selection-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.6rem;
  align-items: center;
  justify-content: flex-start;
  padding-top: 0.75rem;
  border-top: 1px solid var(--color-border);
}

.summary-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.summary-label {
  margin: 0;
  font-weight: 600;
  color: var(--color-text);
}

.summary-total {
  margin: 0;
  font-size: 1.4rem;
  font-weight: 800;
  color: var(--color-heading);
}

.summary-counts {
  display: flex;
  gap: 0.4rem;
  align-items: center;
  flex-wrap: wrap;
}

.composition {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.chip-group {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.chip-group-title {
  margin: 0;
  font-size: 0.85rem;
  color: var(--color-text);
  opacity: 0.8;
}

.chip-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}

.chip {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  background: var(--color-background-mute);
  color: var(--color-text);
  border: 1px solid var(--color-border);
  font-size: 0.9rem;
}

.chip-empty {
  opacity: 0.7;
}

.cone-chip {
  background: rgba(142, 107, 247, 0.18);
  border-color: rgba(142, 107, 247, 0.55);
}

.flavour-chip {
  background: rgba(255, 179, 191, 0.2);
  border-color: rgba(255, 140, 158, 0.55);
}

.topping-chip {
  background: rgba(163, 230, 175, 0.22);
  border-color: rgba(86, 197, 110, 0.55);
}

.product-card.flavour-card {
  border-color: rgba(255, 140, 158, 0.38);
}

.product-card.topping-card {
  border-color: rgba(86, 197, 110, 0.34);
}

.product-card.cone-card {
  border-color: rgba(142, 107, 247, 0.34);
}

.selection-panel.gradient-pink {
  background: var(--color-background-soft);
  border-color: rgba(255, 140, 158, 0.38);
}

.selection-panel.gradient-green {
  background: var(--color-background-soft);
  border-color: rgba(86, 197, 110, 0.34);
}

.selection-panel.gradient-purple {
  background: var(--color-background-soft);
  border-color: rgba(142, 107, 247, 0.34);
}

.selection-section {
  margin-bottom: 2rem;
}

.selection-panel {
  border-radius: 12px;
  padding: 1.05rem 1.2rem;
  border: 1px solid var(--color-border);
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  background: var(--color-background-soft);
  backdrop-filter: blur(2px);
}

.selection-panel h2 {
  margin-top: 0;
  margin-bottom: 0.8rem;
  color: var(--color-heading);
}

.selection-panel-title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  letter-spacing: 0.01em;
  color: var(--color-heading);
}

.flavour-list, .topping-list, .cone-list {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.product-card {
  padding: 1.05rem 1.2rem;
  width: 100%;
  max-width: 620px;
  text-align: left;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: var(--color-background-soft);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  backdrop-filter: blur(2px);
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.35rem;
}

.product-row {
  width: 100%;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.2rem;
}
.product-price {
  margin-left: 1rem;
  color: var(--color-text);
}

.product-name {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--color-heading);
  flex: 1;
}

.flavour-item .product-description,
.topping-item .product-description,
.cone-item .product-description {
  font-size: 0.9rem !important;
  color: var(--color-text) !important;
  margin: 0;
  opacity: 0.9;
}

.flavour-item .product-ingredients,
.topping-item .product-ingredients,
.cone-item .product-ingredients {
  font-size: 0.82rem !important;
  color: var(--color-text) !important;
  opacity: 0.7;
  margin: 0;
}
.product-ingredients strong {
  font-weight: bold;
}

.disabled-button {
  opacity: 0.45;
  cursor: not-allowed;
  box-shadow: none !important;
  filter: grayscale(30%);
  border: 1px solid rgba(0,0,0,0.08);
  pointer-events: none;
}

.validation-box {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;
  padding: 0.8rem 1rem;
  border-radius: 10px;
  border: 1px solid rgba(238, 170, 60, 0.35);
  background-color: rgba(238, 170, 60, 0.12);
  box-shadow: 0 8px 18px rgba(0,0,0,0.06);
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
}
.validation-box--warning {
  border-color: rgba(238, 170, 60, 0.35);
}
.validation-box__icon {
  font-size: 1.05rem;
  line-height: 1.2;
}

/* Custom checkbox/radio styles to match app theme */
input[type="checkbox"], input[type="radio"] {
  appearance: none;
  -webkit-appearance: none;
  width: 16px;
  height: 16px;
  border-radius: 6px;
  border: 2px solid var(--color-border);
  background: var(--color-background);
  display: inline-block;
  vertical-align: text-top;
  margin-right: 0.6rem;
  position: relative;
  top: 2px;
  transition: all 0.2s;
}

input[type="radio"] {
  border-radius: 50%;
}

input[type="checkbox"]:checked, input[type="radio"]:checked {
  background: var(--color-primary);
  border-color: var(--color-primary);
}

input[type="checkbox"]:checked::after,
input[type="radio"]:checked::after {
  content: none;
}

input[type="checkbox"]:focus-visible, input[type="radio"]:focus-visible {
  outline: 3px solid var(--color-primary-light);
  outline-offset: 2px;
}

input[type="checkbox"]:disabled, input[type="radio"]:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Category-specific checkbox/radio colors */
.flavour-card input[type="checkbox"]:checked {
  background: rgba(255, 140, 158, 0.85);
  border-color: rgba(255, 140, 158, 0.85);
}

.topping-card input[type="checkbox"]:checked {
  background: rgba(86, 197, 110, 0.85);
  border-color: rgba(86, 197, 110, 0.85);
}

.cone-card input[type="radio"]:checked {
  background: rgba(142, 107, 247, 0.85);
  border-color: rgba(142, 107, 247, 0.85);
}

.checkout-button {
  margin-top: 1rem;
  display: inline-block;
  text-align: center;
}
</style>
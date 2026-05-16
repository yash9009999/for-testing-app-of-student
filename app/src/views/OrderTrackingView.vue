<template>
  <div class="order-tracking-view">
    <div class="tracking-container">
      <div class="lookup-card">
        <div class="summary-header">
          <div>
            <p class="summary-label">Order Tracking</p>
            <p class="summary-total">Find your order status</p>
          </div>
        </div>
        <div class="lookup-form product-row">
          <input class="input" v-model="orderNumber" placeholder="Enter order number or memorable name" />
          <button class="green-button" @click="search">Find Order</button>
        </div>
        <div v-if="loading" class="lookup-status">Searching...</div>
        <div v-if="error" class="validation-box validation-box--warning">{{ error }}</div>
      </div>

      <div v-if="order" class="order-card">
        <div class="order-card-header">
          <div>
            <p class="summary-label">Order #{{ order.orderId }}</p>
            <p v-if="order.memorableName" class="summary-memorable-name">{{ order.memorableName }}</p>
            <p class="summary-total">{{ formatDateTime(order.orderTime) }}</p>
          </div>
          <div class="summary-counts">
            <span class="badge">{{ order.basketItems.length }} treat{{ order.basketItems.length === 1 ? '' : 's' }}</span>
            <span class="badge">ETA {{ formatDateTime(estimatedDeliveryRaw) }}</span>
          </div>
        </div>

        <div class="order-meta">
          <div class="summary-row">
            <span class="summary-label-sm">Placed</span>
            <span class="summary-value-sm">{{ formatDateTime(order.orderTime) }}</span>
          </div>
          <div class="summary-row">
            <span class="summary-label-sm">Estimated Delivery</span>
            <span class="summary-value-sm">{{ formatDateTime(estimatedDeliveryRaw) }}</span>
          </div>
        </div>

        <div class="basket-items-card">
          <h2 class="section-title">Items</h2>
          <div class="basket-items">
            <div v-for="(treat, idx) in order.basketItems" :key="treat.id || idx" class="cart-item">
              <div class="item-details">
                <p>{{ treat.products ? treat.products.map(p => p.productName).join(', ') : '' }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'OrderTrackingView',
  props: {
    initialOrderNumber: { type: String, default: null },
    initialEstimatedDeliveryTime: { type: String, default: null }
  },
  data() {
    return {
      orderNumber: this.initialOrderNumber || '',
      order: null,
      loading: false,
      error: null,
      // store raw ISO timestamp for estimated delivery and format on render
      estimatedDeliveryRaw: this.initialEstimatedDeliveryTime || null,
      formatter: null
    }
  },
  async created() {
    // create a consistent, human-friendly formatter (weekday, date, time, timezone)
    this.formatter = new Intl.DateTimeFormat(undefined, {
      weekday: 'short', year: 'numeric', month: 'short', day: 'numeric',
      hour: 'numeric', minute: '2-digit', hour12: true, timeZoneName: 'short'
    });

    if (this.orderNumber) await this.search();
  },
  methods: {
    async search() {
      this.loading = true; this.error = null; this.order = null;
      // accept memorable name
      if (!this.orderNumber) {
        this.loading = false;
        this.error = 'Please enter an order number or memorable name.';
        return;
      }
      try {
        const { findOrderByMemorableName, getOrder } = await import('../services/orderService');
        // Try to fetch by ID if it's numeric, otherwise search by memorable name
        let res;
        if (!isNaN(this.orderNumber)) {
          try {
            res = await getOrder(parseInt(this.orderNumber, 10));
          } catch (e) {
            // If numeric fetch fails, try memorable name search
            res = await findOrderByMemorableName(this.orderNumber);
          }
        } else {
          res = await findOrderByMemorableName(this.orderNumber);
        }
        
        if (!res) {
          this.error = 'Order not found';
          return;
        }
        this.order = res;
        // compute a raw estimated delivery timestamp if not provided
        if (!this.estimatedDeliveryRaw) {
          if (res.estimatedDeliveryTime) {
            this.estimatedDeliveryRaw = res.estimatedDeliveryTime;
          } else if (res.orderTime && res.estDeliveryMinutes) {
            const t = new Date(res.orderTime).getTime() + res.estDeliveryMinutes * 60000;
            this.estimatedDeliveryRaw = new Date(t).toISOString();
          } else {
            this.estimatedDeliveryRaw = null;
          }
        }
      } catch (err) {
        this.error = err.message || 'Failed to find order';
      } finally {
        this.loading = false;
      }
    },
    formatDateTime(value) {
      if (!value) return '—';
      try {
        const d = (typeof value === 'number') ? new Date(value) : new Date(value);
        if (isNaN(d.getTime())) return 'Invalid date';
        return this.formatter ? this.formatter.format(d) : d.toLocaleString();
      } catch (e) {
        return 'Invalid date';
      }
    }
  }
}
</script>

<style scoped>
.order-tracking-view {
  padding: 2rem;
}

.tracking-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  max-width: 900px;
  margin: 0 auto;
}

.lookup-card,
.order-card,
.basket-items-card {
  padding: 1.05rem 1.2rem;
  border-radius: 12px;
  border: 1px solid var(--color-border);
  background: var(--color-background-soft);
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  backdrop-filter: blur(2px);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  text-align: left;
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

.summary-memorable-name {
  margin: 0.25rem 0 0 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--vt-c-green);
  font-family: monospace;
  letter-spacing: 0.05em;
}

.summary-total {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--color-heading);
}

.summary-counts {
  display: flex;
  gap: 0.4rem;
  align-items: center;
  flex-wrap: wrap;
}

.badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.6rem;
  border-radius: 999px;
  background: var(--color-background-mute);
  color: var(--color-text);
  border: 1px solid var(--color-border);
  font-size: 0.9rem;
}

.lookup-form {
  display: flex;
  gap: 0.6rem;
  width: 100%;
  align-items: center;
}

.lookup-status {
  margin-top: 0.35rem;
  color: var(--color-text);
  opacity: 0.7;
}

.order-card {
  gap: 1rem;
}

.order-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.order-meta {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  border-top: 1px solid var(--color-border);
  padding-top: 0.75rem;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.summary-label-sm {
  font-weight: 600;
  color: var(--color-text);
  font-size: 0.9rem;
}

.summary-value-sm {
  font-weight: 700;
  color: var(--color-heading);
  font-size: 0.9rem;
}

.basket-items-card {
  gap: 1rem;
}

.section-title {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 800;
  letter-spacing: 0.01em;
  color: var(--color-heading);
}

.basket-items {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.cart-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--color-border);
}

.cart-item:last-child {
  border-bottom: none;
}

.item-details p {
  margin: 0;
  color: var(--color-text);
}

.validation-box {
  margin-top: 0.35rem;
}
</style>

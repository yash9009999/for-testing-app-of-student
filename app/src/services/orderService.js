import { BASE_URL } from '../api';

/** SSD: strip display-only product fields so the API only receives stable ids (mitigates price/name tampering in transit). */
function sanitizeOrderCreatePayload(orderPayload) {
  return {
    promotion: orderPayload.promotion ?? null,
    basketItems: (orderPayload.basketItems || []).map((t) => ({
      products: (t.products || []).map((p) => ({ productId: p.productId }))
    }))
  };
}

/** SSD: same as create, plus numeric order id required for PUT semantics. */
function sanitizeOrderUpdatePayload(orderPayload) {
  return {
    orderId: orderPayload.orderId,
    promotion: orderPayload.promotion ?? null,
    basketItems: (orderPayload.basketItems || []).map((t) => ({
      products: (t.products || []).map((p) => ({ productId: p.productId }))
    }))
  };
}

export async function createOrder(orderPayload) {
  const body = sanitizeOrderCreatePayload(orderPayload);
  console.log('Creating order with payload:', body);
  const res = await fetch(`${BASE_URL}/order`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch(e) { errText = res.statusText; }
    throw new Error(`Failed to create order: ${errText}`);
  }
  return res.json();
}

export async function updateOrder(orderPayload) {
  const body = sanitizeOrderUpdatePayload(orderPayload);
  const res = await fetch(`${BASE_URL}/order`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch(e) { errText = res.statusText; }
    throw new Error(`Failed to update order: ${errText}`);
  }
  return res.json();
}

export async function findOrderByMemorableName(memorableName) {
  const res = await fetch(`${BASE_URL}/order/search?memorableName=${encodeURIComponent(memorableName)}`);
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch(e) { errText = res.statusText; }
    throw new Error(`Failed to search orders: ${errText}`);
  }
  const orders = await res.json();
  // Return the first order if found, or null
  return orders && orders.length > 0 ? orders[0] : null;
}

export async function getOrder(orderId) {
  // If orderId is not a number, treat it as a memorable name
  if (isNaN(orderId)) {
    return findOrderByMemorableName(orderId);
  }
  const res = await fetch(`${BASE_URL}/order/${orderId}`);
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch(e) { errText = res.statusText; }
    throw new Error(`Failed to fetch order: ${errText}`);
  }
  return res.json();
}

export async function checkoutOrder(orderId, payload) {
  const res = await fetch(`${BASE_URL}/order/${orderId}/checkout`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: payload ? JSON.stringify(payload) : null
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch(e) { errText = res.statusText; }
    throw new Error(`Checkout failed: ${errText}`);
  }
  return res.json();
}

export async function clearOrder(orderId) {
  const resp = await fetch(`${BASE_URL}/order/${orderId}`, {
    method: 'DELETE'
  });
  if (!resp.ok) throw new Error(`Failed to clear order: ${resp.status}`);

  // Handle 204 or empty body safely
  if (resp.status === 204) return { orderId };
  const text = await resp.text();
  if (!text) return { orderId };
  return JSON.parse(text);
}

export async function searchOrder(orderNumber) {
  // Expect a numeric id (string or number) and fetch by id
  if (!orderNumber) throw new Error('Order id required');
  const id = Number(orderNumber);
  if (!Number.isInteger(id)) throw new Error('Order id must be a numeric id');
  const res = await fetch(`${BASE_URL}/order/${id}`);
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch(e) { errText = res.statusText; }
    throw new Error(`Failed to find order: ${errText}`);
  }
  return res.json();
}

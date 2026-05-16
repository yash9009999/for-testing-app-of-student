import { BASE_URL } from '../api';
import { getAuthToken } from './authService';

const GUEST_TOKENS_KEY = 's2g_guestTokens';

/** SSD: per-order guest secrets issued at create time — required for unpurchased guest-cart API calls. */
export function storeGuestToken(orderId, token) {
  if (!orderId || !token) return;
  try {
    const map = JSON.parse(localStorage.getItem(GUEST_TOKENS_KEY) || '{}');
    map[String(orderId)] = token;
    localStorage.setItem(GUEST_TOKENS_KEY, JSON.stringify(map));
  } catch {
    /* ignore quota / private mode */
  }
}

export function getGuestToken(orderId) {
  if (!orderId) return null;
  try {
    const map = JSON.parse(localStorage.getItem(GUEST_TOKENS_KEY) || '{}');
    return map[String(orderId)] || null;
  } catch {
    return null;
  }
}

export function clearGuestToken(orderId) {
  if (!orderId) return;
  try {
    const map = JSON.parse(localStorage.getItem(GUEST_TOKENS_KEY) || '{}');
    delete map[String(orderId)];
    localStorage.setItem(GUEST_TOKENS_KEY, JSON.stringify(map));
  } catch {
    /* ignore */
  }
}

function buildOrderHeaders(orderId, includeJsonContentType = true) {
  const headers = {};
  if (includeJsonContentType) {
    headers['Content-Type'] = 'application/json';
  }
  const authToken = getAuthToken();
  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`;
  }
  const guestToken = getGuestToken(orderId);
  if (guestToken) {
    headers['X-Guest-Token'] = guestToken;
  }
  return headers;
}

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

function persistCreatedOrder(created) {
  if (created?.orderId) {
    localStorage.setItem('s2g_orderId', String(created.orderId));
  }
  if (created?.guestAccessToken && created?.orderId) {
    storeGuestToken(created.orderId, created.guestAccessToken);
  }
}

export async function createOrder(orderPayload) {
  const body = sanitizeOrderCreatePayload(orderPayload);
  const res = await fetch(`${BASE_URL}/order`, {
    method: 'POST',
    headers: buildOrderHeaders(null),
    body: JSON.stringify(body)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch { errText = res.statusText; }
    throw new Error(`Failed to create order: ${errText}`);
  }
  const created = await res.json();
  persistCreatedOrder(created);
  return created;
}

export async function updateOrder(orderPayload) {
  const body = sanitizeOrderUpdatePayload(orderPayload);
  const orderId = body.orderId;
  const res = await fetch(`${BASE_URL}/order`, {
    method: 'PUT',
    headers: buildOrderHeaders(orderId),
    body: JSON.stringify(body)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch { errText = res.statusText; }
    throw new Error(`Failed to update order: ${errText}`);
  }
  return res.json();
}

export async function findOrderByMemorableName(memorableName) {
  const res = await fetch(`${BASE_URL}/order/search?memorableName=${encodeURIComponent(memorableName)}`, {
    headers: buildOrderHeaders(null, false)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch { errText = res.statusText; }
    throw new Error(`Failed to search orders: ${errText}`);
  }
  const orders = await res.json();
  return orders && orders.length > 0 ? orders[0] : null;
}

export async function getOrder(orderId) {
  if (isNaN(orderId)) {
    return findOrderByMemorableName(orderId);
  }
  const res = await fetch(`${BASE_URL}/order/${orderId}`, {
    headers: buildOrderHeaders(orderId, false)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch { errText = res.statusText; }
    throw new Error(`Failed to fetch order: ${errText}`);
  }
  return res.json();
}

export async function checkoutOrder(orderId, payload) {
  const res = await fetch(`${BASE_URL}/order/${orderId}/checkout`, {
    method: 'POST',
    headers: buildOrderHeaders(orderId),
    body: payload ? JSON.stringify(payload) : null
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch { errText = res.statusText; }
    throw new Error(`Checkout failed: ${errText}`);
  }
  const result = await res.json();
  clearGuestToken(orderId);
  return result;
}

export async function clearOrder(orderId) {
  const resp = await fetch(`${BASE_URL}/order/${orderId}`, {
    method: 'DELETE',
    headers: buildOrderHeaders(orderId, false)
  });
  if (!resp.ok) throw new Error(`Failed to clear order: ${resp.status}`);

  clearGuestToken(orderId);

  if (resp.status === 204) return { orderId };
  const text = await resp.text();
  if (!text) return { orderId };
  return JSON.parse(text);
}

export async function searchOrder(orderNumber) {
  if (!orderNumber) throw new Error('Order id required');
  const id = Number(orderNumber);
  if (!Number.isInteger(id)) throw new Error('Order id must be a numeric id');
  const res = await fetch(`${BASE_URL}/order/${id}`, {
    headers: buildOrderHeaders(id, false)
  });
  if (!res.ok) {
    let errText = '';
    try { errText = await res.text(); errText = errText || res.statusText; } catch { errText = res.statusText; }
    throw new Error(`Failed to find order: ${errText}`);
  }
  return res.json();
}

import { BASE_URL } from '../api';

const AUTH_SERVER_URL = import.meta.env.VITE_AUTH_SERVER_URL || 'http://localhost:5001';

/**
 * Direct login to Auth Server (returns JWT token directly).
 * For a full OAuth flow, this would redirect to login page instead.
 */
export async function login(username, password) {
  const trimmedUser = (username ?? '').trim();
  const response = await fetch(`${AUTH_SERVER_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      username: trimmedUser,
      password: password ?? ''
    })
  });

  let data;
  try {
    data = await response.json();
  } catch {
    throw new Error(
      response.ok
        ? 'Login failed: invalid response from auth server.'
        : `Login failed (${response.status}). Is the auth server running on ${AUTH_SERVER_URL}?`
    );
  }

  if (!response.ok || !data.success) {
    throw new Error(data.message || `Login failed (${response.status})`);
  }

  return data;
}

/**
 * Direct registration to Auth Server (returns JWT token directly).
 * For a full OAuth flow, this would redirect to registration page instead.
 */
export async function register(fullName, username, email, password) {
  const response = await fetch(`${AUTH_SERVER_URL}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      fullName: (fullName ?? '').trim(),
      username: (username ?? '').trim(),
      email: (email ?? '').trim(),
      password: password ?? ''
    })
  });

  let data;
  try {
    data = await response.json();
  } catch {
    throw new Error(
      response.ok
        ? 'Registration failed: invalid response from auth server.'
        : `Registration failed (${response.status}). Is the auth server running on ${AUTH_SERVER_URL}?`
    );
  }

  if (!response.ok || !data.success) {
    throw new Error(data.message || `Registration failed (${response.status})`);
  }

  return data;
}

export function notifyAuthChanged() {
  window.dispatchEvent(new CustomEvent('auth-changed'));
}

/**
 * Exchange authorization code for access token (OAuth 2.0 flow).
 */
export async function exchangeAuthorizationCode(code) {
  try {
    const response = await fetch(`${AUTH_SERVER_URL}/api/auth/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        code,
        redirectUri: `${window.location.origin}/auth/callback`
      })
    });

    const data = await response.json();

    if (!data.success) {
      throw new Error(data.message || 'Token exchange failed');
    }

    return data;
  } catch (error) {
    console.error('Error exchanging authorization code:', error);
    throw error;
  }
}

export function storeAuthToken(token, userId) {
  localStorage.setItem('s2g_token', token);
  localStorage.setItem('s2g_userId', userId);
}

export function getAuthToken() {
  return localStorage.getItem('s2g_token');
}

export function getUserId() {
  return localStorage.getItem('s2g_userId');
}

export function clearAuth() {
  localStorage.removeItem('s2g_token');
  localStorage.removeItem('s2g_userId');
}

export function isAuthenticated() {
  return !!getAuthToken();
}

/** SSD: CSRF protection for OAuth redirect flow — validate in callback before exchanging code. */
export function beginOAuthState() {
  const state = crypto.randomUUID();
  sessionStorage.setItem('oauth_state', state);
  return state;
}

export function consumeOAuthState(receivedState) {
  const expected = sessionStorage.getItem('oauth_state');
  sessionStorage.removeItem('oauth_state');
  return !!expected && !!receivedState && expected === receivedState;
}

const TOKEN_KEY = 'auth_token';
const GUEST_KEY = 'guest_id';

// PoC fallback guest id (used when not authenticated via Google OAuth)
export const POC_GUEST_ID = '00000000-0000-0000-0000-000000000001';

export interface UserInfo {
  sub: string;
  email?: string;
  name?: string;
  picture?: string;
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function decodeJwtPayload(token: string): Record<string, unknown> {
  // atob() gives Latin-1 bytes; use TextDecoder to correctly handle UTF-8 (e.g. Cyrillic names)
  const base64 = token.split('.')[1].replaceAll('-', '+').replaceAll('_', '/');
  const bytes = Uint8Array.from(atob(base64), c => c.codePointAt(0) ?? 0);
  return JSON.parse(new TextDecoder().decode(bytes));
}

export function getUserInfo(): UserInfo | null {
  const token = getToken();
  if (token === 'demo') {
    return { sub: POC_GUEST_ID, email: 'demo@poc.local', name: 'Demo User' };
  }
  if (!token) return null;
  try {
    const payload = decodeJwtPayload(token);
    return {
      sub:     (payload.sub     as string) ?? '',
      email:   (payload.email   as string) ?? undefined,
      name:    (payload.name    as string) ?? (payload.email as string | undefined)?.split('@')[0] ?? undefined,
      picture: (payload.picture as string) ?? undefined,
    };
  } catch {
    return null;
  }
}

export function getGuestId(): string {
  return localStorage.getItem(GUEST_KEY) ?? POC_GUEST_ID;
}

export function isAuthenticated(): boolean {
  return !!getToken();
}

export function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(GUEST_KEY);
}

// PoC: skip Google OAuth, act as the hardcoded demo guest
export function loginAsDemo() {
  // Store a sentinel (not a real JWT) so isAuthenticated() returns true
  localStorage.setItem(TOKEN_KEY, 'demo');
  localStorage.setItem(GUEST_KEY, POC_GUEST_ID);
}

// Call this on app start — picks up ?token= from the OAuth callback redirect
export function tryConsumeTokenFromUrl() {
  const params = new URLSearchParams(globalThis.location.search);
  const token = params.get('token');
  if (!token) return;

  localStorage.setItem(TOKEN_KEY, token);

  // Decode JWT payload to extract sub (user id / email)
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const sub = payload.sub ?? payload.email ?? POC_GUEST_ID;
    localStorage.setItem(GUEST_KEY, sub);
  } catch {
    // malformed token — use fallback
  }

  // Clean token from URL without a page reload
  params.delete('token');
  const clean = params.toString();
  globalThis.history.replaceState({}, '', clean ? `?${clean}` : globalThis.location.pathname);
}

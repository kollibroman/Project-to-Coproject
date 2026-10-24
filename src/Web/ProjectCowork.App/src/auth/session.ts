import { client } from '../client/client.gen';
import { postApiAuthLogout, postApiAuthTokenRefresh } from '../client/sdk.gen';

// Bazowy adres API. Nadpisywalny przez zmienną środowiskową Vite VITE_API_URL.
const API_BASE_URL =
  import.meta.env.VITE_API_URL ?? 'http://localhost:5298';

// credentials: 'include' jest wymagane, żeby przeglądarka przyjmowała ciasteczko
// z refresh tokenem (Set-Cookie) i odsyłała je do API działającego na innym originie.
client.setConfig({ baseUrl: API_BASE_URL, credentials: 'include' });

// Sprzątanie po poprzedniej wersji, która trzymała tokeny w localStorage.
try {
  localStorage.removeItem('projectcowork.accessToken');
  localStorage.removeItem('projectcowork.refreshToken');
} catch {
  // brak dostępu do localStorage
}

/**
 * unknown       — trwa sprawdzanie sesji (start aplikacji),
 * authenticated — mamy ważny access token w pamięci,
 * anonymous     — brak sesji.
 */
export type AuthStatus = 'unknown' | 'authenticated' | 'anonymous';

export type AuthState = {
  status: AuthStatus;
  token: string | null;
};

type Listener = () => void;

const listeners = new Set<Listener>();

// Access token żyje wyłącznie w pamięci. Refresh token jest w ciasteczku HttpOnly,
// niedostępnym dla JavaScriptu.
let state: AuthState = { status: 'unknown', token: null };

let refreshTimer: ReturnType<typeof setTimeout> | undefined;

function setState(next: AuthState) {
  state = next;
  client.setConfig({ auth: () => next.token ?? undefined });
  scheduleProactiveRefresh(next.token);
  listeners.forEach((listener) => listener());
}

export function getAuthState(): AuthState {
  return state;
}

export function getToken(): string | null {
  return state.token;
}

export function subscribe(listener: Listener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

/** Wywoływane po udanym logowaniu lub rejestracji. */
export function setAccessToken(token: string) {
  setState({ status: 'authenticated', token });
}

function clearSession() {
  setState({ status: 'anonymous', token: null });
}

// --- Synchronizacja między kartami -----------------------------------------

const channel =
  typeof BroadcastChannel !== 'undefined' ? new BroadcastChannel('projectcowork-auth') : null;

channel?.addEventListener('message', (event) => {
  if (event.data === 'logout') {
    clearSession();
  }
});

// --- Odświeżanie tokena -----------------------------------------------------

let refreshPromise: Promise<string | null> | null = null;

async function requestNewAccessToken(): Promise<string | null> {
  try {
    const { data, response } = await postApiAuthTokenRefresh();

    if (!response?.ok || !data?.accessToken) {
      return null;
    }

    return data.accessToken;
  } catch {
    return null;
  }
}

export function refreshAccessToken(): Promise<string | null> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    try {
      const token =
        typeof navigator !== 'undefined' && navigator.locks
          ? await navigator.locks.request('projectcowork-auth-refresh', requestNewAccessToken)
          : await requestNewAccessToken();

      if (token) {
        setAccessToken(token);
      } else {
        clearSession();
      }

      return token;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export function initSession(): Promise<string | null> {
  return refreshAccessToken();
}

export async function logout(): Promise<void> {
  try {
    await postApiAuthLogout();
  } catch {
    // Nawet jeśli serwer nie odpowie, czyścimy lokalną sesję.
  }
  clearSession();
  channel?.postMessage('logout');
}

const REFRESH_MARGIN_MS = 60_000;

function getTokenExpiry(token: string): number | null {
  try {
    const payload = token.split('.')[1];
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    const { exp } = JSON.parse(json) as { exp?: number };
    return typeof exp === 'number' ? exp * 1000 : null;
  } catch {
    return null;
  }
}

function scheduleProactiveRefresh(token: string | null) {
  if (refreshTimer) {
    clearTimeout(refreshTimer);
    refreshTimer = undefined;
  }

  if (!token) {
    return;
  }

  const expiresAt = getTokenExpiry(token);

  if (!expiresAt) {
    return;
  }

  const lifetime = expiresAt - Date.now();
  // Przy bardzo krótkich tokenach odświeżamy w połowie czasu życia.
  const delay = lifetime > REFRESH_MARGIN_MS * 2 ? lifetime - REFRESH_MARGIN_MS : lifetime / 2;

  refreshTimer = setTimeout(() => {
    void refreshAccessToken();
  }, Math.max(delay, 0));
}

// --- Automatyczne odświeżanie przy 401 --------------------------------------

const AUTH_PATHS = ['/api/auth/login', '/api/auth/register', '/api/auth/token/refresh', '/api/auth/logout'];

function isAuthRequest(url: string): boolean {
  return AUTH_PATHS.some((path) => url.includes(path));
}

// Kopia żądania robiona przed wysłaniem: po fetch body jest już odczytane
// i request.clone() rzuciłby wyjątek dla POST/PUT.
const pendingRetries = new WeakMap<Request, Request>();

client.interceptors.request.use((request) => {
  if (!isAuthRequest(request.url)) {
    pendingRetries.set(request, request.clone());
  }
  return request;
});

// Przy 401 odświeżamy token i raz powtarzamy żądanie. Powtórka idzie bezpośrednio
// przez fetch (poza interceptorami), więc nie ma ryzyka pętli.
client.interceptors.response.use(async (response, request) => {
  const retryRequest = pendingRetries.get(request);
  pendingRetries.delete(request);

  if (response.status !== 401 || !retryRequest) {
    return response;
  }

  const newToken = await refreshAccessToken();

  if (!newToken) {
    return response;
  }

  retryRequest.headers.set('Authorization', `Bearer ${newToken}`);

  return fetch(retryRequest);
});

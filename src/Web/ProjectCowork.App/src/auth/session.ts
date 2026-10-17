import { client } from '../client/client.gen';

const STORAGE_KEY = 'projectcowork.accessToken';

type Listener = () => void;

const listeners = new Set<Listener>();

let currentToken: string | null = readToken();

function readToken(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
}

function applyTokenToClient(token: string | null) {
  client.setConfig({ auth: () => token ?? undefined });
}

applyTokenToClient(currentToken);

export function getToken(): string | null {
  return currentToken;
}

export function setToken(token: string | null) {
  currentToken = token;
  try {
    if (token) {
      localStorage.setItem(STORAGE_KEY, token);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // brak dostępu do localStorage — stan trzymamy tylko w pamięci
  }
  applyTokenToClient(token);
  listeners.forEach((listener) => listener());
}

export function subscribe(listener: Listener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

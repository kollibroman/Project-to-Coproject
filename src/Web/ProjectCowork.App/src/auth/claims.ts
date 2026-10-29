// System.IdentityModel zapisuje ClaimTypes.NameIdentifier pod pełnym URI albo skrótem „nameid”,
// zależnie od sposobu tworzenia tokena, więc sprawdzamy wszystkie warianty.
const USER_ID_CLAIMS = [
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
    'nameid',
    'sub',
];

function decodePayload(token: string): Record<string, unknown> | null {
    try {
        const payload = token.split('.')[1];
        const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
        return JSON.parse(json) as Record<string, unknown>;
    } catch {
        return null;
    }
}

/** Id zalogowanego użytkownika odczytane z access tokena (bez weryfikacji podpisu). */
export function getUserIdFromToken(token: string | null): string | null {
    if (!token) {
        return null;
    }

    const payload = decodePayload(token);

    if (!payload) {
        return null;
    }

    for (const claim of USER_ID_CLAIMS) {
        const value = payload[claim];
        if (typeof value === 'string' && value.length > 0) {
            return value;
        }
    }

    return null;
}

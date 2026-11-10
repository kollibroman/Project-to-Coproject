import { ApiError } from '../../Utils/apiError.ts';

/**
 * Komunikat błędu dla użytkownika. Przy 403 podaje politykę, której brakuje,
 * bo uprawnienia sprawdzamy dopiero przy próbie wykonania akcji.
 */
export const getErrorMessage = (error: unknown, fallback: string, requiredPolicy?: string): string => {
    if (error instanceof ApiError && error.isForbidden && requiredPolicy) {
        return `Brak uprawnień do tej operacji. Wymagane uprawnienie: ${requiredPolicy}.`;
    }

    if (error instanceof Error) {
        return error.message;
    }

    if (typeof error === 'string' && error.trim().length > 0) {
        return error;
    }

    return fallback;
};

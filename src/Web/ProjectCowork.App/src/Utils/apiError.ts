/** Błąd HTTP z kodem statusu. Klient hey-api przy throwOnError rzuca samo ciało odpowiedzi, bez statusu. */
export class ApiError extends Error {
    readonly status: number;
    readonly body: unknown;

    constructor(status: number, body: unknown) {
        super(getMessage(status, body));
        this.name = 'ApiError';
        this.status = status;
        this.body = body;
    }

    get isForbidden() {
        return this.status === 403;
    }
}

function getMessage(status: number, body: unknown): string {
    if (status === 401) {
        return 'Sesja wygasła. Zaloguj się ponownie.';
    }

    if (status === 403) {
        return 'Brak uprawnień do wykonania tej operacji.';
    }

    // ProblemDetails z GlobalExceptionHandler trzyma komunikat w `title`.
    if (body && typeof body === 'object' && 'title' in body) {
        const { title } = body as { title: unknown };
        if (typeof title === 'string' && title.length > 0) {
            return title;
        }
    }

    if (typeof body === 'string' && body.trim().length > 0) {
        return body;
    }

    return status === 0 ? 'Brak połączenia z serwerem.' : `Błąd serwera (${status}).`;
}

type SdkResult<T> = {
    data?: T;
    error?: unknown;
    response?: Response;
};

/** Zamienia wynik funkcji SDK (bez throwOnError) na dane albo ApiError ze statusem. */
export async function unwrap<T>(request: Promise<SdkResult<T>>): Promise<T> {
    const { data, error, response } = await request;

    if (response?.ok) {
        return data as T;
    }

    throw new ApiError(response?.status ?? 0, error);
}

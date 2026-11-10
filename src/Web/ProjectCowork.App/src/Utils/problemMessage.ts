export const getProblemMessage = (error: unknown, fallback: string): string => {
    if (error instanceof Error && error.message) {
        return error.message;
    }

    if (error && typeof error === 'object' && 'title' in error) {
        const { title } = error as { title: unknown };
        if (typeof title === 'string' && title.length > 0) {
            return title;
        }
    }

    if (typeof error === 'string' && error.trim().length > 0) {
        return error;
    }

    return fallback;
};

// Backend (.NET) opisuje long w OpenAPI jako "number | string", stąd Number().
export const formatFileSize = (value?: number | string | null): string => {
    if (value === null || value === undefined) return "";
    const bytes = Number(value);
    if (Number.isNaN(bytes)) return "";
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

export const formatCommitDate = (isoDate: string): string =>
    new Date(isoDate).toLocaleString("pl-PL", {
        day: "numeric",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });

const languageByExtension: Record<string, string> = {
    cs: "csharp",
    csproj: "xml",
    sln: "text",
    ts: "typescript",
    tsx: "tsx",
    js: "javascript",
    jsx: "jsx",
    json: "json",
    md: "markdown",
    html: "html",
    css: "css",
    scss: "scss",
    xml: "xml",
    yml: "yaml",
    yaml: "yaml",
    sql: "sql",
    py: "python",
    sh: "bash",
    tex: "latex",
};

export const getLanguageFromPath = (path: string): string => {
    const extension = path.split(".").pop()?.toLowerCase() ?? "";
    return languageByExtension[extension] ?? "text";
};

export const getFileName = (path: string): string => path.split("/").pop() ?? path;

export const getParentPath = (path: string): string => path.split("/").slice(0, -1).join("/");

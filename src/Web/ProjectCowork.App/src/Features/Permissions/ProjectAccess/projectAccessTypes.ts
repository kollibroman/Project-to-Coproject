// Kształty odpowiadają DTO z planu (docs/uprawnienia-per-projekt.md, sekcja 4).
// Po podłączeniu API zastąp je typami z wygenerowanego klienta.

export type ProjectRole = {
    id: string;
    name: string;
    isDefault: boolean;
    permissionIds: number[];
    memberCount: number;
};

export type ProjectMember = {
    userId: string;
    userName: string;
    email: string;
    /** Null dla właściciela, który nie ma roli i zawsze ma pełne uprawnienia. */
    roleId: string | null;
    roleName: string;
    isOwner: boolean;
};

export type ProjectRoleInput = {
    name: string;
    isDefault: boolean;
    permissionIds: number[];
};

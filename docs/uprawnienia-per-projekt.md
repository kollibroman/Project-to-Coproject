# Uprawnienia per projekt: plan implementacji

Każdy projekt ma własne role. Właściciel projektu dowolnie ustawia, jakie uprawnienia (zasób + akcja) ma każda rola, i przypisuje role członkom. Właściciel ma zawsze pełne uprawnienia.

---

## 1. Model danych

```mermaid
erDiagram
    Users ||--o{ Projects : "OwnerId (właściciel)"
    Projects ||--o{ ProjectRoles : "ma role"
    Projects ||--o{ ProjectMembers : "ma członków"
    Users ||--o{ ProjectMembers : "należy do"
    ProjectRoles ||--o{ ProjectMembers : "przypisana rola"
    ProjectRoles ||--o{ ProjectRolePermissions : "ma uprawnienia"
    FeatureActions ||--o{ ProjectRolePermissions : "nadawane jako"
    Features ||--o{ FeatureActions : "zasób (ResourceType)"
    Actions ||--o{ FeatureActions : "akcja (ActionType)"

    Projects {
        Guid Id PK
        string Name
        Guid OwnerId FK
    }
    ProjectRoles {
        Guid Id PK
        Guid ProjectId FK
        string Name "unikalne w projekcie"
        bool IsDefault "rola dla zaproszonych"
    }
    ProjectMembers {
        Guid ProjectId PK, FK
        Guid UserId PK, FK
        Guid ProjectRoleId FK
    }
    ProjectRolePermissions {
        Guid ProjectRoleId PK, FK
        int FeatureActionId PK, FK
    }
    FeatureActions {
        int Id PK
        int FeatureId FK
        int ActionId FK
    }
    Features {
        int Id PK
        string Name "np. Projects"
    }
    Actions {
        int Id PK
        string Name "np. View"
    }
```

### Co znika, a co zostaje

```mermaid
flowchart LR
    subgraph USUWASZ
        direction TB
        X1[RolePermissionEntity / tabela RolePermissions]
        X2[RoleEntity.Permissions]
        X3[Collaborators / AssignedProjects<br/>tabela ProjectCollaborators]
        X4[HasPermissionAttribute, PermissionRequirement,<br/>PermissionAuthorizationHandler, polityki w Program.cs]
    end
    subgraph ZOSTAJE
        direction TB
        K1[Features / Actions / FeatureActions<br/>katalog uprawnień]
        K2[Role Identity: User, Business, Admin<br/>tylko rejestracja]
    end
    subgraph NOWE
        direction TB
        N1[ProjectRoleEntity]
        N2[ProjectMemberEntity]
        N3[ProjectRolePermissionEntity]
    end
```

### Konfiguracja EF

| Relacja / klucz | Ustawienie |
|---|---|
| `ProjectMembers` PK | `(ProjectId, UserId)` |
| `ProjectRolePermissions` PK | `(ProjectRoleId, FeatureActionId)` |
| `ProjectRoles` | unikalny indeks `(ProjectId, Name)` |
| `FeatureActions` | unikalny indeks `(FeatureId, ActionId)` |
| Project → ProjectRoles | Cascade |
| Project → ProjectMembers | Cascade |
| ProjectRole → ProjectRolePermissions | Cascade |
| ProjectRole → ProjectMembers | **NoAction** (rola z członkami nie może być usunięta) |
| FeatureAction → ProjectRolePermissions | Cascade |

### Reguły domenowe

```mermaid
flowchart TD
    R1["Właściciel = Project.OwnerId<br/>nie ma go w ProjectMembers"] --> R1a[zawsze wszystkie uprawnienia]
    R1 --> R1b[nie da się zmienić mu roli ani go usunąć]
    R2["ProjectMember.ProjectRoleId"] --> R2a[rola musi należeć do TEGO SAMEGO projektu]
    R3["IsDefault"] --> R3a[dokładnie jedna rola domyślna w projekcie]
    R3 --> R3b[roli domyślnej nie można usunąć]
    R4["Usuwanie roli"] --> R4a{ma członków?}
    R4a -->|tak| R4b[409 Conflict]
    R4a -->|nie| R4c[usuń]
```

---

## 2. Gdzie sprawdzać uprawnienia

Sprawdzanie jest w **handlerach**, nie w politykach ASP.NET. Wiele endpointów nie ma `projectId` w trasie, tylko `sprintId`, `taskId`, `repositoryId` albo pole w body. Handler i tak ładuje encję, więc zna jej `ProjectId`.

```mermaid
flowchart LR
    C[Controller] --> M[IMediator]
    M --> H[Handler]
    H -->|krok 1: załaduj encję| DB[(DbContext)]
    H -->|krok 2: EnsureProjectPermissionAsync| PS[IPermissionService]
    PS -->|bieżący użytkownik| AUP[IAuthorizedUserProvider]
    PS -->|owner lub member, rola, uprawnienia| DB
    PS -.->|brak uprawnienia| FE[ForbiddenException 403]
    FE -.-> GEH[GlobalExceptionHandler<br/>ProblemDetails]
```

### Przykład: edycja sprintu

```mermaid
sequenceDiagram
    participant UI
    participant API as SprintsController
    participant H as EditSprintCommandHandler
    participant P as PermissionService
    participant DB

    UI->>API: PUT /api/sprints/:sprintId
    API->>H: EditSprintCommand
    H->>DB: Sprints.FirstOrDefault(id)
    alt brak sprintu
        H-->>UI: 404 Not Found
    end
    H->>P: Ensure(sprint.ProjectId, Management, Update)
    P->>DB: owner == user LUB rola członka ma Management.Update?
    alt brak uprawnienia
        P-->>UI: 403 Forbidden
    end
    H->>DB: SaveChanges
    H-->>UI: 204 No Content
```

### Logika `EnsureProjectPermissionAsync`

```mermaid
flowchart TD
    S([EnsureProjectPermissionAsync<br/>projectId, resource, action]) --> U{zalogowany użytkownik?}
    U -->|nie| E401[UnauthenticatedException 401]
    U -->|tak| O{user == Project.OwnerId?}
    O -->|tak| OK([dostęp])
    O -->|nie| MB{jest w ProjectMembers?}
    MB -->|nie| E403[ForbiddenException 403]
    MB -->|tak| RP{rola ma FeatureAction<br/>resource + action?}
    RP -->|tak| OK
    RP -->|nie| E403
```

Całość to jedno zapytanie:

```csharp
_context.Projects
    .Where(p => p.Id == projectId)
    .Select(p => p.OwnerId == userId
        || p.Members.Any(m => m.UserId == userId
            && m.ProjectRole.Permissions.Any(rp =>
                   rp.FeatureAction.Feature.Name == resourceName
                && rp.FeatureAction.Action.Name == actionName)))
    .FirstOrDefaultAsync(ct);
```

Nowe wyjątki w `Domain.Exceptions/Auth`, dziedziczące po `ExceptionBase`:
- `ForbiddenException` (403)
- `UnauthenticatedException` (401)

Cache uprawnień nie jest potrzebny. To jedno zapytanie po indeksach, więc nie trzeba go unieważniać.

---

## 3. Mapa uprawnień dla istniejących handlerów

```mermaid
flowchart LR
    subgraph Projects
        P1[GetProject] --> PV[Projects.View]
        P2[EditProject] --> PU[Projects.Update]
        P3[DeleteProject] --> PD[Projects.Delete]
        P4[InviteUserToProject] --> PC[Permissions.Create]
        P5[GetProjects] --> PF[filtr: owner lub member]
    end
    subgraph Management
        M1[GetProjectSprints, GetSprint,<br/>GetTasksFromProject, GetTask,<br/>GetTasksFromSprint] --> MV[Management.View]
        M2[AddSprint, AddTask] --> MC[Management.Create]
        M3[EditSprint, EditTask] --> MU[Management.Update]
        M4[DeleteSprint, DeleteTask] --> MD[Management.Delete]
    end
    subgraph Postings
        O1[AddProjectPosting] --> OC[Postings.Create]
        O2[UpdateProjectPosting] --> OU[Postings.Update]
        O3[GetPostingApplications] --> AV[Applications.View]
        O4[GetActivePostings, GetProjectPosting,<br/>GetProjectPostings, ApplyForPosting] --> PUB[publiczne]
    end
    subgraph Repositories
        G1[GetRepositories, GetRepository] --> RV[Repositories.View]
        G2[CreateRepository] --> RC[Repositories.Create]
        G3[DeleteRepository] --> RD[Repositories.Delete]
    end
```

| Handler | Skąd `projectId` | Wymagane |
|---|---|---|
| GetProject | request | Projects.View |
| EditProject | **route** (patrz uwaga niżej) | Projects.Update |
| DeleteProject | request | Projects.Delete |
| InviteUserToProject | request | Permissions.Create |
| GetProjects | – | filtr: owner lub member |
| GetProjectSprints / GetTasksFromProject | request | Management.View |
| GetSprint / GetTask / GetTasksFromSprint | encja | Management.View |
| AddSprint / AddTask | body | Management.Create |
| EditSprint / EditTask | encja | Management.Update |
| DeleteSprint / DeleteTask | encja | Management.Delete |
| AddProjectPosting | body | Postings.Create |
| UpdateProjectPosting | encja | Postings.Update |
| GetPostingApplications | encja posting | Applications.View |
| GetActivePostings / GetProjectPosting(s) / ApplyForPosting | – | publiczne |
| GetRepositories | request | Repositories.View |
| GetRepository | encja | Repositories.View |
| CreateRepository | body | Repositories.Create |
| DeleteRepository | encja | Repositories.Delete |

> **Błąd do poprawienia:** `POST /api/projects/{projectId}/edit` bierze `ProjectId` z body, a nie z trasy. Można wtedy podać w body inny projekt niż w adresie. Bierz id z trasy.

> **Handlery strumieniowe** (`IStreamRequestHandler`) pisz jako `async IAsyncEnumerable<T>` z `[EnumeratorCancellation]`: najpierw `await Ensure…`, potem `await foreach … yield return`.

---

## 4. API ról i członków (feature `Permissions`)

```mermaid
flowchart LR
    subgraph api ["/api/projects/{projectId}"]
        direction TB
        subgraph roles
            R1["GET /roles<br/>Permissions.View"]
            R2["POST /roles<br/>name, permissionIds<br/>Permissions.Create"]
            R3["PUT /roles/{roleId}<br/>name, permissionIds<br/>Permissions.Update"]
            R4["DELETE /roles/{roleId}<br/>Permissions.Delete"]
        end
        subgraph members
            M1["GET /members<br/>Permissions.View"]
            M2["PUT /members/{userId}<br/>roleId<br/>Permissions.Update"]
            M3["DELETE /members/{userId}<br/>Permissions.Delete"]
        end
    end
    CAT["GET /api/permissions<br/>katalog, tylko zalogowany"]
```

`PUT roles` **zastępuje cały zestaw** uprawnień roli. Frontend wysyła listę zaznaczonych pól z macierzy i na tym polega dynamiczne ustawianie uprawnień.

### DTO

```mermaid
classDiagram
    class ProjectRoleDto {
        Guid Id
        string Name
        bool IsDefault
        int[] PermissionIds
        int MemberCount
    }
    class ProjectMemberDto {
        Guid UserId
        string UserName
        string Email
        Guid? RoleId
        string RoleName
        bool IsOwner
    }
    class PermissionDto {
        int Id
        ResourceType Resource
        ActionType Action
    }
    ProjectRoleDto ..> PermissionDto : PermissionIds
    ProjectMemberDto ..> ProjectRoleDto : RoleId
```

Właściciel jest pierwszym wierszem listy członków, z `IsOwner = true` i `RoleId = null`.

**Do usunięcia z obecnego feature'a:** Create/Update/DeletePermission, Assign/RevokePermissionFromRole, GetRolePermissions, GetUserPermissions, GetRoles, `RoleDto`, `UserPermissionsDto`.

---

## 5. Cykl życia

### Tworzenie projektu

```mermaid
flowchart TD
    A[AddProjectCommand] --> B[utwórz projekt, OwnerId = bieżący użytkownik]
    B --> C[utwórz domyślne role]
    C --> C1["Administrator<br/>wszystkie uprawnienia"]
    C --> C2["Członek (IsDefault)<br/>wszystkie View + Management C/U/D<br/>+ Repositories.Create"]
    C --> C3["Obserwator<br/>wszystkie View"]
```

### Zaproszenie użytkownika

```mermaid
flowchart TD
    I["InviteUserToProject<br/>email, roleId opcjonalnie"] --> P{Permissions.Create?}
    P -->|nie| E403[403]
    P -->|tak| U{użytkownik istnieje?}
    U -->|nie| E404[404]
    U -->|tak| O{to właściciel albo już członek?}
    O -->|tak| E409[409]
    O -->|nie| R{roleId podane?}
    R -->|tak| R1{rola z tego projektu?}
    R1 -->|nie| E400[400]
    R1 -->|tak| ADD
    R -->|nie| DEF[rola IsDefault] --> ADD["ProjectMembers.Add"]
```

### Katalog uprawnień (seeder przy starcie)

```mermaid
flowchart LR
    M[MigrateAsync] --> S[PermissionCatalogSeeder]
    S --> L{"dla każdej pary<br/>ResourceType x ActionType"}
    L --> F[brak Feature? dodaj]
    L --> A[brak Action? dodaj]
    L --> FA[brak FeatureAction? dodaj]
```

Seeder musi być idempotentny, żeby kolejne uruchomienia niczego nie dublowały. CRUD katalogu przestaje być potrzebny.

### `ProjectAggregate`

```mermaid
classDiagram
    class ProjectAggregate {
        +AddMember(UserEntity user, ProjectRoleEntity role)
        +RemoveMember(Guid userId)
        +ChangeMemberRole(Guid userId, ProjectRoleEntity role)
        -EnsureRoleBelongsToProject(role)
        -EnsureNotOwner(userId)
    }
    note for ProjectAggregate "zastępuje AddCollaborator / RemoveCollaborator"
```

W `GetDiscoverProjects` warunek `Collaborators.All(...)` zamieniasz na `Members.All(m => m.UserId != userId)`.

---

## 6. Migracja

```bash
dotnet ef migrations add ProjectScopedPermissions \
  -p src/Infrastructure/ProjectCowork.Infrastructure.Persistence \
  -s src/Web/ProjectCowork.Api
```

```mermaid
flowchart TD
    B[kopia bazy] --> G[dotnet ef migrations add]
    G --> Q{zachować współpracowników?}
    Q -->|tak| SQL["w Up() przed DropTable ProjectCollaborators:<br/>1: utwórz domyślne role dla istniejących projektów<br/>2: przepisz ProjectCollaborators do ProjectMembers z rolą IsDefault"]
    Q -->|nie| SEED[seeder przy starcie dodaje role<br/>projektom, które ich nie mają]
    SQL --> RUN[uruchom API, MigrateAsync]
    SEED --> RUN
```

> **Uwaga:** migracja usuwa tabelę `ProjectCollaborators` razem z danymi. `Program.cs` wywołuje `MigrateAsync()`, więc bez wygenerowanej migracji EF zgłosi niezgodność modelu (pending model changes) i aplikacja nie wstanie.

---

## 7. Frontend

```mermaid
flowchart TD
    PP[ProjectPage] --> T1[Repozytoria]
    PP --> T2[Sprinty i zadania]
    PP --> T3[Ogłoszenia]
    PP --> T4["Członkowie i role<br/>zamiast zakładki Użytkownicy"]
    T4 --> R[Lista ról: dodaj, zmień nazwę, usuń]
    R --> RM["PermissionMatrix (istniejąca)<br/>granted = role.permissionIds<br/>onToggle: PUT roles z nowym zestawem"]
    T4 --> MB["Tabela członków<br/>Select roli: PUT members<br/>Usuń: DELETE members"]
```

```mermaid
flowchart LR
    subgraph ZOSTAJE
        Z1[PermissionMatrix]
        Z2[permissionEnums.ts]
        Z3["Utils/apiError.ts<br/>unwrap + ApiError ze statusem"]
        Z4["getErrorMessage<br/>przy 403 podaje wymaganą politykę"]
    end
    subgraph hooks [NOWE HOOKI]
        H1["useProjectRoles(projectId)"]
        H2[useCreateProjectRole]
        H3[useUpdateProjectRole]
        H4[useDeleteProjectRole]
        H5["useProjectMembers(projectId)"]
        H6[useChangeMemberRole]
        H7[useRemoveMember]
    end
    subgraph USUWASZ
        U1["strona /permissions + trasa"]
        U2["pozycja Uprawnienia w AppLayout"]
    end
```

- Każdy hook unieważnia klucz zapytania zawierający `projectId`.
- Gdy API zwróci 403 przy innych akcjach (sprinty, zadania itd.), wystarczy, że ich hooki też przejdą na `unwrap`. Wtedy komunikat „Brak uprawnień” pokaże się sam.
- Po przebudowaniu backendu i wygenerowaniu klienta (`npm run dev` / `build`) możesz w hookach użyć wygenerowanych funkcji zamiast `client.get`.

---

## 8. Kolejność pracy

```mermaid
flowchart TD
    S1["1: encje i konfiguracje EF<br/>usunięcie starych relacji"] --> S2["2: migracja<br/>opcjonalnie SQL przenoszący dane"]
    S2 --> S3["3: seeder katalogu uprawnień"]
    S3 --> S4["4: ProjectAggregate, AddProject z rolami,<br/>InviteUserToProject"]
    S4 --> S5["5: IPermissionService, wyjątki 401 i 403,<br/>usunięcie polityk i handlera"]
    S5 --> S6["6: Ensure w handlerach według mapy z sekcji 3<br/>poprawka EditProject"]
    S6 --> S7["7: feature Permissions: role, członkowie, endpointy"]
    S7 --> S8["8: frontend"]
    S8 --> T{{"test: członek bez Management.Update dostaje 403,<br/>właściciel przechodzi zawsze"}}
```

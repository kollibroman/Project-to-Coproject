using System.Net;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Projects;
using ProjectCowork.Domain.Models.Permissions;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Domain.Tests.TestData;

namespace ProjectCowork.Infrastructure.Domain.Tests.Aggregates;

public class ProjectAggregateTests
{
    private const string Administrator = "Administrator";
    private const string Member = "Członek";
    private const string Observer = "Obserwator";

    private readonly List<FeatureActionEntity> _catalog = TestEntities.PermissionCatalog();

    [Test]
    public async Task Constructor_NullProject_Throws()
    {
        await Assert.That(() => new ProjectAggregate(null!)).Throws<ArgumentNullException>();
    }

    // --- Create ---------------------------------------------------------------

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_EmptyName_Throws(string name)
    {
        await Assert.That(() => ProjectAggregate.Create(name, "Opis", Guid.NewGuid(), _catalog))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Create_SetsBasicFields()
    {
        var ownerId = Guid.NewGuid();

        var project = ProjectAggregate.Create("Nazwa", "Opis", ownerId, _catalog);

        await Assert.That(project.Name).IsEqualTo("Nazwa");
        await Assert.That(project.Description).IsEqualTo("Opis");
        await Assert.That(project.OwnerId).IsEqualTo(ownerId);
        await Assert.That(project.LastTaskFriendlyId).IsEqualTo(0L);
        await Assert.That(project.ProjectMembers.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Create_AddsThreeDefaultRoles_OnlyMemberIsDefault()
    {
        var project = ProjectAggregate.Create("Nazwa", "Opis", Guid.NewGuid(), _catalog);

        var roleNames = project.ProjectRoles.Select(x => x.Name).Order().ToList();
        var defaultRoles = project.ProjectRoles.Where(x => x.IsDefault).Select(x => x.Name).ToList();

        await Assert.That(roleNames).IsEquivalentTo(new[] { Administrator, Member, Observer });
        await Assert.That(defaultRoles).IsEquivalentTo(new[] { Member });
    }

    [Test]
    public async Task Create_Administrator_HasWholeCatalog()
    {
        var project = ProjectAggregate.Create("Nazwa", "Opis", Guid.NewGuid(), _catalog);

        var permissions = project.Role(Administrator).FeatureActions.Select(x => x.Key());

        await Assert.That(permissions).IsEquivalentTo(_catalog.Select(x => x.Key()));
    }

    [Test]
    public async Task Create_Observer_HasOnlyViewPermissions()
    {
        var project = ProjectAggregate.Create("Nazwa", "Opis", Guid.NewGuid(), _catalog);

        var permissions = project.Role(Observer).FeatureActions.Select(x => x.Key());
        var expected = Enum.GetValues<ResourceType>().Select(r => $"{r}.{ActionType.View}");

        await Assert.That(permissions).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Create_Member_HasViewEverywhere_FullManagement_AndRepositoryCreate()
    {
        var project = ProjectAggregate.Create("Nazwa", "Opis", Guid.NewGuid(), _catalog);

        var permissions = project.Role(Member).FeatureActions.Select(x => x.Key());
        var expected = Enum.GetValues<ResourceType>().Select(r => $"{r}.{ActionType.View}")
            .Concat(Enum.GetValues<ActionType>().Select(a => $"{ResourceType.Management}.{a}"))
            .Append($"{ResourceType.Repositories}.{ActionType.Create}")
            .Distinct();

        await Assert.That(permissions).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Create_IgnoresPermissionsWithUnknownNames()
    {
        var unknown = new FeatureActionEntity
        {
            Id = 999,
            Feature = new FeatureEntity { Name = "NieistniejacyZasob" },
            Action = new ActionEntity { Id = 999, Name = nameof(ActionType.View) }
        };

        var project = ProjectAggregate.Create("Nazwa", "Opis", Guid.NewGuid(), [.._catalog, unknown]);

        var rolesWithUnknown = project.ProjectRoles.Where(r => r.FeatureActions.Contains(unknown)).ToList();

        await Assert.That(rolesWithUnknown.Count).IsEqualTo(0);
    }

    // --- SetProjectName / SetDescription -------------------------------------

    [Test]
    public async Task SetProjectNameAndDescription_ChangesValues()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);

        aggregate.SetProjectName("Nowa nazwa");
        aggregate.SetDescription("Nowy opis");

        await Assert.That(project.Name).IsEqualTo("Nowa nazwa");
        await Assert.That(project.Description).IsEqualTo("Nowy opis");
    }

    // --- AddRole --------------------------------------------------------------

    [Test]
    public async Task AddRole_TrimsName_AndAssignsPermissions()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var permission = _catalog.Permission(ResourceType.Postings, ActionType.Create);

        var role = aggregate.AddRole("  Tester  ", false, [permission]);

        await Assert.That(role.Name).IsEqualTo("Tester");
        await Assert.That(role.IsDefault).IsFalse();
        await Assert.That(role.ProjectId).IsEqualTo(project.Id);
        await Assert.That(role.FeatureActions.Select(x => x.Key())).IsEquivalentTo(new[] { permission.Key() });
        await Assert.That(project.ProjectRoles.Count).IsEqualTo(4);
    }

    [Test]
    public async Task AddRole_DuplicatedPermissions_AreStoredOnce()
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));
        var permission = _catalog.Permission(ResourceType.Projects, ActionType.Update);

        var role = aggregate.AddRole("Tester", false, [permission, permission]);

        await Assert.That(role.FeatureActions.Count).IsEqualTo(1);
    }

    [Test]
    [Arguments("Członek")]
    [Arguments("członek")]
    [Arguments("  ADMINISTRATOR  ")]
    public async Task AddRole_DuplicateNameIgnoringCaseAndWhitespace_ThrowsConflict(string name)
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);

        var exception = await Assert.That(() => aggregate.AddRole(name, false, []))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(project.ProjectRoles.Count).IsEqualTo(3);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task AddRole_EmptyName_ThrowsBadRequest(string name)
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));

        var exception = await Assert.That(() => aggregate.AddRole(name, false, []))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AddRole_AsDefault_BecomesTheOnlyDefaultRole()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);

        var role = aggregate.AddRole("Nowa domyślna", true, []);

        var defaultRoles = project.ProjectRoles.Where(x => x.IsDefault).ToList();

        await Assert.That(defaultRoles.Count).IsEqualTo(1);
        await Assert.That(defaultRoles[0]).IsSameReferenceAs(role);
    }

    // --- UpdateRole -----------------------------------------------------------

    [Test]
    public async Task UpdateRole_UnknownRole_ThrowsNotFound()
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));

        var exception = await Assert.That(() => aggregate.UpdateRole(Guid.NewGuid(), "X", false, []))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateRole_UnsettingDefaultOnDefaultRole_ThrowsBadRequest()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var member = project.Role(Member);

        var exception = await Assert.That(() => aggregate.UpdateRole(member.Id, Member, false, []))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(member.IsDefault).IsTrue();
    }

    [Test]
    public async Task UpdateRole_ReplacesPermissions()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var observer = project.Role(Observer);
        var target = new[]
        {
            _catalog.Permission(ResourceType.Projects, ActionType.View),
            _catalog.Permission(ResourceType.Postings, ActionType.Delete)
        };

        aggregate.UpdateRole(observer.Id, Observer, false, target);

        await Assert.That(observer.FeatureActions.Select(x => x.Key()))
            .IsEquivalentTo(target.Select(x => x.Key()));
    }

    [Test]
    public async Task UpdateRole_RenameWithinSameRole_IsAllowed()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var observer = project.Role(Observer);

        aggregate.UpdateRole(observer.Id, "  OBSERWATOR ", false, []);

        await Assert.That(observer.Name).IsEqualTo("OBSERWATOR");
    }

    [Test]
    public async Task UpdateRole_RenameToNameOfAnotherRole_ThrowsConflict()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var observer = project.Role(Observer);

        var exception = await Assert.That(() => aggregate.UpdateRole(observer.Id, Administrator, false, []))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(observer.Name).IsEqualTo(Observer);
    }

    [Test]
    public async Task UpdateRole_SetAsDefault_MovesDefaultFlag()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var observer = project.Role(Observer);

        aggregate.UpdateRole(observer.Id, Observer, true, []);

        await Assert.That(observer.IsDefault).IsTrue();
        await Assert.That(project.Role(Member).IsDefault).IsFalse();
        await Assert.That(project.ProjectRoles.Count(x => x.IsDefault)).IsEqualTo(1);
    }

    // --- RemoveRole -----------------------------------------------------------

    [Test]
    public async Task RemoveRole_RemovesNonDefaultRoleWithoutMembers()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);

        aggregate.RemoveRole(project.Role(Observer).Id);

        await Assert.That(project.ProjectRoles.Any(x => x.Name == Observer)).IsFalse();
    }

    [Test]
    public async Task RemoveRole_DefaultRole_ThrowsConflict()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);

        var exception = await Assert.That(() => aggregate.RemoveRole(project.Role(Member).Id))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(project.ProjectRoles.Count).IsEqualTo(3);
    }

    [Test]
    public async Task RemoveRole_RoleWithMembers_ThrowsConflict()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var observer = project.Role(Observer);
        aggregate.AddMember(TestEntities.User(), observer.Id);

        var exception = await Assert.That(() => aggregate.RemoveRole(observer.Id))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(project.ProjectRoles.Contains(observer)).IsTrue();
    }

    [Test]
    public async Task RemoveRole_UnknownRole_ThrowsNotFound()
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));

        var exception = await Assert.That(() => aggregate.RemoveRole(Guid.NewGuid()))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    // --- AddMember ------------------------------------------------------------

    [Test]
    public async Task AddMember_WithoutRole_UsesDefaultRole()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var user = TestEntities.User();

        var member = aggregate.AddMember(user, null);

        await Assert.That(member.UserId).IsEqualTo(user.Id);
        await Assert.That(member.ProjectId).IsEqualTo(project.Id);
        await Assert.That(member.ProjectRole).IsSameReferenceAs(project.Role(Member));
        await Assert.That(member.ProjectRoleId).IsEqualTo(project.Role(Member).Id);
        await Assert.That(project.ProjectMembers.Contains(member)).IsTrue();
    }

    [Test]
    public async Task AddMember_WithRole_UsesGivenRole()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var observer = project.Role(Observer);

        var member = aggregate.AddMember(TestEntities.User(), observer.Id);

        await Assert.That(member.ProjectRole).IsSameReferenceAs(observer);
    }

    [Test]
    public async Task AddMember_Null_Throws()
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));

        await Assert.That(() => aggregate.AddMember(null!, null)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddMember_Owner_ThrowsConflict()
    {
        var owner = TestEntities.User();
        var project = TestEntities.Project(owner.Id, _catalog);
        var aggregate = new ProjectAggregate(project);

        var exception = await Assert.That(() => aggregate.AddMember(owner, null))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(project.ProjectMembers.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddMember_AlreadyMember_ThrowsConflict()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var user = TestEntities.User();
        aggregate.AddMember(user, null);

        var exception = await Assert.That(() => aggregate.AddMember(user, null))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(project.ProjectMembers.Count).IsEqualTo(1);
    }

    [Test]
    public async Task AddMember_UnknownRole_ThrowsBadRequest()
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));

        var exception = await Assert.That(() => aggregate.AddMember(TestEntities.User(), Guid.NewGuid()))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AddMember_ProjectWithoutDefaultRole_ThrowsInternalServerError()
    {
        var project = TestEntities.Project(catalog: _catalog);
        foreach (var role in project.ProjectRoles)
        {
            role.IsDefault = false;
        }

        var aggregate = new ProjectAggregate(project);

        var exception = await Assert.That(() => aggregate.AddMember(TestEntities.User(), null))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
    }

    // --- ChangeMemberRole -----------------------------------------------------

    [Test]
    public async Task ChangeMemberRole_ChangesRole()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var user = TestEntities.User();
        var member = aggregate.AddMember(user, null);
        var admin = project.Role(Administrator);

        aggregate.ChangeMemberRole(user.Id, admin.Id);

        await Assert.That(member.ProjectRole).IsSameReferenceAs(admin);
        await Assert.That(member.ProjectRoleId).IsEqualTo(admin.Id);
    }

    [Test]
    public async Task ChangeMemberRole_Owner_ThrowsBadRequest()
    {
        var ownerId = Guid.NewGuid();
        var project = TestEntities.Project(ownerId, _catalog);
        var aggregate = new ProjectAggregate(project);

        var exception = await Assert.That(() => aggregate.ChangeMemberRole(ownerId, project.Role(Observer).Id))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ChangeMemberRole_NotAMember_ThrowsNotFound()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);

        var exception = await Assert.That(() => aggregate.ChangeMemberRole(Guid.NewGuid(), project.Role(Observer).Id))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ChangeMemberRole_UnknownRole_ThrowsBadRequest_AndKeepsOldRole()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var user = TestEntities.User();
        var member = aggregate.AddMember(user, null);

        var exception = await Assert.That(() => aggregate.ChangeMemberRole(user.Id, Guid.NewGuid()))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(member.ProjectRole).IsSameReferenceAs(project.Role(Member));
    }

    // --- RemoveMember ---------------------------------------------------------

    [Test]
    public async Task RemoveMember_RemovesMember()
    {
        var project = TestEntities.Project(catalog: _catalog);
        var aggregate = new ProjectAggregate(project);
        var user = TestEntities.User();
        aggregate.AddMember(user, null);

        aggregate.RemoveMember(user.Id);

        await Assert.That(project.ProjectMembers.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RemoveMember_Owner_ThrowsBadRequest()
    {
        var ownerId = Guid.NewGuid();
        var aggregate = new ProjectAggregate(TestEntities.Project(ownerId, _catalog));

        var exception = await Assert.That(() => aggregate.RemoveMember(ownerId))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RemoveMember_NotAMember_ThrowsNotFound()
    {
        var aggregate = new ProjectAggregate(TestEntities.Project(catalog: _catalog));

        var exception = await Assert.That(() => aggregate.RemoveMember(Guid.NewGuid()))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}

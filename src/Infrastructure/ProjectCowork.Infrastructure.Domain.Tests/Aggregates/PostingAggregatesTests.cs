using System.Net;
using ProjectCowork.Domain.Exceptions.ProjectPostings;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Domain.Tests.TestData;

namespace ProjectCowork.Infrastructure.Domain.Tests.Aggregates;

public class ProjectPostingAggregateTests
{
    [Test]
    public async Task Constructor_NullPosting_Throws()
    {
        await Assert.That(() => new ProjectPostingAggregate(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Create_SetsFields()
    {
        var projectId = Guid.NewGuid();

        var posting = ProjectPostingAggregate.Create(projectId, "Szukamy frontendowca", "Opis projektu");

        await Assert.That(posting.ProjectId).IsEqualTo(projectId);
        await Assert.That(posting.JobDescription).IsEqualTo("Szukamy frontendowca");
        await Assert.That(posting.ProjectDescription).IsEqualTo("Opis projektu");
    }

    [Test]
    public async Task Setters_ChangeValues()
    {
        var posting = ProjectPostingAggregate.Create(Guid.NewGuid(), "A", "B");
        var aggregate = new ProjectPostingAggregate(posting);

        aggregate.SetJobDescription("Nowa praca");
        aggregate.SetProjectDescription("Nowy projekt");
        aggregate.SetActivityStatus(true);

        await Assert.That(posting.JobDescription).IsEqualTo("Nowa praca");
        await Assert.That(posting.ProjectDescription).IsEqualTo("Nowy projekt");
        await Assert.That(posting.IsActive).IsTrue();

        aggregate.SetActivityStatus(false);

        await Assert.That(posting.IsActive).IsFalse();
    }
}

public class ProjectApplicationAggregateTests
{
    private static ProjectPostingEntity Posting(bool isActive)
    {
        var posting = ProjectPostingAggregate.Create(Guid.NewGuid(), "Praca", "Projekt").WithNewId();
        posting.IsActive = isActive;
        return posting;
    }

    [Test]
    public async Task Constructor_NullApplication_Throws()
    {
        await Assert.That(() => new ProjectApplicationAggregate(null!, [])).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Create_ActivePosting_CreatesApplication()
    {
        var posting = Posting(isActive: true);
        var userId = Guid.NewGuid();

        var application = ProjectApplicationAggregate.Create(posting, "Chętnie dołączę", userId);

        await Assert.That(application.Description).IsEqualTo("Chętnie dołączę");
        await Assert.That(application.UserId).IsEqualTo(userId);
        await Assert.That(application.ProjectPostingId).IsEqualTo(posting.Id);
    }

    [Test]
    public async Task Create_InactivePosting_ThrowsBadRequest()
    {
        var posting = Posting(isActive: false);

        var exception = await Assert.That(() => ProjectApplicationAggregate.Create(posting, "Opis", Guid.NewGuid()))
            .Throws<ProjectApplicationToNonExistentPostingException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Create_NullPosting_Throws()
    {
        await Assert.That(() => ProjectApplicationAggregate.Create(null!, "Opis", Guid.NewGuid()))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task SetApplicationAttachments_AssignsAttachments()
    {
        var application = ProjectApplicationAggregate.Create(Posting(isActive: true), "Opis", Guid.NewGuid());
        List<AttachmentEntity> attachments =
        [
            AttachmentAggregate.Create("cv.pdf", "cv.pdf", 1024),
            AttachmentAggregate.Create("list.pdf", "list.pdf", 2048)
        ];

        new ProjectApplicationAggregate(application, attachments).SetApplicationAttachments();

        await Assert.That(application.Attachments).IsSameReferenceAs(attachments);
    }
}

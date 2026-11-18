using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.DomainEvents.Attachments;
using ProjectCowork.Infrastructure.DomainEvents.Repositories;

namespace ProjectCowork.Infrastructure.Domain.Tests.Aggregates;

public class AttachmentAggregateTests
{
    [Test]
    public async Task Constructor_NullAttachment_Throws()
    {
        await Assert.That(() => new AttachmentAggregate(null!, Stream.Null, "application/pdf"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Create_SetsFields()
    {
        var attachment = AttachmentAggregate.Create("cv.pdf", "stored-cv.pdf", 4096);

        await Assert.That(attachment.FileName).IsEqualTo("cv.pdf");
        await Assert.That(attachment.PersistedFileName).IsEqualTo("stored-cv.pdf");
        await Assert.That(attachment.SizeInBytes).IsEqualTo(4096L);
        await Assert.That(attachment.ProjectApplicationId).IsNull();
    }

    [Test]
    public async Task SetProjectApplicationId_SetsId()
    {
        var attachment = AttachmentAggregate.Create("cv.pdf", "cv.pdf", 1);
        var applicationId = Guid.NewGuid();

        new AttachmentAggregate(attachment, Stream.Null, "application/pdf").SetProjectApplicationId(applicationId);

        await Assert.That(attachment.ProjectApplicationId).IsEqualTo(applicationId);
    }

    [Test]
    public async Task ScheduleBlobUpload_RaisesAttachmentAddedEvent()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var attachment = AttachmentAggregate.Create("cv.pdf", "cv.pdf", stream.Length);
        var aggregate = new AttachmentAggregate(attachment, stream, "application/pdf");

        aggregate.ScheduleBlobUpload();

        var domainEvent = aggregate.DomainEvents.Single();
        await Assert.That(domainEvent).IsTypeOf<AttachmentAddedEvent>();

        var added = (AttachmentAddedEvent)domainEvent;
        await Assert.That(added.FileName).IsEqualTo("cv.pdf");
        await Assert.That(added.ContentType).IsEqualTo("application/pdf");
        await Assert.That(added.ContentStream).IsSameReferenceAs(stream);
    }
}

public class RepositoryAggregateTests
{
    [Test]
    public async Task Constructor_NullRepository_Throws()
    {
        await Assert.That(() => new RepositoryAggregate(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Create_SetsFields_AndGeneratesId()
    {
        var projectId = Guid.NewGuid();

        var repository = RepositoryAggregate.Create("backend", "Repos/x/backend.git", projectId);

        await Assert.That(repository.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(repository.RepositoryName).IsEqualTo("backend");
        await Assert.That(repository.RepoPath).IsEqualTo("Repos/x/backend.git");
        await Assert.That(repository.ProjectId).IsEqualTo(projectId);
    }

    [Test]
    public async Task Create_TwoRepositories_GetDifferentIds()
    {
        var projectId = Guid.NewGuid();

        var first = RepositoryAggregate.Create("a", "a.git", projectId);
        var second = RepositoryAggregate.Create("b", "b.git", projectId);

        await Assert.That(first.Id).IsNotEqualTo(second.Id);
    }

    [Test]
    public async Task DeleteRepository_RaisesRepositoryDeletedEvent()
    {
        var projectId = Guid.NewGuid();
        var repository = RepositoryAggregate.Create("backend", "backend.git", projectId);
        var aggregate = new RepositoryAggregate(repository);

        aggregate.DeleteRepository();

        var domainEvent = aggregate.DomainEvents.Single();
        await Assert.That(domainEvent).IsTypeOf<RepositoryDeletedEvent>();

        var deleted = (RepositoryDeletedEvent)domainEvent;
        await Assert.That(deleted.RepositoryId).IsEqualTo(repository.Id);
        await Assert.That(deleted.ProjectId).IsEqualTo(projectId);
        await Assert.That(deleted.RepositoryName).IsEqualTo("backend");
    }
}

using System.Reflection;

namespace ProjectCowork.Infrastructure.Domain.Tests.TestData;

/// <summary>
/// Większość encji ma <c>Id { get; }</c> bez settera, a wartość nadaje dopiero EF Core.
/// W testach jednostkowych nie ma bazy, więc ustawiamy pole zapasowe auto-właściwości ręcznie.
/// </summary>
internal static class EntityIdExtensions
{
    private const string BackingFieldName = "<Id>k__BackingField";

    public static T WithId<T, TId>(this T entity, TId id) where T : class
    {
        var field = FindBackingField(entity.GetType())
                    ?? throw new InvalidOperationException($"{entity.GetType().Name} has no auto-property Id.");

        field.SetValue(entity, id);
        return entity;
    }

    public static T WithId<T>(this T entity, Guid id) where T : class => entity.WithId<T, Guid>(id);

    public static T WithNewId<T>(this T entity) where T : class => entity.WithId(Guid.NewGuid());

    private static FieldInfo? FindBackingField(Type? type)
    {
        while (type is not null)
        {
            var field = type.GetField(BackingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }
}

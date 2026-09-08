namespace ProjectCowork.Infrastructure.Configuration.Utils;

public class IntegrationSectionNameBuilder
{
    public static string BuildSectionName(string sectionName)
    {
        return $"Integrations:{sectionName}";
    }
}
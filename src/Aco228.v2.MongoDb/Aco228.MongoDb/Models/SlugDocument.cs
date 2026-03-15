using Aco228.MongoDb.Models.Attributes;

namespace Aco228.MongoDb.Models;

public abstract class SlugDocument : MongoLite
{
    [MongoIndex] public string SlugId { get; set; }
    [MongoIndex] public string Name { get; set; }
    [MongoIndex] public string? Description { get; set; }
    public string? SlackChannelReportArticle { get; set; }
    public string? SlackChannelReportResearchTitle { get; set; }
    public string? SlackChannelReportGenerateAds { get; set; }
}
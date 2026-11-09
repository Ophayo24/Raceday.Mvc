using System.ComponentModel.DataAnnotations;

namespace Raceday.Mvc.ViewModels;

public class EventVm
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerImageUrl { get; set; }
}

public class CreateEventVm
{
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    [Required, MaxLength(1000)] public string Description { get; set; } = "";
    [Required] public DateTime EventDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    [Range(0.01, 10000)] public decimal DistanceKm { get; set; }
    [Required] public string EventType { get; set; } = "Run";
    public string? BannerImageUrl { get; set; }
    public IFormFile? BannerFile { get; set; }
}

public class CategoryVm
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? CategoryDistanceKm { get; set; }
}

public class CreateCategoryVm
{
    [Required, MaxLength(100)] public string CategoryName { get; set; } = "";
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? CategoryDistanceKm { get; set; }
}

public class EnrolmentVm
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
}

public class CreateEnrolmentVm
{
    [Required] public int EventId { get; set; }
    [Required] public int CategoryId { get; set; }
}

public class EventEnrolmentVm
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
}

public class ResultVm
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}

public class CreateResultVm
{
    [Required] public TimeSpan FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int FinishingPosition { get; set; }
}

public class MyResultVm
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Category { get; set; } = "";
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}


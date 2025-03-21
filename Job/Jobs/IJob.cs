namespace Job.Jobs;

public interface IJob
{
    Guid JobId { get; set; }
}
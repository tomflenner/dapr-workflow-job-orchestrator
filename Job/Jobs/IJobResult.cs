namespace Job.Jobs;

public interface IJobResult
{
    Guid JobId { get; set; }
}
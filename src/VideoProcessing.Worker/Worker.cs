using VideoProcessing.Worker.Services;

namespace VideoProcessing.Worker;

/// <summary>
/// Background worker that processes video transcoding jobs
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _maxConcurrentJobs;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _maxConcurrentJobs = configuration.GetValue<int>("Processing:MaxConcurrentJobs", 2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Video Processing Worker starting with {MaxConcurrentJobs} concurrent jobs", _maxConcurrentJobs);

        // Create a SemaphoreSlim to limit concurrent processing
        using var semaphore = new SemaphoreSlim(_maxConcurrentJobs, _maxConcurrentJobs);
        var processingTasks = new List<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for available slot
                await semaphore.WaitAsync(stoppingToken);

                // Create a new scope for this job
                using var scope = _serviceProvider.CreateScope();
                var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueueService>();

                // Check queue size
                var queueSize = await jobQueue.GetQueueSizeAsync();
                if (queueSize > 0)
                {
                    _logger.LogInformation("Queue size: {QueueSize} jobs pending", queueSize);
                }

                // Dequeue next job
                var job = await jobQueue.DequeueJobAsync(stoppingToken);

                if (job == null)
                {
                    // No job available, release semaphore and wait before checking again
                    semaphore.Release();
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                _logger.LogInformation("Picked up job for video {VideoId} (User: {UserId})",
                    job.VideoId, job.UserId);

                // Start processing in background (don't await)
                var processTask = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessJobAsync(job, stoppingToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, stoppingToken);

                processingTasks.Add(processTask);

                // Clean up completed tasks
                processingTasks.RemoveAll(t => t.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker main loop");
                semaphore.Release();
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        // Wait for all processing tasks to complete
        _logger.LogInformation("Waiting for {Count} remaining jobs to complete", processingTasks.Count);
        await Task.WhenAll(processingTasks);

        _logger.LogInformation("Video Processing Worker stopped");
    }

    private async Task ProcessJobAsync(Models.VideoProcessingJob job, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var processingService = scope.ServiceProvider.GetRequiredService<VideoProcessingService>();
        var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueueService>();

        try
        {
            _logger.LogInformation("Processing video {VideoId} (Retry {RetryCount}/{MaxRetries})",
                job.VideoId, job.RetryCount, job.MaxRetries);

            var result = await processingService.ProcessVideoAsync(job, cancellationToken);

            if (result.Success)
            {
                await jobQueue.MarkJobCompletedAsync(job.VideoId);

                _logger.LogInformation("Successfully processed video {VideoId} in {Duration}. " +
                    "Generated {QualityCount} quality variants and {ThumbnailCount} thumbnails",
                    job.VideoId,
                    result.ProcessingTime,
                    result.QualityVariants.Count,
                    result.ThumbnailUrls.Count);
            }
            else
            {
                _logger.LogError("Failed to process video {VideoId}: {Error}",
                    job.VideoId, result.ErrorMessage);

                // Requeue for retry
                await jobQueue.RequeueJobAsync(job);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing video {VideoId}", job.VideoId);

            // Requeue for retry
            await jobQueue.RequeueJobAsync(job);
        }
    }
}

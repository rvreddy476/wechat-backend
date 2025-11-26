# Video Processing Worker

Background worker service for processing video transcoding, thumbnail generation, and HLS streaming preparation.

## Features

- **Video Transcoding**: Converts videos to multiple quality variants (1080p, 720p, 480p, 360p)
- **HLS Streaming**: Generates HTTP Live Streaming (HLS) format with adaptive bitrate streaming
- **Thumbnail Generation**: Creates multiple thumbnails at different timestamps
- **Metadata Extraction**: Extracts video codec, bitrate, framerate, duration, and resolution
- **Cloud Storage**: Uploads processed videos and thumbnails to Google Cloud Storage
- **Job Queue**: Uses Redis for reliable job queueing with retry logic
- **Progress Tracking**: Real-time progress updates stored in MongoDB

## Technology Stack

- **.NET 8 Worker Service**: Background processing host
- **FFmpeg**: Video transcoding and processing
- **MongoDB**: Video metadata and status storage
- **Redis**: Job queue management
- **Google Cloud Storage**: Video and thumbnail storage
- **Serilog**: Structured logging

## Configuration

### appsettings.json

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "wechat_videos"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "GCP": {
    "Storage": {
      "BucketName": "wechat-videos"
    },
    "CredentialsPath": "/app/config/gcp-service-account.json"
  },
  "FFmpeg": {
    "FFmpegPath": "/usr/bin/ffmpeg",
    "FFprobePath": "/usr/bin/ffprobe"
  },
  "Processing": {
    "TempDirectory": "/tmp/video-processing",
    "MaxConcurrentJobs": 2
  }
}
```

### Environment Variables

- `MongoDB__ConnectionString`: MongoDB connection string
- `Redis__ConnectionString`: Redis connection string
- `GCP__Storage__BucketName`: GCS bucket name
- `GCP__CredentialsPath`: Path to GCP service account JSON file
- `Processing__MaxConcurrentJobs`: Number of concurrent video processing jobs (default: 2)

## Processing Workflow

1. **Download**: Download source video from GCS presigned URL
2. **Extract Metadata**: Extract video information (duration, resolution, codecs, etc.)
3. **Generate Thumbnails**: Create 5 thumbnails at evenly spaced intervals
4. **Transcode Videos**:
   - Generate HLS manifest with multiple quality variants (adaptive streaming)
   - OR generate individual MP4 files for each quality level
5. **Upload**: Upload all processed files to GCS
6. **Update Database**: Mark video as "Ready" with streaming URLs and metadata

## Video Processing Options

```csharp
public class VideoProcessingOptions
{
    public bool GenerateQualityVariants { get; set; } = true;
    public bool GenerateHLS { get; set; } = true;
    public int ThumbnailCount { get; set; } = 5;
    public string TargetCodec { get; set; } = "h264";
    public List<QualityPreset> QualityPresets { get; set; } = [
        new QualityPreset { Name = "1080p", Width = 1920, Height = 1080, VideoBitrate = 4500, AudioBitrate = 192 },
        new QualityPreset { Name = "720p", Width = 1280, Height = 720, VideoBitrate = 2500, AudioBitrate = 128 },
        new QualityPreset { Name = "480p", Width = 854, Height = 480, VideoBitrate = 1000, AudioBitrate = 96 },
        new QualityPreset { Name = "360p", Width = 640, Height = 360, VideoBitrate = 600, AudioBitrate = 64 }
    ];
}
```

## Job Queue

Jobs are enqueued to Redis list: `video:processing:queue`

### Enqueue a Job (from VideoService.Api)

```csharp
var job = new VideoProcessingJob
{
    VideoId = video.Id,
    UserId = video.UserId,
    SourceUrl = presignedUrl,
    OriginalFileName = video.OriginalFileName,
    Options = new VideoProcessingOptions
    {
        GenerateHLS = true,
        ThumbnailCount = 5
    }
};

var jobJson = JsonSerializer.Serialize(job);
await redis.GetDatabase().ListLeftPushAsync("video:processing:queue", jobJson);
```

## Docker Deployment

### Build Image

```bash
docker build -f src/VideoProcessing.Worker/Dockerfile -t video-processing-worker:latest .
```

### Run Container

```bash
docker run -d \
  --name video-processing-worker \
  -e MongoDB__ConnectionString="mongodb://mongo:27017" \
  -e Redis__ConnectionString="redis:6379" \
  -e GCP__Storage__BucketName="wechat-videos" \
  -v /path/to/gcp-credentials.json:/app/config/gcp-service-account.json:ro \
  -v /tmp/video-processing:/tmp/video-processing \
  video-processing-worker:latest
```

### Docker Compose

```yaml
services:
  video-processing-worker:
    build:
      context: .
      dockerfile: src/VideoProcessing.Worker/Dockerfile
    environment:
      - MongoDB__ConnectionString=mongodb://mongo:27017
      - Redis__ConnectionString=redis:6379
      - GCP__Storage__BucketName=wechat-videos
      - Processing__MaxConcurrentJobs=2
    volumes:
      - ./config/gcp-service-account.json:/app/config/gcp-service-account.json:ro
      - /tmp/video-processing:/tmp/video-processing
    depends_on:
      - mongo
      - redis
    restart: unless-stopped
```

## FFmpeg Operations

### Video Transcoding

```bash
ffmpeg -i input.mp4 \
  -c:v libx264 -preset fast \
  -b:v 2500k -maxrate 2500k -bufsize 5000k \
  -vf "scale=1280:720" \
  -c:a aac -b:a 128k \
  -movflags +faststart \
  output_720p.mp4
```

### HLS Generation

```bash
ffmpeg -i input.mp4 \
  -c:v libx264 -preset fast \
  -b:v 2500k \
  -vf "scale=1280:720" \
  -c:a aac -b:a 128k \
  -hls_time 6 \
  -hls_list_size 0 \
  -hls_segment_filename "segment_%03d.ts" \
  -f hls playlist.m3u8
```

### Thumbnail Extraction

```bash
ffmpeg -ss 00:00:10 -i input.mp4 \
  -vf "scale=854:-1" \
  -vframes 1 \
  thumbnail.jpg
```

## Monitoring

### Health Check

The worker includes health checks for Redis and MongoDB connectivity.

### Logs

Logs are written to:
- Console (stdout)
- File: `logs/video-processing-YYYY-MM-DD.txt`

### Metrics

Monitor these key metrics:
- Queue size: Number of pending jobs
- Processing time: Average time per video
- Success rate: Percentage of successful processing
- Active jobs: Number of currently processing videos

## Troubleshooting

### FFmpeg Not Found

Ensure FFmpeg is installed:
```bash
apt-get update && apt-get install -y ffmpeg
```

### Out of Memory

Reduce `MaxConcurrentJobs` in configuration or increase container memory limit.

### Slow Processing

- Check available CPU cores
- Increase `MaxConcurrentJobs` if CPU usage is low
- Use faster FFmpeg presets (`-preset ultrafast` instead of `fast`)

### Failed Jobs

Jobs automatically retry up to 3 times. Check logs for error details:
```bash
docker logs video-processing-worker
```

## Performance Optimization

1. **Parallel Processing**: Adjust `MaxConcurrentJobs` based on available CPU cores (recommended: CPU cores - 1)
2. **FFmpeg Presets**: Use `ultrafast` or `veryfast` presets for faster encoding at the cost of file size
3. **Hardware Acceleration**: Enable GPU encoding (NVENC, QuickSync, VA-API) for faster transcoding
4. **Storage**: Use SSD storage for temporary files to improve I/O performance

## Scaling

- **Horizontal Scaling**: Run multiple worker instances to process more videos concurrently
- **Queue Partitioning**: Use multiple Redis queues for different priority levels
- **Dedicated Workers**: Separate workers for thumbnails vs. full transcoding

## License

Proprietary - WeChat Social Media Platform

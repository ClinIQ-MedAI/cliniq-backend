# Backend Integration Test Guide

Your .NET backend can now communicate with the ClinIQ AI platform via Redis. This guide shows you how to test it.

---

## Prerequisites

1. **runs.sh is running** with async queue enabled:
   ```bash
   export QUEUE_BACKEND=redis
   export REDIS_CONNECTION='your-upstash-host:6379,password=YOUR_TOKEN,ssl=true'
   ./runs.sh
   ```

2. **Redis is accessible** from your backend:
   ```bash
   # Test connectivity (from your .NET machine)
   redis-cli -h causal-leopard-72320.upstash.io -p 6379 -a YOUR_TOKEN --tls ping
   # Should return: PONG
   ```

---

## Test Case 1: Simple Bone X-Ray Detection

### Step 1: Prepare an image

You need a **JPEG image** (can be any image for testing). For real testing, use an actual wrist X-ray.

```bash
# Create a dummy test image (or use your own)
curl -o /tmp/test_xray.jpg https://via.placeholder.com/512x512
```

### Step 2: Encode image to base64

**C# example:**
```csharp
using System;
using System.IO;

string imagePath = "C:\\path\\to\\xray.jpg";
byte[] imageBytes = File.ReadAllBytes(imagePath);
string base64Image = Convert.ToBase64String(imageBytes);
Console.WriteLine(base64Image); // use this in the next step
```

**Bash alternative:**
```bash
cat /tmp/test_xray.jpg | base64 -w 0
```

### Step 3: Publish a job to Redis

**C# (StackExchange.Redis):**

```csharp
using StackExchange.Redis;
using System.Text.Json;

// Connect to Redis (same credentials as your AI platform)
var redis = ConnectionMultiplexer.Connect(
    "causal-leopard-72320.upstash.io:6379,password=YOUR_TOKEN,ssl=true");
var db = redis.GetDatabase();

// Create the job
var job = new
{
    job_id = Guid.NewGuid().ToString("N"),
    modality = "bone",
    image_base64 = base64ImageString, // from step 2
    patient_id = "test_patient_001",
    options = new { include_gradcam = false },
    enqueued_at = DateTime.UtcNow.ToString("O")
};

// Serialize to JSON
string jobJson = JsonSerializer.Serialize(job);

// Publish to the bone jobs queue
await db.StreamAddAsync(
    "cliniq:jobs:bone",
    new NameValueEntry[] { new("data", jobJson) }
);

Console.WriteLine($"✓ Job published: {job.job_id}");
```

### Step 4: Wait for the result (poll Redis)

**C# (StackExchange.Redis):**

```csharp
// Consumer group setup (do this once)
try
{
    await db.StreamCreateConsumerGroupAsync(
        "cliniq:results",
        "backend", // your consumer group name
        "0-0",     // read all existing + new messages
        createStream: true
    );
}
catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
{
    // Group already exists
}

// Poll for results (in a loop)
var deadline = DateTime.UtcNow.AddSeconds(60);
while (DateTime.UtcNow < deadline)
{
    var entries = await db.StreamReadGroupAsync(
        "cliniq:results",
        "backend",
        "api-worker",
        count: 10
    );

    foreach (var entry in entries ?? Array.Empty<StreamEntry>())
    {
        var resultJson = (string)entry.Values[0].Value;
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
        
        // Extract job_id from result
        string returnedJobId = result.GetProperty("job_id").GetString();
        
        if (returnedJobId == job.job_id)
        {
            // Found your result!
            Console.WriteLine("✓ Result received!");
            Console.WriteLine($"  Status: {result.GetProperty("status")}");
            
            var aiResult = result.GetProperty("result");
            Console.WriteLine($"  Modality: {aiResult.GetProperty("modality")}");
            Console.WriteLine($"  Summary: {aiResult.GetProperty("summary")}");
            
            // Acknowledge the message
            await db.StreamAcknowledgeAsync("cliniq:results", "backend", entry.ID);
            return;
        }
    }

    await Task.Delay(500); // Poll every 500ms
}

Console.WriteLine("✗ Timeout waiting for result");
```

---

## Test Case 2: Multi-Modal Job (Test All Services)

Submit jobs to all services in parallel:

```csharp
var jobs = new[] {
    ("bone", "cliniq:jobs:bone"),
    ("dental_xray", "cliniq:jobs:dental_xray"),
    ("chest", "cliniq:jobs:chest"),
    ("dental_photo", "cliniq:jobs:dental_photo"),
};

var jobIds = new List<string>();

foreach (var (modality, queue) in jobs)
{
    var jobId = Guid.NewGuid().ToString("N");
    var job = new
    {
        job_id = jobId,
        modality = modality,
        image_base64 = base64ImageString,
        patient_id = "test_patient_multi",
        options = new { },
        enqueued_at = DateTime.UtcNow.ToString("O")
    };

    await db.StreamAddAsync(queue, new NameValueEntry[] { new("data", JsonSerializer.Serialize(job)) });
    jobIds.Add(jobId);
    Console.WriteLine($"✓ Job published to {modality}");
}

// Wait for all results (same polling loop as above, but check all job_ids)
```

---

## Quick Bash Test (No C# needed)

If you just want to verify the queue works before building the full integration:

```bash
# 1. Set Redis credentials
export REDIS_HOST=causal-leopard-72320.upstash.io
export REDIS_PORT=6379
export REDIS_PASSWORD=YOUR_TOKEN

# 2. Create a test job and publish
JOBID=$(uuidgen)
IMAGE_B64=$(base64 -w 0 < /tmp/test_xray.jpg)

JOB=$(cat <<EOF
{
  "job_id": "$JOBID",
  "modality": "bone",
  "image_base64": "$IMAGE_B64",
  "patient_id": "test_patient_bash",
  "options": {},
  "enqueued_at": "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
}
EOF
)

# 3. Publish to Redis (using redis-cli)
echo "$JOB" | redis-cli \
  -h $REDIS_HOST \
  -p $REDIS_PORT \
  -a $REDIS_PASSWORD \
  --tls \
  XADD cliniq:jobs:bone \* \
  data "$JOB"

echo "✓ Job published: $JOBID"

# 4. Poll for result
echo "Waiting for result..."
for i in {1..60}; do
  RESULT=$(redis-cli \
    -h $REDIS_HOST \
    -p $REDIS_PORT \
    -a $REDIS_PASSWORD \
    --tls \
    XREAD COUNT 1 STREAMS cliniq:results 0)
  
  if echo "$RESULT" | grep -q "$JOBID"; then
    echo "✓ Result received!"
    echo "$RESULT"
    break
  fi
  sleep 1
done
```

---

## Expected Result

When a job completes, you'll get a message like:

```json
{
  "job_id": "abc123def456",
  "modality": "bone",
  "status": "completed",
  "result": {
    "modality": "X-ray",
    "body_part": "Wrist",
    "detections": [
      {
        "class_name": "fracture",
        "confidence": 0.923,
        "bbox": [120, 80, 200, 160],
        "severity": "HIGH"
      }
    ],
    "annotated_image_base64": "...",
    "summary": "Detected: 1 fracture.",
    "urgency": "HIGH",
    "recommendations": ["URGENT: Fracture detected..."]
  },
  "model_version": "best.pt@1bf748929178 (109.1MB)",
  "duration_ms": 15234.5,
  "worker": "hostname:bone",
  "finished_at": "2026-07-05T12:34:56Z"
}
```

---

## Troubleshooting

| Issue | Fix |
|---|---|
| "Connection refused" | Check Redis is accessible. Test with `redis-cli` first. |
| "BUSYGROUP" error | The consumer group already exists (normal on 2nd run). Catch and continue. |
| Result never arrives | Check that `runs.sh` is still running. Tail bone-detect logs: `tail -f bone-detect/api/server.py` |
| Result has `"status": "failed"` | Check the `error` field. Usually means bad base64 or missing model. |

---

## Next Steps

1. **Replace dummy image** with a real wrist X-ray for bone-detect testing.
2. **Add to your backend** the job publishing + polling logic above.
3. **Test all modalities** (dental, chest, prescription) using the same pattern.
4. **Connect to your UI** — push results to clients via SignalR/WebSocket.
5. **Add retry logic** — if a job fails, republish after a delay.

For the full message contract, see [QUEUE_INTEGRATION.md](QUEUE_INTEGRATION.md).

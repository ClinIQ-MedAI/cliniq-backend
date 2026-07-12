# AI Engineer Guide: Upstash QStash Integration

This guide outlines the changes made to the .NET backend to support **Upstash QStash** as a message queue backend and explains what modifications are required in the Python AI microservices to support this integration.

---

## 1. Architectural Shift: Redis Stream vs. HTTP Push

Previously, under the **Redis Streams** configuration, the workflow was pull-based:
- Python microservices subscribed to Redis Stream channels:
  - `cliniq:jobs:<modality>` (for scans and prescriptions)
  - `cliniq:chat:requests` (for chatbot messages)
- Results were pushed back to:
  - `cliniq:results`
  - `cliniq:chat:results`

Under the **QStash** configuration, the workflow is push-based (serverless-friendly):
1. The .NET backend pushes a job payload to **QStash**.
2. **QStash** forwards the job via an HTTP `POST` request to your Python microservice.
3. Your Python microservice performs the prediction and returns the result **directly in the HTTP response body**.
4. **QStash** receives your HTTP response, wraps it, and sends a callback request to the .NET backend.

This means **your Python microservices no longer need to connect to Redis or publish result streams**. They only need to expose an HTTP server (e.g. using FastAPI or Flask).

---

## 2. API Specifications for Python Microservices

### A. Scans & Prescription Modality Endpoint

- **Endpoint URL**: `POST /predict_for_llm`
- **Incoming Request Body (from QStash)**:
  ```json
  {
    "job_id": "string (guid)",
    "modality": "string (e.g., bone, dental_xray, chest, dental_photo, prescription)",
    "image_base64": "string (optional base64 image data)",
    "image_url": "string (optional public image URL)",
    "patient_id": "string",
    "options": {} or null, // options object for chest/dental_photo
    "reply_to": "string (optional)",
    "enqueued_at": "ISO-8601 string"
  }
  ```

- **Expected HTTP Response Body (JSON)**:
  Your endpoint must process the prediction and return the response below with `200 OK`. Do not wrap or encode the response; QStash will handle that automatically.
  ```json
  {
    "job_id": "string (must match incoming job_id)",
    "modality": "string",
    "status": "completed", // or "failed"
    "result": {}, // The actual analysis payload (e.g., structured scan annotations)
    "error": "string (optional message if failed)",
    "patient_id": "string",
    "worker": "string (identifier of your service instance)",
    "duration_ms": 123.45, // processing duration
    "finished_at": "ISO-8601 string"
  }
  ```

---

### B. Chatbot Endpoint

- **Endpoint URL**: `POST /chat`
- **Incoming Request Body (from QStash)**:
  ```json
  {
    "chat_id": "string (guid)",
    "message": "string (patient query text)",
    "patient_id": "string",
    "language_preference": "string (e.g., ar, en)",
    "enqueued_at": "ISO-8601 string"
  }
  ```

- **Expected HTTP Response Body (JSON)**:
  ```json
  {
    "chat_id": "string (must match incoming chat_id)",
    "status": "completed", // or "failed"
    "reply": "string (the markdown or text chatbot reply)",
    "query_type": "string (optional metadata, e.g. general, symptoms)",
    "show_upload": false, // whether to prompt patient to upload a scan
    "patient_id": "string",
    "error": "string (optional message if failed)",
    "worker": "string (identifier of your service instance)",
    "duration_ms": 123.45,
    "finished_at": "ISO-8601 string"
  }
  ```

---

## 3. Checklist for AI Engineer

1. **Expose HTTP Routes**:
   - Ensure scan modality services expose a `POST /predict_for_llm` endpoint.
   - Ensure the chatbot service exposes a `POST /chat` endpoint.
2. **Synchronous Execution**:
   - The endpoints should block and perform predictions synchronously, then return the response within the HTTP timeout limit configured in QStash.
3. **No Signature / Callback Logic Needed**:
   - You do not need to implement HMAC validation or callback requests in the Python code. QStash acts as the broker; it authenticates the request to your services and routes the response back to the .NET callback endpoints.
4. **Deploying / Routing**:
   - Ensure the URLs configured in the .NET backend `AIServiceSettings` (`BoneUrl`, `DentalXrayUrl`, `ChestUrl`, `DentalPhotoUrl`, `PrescriptionUrl`, and `ChatbotUrl`) point to the public domains (or internal endpoints reachable by QStash) of your Python microservices.

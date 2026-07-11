# Flutter Integration Guide: ClinIQ AI Chatbot Hub

The .NET backend exposes a real-time SignalR socket endpoint for the AI Chatbot assistant at `/hubs/chatbot`. This document explains how to connect, authenticate, and listen for messages in Flutter.

---

## 1. Prerequisites

In Flutter, the recommended package for interacting with ASP.NET Core SignalR is `signalr_netcore`.

Add this to your `pubspec.yaml`:

```yaml
dependencies:
  signalr_netcore: ^1.3.1 # or the latest version
```

---

## 2. Hub Details

| Parameter | Value | Description |
|---|---|---|
| **Hub URL** | `https://cliniq.runasp.net/hubs/chatbot` | Production endpoint |
| **Local URL (Android Emulator)** | `http://10.0.2.2:5000/hubs/chatbot` | Standard loopback interface for local testing |
| **Local URL (iOS Simulator / Web)** | `http://localhost:5000/hubs/chatbot` | Local address |
| **Authentication** | **Required** (JWT Bearer Token) | Passed in the connection options header |
| **Event Name** | `"ReceiveChatbotReply"` | The socket message callback you register |

---

## 3. How Authentication Works

Since the hub is marked with `[Authorize]`, anonymous connection attempts will be rejected with an HTTP `401 Unauthorized` status. You must provide the JWT access token that you received from `/api/auth/login`.

SignalR handles this via the `accessTokenFactory` property in the connection options.

---

## 4. Message Contract (`ReceiveChatbotReply`)

When the AI chatbot processes a request and publishes a response, the backend pushes a payload to the listener registered for the `"ReceiveChatbotReply"` event.

The argument payload received by the client contains a JSON Map with the following structure:

```jsonc
{
  "chat_id": "3f2c…",               // UUID hex matching the request
  "status": "Completed",            // "Completed" | "Failed"
  "reply": "الصداع له أسباب كثيرة…", // The full response string
  "query_type": "health",           // health | appointment | availability | faq | upload | unknown
  "show_upload": false,             // true => User was prompted to upload an image scan
  "patient_id": "patient_demo",     // Patient user identifier
  "error": null,                    // Error message string (if status == "Failed")
  "worker": "lair-g2:chat",         // GPU processing node
  "duration_ms": 1840.5,            // Processing duration
  "finished_at": "2026-07-10T10:00:02Z"
}
```

> [!IMPORTANT]
> **Acting on `show_upload`**: If the incoming message has `"show_upload": true`, it means the chatbot detected the user wants to upload a medical scan or prescription. The Flutter app should display an image upload prompt to the user and dispatch that image to the **Modal AI Scan Jobs** endpoint (`/api/scans/upload`), **not** the chatbot endpoint.

---

## 5. Dart Integration Example

Here is a full example class implementing the connection, message handling, and connection recovery logic:

```dart
import 'package:signalr_netcore/signalr_netcore.dart';

class ChatbotSocketService {
  HubConnection? _hubConnection;
  
  // Callback invoked when a reply is received
  final Function(Map<String, dynamic> reply) onReplyReceived;
  
  ChatbotSocketService({required this.onReplyReceived});

  /// Initialize and connect to the Hub
  Future<void> connect(String hubUrl, String jwtToken) async {
    final httpOptions = HttpConnectionOptions(
      // Pass the JWT bearer token for Hub authentication
      accessTokenFactory: () async => jwtToken,
      // You can configure custom headers or transport here if needed
      logMessageContent: true,
    );

    _hubConnection = HubConnectionBuilder()
        .withUrl(hubUrl, options: httpOptions)
        .withAutomaticReconnect() // Automatically handles transient dropouts
        .build();

    // Setup connection state listener callbacks
    _hubConnection!.onclose(({error}) {
      print("Chatbot socket connection closed: $error");
    });

    _hubConnection!.onreconnecting(({error}) {
      print("Chatbot socket attempting to reconnect: $error");
    });

    _hubConnection!.onreconnected(({connectionId}) {
      print("Chatbot socket successfully reconnected: $connectionId");
    });

    // Register event listener for chatbot replies
    _hubConnection!.on("ReceiveChatbotReply", _handleIncomingReply);

    // Start the connection
    try {
      await _hubConnection!.start();
      print("Connected successfully to Chatbot SignalR Hub!");
    } catch (e) {
      print("Error connecting to Chatbot Hub: $e");
    }
  }

  /// Disconnect cleanly
  Future<void> disconnect() async {
    if (_hubConnection != null) {
      await _hubConnection!.stop();
      _hubConnection = null;
      print("Disconnected from Chatbot Hub.");
    }
  }

  /// Handle incoming socket messages
  void _handleIncomingReply(List<dynamic>? arguments) {
    if (arguments != null && arguments.isNotEmpty) {
      // The payload maps directly to the ChatReply structure
      final Map<String, dynamic> payload = arguments.first as Map<String, dynamic>;
      onReplyReceived(payload);
    }
  }
}
```

---

## 6. Typical Conversation Flow

1. Authenticate with backend and obtain your `jwtToken`.
2. Connect to the socket using `ChatbotSocketService.connect(hubUrl, jwtToken)`.
3. Submit a new chatbot query by making a standard HTTP `POST` request:
   - **Endpoint**: `POST /api/chatbot`
   - **Headers**: `Authorization: Bearer <jwtToken>`
   - **Body**:
     ```json
     {
       "message": "أشعر بألم شديد في الأسنان، ماذا أفعل؟",
       "languagePreference": "ar"
     }
     ```
   - **Response**: The API will return an HTTP 200 with the message record state (status `Pending`), including the generated `chatId` (e.g. `3f2c…`).
4. Display a loading indicator/bubble in the UI for the message matching that `chatId`.
5. The backend forwards the request to the GPU node queue. When the AI finishes processing, the backend pushes the reply onto the socket.
6. The Flutter client receives the `"ReceiveChatbotReply"` event payload, matches the `chatId`, updates the loading bubble to display the `reply` text, and switches status to `Completed`.

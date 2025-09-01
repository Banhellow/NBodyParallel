using System;
using System.Text;
using DefaultNamespace.Responses;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using WebSocket = NativeWebSocket.WebSocket;

namespace Communication
{
    public class WebSocketConnection
    {
        private readonly WebSocket webSocket;
        private readonly MessageHandler messageHandler;

        public WebSocketConnection(string url, MessageHandler messageHandler)
        {
            webSocket = new WebSocket(url);
            this.messageHandler = messageHandler;
        }

        public async void InitializeConnection()
        {
            webSocket.OnOpen += OnConnectionStarted;
            webSocket.OnMessage += OnMessageReceived;
            webSocket.OnClose += OnConnectionClosed;
            webSocket.OnError += OnError;
            await webSocket.Connect();
        }

        public void CloseConnection()
        {
            webSocket.OnOpen -= OnConnectionStarted;
            webSocket.OnMessage -= OnMessageReceived;
            webSocket.OnClose -= OnConnectionClosed;
            webSocket.OnError -= OnError;
            webSocket.Close();
        }

        private async void OnConnectionStarted()
        {
            Debug.Log("Connection established");
            await SendInitialMessage();
        }

        public void AcceptMessage()
        {
            webSocket.DispatchMessageQueue();
        }

        private void OnConnectionClosed(WebSocketCloseCode closeCode)
        {
            Debug.Log($"Connection Closed with code: {closeCode}");
        }

        private void OnError(string errorMsg)
        {
            Debug.LogError($"Message with error received: {errorMsg}");
        }

        private void OnMessageReceived(byte[] bytes)
        {
            Debug.Log("message received");
            var json = Encoding.UTF8.GetString(bytes);
            var response = JsonConvert.DeserializeObject<PredictionResponse>(json);
            messageHandler.AddFrame(response.PredictedPositions);
        }

        private async Awaitable SendInitialMessage()
        {
            var initJson = JsonConvert.SerializeObject(messageHandler.SimulatedData);
            await webSocket.SendText(initJson);
        }
    }
}

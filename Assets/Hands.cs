using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class HandGestureReceiver : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    public Material leftMaterial;
    public Material rightMaterial;


    private Dictionary<string, string> leftHandImages = new Dictionary<string, string>
    {
        { "Fist", "Hands/fist_left" },
        { "HighFive", "Hands/none_left" },
    };

    private Dictionary<string, string> rightHandImages = new Dictionary<string, string>
    {
        { "1", "Hands/I_right" },
        { "2", "Hands/II_right" },
        { "3", "Hands/III_right" },
        { "4", "Hands/IV_right" },
    };

    async void Start()
    {
        Application.targetFrameRate = 240;
        try
        {
            client = new TcpClient("127.0.0.1", 12345);
            stream = client.GetStream();
            Debug.Log("Connected to Python server.");
            await ReadDataAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to connect: " + e.Message);
        }
    }

    async Task ReadDataAsync()
    {
        while (client != null && client.Connected)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    Debug.Log("Received from Python: " + message);
                    string[] parts = message.Split(';');
                    string leftMsg = null;
                    string rightMsg = null;
                    foreach (string part in parts)
                    {
                        string[] subparts = part.Split(':');
                        if (subparts.Length == 2)
                        {
                            if (subparts[0] == "Left")
                                leftMsg = subparts[1];
                            else if (subparts[0] == "Right")
                                rightMsg = subparts[1];
                        }
                    }
                    actionQueue.Enqueue(() =>
                    {
                        // Process left-hand message
                        if (!string.IsNullOrEmpty(leftMsg))
                        {
                            if (leftHandImages.TryGetValue(leftMsg, out string leftImagePath))
                            {
                                Texture2D leftTexture = Resources.Load<Texture2D>(leftImagePath);
                                if (leftTexture != null && leftMaterial != null)
                                {
                                    leftMaterial.mainTexture = leftTexture;
                                    Debug.Log($"Applied left texture: {leftImagePath}");
                                }
                                else
                                {
                                    Debug.LogWarning("Left texture not found or left material is null: " + leftImagePath);
                                }
                            }
                        }
                        // Process right-hand message
                        if (!string.IsNullOrEmpty(rightMsg))
                        {
                            if (rightHandImages.TryGetValue(rightMsg, out string rightImagePath))
                            {
                                Texture2D rightTexture = Resources.Load<Texture2D>(rightImagePath);
                                if (rightTexture != null && rightMaterial != null)
                                {
                                    rightMaterial.mainTexture = rightTexture;
                                    Debug.Log($"Applied right texture: {rightImagePath}");
                                }
                                else
                                {
                                    Debug.LogWarning("Right texture not found or right material is null: " + rightImagePath);
                                }
                            }
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("Connection closed by Python.");
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading data: " + e.Message);
                break;
            }
        }
    }

    void Update()
    {
        while (actionQueue.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}

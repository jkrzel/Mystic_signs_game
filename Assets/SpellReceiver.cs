using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class SpellReceiver : MonoBehaviour
{
    
    public static event Action<string> OnSpellReceived;

    [Header("Unity GameObjects")]
    public GameObject LeftDisplay;
    public GameObject RightDisplay;

    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    
    private Renderer spellRenderer;
    private Material spellMaterial;
    private Renderer leftRenderer;
    private Material leftMaterial;
    private Renderer rightRenderer;
    private Material rightMaterial;

   
    private readonly Dictionary<string, string> spellImages = new Dictionary<string, string>
    {
        { "MagicAttack",   "Spell/magic_fist"    },
        { "FreezeAttack",  "Spell/ice_fist"      },
        { "PoisonAttack",  "Spell/poison_fist"   },
        { "FireAttack",    "Spell/fire_fist"     },
        { "InstantHeal",   "Spell/instant_heal"  },
        { "Regeneration",  "Spell/fire_fist"     },
        { "Cleanse",       "Spell/purify"        },
        { "Shield",        "Spell/shield_hand"   },
        { "Bubble",        "Spell/bubble_shield" },
    };

    
    private readonly Dictionary<string, string> leftHandImages = new Dictionary<string, string>
    {
        { "fist",      "Hands/fist_left" },
        { "victory",   "Hands/II_left"   },
        { "high_five", "Hands/none_left" },
        { "none",      "Hands/none_left" },
    };

    
    private readonly Dictionary<string, string> rightHandImages = new Dictionary<string, string>
    {
        { "1",    "Hands/I_right"   },
        { "2",    "Hands/II_right"  },
        { "3",    "Hands/III_right" },
        { "4",    "Hands/IV_right"  },
        { "none", "Hands/none_right" },
    };

    async void Start()
    {
        
        spellRenderer = GetComponent<Renderer>();
        spellMaterial = spellRenderer.material;

        if (LeftDisplay != null)
        {
            leftRenderer = LeftDisplay.GetComponent<Renderer>();
            leftMaterial = leftRenderer.material;
        }
        if (RightDisplay != null)
        {
            rightRenderer = RightDisplay.GetComponent<Renderer>();
            rightMaterial = rightRenderer.material;
        }

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
                if (bytesRead <= 0)
                {
                    Debug.LogWarning("Connection closed by Python.");
                    break;
                }

                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                Debug.Log("Received from Python: " + message);

                
                string leftVal = "none";
                string rightVal = "none";
                string spellVal = "none";

                var parts = message.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var kv = part.Split(new[] { ':' }, 2);
                    if (kv.Length < 2) continue;
                    switch (kv[0])
                    {
                        case "left_hand": leftVal = kv[1]; break;
                        case "right_hand": rightVal = kv[1]; break;
                        case "spell": spellVal = kv[1]; break;
                    }
                }

                
                actionQueue.Enqueue(() =>
                {
                    // 1) Spell texture
                    if (spellVal != "none" && spellImages.TryGetValue(spellVal, out string spellPath))
                    {
                        var tex = Resources.Load<Texture2D>(spellPath);
                        if (tex) spellMaterial.mainTexture = tex;
                        else Debug.LogWarning("Spell image not found: " + spellPath);
                    }

                    // 2) Left hand texture
                    if (leftRenderer != null && leftHandImages.TryGetValue(leftVal, out string leftPath))
                    {
                        var lt = Resources.Load<Texture2D>(leftPath);
                        if (lt) leftMaterial.mainTexture = lt;
                        else Debug.LogWarning("Left image not found: " + leftPath);
                    }

                    // 3) Right hand texture
                    if (rightRenderer != null && rightHandImages.TryGetValue(rightVal, out string rightPath))
                    {
                        var rt = Resources.Load<Texture2D>(rightPath);
                        if (rt) rightMaterial.mainTexture = rt;
                        else Debug.LogWarning("Right image not found: " + rightPath);
                    }

                    // 4) Fire BattleSystem event
                    if (spellVal != "none")
                        OnSpellReceived?.Invoke(spellVal);
                });
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


using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class SimpleMQTTUnity : MonoBehaviour
{
    [Header("MQTT Broker 設定")]
    [SerializeField] private string brokerIP = "192.168.38.5";
    [SerializeField] private int brokerPort = 1883;
    [SerializeField] private string subscribeTopic = "M2MQTT_Unity/test";

    [Header("發送訊息")]
    [SerializeField] private string publishTopic = "M2MQTT_Unity/test";
    [SerializeField] private string messageToSend = "Hello from Unity!";
    [SerializeField] private bool sendMessage = false;

    [Header("連線控制（按鈕觸發）")]
    [SerializeField] private bool autoReconnect = true;        // 是否在斷線後自動重連
    [SerializeField] private float reconnectDelaySeconds = 3f; // 重連間隔
    [SerializeField] private int connectTimeoutMs = 3000;      // 逾時（若你的 M2Mqtt 版本支援設定）
    [SerializeField] private bool dontDestroyOnLoad = true;    // 是否跨場景保留

    private MqttClient client;
    private bool isConnecting = false;
    private bool connectLoopRunning = false; // 防止重複啟動協程
    private bool wantReconnect = false;      // 由按鈕控制是否啟用重連循環

    private string latestMessage = "";
    public string LatestMessage => latestMessage;

    public event Action<string> OnMessageUpdated;

    private readonly Queue<Action> mainThreadActions = new Queue<Action>();




    void Awake()
    {
     // 僅保留唯一實例
        var existing = FindObjectsOfType<SimpleMQTTUnity>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);  // ★ 讓 MQTT_test 跨場景保留
    }




    // --- 移除 Start() 的自動連線 --- //
    // void Start() { }  // 不用

    /// <summary>
    /// 按鈕呼叫：開始連線（並啟動重連循環）
    /// </summary>
    

    // 供 UI 呼叫：設定 IP（不立即連線）
    public void SetBrokerIP(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            Debug.LogWarning("brokerIP 空白，忽略。");
            return;
        }

        // 基本驗證（可更嚴謹：Regex 或 IPAddress.TryParse）
        brokerIP = ip.Trim();
        Debug.Log($"已設定 brokerIP = {brokerIP}");
    }

    // 供 UI 呼叫：設定 Port（不立即連線）
    public void SetBrokerPort(string portText)
    {
        if (!int.TryParse(portText, out var port) || port <= 0 || port > 65535)
        {
            Debug.LogWarning($"brokerPort 非法：{portText}");
            return;
        }
        brokerPort = port;
        Debug.Log($"已設定 brokerPort = {brokerPort}");
    }

    //getbrokerstate
    public string GetBrokerIP() => brokerIP;
    public int GetBrokerPort() => brokerPort;


    public void StartConnect()
    {
    // 先保證物件與腳本啟用
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("MQTT_test 為 Inactive，已自動 SetActive(true)。");
            gameObject.SetActive(true);
        }
        if (!enabled)
        {
            Debug.LogWarning("SimpleMQTTUnity 腳本為 Disabled，已自動啟用。");
            enabled = true;
        }

        if (client != null && client.IsConnected)
        {
            Debug.Log("MQTT 已連線，無需再次連線。");
            return;
        }

        wantReconnect = true;
        if (!connectLoopRunning)
        {
            StartCoroutine(ConnectLoop());   // ★ 直接用本腳本啟動協程
            connectLoopRunning = true;
        }
    }



    /// <summary>
    /// 按鈕呼叫：斷線並停止重連
    /// </summary>
    public void Disconnect()
    {
        wantReconnect = false; // 停止重連循環

        if (client != null)
        {
            client.MqttMsgPublishReceived -= OnMessageReceived;

            if (client.IsConnected)
            {
                client.Disconnect();
                Debug.Log("MQTT 已斷線");
            }
            client = null;
        }

        isConnecting = false;
        connectLoopRunning = false;
    }

    /// <summary>
    /// 非阻塞連線循環（協程）
    /// </summary>
    private System.Collections.IEnumerator ConnectLoop()
    {
        // （選用）設定逾時，如果你的 M2Mqtt 版本支援 MqttSettings：
        // try { uPLibrary.Networking.M2Mqtt.MqttSettings.Instance.TimeoutOnConnection = connectTimeoutMs; } catch {}

        while (wantReconnect)
        {
            // 未連線才嘗試
            if (client == null || !client.IsConnected)
            {
                if (!isConnecting)
                {
                    isConnecting = true;

                    try
                    {
                        client = new MqttClient(brokerIP, brokerPort, false, null, null, MqttSslProtocols.None);
                        client.MqttMsgPublishReceived += OnMessageReceived;
                        // 若版本支援可監聽中斷事件：client.ConnectionClosed += OnConnectionClosed;

                        var clientId = Guid.NewGuid().ToString();
                        client.Connect(clientId); // 同步呼叫，但在協程中，失敗就待會再試

                        if (client.IsConnected)
                        {
                            Debug.Log($"MQTT 已連線：{brokerIP}:{brokerPort}");
                            client.Subscribe(new[] { subscribeTopic }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                            Debug.Log($"已訂閱主題：{subscribeTopic}");

                            // 若不需要持續重連（只連一次），可以把 wantReconnect 設為 false
                            if (!autoReconnect)
                            {
                                wantReconnect = false;
                                connectLoopRunning = false;
                                isConnecting = false;
                                yield break;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("MQTT 連線失敗（Connect 返回未連線）。");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("MQTT 連線例外：" + ex.Message);
                    }
                    finally
                    {
                        isConnecting = false;
                    }
                }

                // 失敗或未連線 → 等待再試
                yield return new WaitForSeconds(reconnectDelaySeconds);
            }
            else
            {
                // 已連線 → 等待 1 秒後再次檢查（避免 tight loop）
                yield return new WaitForSeconds(1f);
            }
        }

        connectLoopRunning = false; // 循環結束
    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        var msg = Encoding.UTF8.GetString(e.Message);
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(() =>
            {
                latestMessage = msg;
                Debug.Log($"收到訊息 [{e.Topic}]: {latestMessage}");
                OnMessageUpdated?.Invoke(latestMessage);
            });
        }
    }

    void Update()
    {
        // 主執行緒動作
        while (true)
        {
            Action act = null;
            lock (mainThreadActions)
            {
                if (mainThreadActions.Count > 0)
                    act = mainThreadActions.Dequeue();
            }
            if (act == null) break;
            act();
        }

        // Inspector 勾選一次性觸發發送
        if (sendMessage)
        {
            PublishMessage(messageToSend, publishTopic);
            sendMessage = false;
        }

        // 測試按鍵（P）
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            PublishMessage(messageToSend, publishTopic);
        }
    }

    public void PublishMessage(string msg, string topic)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            Debug.Log($"發送訊息 [{topic}]: {msg}");
        }
        else
        {
            Debug.LogWarning("尚未連線，無法發送訊息！");
        }
    }

    public string GetLatestMessage() => latestMessage;

    private void OnDestroy()
    {
        Disconnect(); // 統一走斷線邏輯
    }
}

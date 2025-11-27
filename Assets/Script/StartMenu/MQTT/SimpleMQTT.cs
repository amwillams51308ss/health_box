
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;


public class SimpleMQTTUnity : MonoBehaviour
{
    [Header("MQTT Broker 設定")]
    [SerializeField] private string brokerIP = "192.168.1.55";
    [SerializeField] private int brokerPort = 1883;
    [SerializeField] private string subscribeTopic = "test/humi";

    [Header("發送訊息")]
    [SerializeField] private string publishTopic = "M2MQTT_Unity/test";
    [SerializeField] private string messageToSend = "Hello from Unity!";
    [SerializeField] private bool sendMessage = false;

    [Header("連線控制（按鈕觸發）")]
    [SerializeField] private bool autoReconnect = true;        // 是否在斷線後自動重連
    [SerializeField] private float reconnectDelaySeconds = 3f; // 重連間隔
    [SerializeField] private int connectTimeoutMs = 3000;      // 逾時（若你的 M2Mqtt 版本支援設定）
    [SerializeField] private bool dontDestroyOnLoad = true;    // 是否跨場景保留

    [Header("連線失敗 UI")]
    [SerializeField] private GameObject connectErrorPanel; // 指到一個 Panel
    [SerializeField] private TMP_Text connectErrorText;        // Panel 裡面的 Text


    public bool IsConnected => client != null && client.IsConnected;//現在有沒有連上
    public bool IsConnecting => isConnecting;//正在連線

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




    // --- 確定ERROR panel 不顯示 --- //
    void Start()
    {
        if (connectErrorPanel != null)
            connectErrorPanel.SetActive(false);
    }

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
        while (wantReconnect)
        {
            // 只有未連線時才嘗試
            if (client == null || !client.IsConnected)
            {
                if (!isConnecting)
                {
                    isConnecting = true;

                    bool success = false;
                    Exception error = null;

                    // 在背景 Thread 執行 Connect，不要卡住主執行緒
                    var connectTask = Task.Run(() =>
                    {
                        try
                        {
                            var c = new MqttClient(brokerIP, brokerPort, false, null, null, MqttSslProtocols.None);
                            c.MqttMsgPublishReceived += OnMessageReceived;

                            var clientId = Guid.NewGuid().ToString();
                            c.Connect(clientId);   // 在背景 Thread 裡阻塞沒關係

                            if (c.IsConnected)
                            {
                                client = c;
                                success = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                        }
                    });

                    // 等候連線完成或逾時（不阻塞 Unity）
                    float elapsed = 0f;
                    float timeoutSeconds = connectTimeoutMs / 1000f;

                    while (!connectTask.IsCompleted && elapsed < timeoutSeconds)
                    {
                        elapsed += Time.deltaTime;
                        yield return null; // 讓 Unity 繼續跑
                    }

                    // 還沒完成 → 當作逾時
                    if (!connectTask.IsCompleted)
                    {
                        ShowConnectError($"MQTT 連線逾時：{brokerIP}:{brokerPort}");
                        wantReconnect = false;   // 不再重試（或你可以保留要不要）
                    }
                    else if (!success)
                    {
                        string msg = error != null ? error.Message : "未知原因";
                        ShowConnectError($"MQTT 連線失敗：{msg}");
                        if (!autoReconnect)
                            wantReconnect = false;
                    }
                    else
                    {
                        Debug.Log($"MQTT 已連線：{brokerIP}:{brokerPort}");
                        client.Subscribe(
                            new[] { subscribeTopic },
                            new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                        Debug.Log($"已訂閱主題：{subscribeTopic}");

                        if (!autoReconnect)
                        {
                            wantReconnect = false;
                            connectLoopRunning = false;
                            isConnecting = false;
                            yield break;
                        }
                    }

                    isConnecting = false;
                }

                // 等待一下再重試
                if (wantReconnect)
                    yield return new WaitForSeconds(reconnectDelaySeconds);
            }
            else
            {
                // 已連線就只是不時檢查一下
                yield return new WaitForSeconds(1f);
            }
        }

        connectLoopRunning = false;
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

    //呼叫 ERROR PANEL
    private void ShowConnectError(string msg)
    {
        Debug.LogError(msg);

        //  遇到錯誤就停掉重連狀態
        wantReconnect = false;
        connectLoopRunning = false;
        isConnecting = false;

        if (connectErrorText != null)
            connectErrorText.text = msg;

        if (connectErrorPanel != null)
            connectErrorPanel.SetActive(true);
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

using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

public class MQTTtest : M2MqttUnityClient
{
    // 這裡設定 MQTT 伺服器的 IP 或網址
    public string broker = "192.168.38.5";
    public int port = 1883;
    public string topic = "M2MQTT_Unity/test";

    protected override void Start()
    {
        // 把你自訂的 broker/port 設給父類別
        this.brokerAddress = broker;
        this.brokerPort = port;
        this.isEncrypted = false;  // MQTT over TCP，不加密

        base.Start();   // 初始化 M2MqttUnityClient
        Connect();      // 實際建立連線
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        Debug.Log("已訂閱主題：" + topic);
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log($"收到訊息 [{topic}]：{msg}");
    }

    public void TestPublish()
    {
        if (client != null && client.IsConnected)
        {
            string msg = "Hello from Unity " + DateTime.Now.ToString("HH:mm:ss");
            client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            Debug.Log("已發佈訊息：" + msg);
        }
        else
        {
            Debug.LogWarning("尚未連線，無法發佈！");
        }
    }

    private void Update()
    {
        base.Update(); // 處理 MQTT 事件

        // 按下 P 鍵發送訊息
        if (Input.GetKeyDown(KeyCode.P))
        {
            TestPublish();
        }
    }

    protected override void OnConnected()
    {
        Debug.Log("已成功連線到 MQTT broker: " + brokerAddress);
    }

    protected override void OnDisconnected()
    {
        Debug.Log("已斷線");
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}

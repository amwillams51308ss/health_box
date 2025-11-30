using UnityEngine;
using TMPro;

public class ConnectButtonController : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;

    public void OnClickConnect()
    {
        // 拿目前活著的 MQTT
        var mqtt = SimpleMQTTUnity.Instance 
                   ?? FindAnyObjectByType<SimpleMQTTUnity>();

        if (mqtt == null)
        {
            Debug.LogError("ConnectButtonController 找不到 SimpleMQTTUnity，無法連線。");
            return;
        }

        // ★★★ 在這裡「直接」用欄位內容更新 IP / Port
        if (ipField != null)
        {
            mqtt.SetBrokerIP(ipField.text);
        }

        if (portField != null)
        {
            mqtt.SetBrokerPort(portField.text);
        }

        // 再開始連線，一定會用到剛剛最新塞進去的 IP/Port
        mqtt.StartConnect();
    }
}

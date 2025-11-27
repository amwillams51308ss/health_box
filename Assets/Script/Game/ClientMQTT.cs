
using UnityEngine;
using TMPro;

public class MQTTMessageUI : MonoBehaviour
{
    [SerializeField] private SimpleMQTTUnity mqtt;        // ← 指向常駐的 MQTT_test
    [SerializeField] private TextMeshProUGUI messageText; // ← 指向 Text (TMP)
    [SerializeField] private string emptyPlaceholder = "(尚無訊息)";

    void OnEnable()
    {
        // 若尚未手動指定，嘗試在場景中找常駐的 SimpleMQTTUnity
        if (mqtt == null)
            mqtt = FindObjectOfType<SimpleMQTTUnity>();    // ★ 自動尋找常駐實例

        if (mqtt == null || messageText == null)
        {
            Debug.LogError("MQTTMessageUI：mqtt 或 messageText 未指定！");
            return;
        }

        mqtt.OnMessageUpdated += OnMessageUpdated;
        OnMessageUpdated(mqtt.LatestMessage); // 立即同步一次
    }

    void OnDisable()
    {
        if (mqtt != null) mqtt.OnMessageUpdated -= OnMessageUpdated;
    }

    private void OnMessageUpdated(string msg)
    {
        messageText.text = string.IsNullOrEmpty(msg) ? emptyPlaceholder : msg;
    }
}

using UnityEngine;
using TMPro;

public class MQTTSettingsBinder : MonoBehaviour
{
    [Header("Refs")]
    // 不要再 SerializeField，避免在 Inspector 綁到會被 Destroy 的那顆
    private SimpleMQTTUnity mqtt;

    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;

    void Awake()
    {
        // 優先從 Singleton 拿，目前活著的那顆 MQTT_test
        mqtt = SimpleMQTTUnity.Instance 
               ?? FindAnyObjectByType<SimpleMQTTUnity>();

        if (mqtt == null)
        {
            Debug.LogError("MQTTSettingsBinder 找不到 SimpleMQTTUnity。請確認場景中有 MQTT_test，且它在 Awake 裡有設定 Instance。");
            return;
        }

        // 初始顯示目前設定值
        if (ipField != null)  ipField.text  = mqtt.GetBrokerIP();
        if (portField != null) portField.text = mqtt.GetBrokerPort().ToString();

        // 綁定輸入事件（建議用 onEndEdit，避免組字階段一直觸發）
        if (ipField != null)  ipField.onEndEdit.AddListener(mqtt.SetBrokerIP);
        if (portField != null) portField.onEndEdit.AddListener(mqtt.SetBrokerPort);
    }

    void OnDestroy()
    {
        // 解除事件（避免殘留 Listener）
        if (mqtt != null)
        {
            if (ipField != null)  ipField.onEndEdit.RemoveListener(mqtt.SetBrokerIP);
            if (portField != null) portField.onEndEdit.RemoveListener(mqtt.SetBrokerPort);
        }
    }
}

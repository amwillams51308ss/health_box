
using UnityEngine;
using TMPro;

public class MQTTSettingsBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SimpleMQTTUnity mqtt;
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;

    void Awake()
    {
        // 如果沒在 Inspector 指派，嘗試尋找場景中的 SimpleMQTTUnity
        if (mqtt == null)
        {
            // 方式 A：傳統
            mqtt = UnityEngine.Object.FindObjectOfType<SimpleMQTTUnity>();

            // 或 方式 B（Unity 2023+）：更明確的 API
            // mqtt = UnityEngine.Object.FindFirstObjectByType<SimpleMQTTUnity>();
            // mqtt = UnityEngine.Object.FindFirstObjectByType<SimpleMQTTUnity>(FindObjectsInactive.Include);
        }

        if (mqtt == null)
        {
            Debug.LogError("找不到 SimpleMQTTUnity。請確認場景中存在該物件，或在 Inspector 指派。");
            return;
        }

        // 初始顯示目前設定值
        if (ipField != null)   ipField.text = mqtt.GetBrokerIP();
        if (portField != null) portField.text = mqtt.GetBrokerPort().ToString();

        // 綁定輸入事件（建議用 onEndEdit，避免組字中不斷觸發）
        if (ipField != null)   ipField.onEndEdit.AddListener(mqtt.SetBrokerIP);
        if (portField != null) portField.onEndEdit.AddListener(mqtt.SetBrokerPort);
    }

    void OnDestroy()
    {
        // 解除事件（避免記憶體/殘留引用）
        if (mqtt != null)
        {
            if (ipField != null)   ipField.onEndEdit.RemoveListener(mqtt.SetBrokerIP);
            if (portField != null) portField.onEndEdit.RemoveListener(mqtt.SetBrokerPort);
        }
    }
}

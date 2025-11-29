using UnityEngine;
using TMPro;

public class TopicCardUI : MonoBehaviour
{
    public TMP_Text topicText;
    public TMP_Text valueText;

    private SimpleMQTTUnity mqtt;
    private string topic;

    public void Setup(SimpleMQTTUnity mqtt, string topic)
    {
        this.mqtt = mqtt;
        this.topic = topic;

        if (topicText != null)
            topicText.text = topic;
    }

    void Update()
    {
        if (mqtt == null || string.IsNullOrEmpty(topic)) return;

        var msg = mqtt.GetLatestMessage(topic);
        if (valueText != null)
            valueText.text = string.IsNullOrEmpty(msg) ? "(尚未收到資料)" : msg;
    }
}

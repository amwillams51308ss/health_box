using UnityEngine;
using TMPro;

public class TopicListUI : MonoBehaviour
{
    [Header("Reference")]
    public SimpleMQTTUnity mqtt;
    public TMP_InputField topicInput;
    public Transform listRoot;        // ScrollView 裡 Content 的 Transform
    public GameObject topicItemPrefab; // 一個 topic item 的 prefab

    private void Start()
    {
        // 如果你有預設已訂閱的 topics，可以一開始就生成
        foreach (var t in mqtt.SubscribedTopics)
        {
            CreateItem(t);
        }
    }

    public void OnClickAddTopic()
    {
        var topic = topicInput.text;
        if (string.IsNullOrWhiteSpace(topic))
            return;

        topic = topic.Trim();

        // 先請 MQTT 端訂閱
        mqtt.SubscribeTopic(topic);

        // 再在 UI 上加一條
        CreateItem(topic);

        topicInput.text = "";
    }

    private void CreateItem(string topic)
    {
        var go = Instantiate(topicItemPrefab, listRoot);
        var item = go.GetComponent<TopicItemUI>();
        item.Setup(topic, this);
    }

    // 給 TopicItemUI 呼叫：從 UI 移除 + 取消訂閱
    public void RemoveTopic(string topic, TopicItemUI item)
    {
        mqtt.UnsubscribeTopic(topic);
        Destroy(item.gameObject);
    }
}

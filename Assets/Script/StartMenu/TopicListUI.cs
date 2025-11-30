using UnityEngine;
using TMPro;

public class TopicListUI : MonoBehaviour
{
    [Header("Reference")]
    private SimpleMQTTUnity mqtt;          // 不再用 Inspector 綁，改用 Singleton 取得
    [SerializeField] private TMP_InputField topicInput;
    [SerializeField] private Transform listRoot;        // ScrollView 裡 Content 的 Transform
    [SerializeField] private GameObject topicItemPrefab; // 一個 topic item 的 prefab

    private void Awake()
    {
        // 優先用 Singleton 取得目前活著的那顆 MQTT
        mqtt = SimpleMQTTUnity.Instance
               ?? FindAnyObjectByType<SimpleMQTTUnity>();

        if (mqtt == null)
        {
            Debug.LogError("TopicListUI 找不到 SimpleMQTTUnity，請確認場景中有 MQTT_test，且它在 Awake 裡有設定 Instance。");
        }
    }

    private void Start()
    {
        if (mqtt == null)
            return;

        // 如果你有預設已訂閱的 topics，一開始就生成
        foreach (var t in mqtt.SubscribedTopics)
        {
            CreateItem(t);
        }
    }

    public void OnClickAddTopic()
    {
        if (mqtt == null)
        {
            Debug.LogError("OnClickAddTopic 時找不到 SimpleMQTTUnity，無法新增訂閱主題。");
            return;
        }

        var topic = topicInput.text.Trim();
        if (string.IsNullOrEmpty(topic))
            return;

        // ✅ 只有 SubscribeTopic 回傳 true 時才生成 list item
        if (mqtt.SubscribeTopic(topic))
        {
            CreateItem(topic);
            topicInput.text = "";
        }
        else
        {
            Debug.LogWarning("Topic 訂閱未成功，不加入列表。");
            // 這裡也可以加一個小錯誤文字 UI 提示玩家
        }
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
        if (mqtt == null)
        {
            Debug.LogError("RemoveTopic 時找不到 SimpleMQTTUnity。");
            Destroy(item.gameObject);
            return;
        }

        mqtt.UnsubscribeTopic(topic);
        Destroy(item.gameObject);
    }
}

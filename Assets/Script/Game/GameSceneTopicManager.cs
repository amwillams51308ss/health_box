using UnityEngine;

public class GameSceneTopicManager : MonoBehaviour
{
    public Transform cardsRoot;        // 放卡片的父物件
    public GameObject topicCardPrefab; // 剛剛做的 TopicCard 預置物

    private SimpleMQTTUnity mqtt;

    void Start()
    {
        // 從 DontDestroyOnLoad 找出 MQTT 物件
        mqtt = FindObjectOfType<SimpleMQTTUnity>();
        if (mqtt == null)
        {
            Debug.LogError("GameSceneTopicManager 找不到 SimpleMQTTUnity，確認 MQTT_test 有 DontDestroyOnLoad。");
            return;
        }

        // 針對每個已訂閱 topic 生成一張卡片
        foreach (var topic in mqtt.SubscribedTopics)
        {
            CreateCardForTopic(topic);
        }
    }

    private void CreateCardForTopic(string topic)
    {
        var go = Instantiate(topicCardPrefab, cardsRoot);
        var card = go.GetComponent<TopicCardUI>();
        card.Setup(mqtt, topic);
    }
}

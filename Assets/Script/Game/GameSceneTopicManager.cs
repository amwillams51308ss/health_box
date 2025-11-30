using UnityEngine;

public class GameSceneTopicManager : MonoBehaviour
{
    public Transform cardsRoot;        // 放卡片的父物件
    public GameObject topicCardPrefab; // 剛剛做的 TopicCard 預置物

    private SimpleMQTTUnity mqtt;

    void Start()
    {
        // 從 DontDestroyOnLoad 找 SimpleMQTTUnity（新版 API）
        mqtt = FindAnyObjectByType<SimpleMQTTUnity>();

        if (mqtt == null)
        {
            Debug.LogError("找不到 SimpleMQTTUnity！請確認 MQTT_test 有 DontDestroyOnLoad。");
            return;
        }

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

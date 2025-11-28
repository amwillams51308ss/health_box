using UnityEngine;
using TMPro;

public class TopicItemUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text label;

    private string topic;
    private TopicListUI list;

    public void Setup(string topic, TopicListUI list)
    {
        this.topic = topic;
        this.list = list;
        label.text = topic;
    }

    // 給按鈕 OnClick 用
    public void OnClickRemove()
    {
        list.RemoveTopic(topic, this);
    }
}

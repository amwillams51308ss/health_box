using System.Collections;
using UnityEngine;

public class StartButtonController : MonoBehaviour
{
    [Header("Reference")]
    public SimpleMQTTUnity mqtt;        // 拖 MQTT_test
    public ScenneManager sceneManager;  // 拖掛 ScenneManager 的物件

    [Header("Connect Settings")]
    public float maxWaitSeconds = 5f;   // 最多等多久 MQTT 連線

    public void OnClickStart()
    {
        StartCoroutine(StartFlow());
    }

    private IEnumerator StartFlow()
    {
        // 開始嘗試連線（但不會阻塞主執行緒）
        mqtt.StartConnect();

        float t = 0f;

        // 等待 MQTT 嘗試完成（成功或失敗）
        while (t < maxWaitSeconds && mqtt.IsConnecting && !mqtt.IsConnected)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 成功 → 換場景
        if (mqtt.IsConnected)
        {
            sceneManager.ToGame();
        }
        else
        {
            // 失敗 → 不切場景，SimpleMQTTUnity 會自己跳錯誤 UI
            Debug.LogWarning("StartButton: MQTT 未連線，不切場景。");
        }
    }
}

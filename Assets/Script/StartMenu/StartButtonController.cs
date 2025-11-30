using UnityEngine;
using System.Collections;

public class StartButtonController : MonoBehaviour
{
    private SimpleMQTTUnity mqtt;
    public ScenneManager sceneManager;
    public float maxWaitSeconds = 5f;

    private void Awake()
    {
        // 從 Singleton 取得實例
        mqtt = SimpleMQTTUnity.Instance;
    }

    public void OnClickStart()
    {
        // 保險一下：如果 Awake 時還沒準備好，就再試著找一次
        if (mqtt == null)
        {
            mqtt = SimpleMQTTUnity.Instance 
                   ?? FindAnyObjectByType<SimpleMQTTUnity>();

            if (mqtt == null)
            {
                Debug.LogError("StartButtonController 找不到 SimpleMQTTUnity，無法開始連線。");
                return;
            }
        }

        StartCoroutine(StartFlow());
    }

    private IEnumerator StartFlow()
    {
        mqtt.StartConnect();

        float t = 0f;
        while (t < maxWaitSeconds && mqtt != null && mqtt.IsConnecting && !mqtt.IsConnected)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (mqtt != null && mqtt.IsConnected)
        {
            sceneManager.ToGame();
        }
        else
        {
            Debug.LogWarning("Start: MQTT 未連線，不切場景。");
        }
    }
}

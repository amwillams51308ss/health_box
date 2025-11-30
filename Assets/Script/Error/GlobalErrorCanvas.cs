using UnityEngine;

public class GlobalErrorCanvas : MonoBehaviour
{
    private void Awake()
    {
        // 找到所有 GlobalErrorCanvas，但不排序（最快）
        var objs = FindObjectsByType<GlobalErrorCanvas>(FindObjectsSortMode.None);

        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}

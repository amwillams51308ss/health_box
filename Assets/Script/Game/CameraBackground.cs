using UnityEngine;
using UnityEngine.UI;

public class CameraBackground : MonoBehaviour
{
    [SerializeField] private RawImage targetImage;

    private WebCamTexture webCamTexture;

    private void Start()
    {
        if (targetImage == null)
            targetImage = GetComponent<RawImage>();

        // 找後鏡頭（或任何一顆可用的相機）
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("找不到任何攝影機");
            return;
        }

        string selectedName = devices[0].name;
        // 嘗試選背面相機（在手機上）
        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                selectedName = devices[i].name;
                break;
            }
        }

        webCamTexture = new WebCamTexture(selectedName, Screen.width, Screen.height, 30);
        targetImage.texture = webCamTexture;
        targetImage.material.mainTexture = webCamTexture;

        webCamTexture.Play();
    }

    private void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}

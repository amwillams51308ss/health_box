using UnityEngine;
using TMPro;
using UnityEngine.UI;   // ★ 記得加這行

public class ToggleDragModeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;      // 按鈕上的文字（可選）
    [SerializeField] private Image background;    // 按鈕的底圖（Image）
    [SerializeField] private Color normalColor = Color.white;   // 關閉拖曳時的顏色
    [SerializeField] private Color activeColor = Color.green;   // 開啟拖曳時的顏色

    public void OnClickToggleDrag()
    {
        // 切換全域拖曳開關
        DraggableUI.DragEnabled = !DraggableUI.DragEnabled;

        Debug.Log("DragEnabled = " + DraggableUI.DragEnabled);

        // 改按鈕文字
        if (label != null)
        {
            label.text = DraggableUI.DragEnabled ? "結束編輯" : "編輯位置";
        }

        // 改按鈕顏色
        if (background != null)
        {
            background.color = DraggableUI.DragEnabled ? activeColor : normalColor;
        }
    }

    //（可選）一進場就把顏色跟文字同步一次
    private void Start()
    {
        if (label != null)
            label.text = DraggableUI.DragEnabled ? "結束編輯" : "編輯位置";

        if (background != null)
            background.color = DraggableUI.DragEnabled ? activeColor : normalColor;
    }
}

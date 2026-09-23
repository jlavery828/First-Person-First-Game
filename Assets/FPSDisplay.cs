using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float refreshInterval = 0.25f;
    [SerializeField, Min(12)] private int fontSize = 50;
    [SerializeField] private Vector2 padding = new Vector2(20f, 20f);

    private int frameCount;
    private float elapsedTime;
    private string label = "FPS: --";
    private GUIStyle labelStyle;

    private void OnEnable()
    {
        frameCount = 0;
        elapsedTime = 0f;
        label = "FPS: --";
    }

    private void Update()
    {
        frameCount++;
        // Keep measuring rendered frames even when gameplay is paused.
        elapsedTime += Time.unscaledDeltaTime;
        if (elapsedTime < Mathf.Max(0.1f, refreshInterval))
            return;

        label = "FPS: " + Mathf.RoundToInt(frameCount / elapsedTime);
        frameCount = 0;
        elapsedTime = 0f;
    }

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.padding = new RectOffset(8, 8, 4, 4);
        }

        labelStyle.fontSize = fontSize;
        Vector2 size = labelStyle.CalcSize(new GUIContent(label));
        Rect position = new Rect(padding.x, padding.y, size.x, size.y);

        Color previousColor = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(position, Texture2D.whiteTexture);
        GUI.color = Color.white;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(position, label, labelStyle);
        GUI.color = previousColor;
    }
}

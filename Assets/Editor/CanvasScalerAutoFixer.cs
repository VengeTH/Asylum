using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CanvasScalerAutoFixer : EditorWindow
{
    [MenuItem("Tools/Auto Fix All CanvasScalers")]
    public static void FixAllCanvasScalers()
    {
        var scalers = Resources.FindObjectsOfTypeAll<CanvasScaler>();
        int fixedCount = 0;
        foreach (var scaler in scalers)
        {
            Undo.RecordObject(scaler, "Auto Fix CanvasScaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(scaler);
            fixedCount++;
        }
        Debug.Log($"Auto-fixed {fixedCount} CanvasScaler components.");
    }
}
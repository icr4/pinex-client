using Unity.VisualScripting;
using UnityEngine;
 
public static class RectTransformExtensions
{
    public static bool IsVerticallyRendered(this RectTransform rectTransform)
    {
        Vector3[] v = new Vector3[4];
        rectTransform.GetComponent<RectTransform>().GetWorldCorners(v);

        float maxY = Mathf.Max(v[0].y, v[1].y, v[2].y, v[3].y);
        float minY = Mathf.Min(v[0].y, v[1].y, v[2].y, v[3].y);

        return !(maxY < 0 || minY > Screen.height);
    }

    public static bool IsHorizontallyRendered(this RectTransform rectTransform)
    {
        Vector3[] v = new Vector3[4];
        rectTransform.GetComponent<RectTransform>().GetWorldCorners(v);

        float maxX = Mathf.Max (v [0].x, v [1].x, v [2].x, v [3].x);
        float minX = Mathf.Min (v [0].x, v [1].x, v [2].x, v [3].x);
        
        return !(minX < 0 || maxX > Screen.height);
    }

    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    public static void SetRectPos(this RectTransform rt, float val) {
        rt.SetTop(val);
        rt.SetBottom(val);
        rt.SetLeft(val);
        rt.SetRight(val);
    }
}
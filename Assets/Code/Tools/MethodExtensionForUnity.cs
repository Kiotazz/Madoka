using UnityEngine;

//Unity的一些扩展方法
static public class MethodExtensionForUnity
{
    static public T GetOrAddComponent<T>(this Component com, bool set_enable = false) where T : Component
    {
        T result = com.GetComponent<T>();
        if (result == null) result = com.gameObject.AddComponent<T>();
        if (set_enable && result is Behaviour) (result as Behaviour).enabled = true;
        return result;
    }

    static public T GetOrAddComponent<T>(this GameObject go, bool set_enable = false) where T : Component
    {
        T result = go.GetComponent<T>();
        if (result == null) result = go.AddComponent<T>();
        if (set_enable && result is Behaviour) (result as Behaviour).enabled = true;
        return result;
    }

    public static void CallRecursively(this Transform tsf, System.Action<Transform> function)
    {
        function(tsf);
        for (int i = 0, length = tsf.childCount; i < length; ++i)
            CallRecursively(tsf.GetChild(i), function);
    }

    public static void SetLayer(this GameObject obj, int layer) { obj.layer = layer; }
    public static void SetLayer(this GameObject obj, int layer, bool includeChild)
    {
        obj.layer = layer;
        if (includeChild)
            obj.transform.SetChildLayer(layer);
    }

    public static void SetChildLayer(this Transform t, int layer)
    {
        for (int i = 0, length = t.childCount; i < length; ++i)
        {
            Transform child = t.GetChild(i);
            child.gameObject.layer = layer;
            child.SetChildLayer(layer);
        }
    }

    public static float SqrDistanceWith(this Vector2 vec, Vector2 tar)
    {
        return (vec - tar).sqrMagnitude;
    }

    public static float SqrDistanceWith(this Vector3 vec, Vector3 tar)
    {
        return (vec - tar).sqrMagnitude;
    }

    public static Vector2 XZ(this Vector3 p)
    {
        return new Vector2(p.x, p.z);
    }

    public static Vector2 DirectionTo(this Transform a, Transform b)
    {
        return a.position.XZ() - b.position.XZ();
    }
}

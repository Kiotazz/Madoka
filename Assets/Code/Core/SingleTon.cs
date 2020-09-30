using UnityEngine;

public class SingleTon<T> where T : new()
{
    public static implicit operator bool(SingleTon<T> myself) { return myself != null; }
    private static object objLocker = new object();
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance != null)
            {
                lock (objLocker)
                {
                    _instance = new T();
                    System.Reflection.MethodInfo initMethod = _instance.GetType().GetMethod("Init");
                    if (initMethod == null)
                        UnityEngine.Debug.LogError(_instance.GetType().FullName + " doesn't contains method: Init!");
                    else
                        initMethod.Invoke(null, null);
                }
            }
            return _instance;
        }
    }
}

public class SingletonM<T> where T : MonoBehaviour
{
    protected static string objName = "Singleton";
    private static GameObject singletonObj;

    protected static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                singletonObj = GameObject.Find(objName);
                if (singletonObj == null)
                    singletonObj = new GameObject(objName);

                _instance = singletonObj.GetComponent<T>();
                if (_instance == null)
                    _instance = singletonObj.AddComponent<T>();
            }
            return _instance;
        }
    }
}

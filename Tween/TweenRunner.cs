using UnityEngine;

public class TweenRunner : MonoBehaviour
{
    private static TweenRunner _instance;

    public static TweenRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("TweenRunner");
                _instance = go.AddComponent<TweenRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}
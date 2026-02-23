using System.Collections.Generic;
using UnityEngine;

public class TweenEngine : MonoBehaviour
{
    private static readonly List<TweenInstance> active = new();
    private static readonly Dictionary<object, TweenInstance> lookup = new();

    public static void Add(TweenInstance instance)
    {
        if (lookup.TryGetValue(instance.Target, out var existing))
        {
            active.Remove(existing);
        }

        lookup[instance.Target] = instance;
        active.Add(instance);
    }

    void Update()
    {
        float delta = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var tween = active[i];

            if (tween.Update(delta))
            {
                lookup.Remove(tween.Target);
                active.RemoveAt(i);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tween
{
    private static readonly Dictionary<object, Coroutine> activeTweens
        = new Dictionary<object, Coroutine>();

    private static void StartTween(object key, IEnumerator routine)
    {
        if (activeTweens.TryGetValue(key, out var existing))
        {
            TweenRunner.Instance.StopCoroutine(existing);
        }

        var coroutine = TweenRunner.Instance.StartCoroutine(routine);
        activeTweens[key] = coroutine;
    }

    private static void ClearTween(object key)
    {
        activeTweens.Remove(key);
    }

    // ----------------------------
    // MOVE
    // ----------------------------

    public static void Move(RectTransform target, Vector2 end, float duration)
    {
        StartTween(target, MoveRoutine(target, end, duration));
    }

    private static IEnumerator MoveRoutine(RectTransform target, Vector2 end, float duration)
    {
        Vector2 start = target.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            target.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        target.anchoredPosition = end;
        ClearTween(target);
    }

    // ----------------------------
    // SLIDE (position + alpha)
    // ----------------------------

    public static void Slide(
        RectTransform panel,
        CanvasGroup group,
        Vector2 targetPos,
        float targetAlpha,
        float duration)
    {
        StartTween(panel, SlideRoutine(panel, group, targetPos, targetAlpha, duration));
    }

    private static IEnumerator SlideRoutine(
        RectTransform panel,
        CanvasGroup group,
        Vector2 targetPos,
        float targetAlpha,
        float duration)
    {
        Vector2 startPos = panel.anchoredPosition;
        float startAlpha = group.alpha;

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        panel.anchoredPosition = targetPos;
        group.alpha = targetAlpha;

        ClearTween(panel);
    }
}

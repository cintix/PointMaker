using UnityEngine;

public class TweenInstance
{
    public object Target;
    public Vector3 Start;
    public Vector3 End;
    public float Duration;

    private float time;
    private System.Action<Vector3> apply;

    public TweenInstance(
        object target,
        Vector3 start,
        Vector3 end,
        float duration,
        System.Action<Vector3> apply)
    {
        Target = target;
        Start = start;
        End = end;
        Duration = duration;
        this.apply = apply;
        time = 0f;
    }

    public bool Update(float deltaTime)
    {
        time += deltaTime;

        float t = Mathf.Clamp01(time / Duration);
        t = Mathf.SmoothStep(0f, 1f, t);

        Vector3 value = Vector3.Lerp(Start, End, t);
        apply(value);

        return t >= 1f;
    }
}
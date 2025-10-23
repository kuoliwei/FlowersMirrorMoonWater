using UnityEngine;
public class HandSmoother
{
    private Vector2 lastSmoothed;
    private bool hasValue = false;

    private readonly float smoothFactor;
    private readonly float minThreshold;

    public HandSmoother(float smoothFactor = 0.2f, float minThreshold = 0.002f)
    {
        this.smoothFactor = smoothFactor;
        this.minThreshold = minThreshold;
    }

    public Vector2 Smooth(Vector2 current)
    {
        if (!hasValue)
        {
            lastSmoothed = current;
            hasValue = true;
            return current;
        }

        // �p�G�ܰʤӤp�A�����ݰ�
        if (Vector2.Distance(lastSmoothed, current) < minThreshold)
            return lastSmoothed;

        // ���ƥ��� (EMA)
        lastSmoothed = Vector2.Lerp(lastSmoothed, current, smoothFactor);
        return lastSmoothed;
    }
}

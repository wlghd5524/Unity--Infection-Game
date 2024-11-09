using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 코루틴 최적화를 위한 Yield 캐싱
public class YieldInstructionCache : MonoBehaviour
{
    private class FloatComparer : IEqualityComparer<float>
    {
        public bool Equals(float x, float y)
        {
            return x == y;
        }

        public int GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    private static readonly Dictionary<float, WaitForSeconds> _dicSeconds = new Dictionary<float, WaitForSeconds>(new FloatComparer());
    private static readonly Dictionary<float, WaitForSecondsRealtime> _dicSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer());

    public static WaitForSeconds WaitForSeconds(float fSeconds)
    {
        if (!_dicSeconds.TryGetValue(fSeconds, out WaitForSeconds waitForSeconds))
            _dicSeconds.Add(fSeconds, waitForSeconds = new WaitForSeconds(fSeconds));
        return waitForSeconds;
    }

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float fSeconds)
    {
        if (!_dicSecondsRealtime.TryGetValue(fSeconds, out WaitForSecondsRealtime waitForSecondsRealtime))
            _dicSecondsRealtime.Add(fSeconds, waitForSecondsRealtime = new WaitForSecondsRealtime(fSeconds));
        return waitForSecondsRealtime;
    }

    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    public static CooldownManager Instance { get; private set; }

    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();
    private Dictionary<string, Coroutine> cooldownCoroutines = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartCooldown(string levelName, float duration, System.Action onComplete)
    {
        if (cooldownCoroutines.ContainsKey(levelName) && cooldownCoroutines[levelName] != null)
        {
            StopCoroutine(cooldownCoroutines[levelName]);
        }

        cooldownTimers[levelName] = duration;
        cooldownCoroutines[levelName] = StartCoroutine(CooldownCoroutine(levelName, onComplete));
    }

    public float GetCooldownTime(string levelName)
    {
        return cooldownTimers.ContainsKey(levelName) ? cooldownTimers[levelName] : 0f;
    }

    private IEnumerator CooldownCoroutine(string levelName, System.Action onComplete)
    {
        while (cooldownTimers[levelName] > 0)
        {
            cooldownTimers[levelName] -= Time.unscaledDeltaTime; // unscaledDeltaTime 사용
            yield return null;
        }

        cooldownTimers[levelName] = 0;
        onComplete?.Invoke();
        cooldownCoroutines[levelName] = null;
    }
}

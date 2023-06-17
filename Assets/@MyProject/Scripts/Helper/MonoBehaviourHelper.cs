using System.Collections;
using UnityEngine;

public static class MonoBehaviourHelper
{
    public static void Invoke(
        this UnityEngine.MonoBehaviour _this,
        System.Action callback,
        float time)
    {
        _this.StartCoroutine(Invoke_Coroutine(_this, callback, time));
    }

    private static IEnumerator Invoke_Coroutine(
        this UnityEngine.MonoBehaviour _this,
        System.Action callback,
        float time)
    {
        yield return new WaitForSeconds(time);
        callback.Invoke();
    }
}
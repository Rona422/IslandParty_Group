using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoroutineRunner : SingletonMonoBehaviour<CoroutineRunner>
{
    // 実行中のコルーチンを管理するキー一覧
    private HashSet<string> m_RunningKeys = new ();
    // 制御なしでコルーチンを始める
    public Coroutine RunCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
    // 重複なしでコルーチンを始める
    public Coroutine RunOnce(string key, IEnumerator coroutine)
    {
        if (m_RunningKeys.Contains(key))
            return null;

        m_RunningKeys.Add(key);
        return StartCoroutine(RunOnceCoroutine(key, coroutine));
    }

    private IEnumerator RunOnceCoroutine(string key, IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        m_RunningKeys.Remove(key); // 完了後に再実行可能に
    }
    // 永続的に一度だけコルーチンを始める
    public void RunOnceForever(string key, IEnumerator coroutine)
    {
        if (m_RunningKeys.Contains(key))
            return;

        m_RunningKeys.Add(key);
        StartCoroutine(coroutine);
    }
    // 遅延してメソッドを実行
    public void DelayedCall(float delaySeconds, Action action)
    {
        if (action == null || delaySeconds < 0f) return;
        StartCoroutine(DelayedCallCoroutine(delaySeconds, action));
    }

    private IEnumerator DelayedCallCoroutine(float delaySeconds, Action action)
    {
        yield return new WaitForSeconds(delaySeconds);
        action?.Invoke();
    }
    // 遅延して重複なしでメソッドを実行
    public void DelayedCallOnce(string key, float delaySeconds, Action action)
    {
        if (string.IsNullOrEmpty(key) || action == null || delaySeconds < 0f)
            return;

        if (m_RunningKeys.Contains(key))
            return;

        m_RunningKeys.Add(key);
        StartCoroutine(DelayedCallOnceCoroutine(key, delaySeconds, action));
    }
    private IEnumerator DelayedCallOnceCoroutine(string key, float delaySeconds, Action action)
    {
        yield return new WaitForSeconds(delaySeconds);
        action?.Invoke();
        m_RunningKeys.Remove(key);
    }
    //汎用Lerp()
    // <○○>(値の適用関数,開始位置,目標位置,時間,補間関数(○○.Lerp))
    public IEnumerator LerpValue<T>(Action<T> setter, T start, T target, float duration, Func<T, T, float, T> lerpFunc)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            setter(lerpFunc(start, target, t));
            yield return null;
        }
        // 最終値を保証
        setter(target);
    }
    // コルーチンのキーを削除
    public void ResetKey(string key)
    {
        m_RunningKeys.Remove(key);
    }
    // キーをすべて削除
    public void ResetAll()
    {
        m_RunningKeys.Clear();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoroutineRunner : SingletonMonoBehaviour<CoroutineRunner>
{
    // ���s���̃R���[�`�����Ǘ�����L�[�ꗗ
    private HashSet<string> m_RunningKeys = new ();
    // ����Ȃ��ŃR���[�`�����n�߂�
    public Coroutine RunCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
    // �d���Ȃ��ŃR���[�`�����n�߂�
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
        m_RunningKeys.Remove(key); // ������ɍĎ��s�\��
    }
    // �i���I�Ɉ�x�����R���[�`�����n�߂�
    public void RunOnceForever(string key, IEnumerator coroutine)
    {
        if (m_RunningKeys.Contains(key))
            return;

        m_RunningKeys.Add(key);
        StartCoroutine(coroutine);
    }
    // �x�����ă��\�b�h�����s
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
    // �x�����ďd���Ȃ��Ń��\�b�h�����s
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
    //�ėpLerp()
    // <����>(�l�̓K�p�֐�,�J�n�ʒu,�ڕW�ʒu,����,��Ԋ֐�(����.Lerp))
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
        // �ŏI�l��ۏ�
        setter(target);
    }
    // �R���[�`���̃L�[���폜
    public void ResetKey(string key)
    {
        m_RunningKeys.Remove(key);
    }
    // �L�[�����ׂč폜
    public void ResetAll()
    {
        m_RunningKeys.Clear();
    }
}

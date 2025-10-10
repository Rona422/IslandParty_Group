using System.Collections;
using UnityEngine;

public class Player_DarumaOtoshi : PlayerBase
{
    [SerializeField] private DarumaManager m_DarumaManager;
    private int m_HitCount;
    public int HitCount
    {
        get
        {
            return m_HitCount;
        }
    }

    private bool m_IsFinished;
    public bool IsFinished
    {
        get
        {
            return m_IsFinished;
        }
    }

    private int m_DarumaDestroyCount;
    public int DarumaDestroyCount
    {
        get
        {
            return m_DarumaDestroyCount;
        }
    }


    private float m_NpcTimer;
    private float m_NpcInterval;

    private Quaternion m_DefaultRot;
    private bool m_IsHitting = false;


    private bool m_IsHitUntilHitting = false;   // もう一度コルーチンを走らせようとしているか
    private bool m_CoroutineRunning = false;    // コルーチンが走っているか

    void Start()
    {
        m_HitCount = 0;
        m_IsFinished = false;
        m_DarumaDestroyCount = 0;
        m_DefaultRot = transform.localRotation;  // 初期の回転を保存
        CoroutineRunner.instance.RunCoroutine(PlayerUpdate());
    }

    private int SetPlayerCount()
    {
        if(!m_IsFinished)
        {
            if(Player.Input.actions["A_Decision"].WasPressedThisFrame())
            {
                // 叩いた瞬間にアニメーション
                if (!m_IsHitting)
                {
                    if (m_CoroutineRunning)
                    {
                        m_IsHitUntilHitting = true;
                    }
                    CoroutineRunner.instance.RunCoroutine(HitMotion());
                    m_DarumaManager.HitDaruma(m_DarumaManager.GetDarumaBodyList(this), this);
                }
            
                return 1;   // 1回叩いた
            }
        }
        return 0;
    }

    private int SetNPCCount()
    {
        if(!m_IsFinished)
        {
            m_NpcTimer += Time.deltaTime;
            if (m_NpcTimer >= m_NpcInterval)
            {
                // タイマーリセット
                m_NpcTimer = 0f;
                m_NpcInterval = Random.Range(0.1f, 0.15f); // 叩く間隔をランダム
                if (!m_IsHitting)
                {
                    if(m_CoroutineRunning)
                    {
                        m_IsHitUntilHitting = true;
                    }
                    CoroutineRunner.instance.RunCoroutine(HitMotion());
                    m_DarumaManager.HitDaruma(m_DarumaManager.GetDarumaBodyList(this), this);
                }

                return 1;   // 1回叩いた
            }
        }
        return 0;
    }

    private void Count(int count)
    {
        m_HitCount += count;
    }

    // Directorでプレイヤーが入力を受け取るかを制御
    public void SetFinished(bool finished)
    {
        m_IsFinished = finished;
    }

    public void AddHit()
    {
        m_HitCount++;
    }

    public void AddDarumaDestroyed()
    {
        m_DarumaDestroyCount++;
    }

    private IEnumerator PlayerUpdate()
    {
        while(!m_IsFinished)
        {
            var count = (!Player.IsNpc) ? SetPlayerCount() : SetNPCCount();
            Count(count);
            yield return null;
        }
    }

    private IEnumerator HitMotion()
    {
        yield return null;  // ヒット中のヒット入力があったときの情報を整理するために1フレーム待機
        m_CoroutineRunning = true;
        m_IsHitting = true;

        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = m_DefaultRot * Quaternion.Euler(0f, -45f, 0f); // 左に45度ひねる

        float duration = 0.05f; // 素早さ（小さいほど速い）
        float t = 0f;

        // 左にひねる
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
            if (m_IsHitUntilHitting)
            {
                m_IsHitUntilHitting = false;
                yield break;
            }
        }

        // すぐ戻す
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localRotation = Quaternion.Slerp(targetRot, m_DefaultRot, t);
            yield return null;
            if (m_IsHitUntilHitting)
            {
                m_IsHitUntilHitting = false;
                yield break;
            }
        }

        m_IsHitting = false;
        m_CoroutineRunning = false;
    }
}

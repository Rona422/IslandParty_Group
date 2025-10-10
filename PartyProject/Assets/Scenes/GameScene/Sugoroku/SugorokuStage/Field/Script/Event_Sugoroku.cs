using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Event_Sugoroku : SingletonMonoBehaviour<Event_Sugoroku>
{
    //デバッグ用-----------------
    public bool m_Action = false;
    public EventObject m_EO;
    public EventState m_ES;
    //---------------------------
    public enum EventObject
    {
        BARRIER_ONE,
        BARRIER_TWO,
        BARRIER_THREE,
        BARRIER_LAST,
    }
    public enum EventState
    {
        START,
        LOSE,
        WIN,
    }
    private readonly Dictionary<EventObject, Dictionary<EventState, Func<IEnumerator>>> m_Actions = new();
    private Sugoroku_PlayerBase m_CurrentPlayer;
    /// 一つ目の関門で使う奴ら
    [SerializeField]
    private GameObject m_BarrierOneObject;
    [SerializeField]
    private ParticleSystem m_BarrierOneSkyObjectOpenParticle;
    [SerializeField]
    private ParticleSystem m_BarrierOneSkyObjectEndParticle;
    [SerializeField]
    private Material m_BarrierOneSkyBox;

    /// 二つ目の関門で使う奴ら
    [SerializeField]
    private GameObject m_BarrierTwoObject;
    [SerializeField]
    private GameObject m_BarrierTwoRightObject;
    [SerializeField]
    private GameObject m_BarrierTwoLeftObject;

    /// 三つ目の関門で使う奴ら
    [SerializeField]
    private GameObject m_BarrierThreeObject;
    [SerializeField]
    private ParticleSystem m_BarrierThreeObjectParticle;
    [SerializeField]
    private ParticleSystem m_BarrierThreeObjectParticleBreak;
    [SerializeField]
    private GameObject m_BarrierThreeObjectBall;
    [SerializeField]
    private GameObject m_BarrierThreeObjectBallRoll;

    /// ラストの関門で使う奴ら
    [SerializeField]
    private GameObject m_BarrierLastObject;
    [SerializeField]
    private ParticleSystem m_BarrierLastObjectParticle;
    [SerializeField]
    private Transform m_BarrierLastCrownTransform;
    private readonly Dictionary<Renderer, Material> m_MaterialCache = new();

    protected override void Awake()
    {
        base.Awake();
        m_Actions[EventObject.BARRIER_ONE] = new()
        {
            { EventState.START,  BarrierOneStart },
            { EventState.LOSE,    BarrierOneLose },
            { EventState.WIN,    BarrierOneWin },
        };
        m_Actions[EventObject.BARRIER_TWO] = new()
        {
            { EventState.START,  BarrierTwoStart },
            { EventState.LOSE,    BarrierTwoLose },
            { EventState.WIN,    BarrierTwoWin },
        };
        m_Actions[EventObject.BARRIER_THREE] = new()
        {
            { EventState.START,  BarrierThreeStart },
            { EventState.LOSE,    BarrierThreeLose },
            { EventState.WIN,    BarrierThreeWin },
        };
        m_Actions[EventObject.BARRIER_LAST] = new()
        {
            { EventState.START,  BarrierLastStart },
            { EventState.LOSE,    BarrierLastLose },
            { EventState.WIN,    BarrierLastWin },
        };
    }
    void Start()
    {
        CacheMaterials();
        ///一つ目の関門の初期化
        m_BarrierOneObject.SetActive(true);
        //SkyBoxの色初期化
        {
            Color InitColor = m_BarrierOneSkyBox.color;
            InitColor.a = 0.0f;
            m_BarrierOneSkyBox.color = InitColor;
        }
        ///二つ目の関門の初期化
        m_BarrierTwoObject.transform.position = new Vector3(322.0f, -350.0f, -122.0f);
        m_BarrierTwoObject.transform.localEulerAngles = new Vector3(0.0f, -195.0f, 0.0f);
        BarrierTwoInitObject();
        ///三つ目の関門の初期化
        var ParMain = m_BarrierThreeObjectParticle.main;
        ParMain.startSpeed = 1.0f;
        m_BarrierThreeObjectParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        m_BarrierThreeObjectParticleBreak.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        InitBall();
    }
    private void Update()
    {
        if (m_Action)
        {
            m_Action = false;
            CoroutineRunner.instance.RunCoroutine(PlayEvent(m_EO, m_ES));
        }
    }
    //イベントを受けるプレイヤーをセット
    public void SetPlayer(Sugoroku_PlayerBase _SetPlayer)
    {
        m_CurrentPlayer = _SetPlayer;
    }
    //イベントを呼び出す関数
    public IEnumerator PlayEvent(EventObject _EventObject, EventState _EventState)
    {
        //m_CurrentUpdateStateに対応する関数を回す
        if (m_Actions.TryGetValue(_EventObject, out var dict))
        {
            if (dict.TryGetValue(_EventState, out var action))
            {
                yield return CoroutineRunner.instance.RunCoroutine(action.Invoke());
            }
        }
    }
    /// 一個目の関門用イベント---------------
    private IEnumerator BarrierOneStart()
    {
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_1);
        yield break;
    }
    private IEnumerator BarrierOneLose()
    {
        //何も起きなかった
        yield break;
    }
    private IEnumerator BarrierOneWin()
    {
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_1);
        //BGMを止める
        AudioManager.instance.StopBGM();
        //蓋を開く
        Animation Anim = m_BarrierOneObject.GetComponent<Animation>();
        Anim.Play("Open");
        //開かれるまで待機
        //待ってる間SE
        AudioManager.instance.PlaySE("SugorokuEvent", "BarrierOne_Start");
        yield return new WaitForSeconds(2.5f);
        //BGM変更
        AudioManager.instance.PlayBGM("Sugoroku", "SgorokuBGM");
        //魔法開放
        m_BarrierOneSkyObjectOpenParticle.Play();
        yield return new WaitUntil(() => !Anim.isPlaying);
        //skybox不透明化
        Color NewColor = m_BarrierOneSkyBox.color;
        yield return CoroutineRunner.instance.RunCoroutine(
               CoroutineRunner.instance.LerpValue<float>(
                   s =>
                   {
                       NewColor.a = s;
                       m_BarrierOneSkyBox.color = NewColor;
                   },
                   0.0f,
                   1.0f,
                   1.0f,
                   Mathf.Lerp
               )
           );
        //壺の削除パーティクル
        m_BarrierOneSkyObjectEndParticle.Play();
        //消える音
        AudioManager.instance.PlaySE("SugorokuEvent", "Vanish");
        //いい感じのタイミングで非アクティブ
        yield return new WaitForSeconds(2.0f);
        m_BarrierOneObject.SetActive(false);
    }
    /// -------------------------------------

    /// 二個目の関門用イベント---------------
    private IEnumerator BarrierTwoStart()
    {
        //初期化
        BarrierTwoInitObject();
        //登場
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_2);
        AudioManager.instance.PlaySE("SugorokuEvent", "StatueUp");
        yield return CoroutineRunner.instance.RunCoroutine(BarrierTwoAdvent(false));
        //スポット
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_2_1);
        AudioManager.instance.PlaySE("SugorokuEvent", "BarrierTwo_Spot");
        yield return new WaitForSeconds(AudioManager.instance.GetClip(true, "SugorokuEvent", "BarrierTwo_Spot").length);
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_2_2);
        AudioManager.instance.PlaySE("SugorokuEvent", "BarrierTwo_Spot");
        yield return new WaitForSeconds(AudioManager.instance.GetClip(true, "SugorokuEvent", "BarrierTwo_Spot").length);
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_2_3);
        AudioManager.instance.PlaySE("SugorokuEvent", "BarrierTwo_Spot2");
        yield return new WaitForSeconds(AudioManager.instance.GetClip(true, "SugorokuEvent", "BarrierTwo_Spot2").length);
        //選択
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_2);
        yield return CellExplanation.instance.ShowWaitExplanation(
            "「腹が減った！ここを通りたくば\r\n供物を捧げよ！」", m_CurrentPlayer.Player
        );
        //ランダムなオブジェクトをアクティブ化
        int RightObjectIndex = UnityEngine.Random.Range(0, m_BarrierTwoRightObject.transform.childCount);
        int LeftObjectIndex = UnityEngine.Random.Range(0, m_BarrierTwoLeftObject.transform.childCount);
        Transform ActiveRight = m_BarrierTwoRightObject.transform.GetChild(RightObjectIndex);
        Transform ActiveLeft = m_BarrierTwoLeftObject.transform.GetChild(LeftObjectIndex);
        ActiveRight.gameObject.SetActive(true);
        ActiveLeft.gameObject.SetActive(true);
        //入力
        GameObject SelectObject = new ();
        if (m_CurrentPlayer.Player.IsNpc)
        {
            yield return new WaitForSeconds(1.5f);
            switch (UnityEngine.Random.Range(0, 2))
            {
                case 0:
                    SelectObject = m_BarrierTwoRightObject;
                    break;
                case 1:
                    SelectObject = m_BarrierTwoLeftObject;
                    break;
            }
        }
        else
        {
            yield return new WaitUntil(() => 
                m_CurrentPlayer.Player.Input.actions["X_Action"].triggered||
                m_CurrentPlayer.Player.Input.actions["Y_Interactive"].triggered);
            if (m_CurrentPlayer.Player.Input.actions["X_Action"].triggered)
            {
                SelectObject = m_BarrierTwoRightObject;
            }
            else if (m_CurrentPlayer.Player.Input.actions["Y_Interactive"].triggered)
            {
                SelectObject = m_BarrierTwoLeftObject;
            }
        }
        //アイテムを投げる
        yield return CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<float>(
                f =>
                {
                    Vector3 NewPos = SelectObject.transform.localPosition;
                    NewPos.z = f;
                    SelectObject.transform.localPosition = NewPos;
                },
                3.5f,
                100.0f,
                1.0f,
                Mathf.Lerp
            )
        );
        //初期化
        BarrierTwoInitObject();
    }
    private IEnumerator BarrierTwoLose()
    {
        AudioManager.instance.PlaySE("SugorokuEvent", "InCorrect");
        //ランダムな不機嫌なセリフを吐かせる
        switch (UnityEngine.Random.Range(0,3))
        {
            case 0:
                yield return CellExplanation.instance.ShowWaitExplanation(
                    "「それはもう食べ飽きた！使えん\r\n奴め！」", m_CurrentPlayer.Player
                );
                break;
            case 1:
                yield return CellExplanation.instance.ShowWaitExplanation(
                    "「それの気分じゃない！失せよ」\r\n", m_CurrentPlayer.Player
                );
                break;
            case 2:
                yield return CellExplanation.instance.ShowWaitExplanation(
                    "「片腹痛し！笑止千万！雨あられ\r\n！」", m_CurrentPlayer.Player
                );
                break;
        }
        AudioManager.instance.PlaySE("SugorokuEvent", "StatueDown");
        yield return CoroutineRunner.instance.RunCoroutine(BarrierTwoAdvent(true));
    }
    private IEnumerator BarrierTwoWin()
    {
        AudioManager.instance.PlaySE("SugorokuEvent", "Correct");
        //ランダムな機嫌なセリフを吐かせる
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                yield return CellExplanation.instance.ShowWaitExplanation(
                    "「良き供物よ！お主、なかなかに\r\n見どころがあるぞ！」", m_CurrentPlayer.Player
                );
                break;
            case 1:
                yield return CellExplanation.instance.ShowWaitExplanation(
                    "「良きかな良きかな！よかろう、\r\n通るがいい！」", m_CurrentPlayer.Player
                );
                break;
            case 2:
                yield return CellExplanation.instance.ShowWaitExplanation(
                    "「愉快、愉快、褒美に道を開けて\r\nやろう！」", m_CurrentPlayer.Player
                );
                break;
        }
        AudioManager.instance.PlaySE("SugorokuEvent", "StatueDown");
        yield return CoroutineRunner.instance.RunCoroutine(BarrierTwoAdvent(true));
    }
    /// -------------------------------------
    private IEnumerator BarrierTwoAdvent(bool _IsReverse)
    {
        float duration = (_IsReverse) ?
            AudioManager.instance.GetClip(true, "SugorokuEvent", "StatueDown").length :
            AudioManager.instance.GetClip(true, "SugorokuEvent", "StatueUp").length;
        //座標の更新
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<float>(
                f =>
                {
                    Vector3 NewPos = m_BarrierTwoObject.transform.localPosition;
                    NewPos.y = f;
                    m_BarrierTwoObject.transform.localPosition = NewPos;
                },
                _IsReverse ? 265f : -30f,
                _IsReverse ? -30f : 265f,
                duration,
                Mathf.Lerp
            )
        );
        //回転の更新
        yield return CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<float>(
                f =>
                {
                    Vector3 NewAngl = m_BarrierTwoObject.transform.localEulerAngles;
                    NewAngl.y = f;
                    m_BarrierTwoObject.transform.localEulerAngles = NewAngl;
                },
                _IsReverse ? -195f : 10000f,
                _IsReverse ? 10000f: -195f,
                duration,
                Mathf.Lerp
            )
        );
    }
    private void BarrierTwoInitObject()
    {
        //右のオブジェクトの子を非アクティブ
        for (int i = 0; i < m_BarrierTwoRightObject.transform.childCount; i++)
        {
            Transform activeRight = m_BarrierTwoRightObject.transform.GetChild(i);
            activeRight.gameObject.SetActive(false);
        }
        //左のオブジェクトの子を非アクティブ
        for (int i = 0; i < m_BarrierTwoLeftObject.transform.childCount; i++)
        {
            Transform activeLeft = m_BarrierTwoLeftObject.transform.GetChild(i);
            activeLeft.gameObject.SetActive(false);
        }
        //右のオブジェクトの場所初期化
        {
            Vector3 NewPos = m_BarrierTwoRightObject.transform.localPosition;
            NewPos.z = 3.5f;
            m_BarrierTwoRightObject.transform.localPosition = NewPos;
        }
        //左のオブジェクトの場所初期化
        {
            Vector3 NewPos = m_BarrierTwoLeftObject.transform.localPosition;
            NewPos.z = 3.5f;
            m_BarrierTwoLeftObject.transform.localPosition = NewPos;
        }
    }

    /// 三個目の関門用イベント---------------
    private IEnumerator BarrierThreeStart()
    {
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_3);
        //大砲が発射しそうになる
        AudioManager.instance.PlaySE("SugorokuEvent", "TankStandby");
        m_BarrierThreeObjectParticle.Play();
        yield return CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<float>(
                f=>{
                    var ParMain = m_BarrierThreeObjectParticle.main;
                    ParMain.startSpeed = f;
                },
                1.0f,
                40.0f,
                AudioManager.instance.GetClip(true,"SugorokuEvent", "TankStandby").length,
                Mathf.Lerp
            )
        );
        yield break;
    }
    private IEnumerator BarrierThreeLose()
    {
        InitBall();
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_3_BALL);
        List<Sugoroku_PlayerBase> Players = SugorokuManager.instance.GetPlayers().
            Where(p => p.GetCurrentCellNum() <= 39&& p.GetCurrentCellNum() > 26).ToList();
        //球発射準備
        AudioManager.instance.PlaySE("SugorokuEvent", "TankCharge");
        yield return new WaitForSeconds(AudioManager.instance.GetClip(true, "SugorokuEvent", "TankCharge").length);
        //球発射
        AudioManager.instance.PlaySE("SugorokuEvent", "TankShot");
        m_BarrierThreeObjectBall.SetActive(true);
        CoroutineRunner.instance.RunCoroutine(RollBall());
        //スケールを通常に戻してく
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector3>(
                    v =>
                    {
                        m_BarrierThreeObjectBall.transform.localScale = v;
                    },
                    Vector3.one*0.1f,
                    Vector3.one,
                    0.3f,
                    Vector3.Lerp
            )
        );
        //セルを順に歩いてく
        for (int i = 39; i > 26; i--)
        {
            //あたったプレイヤーをぶっ飛ばす
            foreach (var Player in Players)
            {
                if (Player.GetCurrentCellNum() == i)
                {
                    AudioManager.instance.PlaySE("SugorokuEvent", "Fall");
                    CoroutineRunner.instance.RunCoroutine(BarrierThreePlayerFall(Player));
                }
            }
            //次に向かうセルの位置
            Vector3 CellPos = SugorokuStage.instance.GetCell(i).gameObject.transform.position + new Vector3(0.0f, 30.0f, 0.0f);
            //セルの方向を見る
            CoroutineRunner.instance.RunCoroutine(
                CoroutineRunner.instance.LerpValue<Quaternion>(
                    w =>
                    {
                        m_BarrierThreeObjectBall.transform.rotation = w;
                    },
                    m_BarrierThreeObjectBall.transform.rotation,
                    Quaternion.LookRotation(CellPos - m_BarrierThreeObjectBall.transform.position),
                    0.3f,
                    Quaternion.Slerp
                )
            );
            m_BarrierThreeObjectBall.transform.rotation = Quaternion.LookRotation(CellPos - m_BarrierThreeObjectBall.transform.position);
            //セルの方向に走る
            yield return CoroutineRunner.instance.RunCoroutine(
                CoroutineRunner.instance.LerpValue<Vector3>(
                    v =>
                    {
                        m_BarrierThreeObjectBall.transform.position = v;
                    },
                    m_BarrierThreeObjectBall.transform.position,
                    CellPos,
                    0.5f,
                    Vector3.Lerp
                )
            );
        }
        //最後のセルが終わったら空の彼方へ
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector3>(
                    v =>
                    {
                        m_BarrierThreeObjectBall.transform.position = v;
                    },
                    m_BarrierThreeObjectBall.transform.position,
                    m_BarrierThreeObjectBall.transform.position+ m_BarrierThreeObjectBall.transform.forward*2400f,
                    12.0f,
                    Vector3.Lerp
            )
        );
        //プレイヤーの戻るセルに視線
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_3_BALLEND);
        yield return new WaitForSeconds(2.3f);
        //プレイヤーの居場所を第二関門に移動
        yield return CoroutineRunner.instance.RunCoroutine(MovePlayers(Players));
        CameraManager.instance.SwitchToSugorokuCam((CameraManager.SugorokuCameraType)m_CurrentPlayer.GetPlayerNum());
    }
    private IEnumerator BarrierThreeWin()
    {
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_3);
        //大砲を壊す
        m_BarrierThreeObjectParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        m_BarrierThreeObjectParticleBreak.Play();
        AudioManager.instance.PlaySE("SugorokuEvent", "TankBreak");
        yield return new WaitForSeconds(AudioManager.instance.GetClip(true, "SugorokuEvent", "TankBreak").length);
        CameraManager.instance.SwitchToSugorokuCam((CameraManager.SugorokuCameraType)m_CurrentPlayer.GetPlayerNum());
        yield return new WaitForSeconds(0.5f);
    }
    /// -------------------------------------
    private IEnumerator BarrierThreePlayerFall(Sugoroku_PlayerBase _Player)
    {
        UnityEngine.Vector3 MoveVec = (_Player.transform.position - this.transform.position).normalized;
        yield return null;
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector3>
            (
                f =>
                {
                    _Player.transform.position = f;
                },
                _Player.transform.position,
                _Player.transform.position + MoveVec*500f,
                2.5f,
                Vector3.Lerp
            )
        );
    }
    private IEnumerator MovePlayers(List<Sugoroku_PlayerBase> _Players)
    {
        Cell cell = SugorokuStage.instance.GetCell(26);
        foreach (var Player in _Players)
        {
            yield return new WaitForSeconds(0.2f);
            Player.SetCurrentCell(26);
            //回転値設定
            Vector3 euler = Player.transform.eulerAngles;
            euler.y = cell.gameObject.transform.eulerAngles.y;
            Player.transform.eulerAngles = euler;
            //ポジション設定
            Vector3 EndPos = cell.gameObject.transform.TransformPoint(Player.GetOffsetPos());
            Vector3 StartPos = EndPos + Vector3.up * 60.0f;
            CoroutineRunner.instance.RunCoroutine(
                CoroutineRunner.instance.LerpValue<Vector3>(
                    v =>
                    {
                        Player.transform.position = v;
                    },
                    StartPos,
                    EndPos,
                    1.0f,
                    Vector3.Lerp
                )
            );
        }
        yield return new WaitForSeconds(1.0f);
    }
    private IEnumerator RollBall()
    {
        Quaternion InitRot = m_BarrierThreeObjectBallRoll.transform.rotation;
        Vector3 Rot = new (90.0f,360.0f,0.0f);
        //アクティブな限り永遠と回す
        while (m_BarrierThreeObjectBallRoll.activeSelf)
        {
            m_BarrierThreeObjectBallRoll.transform.Rotate(Rot*Time.deltaTime);
            yield return null;
        }
        m_BarrierThreeObjectBallRoll.transform.rotation = InitRot;
    }
    private void InitBall()
    {
        m_BarrierThreeObjectBall.transform.localPosition = new Vector3(19.0f,483.0f,-230.0f);
        m_BarrierThreeObjectBall.transform.localScale = Vector3.one * 0.1f;
        m_BarrierThreeObjectBall.SetActive( false );
    }
    /// ラストの関門用イベント---------------
    private IEnumerator BarrierLastStart()
    {
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_LAST);
        yield return CoroutineRunner.instance.RunCoroutine(BarrierLastColorFadeChange(true));
    }
    private IEnumerator BarrierLastLose()
    {
        //彩度を戻す同時進行で
        yield return CoroutineRunner.instance.RunCoroutine(BarrierLastColorFadeChange(false));
    }
    private IEnumerator BarrierLastWin()
    {
        //土煙パーティクル
        m_BarrierLastObjectParticle.Play();
        //彩度を戻す同時進行で
        CoroutineRunner.instance.RunCoroutine(BarrierLastColorFadeChange(false));//彩度を戻す同時進行で
        float time = 0.0f;
        //揺れながら沈む
        while (time < 5.0f)
        {
            time += Time.deltaTime;
            Vector3 dfa = new(
                UnityEngine.Random.Range(-0.75f, 0.75f),
                UnityEngine.Random.Range(-0.75f, 0.75f),
                UnityEngine.Random.Range(-0.75f, 0.75f)
            );
            m_BarrierLastObject.transform.position += new Vector3(0.0f, -time * 0.3f, 0.0f) + dfa;
            yield return null;
        }
        //カメラを引く
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.EVENT_LAST_1);
        CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector3>(
                v =>
                {
                    m_CurrentPlayer.gameObject.transform.localScale = v;
                },
                m_CurrentPlayer.gameObject.transform.localScale,
                Vector3.one*175f,
                5f,
                Vector3.Lerp
            )
        );
        yield return CoroutineRunner.instance.RunCoroutine(
            CoroutineRunner.instance.LerpValue<Vector3>(
                v =>
                {
                    m_CurrentPlayer.gameObject.transform.position = v;
                }, 
                m_CurrentPlayer.gameObject.transform.position,
                m_BarrierLastCrownTransform.position,
                5f,
                Vector3.Lerp
            )
        );
    }
    /// -------------------------------------
    private IEnumerator BarrierLastColorFadeChange(bool _Sign)
    {
        float start = _Sign ? 0f : 1f;
        float end = _Sign ? 1f : 0f;
        float duration = 1.0f;
        foreach (var kvp in m_MaterialCache)
        {
            Material mat = kvp.Value;
            CoroutineRunner.instance.RunCoroutine(
                CoroutineRunner.instance.LerpValue<float>(
                    s =>
                    {
                        // HSVのSだけ変える
                        Color newColor = Color.HSVToRGB(0.0f, s, 1.0f);
                        mat.color = newColor;
                    },
                    start,
                    end,
                    duration,
                    Mathf.Lerp
                )
            );
        }
        yield return new WaitForSeconds(duration);
    }
    private void CacheMaterials()
    {
        Renderer[] renderers = m_BarrierLastObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (!m_MaterialCache.ContainsKey(rend))
            {
                // マテリアルを複製して共有マテリアルの変更を防ぐ
                Material newMat = new(rend.material);
                m_MaterialCache[rend] = newMat;
                rend.material = newMat;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class MiniGameBase : MonoBehaviour
{
    [Header("Awake()使用禁止、Start()を使用")]
    [Header("リザルト画面に遷移するときはプレイヤーの順位を設定してFinish()")]
    [Header("ゲームタイプを入れてね")]
    public  MiniGameManager.GameType m_GameType;
    [Header("チーム人数の少ない順に4人まで入れてください")]
    [SerializeField]protected List<PlayerBase> m_MiniGamePlayers = new ();
    [Header("MiniGameで使用するカメラ")]
    [SerializeField] protected CinemachineVirtualCamera m_MiniGameCam;
    public List<PlayerBase> GetPlayers() { return m_MiniGamePlayers; }
    [Header("ゲームタイトル")]
    [SerializeField,TextArea] public String m_GameTitle;
    [Header("プレイ画像")]
    [SerializeField] public Sprite m_PlayImage;
    [Header("操作説明")]
    [SerializeField, TextArea] public  String m_GameControl;
    [Header("ゲーム説明")]
    [SerializeField, TextArea] public  String m_GameTutorial;
    //ミニゲームに参加するプレイヤーリスト
    private List<MainPlayer> m_MiniGameJoinPlayers = new ();
    public void InitPlayers()
    {
        //参加する順になってるプレイヤーたちを呼び出す
        m_MiniGameJoinPlayers = MiniGameManager.instance.GetMiniGameJoinPlayers();
        //ミニゲーム側でのプレイヤーリストとミニゲーム参加プレイヤーリストは同じ人数でなければならない
        if (m_MiniGamePlayers.Count != m_MiniGameJoinPlayers.Count)
        {
            Debug.LogError("gamePlayers数とm_MiniGamePlayers数が不一致です");
            Debug.LogError(m_MiniGamePlayers.Count);
            Debug.LogError(m_MiniGameJoinPlayers.Count);
            return;
        }
        SetPlayer(m_MiniGamePlayers, m_MiniGameJoinPlayers);
        ChangeColor(m_MiniGamePlayers, m_MiniGameJoinPlayers);
    }
    public static void SetPlayer(List<PlayerBase> _SetPlayers, List<MainPlayer> _JoinPlayers)
    {
        //プレイヤー数分初期化
        for (int i = 0; i < _JoinPlayers.Count; i++)
        {
            //ミニゲーム側のプレイヤーのPlayerBaseを呼ぶ
            PlayerBase _playerBase = _SetPlayers[i];
            if (_playerBase == null)
            {
                //ミニゲーム側のプレイヤーにはPlayerBaseを継承したスクリプトを貼らないといけない
                Debug.LogError("PlayerBase無いとプレイヤーのセット出来んのやけど");
                return;
            }
            //ミニゲーム側のオブジェクトにプレイヤーをセット
            _playerBase.SetMainPlayer(_JoinPlayers[i]);
        }
    }
    public static void ChangeColor(List<PlayerBase> _SetPlayers, List<MainPlayer> _JoinPlayers)
    {
        //プレイヤー数分初期化
        for (int i = 0; i < _JoinPlayers.Count; i++)
        {
            MeshRenderer _mesh = _SetPlayers[i].gameObject.GetComponent<MeshRenderer>();
            if (_mesh == null)
            {
                Debug.LogWarning("スキンが唯一のプレイヤーの判別方法だけど、MeshRenderer無くて大丈夫そ？");
                return;
            }
            //ミニゲーム側のオブジェクトにスキンを反映
            _mesh.material.color = _JoinPlayers[i].SkinColor;
        }
    }
    public void Finish()
    {
        Debug.Log("finishよばれたぜぁあぁ");
        //プレイヤーのランキングが設定されているか確認
        for (int i = 0; i < m_MiniGameJoinPlayers.Count;i++)
        {
            if (PlayerManager.instance.m_Characters[i].rank == -2147483647)
            {
                Debug.LogWarning("プレイヤーの順位がそのまんまの奴が居んねん\n一応一律最下位で進んどくわ");
            }
        }
        //ミニゲームの状態をリザルト状態に推移
        MiniGameManager.instance.PlayFinish();
    }
    public void SetFrozen(bool freeze)
    {
        //全ての子オブジェクトのコンポーネントを停止
        foreach (var behaviour in GetComponentsInChildren<MonoBehaviour>())
        {
            behaviour.enabled = !freeze;
        }
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = freeze;
            if (freeze)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        foreach (var animator in GetComponentsInChildren<Animator>())
        {
            animator.enabled = !freeze;
        }
        foreach (var agent in GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>())
        {
            agent.enabled = !freeze;
        }
    }
    public CinemachineVirtualCamera GetMiniGameVCam()
    {
        return m_MiniGameCam;
    }
}

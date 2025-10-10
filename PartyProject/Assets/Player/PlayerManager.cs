using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.TextCore.Text;
using UnityEngine.Windows;
public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    //最大プレイヤー数
    public const int PlayerMax = 4;
    //主軸となるプレイヤーのプレファブ
    public GameObject PlayerPrefab;
    //プレイヤーの種類
    public enum CharacterType
    {
        ONE,
        TWO,
        THREE,
        FOUR,
    }
    //NPCの強さ
    public enum CharacterStrong
    {
        NORMAL,
        STRONG,
        NONE,
    }
    //キャラクターリスト(容量初期化だけで制限はしていない
    [System.NonSerialized]
    public List<MainPlayer> m_Characters = new(PlayerMax);
    [Header("今プレイヤーの参加を受け付けている状態か")]
    [SerializeField] private bool m_IsJoin;
    protected override void Awake()
    {
        base.Awake();
        m_IsJoin = false;
        m_Characters.Clear();
        //インスペクター上でのアナログ操作チェック
        if (PlayerPrefab == null)
        {
            Debug.LogError("PlayerPrefabが設定されていません！");
            return;
        }
        for (int i = 0; i < PlayerMax; i++)
        {
            // BLUE, RED, GREEN, YELLOW を割り当て
            MainPlayer _mainPlayer = Instantiate(PlayerPrefab).GetComponent<MainPlayer>();
            //インスペクター上でのアナログ操作チェック
            if (_mainPlayer == null)
            {
                Debug.LogError("PlayerPrefabにMainPlayerコンポーネントがありません！");
                continue;
            }
            //NPCとして初期化
            _mainPlayer.Init((CharacterType)i);
            m_Characters.Add(_mainPlayer);
        }
    }
    private void Update()
    {
        //nullになってたらNPC化
        for (int i = 0; i < m_Characters.Count; i++)
        {
            var character = m_Characters[i];
            //プレイヤーだけ見ていく
            if (!character.IsNpc)
            {
                var input = character.Input;
                // Input自体がnullなら退室
                if (input == null)
                {
                    Debug.Log("入力情報がnullです");
                    OnPlayerLeft(character, i);
                    continue;
                }
                // devicesがnullまたは有効なdeviceがない
                var devices = input.devices;
                //デバイスがペアリングしたら
                if (!character.isDeviceJoin && devices.Count > 0 && devices.Any(d => d.added && d.enabled))
                {
                    character.isDeviceJoin = true;
                }
                //一度でもデバイスがペアリングしていたら
                if (character.isDeviceJoin)
                {
                    if (devices.Count == 0)
                    {
                        Debug.Log("コントローラーない!!（devices.Count==0）");
                        OnPlayerLeft(character, i);
                        continue;
                    }
                    //全てのデバイスが削除されたら退室
                    bool allInvalid = true;
                    foreach (var device in devices)
                    {
                        if (device.added && device.enabled)
                        {
                            allInvalid = false;
                            break;
                        }
                    }
                    if (allInvalid)
                    {
                        Debug.Log("コントローラーぶっち!!");
                        OnPlayerLeft(character, i);
                    }
                }
            }
        }
    }
    // プレイヤー入室時処理
    public void OnPlayerJoined(PlayerInput input)
    {
        //受付状態でなければブロック
        if (!m_IsJoin)
        {
            Destroy(input.gameObject);
            return;
        }
        if (input == null || input.user == null || !input.user.valid)
        {
            Debug.LogWarning("無効なUserが入室！");
            if (input != null) Destroy(input.gameObject);
            return;
        }
        //既に参加してあるデバイス判定
        foreach (var character in m_Characters)
        {
            if (!character.IsNpc && character.Input.user == input.user)
            {
                Debug.LogWarning("このデバイスは既に参加しています！");
                Destroy(input.gameObject);
                return;
            }
        }
        //NPCを見つけたらプレイヤーｲﾋ
        foreach (var character in m_Characters)
        {
            if (character.IsNpc)
            {
                character.SetPlayer(input);
                PlayersUi.instance.JoinLeftPlayer(character.Type);
                AudioManager.instance.PlaySE("Player", "Join");
                Debug.Log($"プレイヤー#{input.user.index}が入室！（Input ID: {input.user.id}）");
                return;
            }
        }
        Debug.Log("これ以上プレイヤーを追加できません。");
        Destroy(input.gameObject);
    }
    // プレイヤー退室処理
    public void OnPlayerLeft(MainPlayer _MainPlayer, int _i)
    {
        var input = _MainPlayer.Input;
        ulong leftUserId = input?.user.id ?? 0;
        //InputのGameObjectを削除（nullでない場合のみ）
        if (input != null)
        {
            Destroy(input.gameObject);
        }
        _MainPlayer.SetNpc();
        PlayersUi.instance.JoinLeftPlayer(_MainPlayer.Type);
        AudioManager.instance.PlaySE("Player", "Left");
        Debug.Log($"プレイヤー#{_i}が退室！（Input ID: {leftUserId}）");
    }
    // 今入室させていいかの状態
    public void IsJoined(bool _Switch)
    {
        m_IsJoin = _Switch;
    }
}

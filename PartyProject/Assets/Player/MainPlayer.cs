using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerManager;
public class MainPlayer : MonoBehaviour
{
    //プレイヤーの種類
    private CharacterType _Type;
    public CharacterType Type { get { return _Type; } }
    //色
    public Color SkinColor;
    //NPCかどうか
    private bool _IsNpc;
    public bool IsNpc { get { return _IsNpc; } }
    //プレイヤーのインプット
    private PlayerInput _Input;
    public PlayerInput Input { get { return _Input; } }
    //NPCの強さ
    private CharacterStrong _Strong;
    public CharacterStrong Strong { get { return _Strong; } }
    private CharacterStrong oldStrong;
    //今の順位を保存しておく場所
    public int rank;
    public bool isDeviceJoin = false;
    public void SetPlayer(PlayerInput playerInput)
    {

        _IsNpc = false;
        _Input = playerInput;
        _Strong = CharacterStrong.NONE;
        isDeviceJoin = false;
    }//プレイヤー化
    public void SetNpc()
    {
        _IsNpc = true;
        _Input = null;
        _Strong = oldStrong;
    }//NPC化
     //最初はNPCとして宣言
    public void Init(CharacterType type)
    {
        _Type = type;
        _Input = null;
        _Strong = CharacterStrong.NORMAL;
        oldStrong = _Strong;
        _IsNpc = true;
        rank = -2147483647;
        switch (type)
        {
            case PlayerManager.CharacterType.ONE:
                SkinColor = Color.blue;
                break;
            case PlayerManager.CharacterType.TWO:
                SkinColor = Color.red;
                break;
            case PlayerManager.CharacterType.THREE:
                SkinColor = Color.green;
                break;
            case PlayerManager.CharacterType.FOUR:
                SkinColor = Color.yellow;
                break;
        }
    }
}

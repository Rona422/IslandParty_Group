using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerManager;
public class MainPlayer : MonoBehaviour
{
    //�v���C���[�̎��
    private CharacterType _Type;
    public CharacterType Type { get { return _Type; } }
    //�F
    public Color SkinColor;
    //NPC���ǂ���
    private bool _IsNpc;
    public bool IsNpc { get { return _IsNpc; } }
    //�v���C���[�̃C���v�b�g
    private PlayerInput _Input;
    public PlayerInput Input { get { return _Input; } }
    //NPC�̋���
    private CharacterStrong _Strong;
    public CharacterStrong Strong { get { return _Strong; } }
    private CharacterStrong oldStrong;
    //���̏��ʂ�ۑ����Ă����ꏊ
    public int rank;
    public bool isDeviceJoin = false;
    public void SetPlayer(PlayerInput playerInput)
    {

        _IsNpc = false;
        _Input = playerInput;
        _Strong = CharacterStrong.NONE;
        isDeviceJoin = false;
    }//�v���C���[��
    public void SetNpc()
    {
        _IsNpc = true;
        _Input = null;
        _Strong = oldStrong;
    }//NPC��
     //�ŏ���NPC�Ƃ��Đ錾
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

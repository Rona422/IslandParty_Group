using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using Random = UnityEngine.Random;

public class Player_MeteorPanic : PlayerBase
{
    private int m_PlayerLife;
    private bool m_IsAlive;
    private bool m_CanMove;
    private Rigidbody m_Rigidbody;
    private const float MOVE_SPEED = 5.0f;
    private Vector3 m_StagePosLeftUp;
    private Vector3 m_StagePosRight;

    // �G�̈ړ������Ɏg�p����ϐ�
    private float m_NextChangeTime = 0f;
    private Vector3 m_TargetPos = Vector3.zero;
    private Vector3 m_CurrentDir = Vector3.forward;
    private float m_WaitTime = 0f;
    private bool m_IsWaiting = false;

    public bool IsAlive
    {
        get
        {
            return m_IsAlive;
        }
    }
    public int Life
    {
        get { return m_PlayerLife; }
    }
    void Start()
    {
        m_PlayerLife = 2;
        m_IsAlive = true;
        m_CanMove = true;
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if(!m_IsAlive || !m_CanMove) return;
        var dir = (!Player.IsNpc) ? SetPlayerDirection() : SetNPCDirection();
        Move(dir);
    }

    private Vector3 SetPlayerDirection()
    {
        var dir = Player.Input.actions["Move"].ReadValue<Vector2>();
        return new Vector3(dir.x, 0, dir.y).normalized;
    }
    private Vector3 SetNPCDirection()
    {
        // �ҋ@���Ȃ�^�C�}�[�����炷
        if (m_IsWaiting)
        {
            m_WaitTime -= Time.deltaTime;
            if (m_WaitTime <= 0f)
            {
                m_IsWaiting = false;
                m_NextChangeTime = 0f; // �ēx�ړI�n�����߂�
            }
            return Vector3.zero; // �ҋ@���͕����[���Œ�~
        }

        // �ړI�n�ύX�^�C�~���O
        m_NextChangeTime -= Time.deltaTime;
        if (m_NextChangeTime <= 0f)
        {
            // ���̖ړI�n�������_���ɐݒ�
            float x = Random.Range(-5.5f, 5.5f);
            float z = Random.Range(-2.5f, 2.5f);
            m_TargetPos = new Vector3(x, 0f, z) + transform.parent.position;

            // ���̕ύX�܂ł̎���
            m_NextChangeTime = Random.Range(0.5f, 3f);

            // ���܂Ɏ~�܂�
            if (Random.value < 0.3f)
            {
                m_IsWaiting = true;
                m_WaitTime = Random.Range(0.5f, 1.5f);
            }
        }

        // �ڕW�������v�Z
        Vector3 targetDir = (m_TargetPos - transform.position);
        targetDir.y = 0f;
        if (targetDir.sqrMagnitude > 0.01f)
            targetDir.Normalize();

        // ���X�Ɍ�����ς���
        m_CurrentDir = Vector3.Lerp(m_CurrentDir, targetDir, Time.deltaTime * 2f);

        return m_CurrentDir;
    }

    private void Move(Vector3 dir)
    {
        if (dir != Vector3.zero)
        {
            Vector3 newPosition = m_Rigidbody.position + dir * MOVE_SPEED * Time.deltaTime;
            m_Rigidbody.MovePosition(newPosition);
        }
    }

    public void SetCanMove(bool canMove)
    {
        m_CanMove = canMove;
    }

    public void TakeDamage()
    {
        if(m_PlayerLife > 1)
        {
            m_PlayerLife -= 1;
        }
        // �����Ń_���[�W���o�⎀�S����Ȃǂ���������
        else
        {
            m_IsAlive = false;
            Debug.Log(this.name + "�E���I");
        }
    }
}

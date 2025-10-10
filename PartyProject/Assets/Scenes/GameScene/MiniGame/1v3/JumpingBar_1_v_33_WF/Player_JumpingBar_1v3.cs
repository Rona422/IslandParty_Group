using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 1v3�̃L�����N�^�[�z�u
public class Player_JumpingBar_1v3 : PlayerBase_JumpingBar_1v3
{
    private float m_JumpPower;
    private bool m_IsJumping;
    private bool m_IsCooldown;
    private bool m_IsAlive;
    private Rigidbody m_Rigidbody;
    private DefaultInput m_controls;
    private Vector2 m_MoveInput;

    public bool IsAlive
    {
        get
        {
            return m_IsAlive;
        }
    }

    public override void GameStart()
    {
        m_JumpPower = 6.0f;
        m_IsJumping = false;
        m_IsCooldown = false;
        m_IsAlive = true;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public override void GameUpdate()
    {
        if (!m_IsAlive) return;

        float moveSpeed = 5f;

        Vector3 moveDirection = new Vector3(m_MoveInput.x, 0, m_MoveInput.y);

        if (moveDirection != Vector3.zero)
        {
            moveDirection = moveDirection.normalized;
            Vector3 newPosition = m_Rigidbody.position + moveDirection * moveSpeed * Time.deltaTime;
            m_Rigidbody.MovePosition(newPosition);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && m_IsJumping)
        {
            m_IsJumping = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bar"))
        {
            if (m_Rigidbody != null && m_IsAlive)
            {
                Vector3 knockback = Vector3.up * 20.0f; // �������20�̗͂Ő�����΂��i���l�͒����\�j
                m_Rigidbody.velocity = Vector3.zero;   // ���O�̑��x�����Z�b�g�i�����𑊎E�j
                m_Rigidbody.AddForce(knockback, ForceMode.Impulse);
                m_IsAlive = false;
            }
        }
    }

    private void OnEnable()
    {
        if(m_controls == null)
        {
            m_controls = new DefaultInput();
        }
        
        // Input�A�N�V������L����
        m_controls.Enable();

        // ActionMaps[Player]�̒���[Move]�Ƃ���Action�ɕR�Â��C�x���g���X�i�[��o�^
        m_controls.Player.Move.performed += OnMovePerformed;
        m_controls.Player.Move.canceled += OnMoveCanceled;
        // ActionMaps[Player]�̒���[X_Action]�Ƃ���Action�ɕR�Â��C�x���g���X�i�[��o�^
        m_controls.Player.X_Action.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        // ActionMaps[Player]�̒���[Move]�Ƃ���Action�ɕR�Â��C�x���g���X�i�[������
        m_controls.Player.Move.performed -= OnMovePerformed;
        m_controls.Player.Move.canceled -= OnMoveCanceled;
        // ActionMaps[Player]�̒���[X_Action]�Ƃ���Action�ɕR�Â��C�x���g���X�i�[������
        m_controls.Player.X_Action.performed -= OnJumpPerformed;
        // Input�A�N�V�����𖳌���
        m_controls.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Move���Ă΂ꂽ");

        m_MoveInput = context.ReadValue<Vector2>();
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Move���L�����Z�����ꂽ");
        m_MoveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("X_Action���Ă΂ꂽ");
        
        // Rigidbody�ɏ�����̗͂�������
        if (m_Rigidbody != null && !m_IsJumping && !m_IsCooldown)
        {
            m_IsJumping = true; 
            m_Rigidbody.AddForce(Vector3.up * m_JumpPower, ForceMode.Impulse);
            StartCoroutine(Cooldown(1.0f));
        }
    }

    private IEnumerator Cooldown(float dulation)
    {
        m_IsCooldown = true;
        yield return new WaitForSeconds(dulation);
        m_IsCooldown = false;
        Debug.Log("�W�����v�\");
    }
}

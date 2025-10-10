using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bar_JumpingBar_1v3 : PlayerBase_JumpingBar_1v3
{
    [SerializeField] private GameObject m_Bar;
    [SerializeField] private GameObject m_SlowBar;

    private float m_BarAngleY;
    private float m_SlowBarAngleY;

    private bool m_IsBarRightRotation;
    private bool m_IsSlowBarRightRotation;
    private bool m_IsBarCooldown;
    private bool m_IsSlowBarCooldown;

    DefaultInput m_controls;

    public override void GameStart()
    {
        m_BarAngleY = 45f;
        m_SlowBarAngleY = -45f; 
        
        m_IsBarRightRotation = true;
        m_IsSlowBarRightRotation = true;
        m_IsBarCooldown = false;
        m_IsSlowBarCooldown = false;

        m_Bar.transform.localEulerAngles = new Vector3(90.0f, 45.0f, 0.0f);
        m_SlowBar.transform.localEulerAngles = new Vector3(90.0f, -45.0f, 0.0f);
    }
    public override void GameUpdate()
    {
        float rotationSpeed = 100.0f * Time.deltaTime;

        m_BarAngleY += (m_IsBarRightRotation ? 1 : -1) * rotationSpeed;
        m_BarAngleY %= 360f;
        m_Bar.transform.localRotation = Quaternion.Euler(90f, m_BarAngleY, 0f);

        m_SlowBarAngleY += (m_IsSlowBarRightRotation ? 1 : -1) * (rotationSpeed / 2);
        m_SlowBarAngleY %= 360f;
        m_SlowBar.transform.localRotation = Quaternion.Euler(90f, m_SlowBarAngleY, 0f);
    }

    private void OnEnable()
    {
        if (m_controls == null)
        {
            m_controls = new DefaultInput();
        }
        // Inputアクションを有効化
        m_controls.Enable();

        m_controls.Player.X_Action.performed += X_Action_performed;

        m_controls.Player.Y_Interactive.performed += Y_Interactive_performed;
    }
    private void OnDisable()
    {
        m_controls.Player.X_Action.performed -= X_Action_performed;

        m_controls.Player.Y_Interactive.performed -= Y_Interactive_performed;

        // Inputアクションを無効化
        m_controls.Disable();
    }

    private void X_Action_performed(InputAction.CallbackContext context)
    {
        if (m_IsBarCooldown) return;

        Debug.Log("X_Actionが呼ばれた");

        m_IsBarRightRotation = !m_IsBarRightRotation;
        StartCoroutine(BarCooldown(5.0f));
    }

    private void Y_Interactive_performed(InputAction.CallbackContext context)
    {
        if (m_IsSlowBarCooldown) return;

        Debug.Log("Y_Interactiveが呼ばれた");

        m_IsSlowBarRightRotation = !m_IsSlowBarRightRotation;
        StartCoroutine(SlowBarCooldown(3.0f));
    }


    private IEnumerator BarCooldown(float dulation)
    {
        m_IsBarCooldown = true;
        yield return new WaitForSeconds(dulation);
        m_IsBarCooldown = false;
        Debug.Log("Bar回転方向変更可能");
    }

    private IEnumerator SlowBarCooldown(float dulation)
    {
        m_IsSlowBarCooldown = true;
        yield return new WaitForSeconds(dulation);
        m_IsSlowBarCooldown = false;
        Debug.Log("SlowBar回転方向変更可能");
    }
}

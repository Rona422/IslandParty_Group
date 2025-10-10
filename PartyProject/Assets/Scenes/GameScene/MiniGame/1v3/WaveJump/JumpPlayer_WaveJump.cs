using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpPlayer_WaveJump : PlayerBase
{
    private Rigidbody Rigidbody;
    private PlayerInput PlayerInput;
    private InputAction JumpAction;
    private InputAction MoveAction;
    private Vector2 MoveInput;
    private bool IsJumping;
    private float JumpPower;
    public void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        PlayerInput = Player.Input;
        if (!Player.IsNpc)
        {
            // InputActionÇÃéÊìæ
            MoveAction = PlayerInput.actions["Move"];
            JumpAction = PlayerInput.actions["X_Action"];
        }
        IsJumping = false;
        JumpPower = 300;
    }
    void Update()
    {
        Jump();
    }
    private void Jump()
    {
        if (!Player.IsNpc && JumpAction.triggered)
        {
            if (Rigidbody != null && !IsJumping)
            {
                Debug.Log("ÉWÉÉÉìÉv");
                IsJumping = true;
                // RigidbodyÇ…è„ï˚å¸ÇÃóÕÇâ¡Ç¶ÇÈ
                Rigidbody.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
            }
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && IsJumping)
        {
            Debug.Log("íÖín");
            IsJumping = false;
        }
    }
}

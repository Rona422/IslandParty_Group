using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
public class DeviceJoinManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    //�S���͔���p�ϐ�
    private IDisposable anyButtonPressSubscription;
    private void Awake()
    {
        //NULL�`�F�b�N
        if (PlayerPrefab == null || PlayerPrefab.GetComponent<PlayerInput>() == null)
        {
            Debug.LogError("PlayerPrefab���s���S�ł�");
            return;
        }
    }
    private void OnEnable()
    {
        //�C�x���g�����ׂẴf�o�C�X�̃{�^�����͂�����Ă��邩�ŊĎ�����
        anyButtonPressSubscription = InputSystem.onAnyButtonPress.Subscribe(new InputControlObserver(OnAnyButtonPressed));
    }
    private void OnDisable()
    {

        //�Ď��̒�~
        anyButtonPressSubscription?.Dispose();
        anyButtonPressSubscription = null;
    }
    //IObserver<InputControl> �̓��̓f�[�^���쐬����N���X
    private class InputControlObserver : IObserver<InputControl>
    {
        private readonly Action<InputControl> onNextAction;
        public InputControlObserver(Action<InputControl> onNext)
        {
            onNextAction = onNext;
        }
        public void OnCompleted() { }
        public void OnError(Exception error)
        {
            Debug.LogError($"InputControlObserver�ŃG���[����: {error}");
        }
        public void OnNext(InputControl value)
        {
            onNextAction?.Invoke(value);
        }
    }
    //���͂̂���f�o�C�X���Q��������֐�
    private void OnAnyButtonPressed(InputControl control)
    {
        //���͂����f�o�C�X
        var device = control.device;
        // ���̃f�o�C�X�����łɑ���InputUser�Ƀy�A�����O����Ă��邩�`�F�b�N
        if (PlayerManager.instance.m_Characters.Any(c => !c.IsNpc && c.Input != null && c.Input.devices.Any(d => d.deviceId == device.deviceId)))
        {
            //�Q���ς݂ł����return
            return;
        }
        // �Q������
        PlayerInput newPlayer = PlayerInput.Instantiate(
            PlayerPrefab,
            PlayerInput.all.Count,
            controlScheme: null,
            pairWithDevice: device
        );
        PlayerManager.instance.OnPlayerJoined(newPlayer);
    }
}
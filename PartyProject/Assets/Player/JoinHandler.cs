using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
public class DeviceJoinManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    //全入力判定用変数
    private IDisposable anyButtonPressSubscription;
    private void Awake()
    {
        //NULLチェック
        if (PlayerPrefab == null || PlayerPrefab.GetComponent<PlayerInput>() == null)
        {
            Debug.LogError("PlayerPrefabが不完全です");
            return;
        }
    }
    private void OnEnable()
    {
        //イベントをすべてのデバイスのボタン入力がされているかで監視する
        anyButtonPressSubscription = InputSystem.onAnyButtonPress.Subscribe(new InputControlObserver(OnAnyButtonPressed));
    }
    private void OnDisable()
    {

        //監視の停止
        anyButtonPressSubscription?.Dispose();
        anyButtonPressSubscription = null;
    }
    //IObserver<InputControl> の入力データを作成するクラス
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
            Debug.LogError($"InputControlObserverでエラー発生: {error}");
        }
        public void OnNext(InputControl value)
        {
            onNextAction?.Invoke(value);
        }
    }
    //入力のあるデバイスを参加させる関数
    private void OnAnyButtonPressed(InputControl control)
    {
        //入力したデバイス
        var device = control.device;
        // このデバイスがすでに他のInputUserにペアリングされているかチェック
        if (PlayerManager.instance.m_Characters.Any(c => !c.IsNpc && c.Input != null && c.Input.devices.Any(d => d.deviceId == device.deviceId)))
        {
            //参加済みであればreturn
            return;
        }
        // 参加処理
        PlayerInput newPlayer = PlayerInput.Instantiate(
            PlayerPrefab,
            PlayerInput.all.Count,
            controlScheme: null,
            pairWithDevice: device
        );
        PlayerManager.instance.OnPlayerJoined(newPlayer);
    }
}
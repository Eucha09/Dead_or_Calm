using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    PlayerController _player;

    public Vector2 MoveInput { get; private set; }
    public bool RunInput { get; private set; }
    public bool FireInput { get; private set; }
    public bool ReloadInput { get; private set; }
    public bool InteractionInput { get; private set; }

    public float Attention { get; private set; }
    public float Meditation { get; private set; }

    void Start()
    {
        _player = GetComponent<PlayerController>();

        Managers.Input.KeyAction -= OnKey;
        Managers.Input.KeyAction += OnKey;
        Managers.Input.KeyAction -= OnTab;
        Managers.Input.KeyAction += OnTab;
        Managers.Input.KeyAction -= OnBCI;
        Managers.Input.KeyAction += OnBCI;

    }

    void OnKey()
    {
        InteractionInput = Input.GetKeyDown(KeyCode.F);

        if (Managers.Game.IsGameover || (_player.State != Define.PlayerState.Infil && _player.State != Define.PlayerState.Detec))
        {
            MoveInput = Vector2.zero;
            FireInput = false;
            ReloadInput = false;
            return;
        }

        MoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (MoveInput.sqrMagnitude > 1) 
            MoveInput = MoveInput.normalized;

        RunInput = Input.GetKey(KeyCode.LeftShift);
        FireInput = Input.GetButton("Fire1");
        ReloadInput = Input.GetButton("Reload");
    }

    void OnTab()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_player.State == Define.PlayerState.Infil)
                _player.State = Define.PlayerState.Recon;
            else if (_player.State == Define.PlayerState.Recon)
                _player.State = Define.PlayerState.Infil;
        }
    }

    void OnBCI()
    {
        // 키 1을 누르면 attention 증가
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (Attention >= 100)
                return;
            Attention += 20;
        }
        // 키 2를 누르면 attention 감소

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (Attention <= 0)
                return;
            Attention -= 20;
        }
        // 키 3을 누르면 meditation 증가
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (Meditation >= 100)
                return;
            Meditation += 20;
        }
        // 키 4를 누르면 meditation 감소
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (Meditation <= 0)
                return;
            Meditation -= 20;
        }
    }

}

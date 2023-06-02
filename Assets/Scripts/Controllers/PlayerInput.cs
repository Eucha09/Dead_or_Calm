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
}

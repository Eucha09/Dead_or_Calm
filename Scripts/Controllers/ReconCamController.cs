using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReconCamController : MonoBehaviour
{
    [SerializeField]
    PlayerController _player;

    [SerializeField]
    float _speed = 5.0f;

    Vector3 _move;

    void Start()
    {
        Managers.Input.KeyAction -= OnKey;
        Managers.Input.KeyAction += OnKey;
    }

    void Update()
    {
        if (_player.State == Define.PlayerState.Recon)
            transform.Translate(_move * _speed * Time.deltaTime);
        else
            transform.position = _player.transform.position;
    }

    void OnKey()
    {
        if (Managers.Game.IsGameover || _player.State != Define.PlayerState.Recon)
        {
            _move = Vector3.zero;
            return;
        }
        _move.x = Input.GetAxis("Horizontal");
        _move.z = Input.GetAxis("Vertical");
    }
}

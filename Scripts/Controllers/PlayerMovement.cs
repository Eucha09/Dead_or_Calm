using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    CharacterController _characterController;
    PlayerInput _playerInput;
    PlayerShooter _playerShooter;
    PlayerController _playerController;
    Animator _animator;

    Camera _followCam;

    float _speed = 1f;
    float _runSpeed = 3f;
    [Range(0.01f, 1f)] float _airControlPercent = 0.1f;

    float _speedSmoothTime = 0.1f;
    float _turnSmoothTime = 0.1f;

    float _speedSmoothVelocity;
    float _turnSmoothVelocity;

    float _currentVelocityY;

    public float CurrentSpeed { get { return new Vector2(_characterController.velocity.x, _characterController.velocity.z).magnitude; } }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        _playerShooter = GetComponent<PlayerShooter>();
        _playerController = GetComponent<PlayerController>();
        _followCam = Camera.main;
        _characterController = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        if ((CurrentSpeed > 0.2f || _playerInput.FireInput) && 
            (_playerController.State == Define.PlayerState.Infil || _playerController.State == Define.PlayerState.Detec))
            Rotate();

        Move();
    }

    void Update()
    {
        UpdateAnimation(_playerInput.MoveInput);
    }

    void Move()
    {
        float targetSpeed;
        Vector3 moveDirection;

        if (_playerInput.RunInput) // run
        {
            targetSpeed = _runSpeed * _playerInput.MoveInput.magnitude;
            moveDirection = Vector3.Normalize(transform.forward * Mathf.Max(0, _playerInput.MoveInput.y));
            _animator.SetBool("Run", true);
        }
        else // walk
        {
            targetSpeed = _speed * _playerInput.MoveInput.magnitude;
            moveDirection = Vector3.Normalize(transform.forward * _playerInput.MoveInput.y + transform.right * _playerInput.MoveInput.x);
            _animator.SetBool("Run", false);
        }

        float smoothTime = _characterController.isGrounded ? _speedSmoothTime : _speedSmoothTime / _airControlPercent;

        targetSpeed = Mathf.SmoothDamp(CurrentSpeed, targetSpeed, ref _speedSmoothVelocity, smoothTime);

        _currentVelocityY += Time.deltaTime * Physics.gravity.y;

        Vector3 velocity = moveDirection * targetSpeed + Vector3.up * _currentVelocityY;

        _characterController.Move(velocity * Time.deltaTime);

        if (_characterController.isGrounded) _currentVelocityY = 0;
    }

    void Rotate()
    {
        float targetRotation = _followCam.transform.eulerAngles.y;

        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _turnSmoothVelocity, _turnSmoothTime);
    }

    void UpdateAnimation(Vector2 moveInput)
    {
        float animationSpeedPercent = CurrentSpeed / _speed;

        _animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime);
        _animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
    }

    public void Step()
    {
        Managers.Sound.Play("Step");
    }
}

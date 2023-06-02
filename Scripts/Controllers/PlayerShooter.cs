using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public Define.AimState AimState { get; private set; }

    [SerializeField]
    Gun _gun; // 사용할 총

    PlayerController _playerController;
    PlayerInput _playerInput;
    Animator _playerAnimator; // 애니메이터 컴포넌트
    Camera _playerCamera;

    float _waitingTimeForReleasingAim = 2.5f;
    float _lastFireInputIime;

    Vector3 _aimPoint;
    bool LinedUp => !(Mathf.Abs(_playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);
    bool HasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * _gun.FireTransform.position.y, _gun.FireTransform.position);

    private void Start()
    {
        _playerCamera = Camera.main;
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<Animator>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        AimState = Define.AimState.Idle;
        _gun.gameObject.SetActive(true);
        _gun.Setup(this);
    }

    private void OnDisable()
    {
        AimState = Define.AimState.Idle;
        _gun.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (_playerInput.FireInput && !_playerInput.RunInput)
        {
            _lastFireInputIime = Time.time;
            Shoot();
        }
        else if (_playerInput.ReloadInput && !_playerInput.RunInput)
        {
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();

        float angle = _playerCamera.transform.eulerAngles.x;
        if (angle > 270f) angle -= 360f;

        angle = angle / 150f * -1f + 0.5f;

        _playerAnimator.SetFloat("Angle", angle);

        if (!_playerInput.FireInput && Time.time >= _lastFireInputIime + _waitingTimeForReleasingAim)
        {
            AimState = Define.AimState.Idle;
        }

        UpdateUI();
    }

    public void Shoot()
    {
        if (AimState == Define.AimState.Idle)
        {
            if (LinedUp) AimState = Define.AimState.HipFire;
        }
        else if (AimState == Define.AimState.HipFire)
        {
            if (HasEnoughDistance)
            {
                if (_gun.Fire(_aimPoint))
                    _playerAnimator.SetTrigger("Shoot");
            }
            else
            {
                AimState = Define.AimState.Idle;
            }
        }
    }

    public void Reload()
    {
        // 재장전 입력 감지시 재장전
        if (_gun.Reload())
            _playerAnimator.SetTrigger("Reload");
    }

    private void UpdateAimTarget()
    {
        RaycastHit hit;

        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));

        if (Physics.Raycast(ray, out hit, _gun.FireDistance, ~(1 << (int)Define.Layer.Player)))
        {
            _aimPoint = hit.point;

            if (Physics.Linecast(_gun.FireTransform.position, hit.point, out hit, ~(1 << (int)Define.Layer.Player)))
            {
                _aimPoint = hit.point;
            }
        }
        else
        {
            _aimPoint = _playerCamera.transform.position + _playerCamera.transform.forward * _gun.FireDistance;
        }

        Debug.DrawLine(_gun.FireTransform.position, _aimPoint, Color.red);
    }

    // 탄약 UI 갱신
    private void UpdateUI()
    {
        if (_gun == null || Managers.UI.SceneUI == null) return;

        // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
        UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
        ui.UpdateAmmoText(_gun.MagAmmo, _gun.AmmoRemain);

        // 크로스 헤어 UI 갱신
        //ui.SetActiveCrosshair(HasEnoughDistance);
        //ui.UpdateCrossHairPosition(_aimPoint);
    }
}

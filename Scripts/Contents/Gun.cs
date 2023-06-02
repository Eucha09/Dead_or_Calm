using System.Collections;
using UnityEditorInternal;
using UnityEngine;

// 총을 구현
public class Gun : MonoBehaviour 
{
    public Define.GunState State { get; private set; } // 현재 총의 상태

    [SerializeField]
    PlayerShooter _gunHolder;
    LineRenderer _bulletLineRenderer; // 탄알 궤적을 그리기 위한 렌더러

    [SerializeField]
    ParticleSystem _muzzleFlashEffect; // 총구 화염 효과
    [SerializeField]
    ParticleSystem _shellEjectEffect; // 탄피 배출 효과

    [SerializeField]
    Transform _fireTransform; // 탄알이 발사될 위치
    //[SerializeField]
    //Transform _leftHandMount;

    [SerializeField]
    GunData _gunData; // 총의 현재 데이터

    public float FireDistance { get; private set; } = 100f; // 사정거리

    public int AmmoRemain { get; set; } = 100; // 남은 전체 탄알
    public int MagAmmo { get; set; } // 현재 탄알집에 남아 있는 탄알

    // 탄퍼짐
    public float Stability { get; set; }

    float _lastFireTime; // 총을 마지막으로 발사한 시점

    public Transform FireTransform { get { return _fireTransform; } }

    void Awake() 
    {
        // 사용할 컴포넌트의 참조 가져오기
        _bulletLineRenderer = GetComponent<LineRenderer>();

        _bulletLineRenderer.positionCount = 2;
        _bulletLineRenderer.enabled = false;
    }

    public void Setup(PlayerShooter gunHolder)
    {
        this._gunHolder = gunHolder;
    }

    void OnEnable() 
    {
        // 총 상태 초기화

        AmmoRemain = _gunData.startAmmoRemain;
        MagAmmo = _gunData.magCapacity;

        State = Define.GunState.Ready;
        _lastFireTime = 0;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {

    }

    // 발사 시도
    public bool Fire(Vector3 aimTarget) 
    {
        if (State == Define.GunState.Ready && Time.time >= _lastFireTime + _gunData.timeBetFire)
        {
            //float xError = Util.GetRandomNormalDistribution(0f, _currentSpread);
            //float yError = Util.GetRandomNormalDistribution(0f, _currentSpread);
            float error = (10f - Stability) / 2;
            float xError = Random.Range(-error, error);
            float yError = Random.Range(-error, error);

            Vector3 fireDirection = aimTarget - _fireTransform.position;

            fireDirection = Quaternion.AngleAxis(yError, Vector3.up) * fireDirection;
            fireDirection = Quaternion.AngleAxis(xError, Vector3.right) * fireDirection;

            _lastFireTime = Time.time;
            Shot(_fireTransform.position, fireDirection);

            return true;
        }
        return false;
    }

    // 실제 발사 처리
    void Shot(Vector3 startPoint, Vector3 direction) 
    {
        RaycastHit hit;
        Vector3 hitPosition = Vector3.zero;

        Debug.DrawRay(startPoint, direction, Color.green);
        if (Physics.Raycast(startPoint, direction, out hit, FireDistance))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if (target != null)
            {
                target.OnDamage(_gunData.damage, hit.point, hit.normal, _gunHolder.gameObject);
            }
            else
            {
                Managers.Resource.Instantiate("particle/Metal Impact", hit.point, Quaternion.LookRotation(hit.normal), hit.transform);
            }

            hitPosition = hit.point;
        }
        else
        {
            hitPosition = _fireTransform.position + direction.normalized * FireDistance;
        }

        // 크로스헤어 위치 업데이트
        UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
        ui.UpdateCrossHairPosition(hitPosition);

        StartCoroutine(ShotEffect(hitPosition));

        MagAmmo--;
        if (MagAmmo <= 0)
        {
            State = Define.GunState.Empty;
        }
    }

    // 발사 이펙트와 소리를 재생하고 탄알 궤적을 그림
    IEnumerator ShotEffect(Vector3 hitPosition) {
        _muzzleFlashEffect.Play();
        _shellEjectEffect.Play();

        Managers.Sound.Play(_gunData.shotClip);

        _bulletLineRenderer.SetPosition(0, _fireTransform.position);
        _bulletLineRenderer.SetPosition(1, hitPosition);
        // 라인 렌더러를 활성화하여 탄알 궤적을 그림
        _bulletLineRenderer.enabled = true;

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 탄알 궤적을 지움
        _bulletLineRenderer.enabled = false;
    }

    // 재장전 시도
    public bool Reload() {
        if (State == Define.GunState.Reloading || AmmoRemain <= 0 || MagAmmo >= _gunData.magCapacity)
            return false;

        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    IEnumerator ReloadRoutine() {
        // 현재 상태를 재장전 중 상태로 전환
        State = Define.GunState.Reloading;
        Managers.Sound.Play(_gunData.reloadClip);
      
        // 재장전 소요 시간 만큼 처리 쉬기
        yield return new WaitForSeconds(_gunData.reloadTime);

        int ammoToFill = _gunData.magCapacity - MagAmmo;

        if (AmmoRemain < ammoToFill)
            ammoToFill = AmmoRemain;

        MagAmmo += ammoToFill;
        AmmoRemain -= ammoToFill;

        // 총의 현재 상태를 발사 준비된 상태로 변경
        State = Define.GunState.Ready;
    }
}
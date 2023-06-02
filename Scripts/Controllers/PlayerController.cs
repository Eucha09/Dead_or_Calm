using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Define.PlayerState _state = Define.PlayerState.Infil;

    GameObject _followCam;
    GameObject _reconCam;
    GameObject _hideCam;
    public GameObject GameOverCam { get; set; }

    public Define.PlayerState State
    {
        get { return _state; }
        set
        {
            _state = value;

            switch (_state)
            {
                case Define.PlayerState.Infil:
                    _followCam.SetActive(true);
                    _reconCam.SetActive(false);
                    _hideCam.SetActive(false);
                    GameOverCam.SetActive(false);
                    (Managers.UI.SceneUI as UI_HUD).SetActiveCrosshair(true);
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    break;
                case Define.PlayerState.Recon:
                    _reconCam.SetActive(true);
                    _followCam.SetActive(false);
                    _hideCam.SetActive(false);
                    GameOverCam.SetActive(false);
                    (Managers.UI.SceneUI as UI_HUD).SetActiveCrosshair(false);
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    break;
                case Define.PlayerState.Detec:
                    _followCam.SetActive(true);
                    _reconCam.SetActive(false);
                    _hideCam.SetActive(false);
                    GameOverCam.SetActive(false);
                    (Managers.UI.SceneUI as UI_HUD).SetActiveCrosshair(true);
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    _beDiscoveredRange = 20.0f;
                    _bgmVolume = 0.0f;
                    Managers.Sound.Play("Game Detection BGM", Define.Sound.Bgm, 0.0f);
                    break;
                case Define.PlayerState.Hide:
                    _hideCam.SetActive(true);
                    _followCam.SetActive(false);
                    _reconCam.SetActive(false);
                    GameOverCam.SetActive(false);
                    _sensingRoot.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    break;
                case Define.PlayerState.Failed:
                    Managers.Sound.Play("Game Detection BGM", Define.Sound.Bgm, 0.0f);
                    break;
                case Define.PlayerState.Die:
                    GameOverCam.SetActive(true);
                    _hideCam.SetActive(false);
                    _followCam.SetActive(false);
                    _reconCam.SetActive(false);
                    break;
            }
        }
    }

    [SerializeField][Range(1f, 10f)]
    float _beDiscoveredRange;

    [SerializeField][Range(0f, 10f)]
    float _stability;

    [SerializeField]
    Transform _sensingRoot;
    [SerializeField]
    Gun _gun;

    PlayerInput _playerInput;
    Animator _animator;
    SphereCollider _collider;

    bool _isCoverAround;
    Transform _cover;
    bool _success;
    float _bgmVolume = 1.0f;
    
    public float updateInterval = 30f; // 이완수치 업데이트 간격 (초)
    public float measurementDuration = 10f; // 이완수치 측정 기간 (초)

    private float averageValue = 0f; // 평균 값
    private float targetValue = 0f; // 변경할 반경 값

    private float maxRange = 3f; // 탐지범위 최대 반경
    private float minRange = 1f; // 탐지범위 최소 반경


    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();

        _followCam = GameObject.Find("Follow Cam");
        _reconCam = GameObject.Find("Recon Cam");
        _hideCam = GameObject.Find("Hide Cam");
        GameOverCam = GameObject.Find("GameOver Cam");

        State = Define.PlayerState.Infil;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (_isCoverAround && _playerInput.InteractionInput && State != Define.PlayerState.Hide)
            StartCoroutine("Hide");
        if (State == Define.PlayerState.Hide && _success && _playerInput.InteractionInput)
        {
            _animator.SetBool("Hide", false);
            State = Define.PlayerState.Infil;
            _success = false;
            _bgmVolume = 1.0f;
            Managers.Sound.Play("Game Default BGM", Define.Sound.Bgm, 1.0f);
        }

        // 사운드 부드럽게 전환
        if (State == Define.PlayerState.Hide)
        {
            _bgmVolume = Mathf.Max(0.0f, _bgmVolume - 0.2f * Time.deltaTime) ;
            Managers.Sound.SetVolume(Define.Sound.Bgm, _bgmVolume);
        }
        if (State == Define.PlayerState.Detec || State == Define.PlayerState.Failed)
        {
            _bgmVolume = Mathf.Min(0.6f, _bgmVolume + 0.4f * Time.deltaTime);
            Managers.Sound.SetVolume(Define.Sound.Bgm, _bgmVolume);
        }

        // _beDiscoveredRange 변경
        targetValue = (float)(maxRange + (minRange - maxRange) * (_playerInput.Meditation / 100f));
        // 선형 보간을 사용하여 반경 값 변경
        _beDiscoveredRange = Mathf.Lerp(_beDiscoveredRange, targetValue, Time.deltaTime);

        // _stability 변경
        _stability = _playerInput.Attention / 10f;


        _sensingRoot.localScale = new Vector3(_beDiscoveredRange, 1f, _beDiscoveredRange);
        _collider.radius = _beDiscoveredRange;
        _gun.Stability = _stability;
    }

    IEnumerator Hide()
    {
        transform.position = _cover.position;
        transform.rotation = _cover.rotation;

        _animator.SetBool("Hide", true);
        State = Define.PlayerState.Hide;
        Managers.Sound.Play("Player Breath");

        UI_MeditationBar ui = Managers.UI.ShowPopupUI<UI_MeditationBar>();
        yield return new WaitForSeconds(10f);
        if (ui.Success)
            _success = true;
        else
            State = Define.PlayerState.Failed; // 실패
        ui.ClosePopupUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cover")
        {
            _isCoverAround = true;
            _cover = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cover")
            _isCoverAround = false;
    }
}

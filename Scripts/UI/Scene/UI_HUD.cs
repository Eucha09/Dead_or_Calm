using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HUD : UI_Scene
{
    enum Texts
    {
        AmmoText,
        CollectionText,
    }

    enum Images
    {
        AimPoint,
        HitPoint,
        BCI,
    }

    enum GameObjects
    {
        HPBar,
    }

    PlayerHealth _playerHealth;

    float _smoothTime = 0.2f;
    Vector3 _currentHitPointVelocity;

    float _colorA;
    float _sign = 1.0f;

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        _playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
    }

    void Update()
    {
        UpdateHitPoint();

        float ratio = _playerHealth.Health / _playerHealth.StartingHealth;
        SetHpRatio(ratio);

        _colorA += _sign * Time.deltaTime;
        if (_colorA < 0)
        {
            _colorA = 0.0f;
            _sign *= -1.0f;
        }
        else if (_colorA > 1.0f)
        {
            _colorA = 1.0f;
            _sign *= -1.0f;
        }
        GetImage((int)Images.BCI).color = new Color(1.0f, 1.0f, 1.0f, _colorA);
    }

    void UpdateHitPoint()
    {
        if (!GetImage((int)Images.HitPoint).enabled)
            return;

        RectTransform hitPointRect = GetImage((int)Images.HitPoint).rectTransform;
        Vector3 targetPoint = GetImage((int)Images.AimPoint).rectTransform.position;
        hitPointRect.position = Vector3.SmoothDamp(hitPointRect.position, targetPoint, ref _currentHitPointVelocity, _smoothTime);
    }

    // 탄약 텍스트 갱신
    public void UpdateAmmoText(int magAmmo, int remainAmmo)
    {
        GetText((int)Texts.AmmoText).text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateCollectionText(string name, int count, int maxCount)
    {
        GetText((int)Texts.CollectionText).text = name + " " + count + "/" + maxCount;
    }

    public void SetHpRatio(float ratio)
    {
        GetObject((int)GameObjects.HPBar).GetComponent<Slider>().value = ratio;
    }

    public void SetActiveCrosshair(bool active)
    {
        GetImage((int)Images.HitPoint).enabled = active;
        GetImage((int)Images.AimPoint).enabled = active;
    }

    public void UpdateCrossHairPosition(Vector3 worldPoint)
    {
        GetImage((int)Images.HitPoint).rectTransform.position = Camera.main.WorldToScreenPoint(worldPoint);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MeditationBar : UI_Popup
{
    enum GameObjects
    { 
        MeditationBar,
    }

    PlayerInput _playerInput;
    float _curRatio;
    
    public bool Success { get; private set; }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));

        _playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();

        _curRatio = 0.0f;
    }

    void Update()
    {
        float targetRatio = _playerInput.Meditation / 100.0f;
        _curRatio = Mathf.Lerp(_curRatio, targetRatio, Time.deltaTime);
        SetMeditationRatio(_curRatio);

        Success = _curRatio >= 0.8f;
    }

    public void SetMeditationRatio(float ratio)
    {
        GetObject((int)GameObjects.MeditationBar).GetComponent<Slider>().value = ratio;
    }
}

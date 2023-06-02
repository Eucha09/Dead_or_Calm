using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthSlider : UI_Base
{
    enum GameObjects
    { 
        HealthSlider,
    }

    PlayerHealth _player;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        _player = transform.parent.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        GetObject((int)GameObjects.HealthSlider).GetComponent<Slider>().value = _player.Health;
    }
}

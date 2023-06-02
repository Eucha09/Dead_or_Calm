using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Gameover : UI_Popup
{
    enum Buttons
    {
        RestartButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));


        GetButton((int)Buttons.RestartButton).gameObject.BindEvent(GameRestart);
    }

    // 게임 재시작
    public void GameRestart(PointerEventData data)
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Managers.Game.RestartGame();
    }
}

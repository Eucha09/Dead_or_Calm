using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    //PlayerController _player;

    string _collectionName;
    int _collectionCnt;
    int _maxCollectionCnt;

    public bool IsGameover { get; private set; } // 게임 오버 상태

    public void Init()
    {

        // temp
        _collectionName = "USB";
        _collectionCnt = 0;
        _maxCollectionCnt = 3;
    }

    public void AddCollection()
    {
        _collectionCnt++;

        UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
        if (ui != null)
            ui.UpdateCollectionText(_collectionName, _collectionCnt, _maxCollectionCnt);
    }

    public void EndGame()
    {
        // 게임 오버 상태를 참으로 변경
        IsGameover = true;
        // 게임 오버 UI를 활성화
        Managers.UI.ShowPopupUI<UI_Gameover>();
    }

    public void RestartGame()
    {
        Managers.Scene.LoadScene(Define.Scene.Game);
        IsGameover = false;
    }
}

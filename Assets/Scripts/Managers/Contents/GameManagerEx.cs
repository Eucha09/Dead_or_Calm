using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEx
{
    //PlayerController _player;

    string _collectionName;
    int _collectionCnt;
    int _maxCollectionCnt;

    public bool IsGameover { get; private set; } // ���� ���� ����

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
        // ���� ���� ���¸� ������ ����
        IsGameover = true;
        // ���� ���� UI�� Ȱ��ȭ
        Managers.UI.ShowPopupUI<UI_Gameover>();
    }

    public void RestartGame()
    {
        Managers.Scene.LoadScene(Define.Scene.Game);
        IsGameover = false;
    }
}

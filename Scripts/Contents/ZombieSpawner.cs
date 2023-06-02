using System.Collections.Generic;
using UnityEngine;

// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviour
{
    [SerializeField]
    ZombieData[] _zombieDatas; // 사용할 좀비 셋업 데이터들
    [SerializeField]
    Transform[] _spawnPoints; // 좀비 AI를 소환할 위치들

    List<ZombieController> _zombies = new List<ZombieController>(); // 생성된 좀비들을 담는 리스트
    int _wave; // 현재 웨이브

    void Update() 
    {
        // 게임 오버 상태일때는 생성하지 않음
        if (Managers.Game.IsGameover)
            return;

        // 좀비를 모두 물리친 경우 다음 스폰 실행
        if (_zombies.Count <= 0)
        {
            SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    void UpdateUI() 
    {
        // 현재 웨이브와 남은 적 수 표시
        //UI_HUD ui = Managers.UI.SceneUI as UI_HUD;
        //if (ui != null)
           //ui.UpdateWaveText(_wave, _zombies.Count);
    }

    // 현재 웨이브에 맞춰 좀비들을 생성
    void SpawnWave() 
    {
        _wave++;

        int spawnCount = Mathf.RoundToInt(_wave * 1.5f);

        for (int i = 0; i < spawnCount; i++)
        {
            CreateZombie();
        }
    }

    // 좀비를 생성하고 생성한 좀비에게 추적할 대상을 할당
    void CreateZombie() 
    {
        ZombieData zombieData = _zombieDatas[Random.Range(0, _zombieDatas.Length)];

        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        ZombieController zombie = Managers.Resource.Instantiate("Zombie", spawnPoint.position, spawnPoint.rotation)
                .GetComponent<ZombieController>();

        //zombie.Setup(zombieData);

        _zombies.Add(zombie);

        zombie.OnDeath += () => _zombies.Remove(zombie);
        zombie.OnDeath += () => Managers.Resource.Destroy(zombie.gameObject, 10f);
        //zombie.OnDeath += () => Managers.Game.AddScore(100);
    }
}
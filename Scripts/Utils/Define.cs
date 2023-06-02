using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    // 총의 상태를 표현하는 데 사용할 타입을 선언
    public enum GunState
    {
        Ready, // 발사 준비됨
        Empty, // 탄알집이 빔
        Reloading // 재장전 중
    }

    public enum PlayerState
    { 
        Infil, // Infiltration 잠입
        Recon, // Reconnaissance 정찰
        Detec, // Detection 발각
        Hide, // 엄폐
        Failed,
        Die,
    }

    public enum AimState
    {
        Idle,
        HipFire,
    }

    public enum ZombieState
    {
        Idle,
        Patrol,
        Scream,
        Tracking,
        AttackBegin,
        Attacking,
        Damage,
    }

 //   public enum WorldObject
 //   {
 //       Unknown,
 //       Player,
 //       Monster,
 //   }

	//public enum State
	//{
	//	Die,
	//	Moving,
	//	Idle,
	//	Skill,
 //   }

    public enum Layer
    {
        Monster = 8,
        Player = 9,
        Block = 10,
    }

    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
        PointerDown,
        PointerUp,
    }

    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click,
    }

    public enum CameraMode
    {
        QuarterView,
        BackView,
    }
}

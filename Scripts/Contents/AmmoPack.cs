using UnityEngine;

// 총알을 충전하는 아이템
public class AmmoPack : MonoBehaviour, IItem
{
    //[SerializeField]
    //int ammo = 30; // 충전할 총알 수

    public void Use(GameObject target) 
    {
        // 전달 받은 게임 오브젝트로부터 PlayerController 컴포넌트를 가져오기 시도
        //PlayerController player = target.GetComponent<PlayerController>();

        // PlayerController 컴포넌트가 있으며, 총 오브젝트가 존재하면
        //if (player != null && player.Gun != null)
        //{
        //    // 총의 남은 탄환 수를 ammo 만큼 더한다
        //    player.Gun.AmmoRemain += ammo;
        //}

        // 사용되었으므로, 자신을 파괴
        Managers.Resource.Destroy(gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour, IItem
{
    public void Use(GameObject target)
    {
        Managers.Game.AddCollection();

        Managers.Resource.Destroy(gameObject);
    }
}

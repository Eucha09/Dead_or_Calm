using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// json 형식의 파일 내용을 저장할 데이터 class들 정의
namespace Data
{
    /*ex)
    #region Stat

    [Serializable]
    public class Stat
    {
       public int level;
       public int maxHp;
       public int attack;
       public int totalExp;
    }

    [Serializable]
    public class StatData : ILoader<int, Stat>
    {
       public List<Stat> stats = new List<Stat>();

       public Dictionary<int, Stat> MakeDict()
       {
           Dictionary<int, Stat> dict = new Dictionary<int, Stat>();
           foreach (Stat stat in stats)
               dict.Add(stat.level, stat);
           return dict;
       }
    }

    #endregion
	*/
}
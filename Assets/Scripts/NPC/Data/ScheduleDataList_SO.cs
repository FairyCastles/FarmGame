using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.NPC
{
    [CreateAssetMenu(fileName = "ScheduleDataList_SO", menuName = "NPC Schedule/ScheduleDataList")]
    public class ScheduleDataList_SO : ScriptableObject 
    {
        public List<ScheduleDetails> scheduleList;
    }
}

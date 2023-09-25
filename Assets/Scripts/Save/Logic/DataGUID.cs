using UnityEngine;

namespace Farm.Save
{
    [ExecuteAlways]
    public class DataGUID : MonoBehaviour
    {
        [HideInInspector]
        public string guid;

        #region Life Function

        private void Awake()
        {
            if (guid == string.Empty)
            {
                guid = System.Guid.NewGuid().ToString();
            }
        }

        #endregion
    }
}

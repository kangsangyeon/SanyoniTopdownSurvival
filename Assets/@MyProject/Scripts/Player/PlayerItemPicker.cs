using UnityEngine;

namespace MyProject
{
    public class PlayerItemPicker : MonoBehaviour
    {
        private Player m_Player;
        public Player player => m_Player;

        private void Awake()
        {
            m_Player = GetComponentInParent<Player>();
        }

        private void OnTriggerStay(Collider _other)
        {
            if (_other is IObtainableItem _item)
            {
                if (_item.canObtain)
                {
                    _item.OnObtain(m_Player);
                }
            }
        }
    }
}
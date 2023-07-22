using FishNet;
using UnityEngine;

namespace MyProject
{
    public class PlayerItemPicker : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        private void Awake()
        {
            if (InstanceFinder.IsServer == false)
            {
                enabled = false;
                return;
            }
        }

        private void OnTriggerStay(Collider _other)
        {
            if (_other.GetComponent<IObtainableItem>() is IObtainableItem _item)
            {
                if (_item.canObtain)
                {
                    _item.Obtain(m_Player);
                    Debug.Log($"obtain item! {_other.gameObject.name}");
                }
            }
        }
    }
}
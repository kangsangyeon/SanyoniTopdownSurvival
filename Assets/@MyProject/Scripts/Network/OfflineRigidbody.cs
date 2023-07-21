using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyProject
{
    public sealed class OfflineRigidbody : MonoBehaviour
    {
        private Rigidbody[] m_RigidBodyArray;

        private void Start()
        {
            if (InstanceFinder.IsServer)
            {
                // 서버에서는 csp에 의한 replay가 실시되지 않습니다.
                // 따라서 rigidbody를 비활성화/활성화 토글할 필요가 없습니다.
                return;
            }

            m_RigidBodyArray = GetComponentsInChildren<Rigidbody>();

            if (m_RigidBodyArray.Length == 0)
            {
                return;
            }

            InstanceFinder.PredictionManager.OnPreReconcile += DisableRigidbody;
            InstanceFinder.PredictionManager.OnPostReconcile += EnableRigidbody;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.IsServer)
            {
                // 서버에서는 csp에 의한 replay가 실시되지 않습니다.
                // 따라서 rigidbody를 비활성화/활성화 토글할 필요가 없습니다.
                return;
            }

            if (m_RigidBodyArray.Length == 0)
            {
                return;
            }

            InstanceFinder.PredictionManager.OnPreReconcile -= DisableRigidbody;
            InstanceFinder.PredictionManager.OnPostReconcile -= EnableRigidbody;
        }

        private void DisableRigidbody(NetworkBehaviour _networkBehaviour)
        {
            foreach (var _rigidbody in m_RigidBodyArray)
            {
                _rigidbody.isKinematic = true;
            }
        }

        private void EnableRigidbody(NetworkBehaviour _networkBehaviour)
        {
            foreach (var _rigidbody in m_RigidBodyArray)
            {
                _rigidbody.isKinematic = false;
            }
        }
    }
}
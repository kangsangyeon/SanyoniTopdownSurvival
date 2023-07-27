using FishNet.Object;

namespace MyProject
{
    public class AppendNetworkIdToName : NetworkBehaviour
    {
        private bool m_Initialized = false;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (m_Initialized == false)
            {
                m_Initialized = true;
                gameObject.name = $"{gameObject.name}_{base.NetworkObject.ObjectId}";
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (m_Initialized == false)
            {
                m_Initialized = true;
                gameObject.name = $"{gameObject.name}_{base.NetworkObject.ObjectId}";
            }
        }
    }
}
using FishNet;
using FishNet.Connection;
using FishNet.Object;

namespace MyProject
{
    public class ClientInstance : NetworkBehaviour
    {
        /// <summary>
        /// 클라이언트 본인이 소유한 ClientInstance 레퍼런스입니다.
        /// * 이 레퍼런스는 클라이언트에서만 초기화됩니다.
        /// </summary>
        public static ClientInstance Instance;

        public PlayerSpawner PlayerSpawner { get; private set; }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
            {
                Instance = this;
                PlayerSpawner = GetComponent<PlayerSpawner>();
                PlayerSpawner.TryRespawn();
            }
        }

        /// <summary>
        /// connection이 소유한 client instance를 반환합니다.
        /// * 이 함수의 반환값은 서버에서 실행될 때만 정상적으로 반환됩니다.
        /// </summary>
        public static ClientInstance ReturnClientInstance(NetworkConnection conn)
        {
            /* If server and connection isnt null.
             * When trying to access as server connection
             * will always contain a value. But if client it will be
             * null. */
            if (InstanceFinder.IsServer && conn != null)
            {
                NetworkObject nob = conn.FirstObject;
                return (nob == null) ? null : nob.GetComponent<ClientInstance>();
            }
            else
            {
                //If not server or connection is null, then is client.
                return Instance;
            }
        }
    }
}
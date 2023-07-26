using UnityEngine;

namespace MyProject
{
    public class UIBillboard : MonoBehaviour
    {
        void LateUpdate()
        {
            // transform.LookAt(Camera.main.transform);
            // transform.Rotate(0, 180, 0);

            transform.forward = Camera.main.transform.forward;
        }
    }
}
using UnityEngine;

namespace MainMenu
{
    public class CameraRotation : MonoBehaviour
    {
        public float cameraSpeed;
        public Transform target;
        
        void Update()
        {
            transform.RotateAround(target.position, Vector3.up, cameraSpeed * Time.deltaTime);
        }
    }
}

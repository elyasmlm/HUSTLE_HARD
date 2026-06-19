using UnityEngine;

namespace MenuGame
{
    public class SkinPreviewRotator : MonoBehaviour
    {
        [Tooltip("Vitesse de rotation en degres par seconde.")]
        public float rotationSpeed = 45f;

        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.unscaledDeltaTime, Space.World);
        }
    }
}

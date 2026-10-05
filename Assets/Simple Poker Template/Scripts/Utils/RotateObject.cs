using UnityEngine;

namespace SimplePoker.Visual
{
    public class RotateObject : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 3f;

        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}
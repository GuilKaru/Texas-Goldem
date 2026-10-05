using System.Collections;
using UnityEngine;

namespace SimplePoker.Visual
{
    public class DesactiveObject : MonoBehaviour
    {
        [SerializeField] private float timeToDesactive = 2f;
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(timeToDesactive);
            gameObject.SetActive(false);
        }
    }
}
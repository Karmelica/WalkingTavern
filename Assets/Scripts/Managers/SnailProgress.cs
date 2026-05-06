using System;
using PlayerScripts;
using UnityEngine;

namespace Managers
{
    public class SnailProgress : MonoBehaviour
    {
        [SerializeField] private Transform snail;
        [SerializeField] private Transform finish;
        private float _distance;

        private void Start()
        {
            _distance = Vector3.Distance(finish.position, snail.position);
        }

        private void Update()
        {
            if (!snail || !finish) return;
            var snailDistance = Vector3.Distance(finish.position, snail.position);
            var progress = 1 - snailDistance / _distance;
            progress = Mathf.Clamp(progress, -1, 1);
            PlayerGUI.OnSnailProgressChanged?.Invoke(progress > 0 ? progress : -1);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace OrbitBreaker
{
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class ResponsiveCanvasScaler : MonoBehaviour
    {
        private CanvasScaler scaler;
        private int width = -1, height = -1;
        private void Awake() { scaler = GetComponent<CanvasScaler>(); Refresh(); }
        private void Update() { if (width != Screen.width || height != Screen.height) Refresh(); }
        private void Refresh()
        {
            width = Screen.width; height = Screen.height;
            if (scaler == null) scaler = GetComponent<CanvasScaler>();
            scaler.matchWidthOrHeight = width / Mathf.Max(1f, height) <= 0.5625f ? 0f : 1f;
        }
    }
}

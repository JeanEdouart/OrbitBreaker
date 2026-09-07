using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed class ToggleSwitchVisual : MonoBehaviour
    {
        private static readonly Color OffColor = new Color(0.06f, 0.16f, 0.24f, 1f);
        private static readonly Color OnColor = new Color(0.08f, 0.62f, 0.72f, 1f);
        private Image track;
        private RectTransform knob;
        private float position;
        private float target;

        public void Initialize(Image trackImage, RectTransform knobTransform, bool isOn)
        {
            track = trackImage;
            knob = knobTransform;
            position = target = isOn ? 1f : 0f;
            Apply();
        }

        public void SetValue(bool isOn)
        {
            target = isOn ? 1f : 0f;
        }

        private void Update()
        {
            if (Mathf.Approximately(position, target)) return;
            position = Mathf.MoveTowards(position, target, Time.unscaledDeltaTime * 7.5f);
            Apply();
        }

        private void Apply()
        {
            if (track == null || knob == null) return;
            float center = Mathf.Lerp(0.27f, 0.73f, position);
            knob.anchorMin = new Vector2(center - 0.19f, 0.12f);
            knob.anchorMax = new Vector2(center + 0.19f, 0.88f);
            knob.offsetMin = Vector2.zero;
            knob.offsetMax = Vector2.zero;
            track.color = Color.Lerp(OffColor, OnColor, position);
        }
    }
}

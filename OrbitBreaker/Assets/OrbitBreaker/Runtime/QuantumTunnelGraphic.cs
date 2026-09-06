using UnityEngine;
using UnityEngine.UI;

namespace OrbitBreaker
{
    // Soft perspective shells: transparent centre keeps planets readable during travel.
    public sealed class QuantumTunnelGraphic : MaskableGraphic
    {
        private float intensity;
        private float phase;
        public void SetIntensity(float value)
        {
            intensity = Mathf.Clamp01(value);
            phase += Time.deltaTime * Mathf.Lerp(0.25f, 0.85f, intensity);
            SetVerticesDirty();
        }
        public void ResetTunnel() { phase = 0f; SetIntensity(0f); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (intensity < 0.001f) return;
            Rect rect = rectTransform.rect;
            Vector2 centre = new Vector2(rect.center.x, rect.yMin + rect.height * 0.64f);
            for (int shell = 0; shell < 7; shell++)
            {
                float depth = Mathf.Repeat(shell / 7f + phase, 1f);
                float radius = Mathf.Lerp(0.025f, 1.8f, depth * depth);
                float fade = Mathf.Sin(depth * Mathf.PI) * intensity;
                for (int segment = 0; segment < 72; segment++)
                for (int band = 0; band < 4; band++)
                {
                    int first = mesh.currentVertCount;
                    AddVertex(mesh, centre, rect.size, radius, segment, band, depth, fade);
                    AddVertex(mesh, centre, rect.size, radius, segment + 1, band, depth, fade);
                    AddVertex(mesh, centre, rect.size, radius, segment + 1, band + 1, depth, fade);
                    AddVertex(mesh, centre, rect.size, radius, segment, band + 1, depth, fade);
                    mesh.AddTriangle(first, first + 1, first + 2);
                    mesh.AddTriangle(first, first + 2, first + 3);
                }
            }
        }
        private void AddVertex(VertexHelper mesh, Vector2 centre, Vector2 size, float radius,
            int segment, int band, float depth, float fade)
        {
            float angle = segment / 72f * Mathf.PI * 2f;
            float twist = phase * 0.8f + depth * 2.4f;
            float wave = 1f + 0.055f * Mathf.Sin(angle * 3f + twist) + 0.035f * Mathf.Cos(angle * 5f - twist);
            float thickness = (band / 4f - 0.5f) * 0.16f;
            float r = radius * wave + thickness * Mathf.Lerp(0.12f, 1f, depth);
            Vector2 position = centre + new Vector2(Mathf.Cos(angle) * size.x * 0.65f, Mathf.Sin(angle) * size.y * 0.64f) * r;
            float tint = (Mathf.Sin(angle * 2f + twist) + 1f) * 0.5f;
            Color shade = Color.Lerp(new Color(0.08f, 0.75f, 1f), new Color(0.48f, 0.23f, 1f), tint);
            shade.a = (band == 0 || band == 4 ? 0f : band == 2 ? 0.3f : 0.07f) * fade;
            mesh.AddVert(position, shade, Vector2.zero);
        }
    }
}

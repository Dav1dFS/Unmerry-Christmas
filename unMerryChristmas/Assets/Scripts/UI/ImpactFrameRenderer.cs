using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ImpactFrameRenderer : MonoBehaviour
{
    private float _duration;
    private float _elapsed;
    private float _scale;
    private Color _color;
    private Material _mat;

    public void Init(float scale, float duration, Color color)
    {
        _scale = scale;
        _duration = duration;
        _color = color;

        _mat = new Material(Shader.Find("Sprites/Default"));
        _mat.color = color;
        GetComponent<MeshRenderer>().material = _mat;

        BuildMesh();
    }

    private void BuildMesh()
    {
        int lineCount = 32;
        float innerRadius = 0.1f * _scale;
        float outerRadius = _scale;

        var verts = new Vector3[lineCount * 4];
        var tris = new int[lineCount * 6];
        var uvs = new Vector2[lineCount * 4];

        for (int i = 0; i < lineCount; i++)
        {
            float angle = (360f / lineCount) * i * Mathf.Deg2Rad;
            float angleNext = angle + (Mathf.PI / lineCount) * 0.4f;
            float anglePrev = angle - (Mathf.PI / lineCount) * 0.4f;

            // Inner two verts — narrow at base
            verts[i * 4 + 0] = new Vector3(Mathf.Cos(anglePrev), Mathf.Sin(anglePrev), 0) * innerRadius;
            verts[i * 4 + 1] = new Vector3(Mathf.Cos(angleNext), Mathf.Sin(angleNext), 0) * innerRadius;

            // Outer two verts — taper to point
            float outerR = outerRadius * Random.Range(0.7f, 1.3f);
            verts[i * 4 + 2] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * outerR;
            verts[i * 4 + 3] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * outerR * 0.95f;

            tris[i * 6 + 0] = i * 4 + 0;
            tris[i * 6 + 1] = i * 4 + 2;
            tris[i * 6 + 2] = i * 4 + 1;
            tris[i * 6 + 3] = i * 4 + 1;
            tris[i * 6 + 4] = i * 4 + 2;
            tris[i * 6 + 5] = i * 4 + 3;

            uvs[i * 4 + 0] = Vector2.zero;
            uvs[i * 4 + 1] = Vector2.zero;
            uvs[i * 4 + 2] = Vector2.one;
            uvs[i * 4 + 3] = Vector2.one;
        }

        var mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update()
    {
        _elapsed += Time.unscaledDeltaTime;
        float t = _elapsed / _duration;

        // Scale up and fade out
        float s = Mathf.Lerp(0.5f, 1.5f, t);
        transform.localScale = Vector3.one * s * _scale;

        Color c = _color;
        c.a = Mathf.Lerp(1f, 0f, t);
        _mat.color = c;

        if (_elapsed >= _duration)
            Destroy(gameObject);
    }
}
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ImpactFrameRenderer : MonoBehaviour
{
    private float _duration;
    private float _elapsed;
    private float _scale;
    private Color _color;
    private Material _mat;
    private Material _fillMat;

    public void Init(float scale, float duration, Color color)
    {
        _scale = scale;
        _duration = duration;
        _color = color;

        // Two materials — fill (white inner circle) and lines
        _fillMat = new Material(Shader.Find("Sprites/Default"));
        _fillMat.color = Color.white;

        _mat = new Material(Shader.Find("Sprites/Default"));
        _mat.color = color;

        GetComponent<MeshRenderer>().materials = new Material[] { _fillMat, _mat };

        BuildMesh();
    }

    private void BuildMesh()
    {
        // Lines — varied count, size, spacing for organic look
        int lineCount = Random.Range(28, 48);
        float innerRadius = 0.18f * _scale;
        float outerRadius = _scale;

        // Fill circle verts (submesh 0)
        int fillSegments = 32;
        int fillVertCount = fillSegments + 1; // centre + rim
        var fillVerts = new Vector3[fillVertCount];
        var fillTris = new int[fillSegments * 3];
        fillVerts[0] = Vector3.zero;
        for (int i = 0; i < fillSegments; i++)
        {
            float a = (360f / fillSegments) * i * Mathf.Deg2Rad;
            fillVerts[i + 1] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * innerRadius * 1.1f;
        }
        for (int i = 0; i < fillSegments; i++)
        {
            fillTris[i * 3 + 0] = 0;
            fillTris[i * 3 + 1] = i + 1;
            fillTris[i * 3 + 2] = (i + 1) % fillSegments + 1;
        }

        // Line verts (submesh 1)
        var lineVerts = new Vector3[lineCount * 4];
        var lineTris = new int[lineCount * 6];

        float angleStep = 360f / lineCount;
        float angleCursor = 0f;

        for (int i = 0; i < lineCount; i++)
        {
            // Random jitter on angle to break uniform spacing
            float jitter = Random.Range(-angleStep * 0.35f, angleStep * 0.35f);
            float angle = (angleCursor + jitter) * Mathf.Deg2Rad;
            angleCursor += angleStep;

            // Random width — thicker some, thinner others
            float halfWidth = Random.Range(0.008f, 0.045f) * _scale;

            // Perpendicular for width
            float px = -Mathf.Sin(angle) * halfWidth;
            float py = Mathf.Cos(angle) * halfWidth;

            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

            // Inner edge with small random offset so lines don't all start at same radius
            float inner = innerRadius * Random.Range(0.9f, 1.3f);
            float outer = outerRadius * Random.Range(0.65f, 1.35f);

            lineVerts[i * 4 + 0] = dir * inner + new Vector3(px, py, 0f);
            lineVerts[i * 4 + 1] = dir * inner - new Vector3(px, py, 0f);
            lineVerts[i * 4 + 2] = dir * outer + new Vector3(px * 0.1f, py * 0.1f, 0f); // taper
            lineVerts[i * 4 + 3] = dir * outer - new Vector3(px * 0.1f, py * 0.1f, 0f);

            lineTris[i * 6 + 0] = i * 4 + 0;
            lineTris[i * 6 + 1] = i * 4 + 2;
            lineTris[i * 6 + 2] = i * 4 + 1;
            lineTris[i * 6 + 3] = i * 4 + 1;
            lineTris[i * 6 + 4] = i * 4 + 2;
            lineTris[i * 6 + 5] = i * 4 + 3;
        }

        // Combine all verts into one mesh with two submeshes
        int totalVerts = fillVertCount + lineCount * 4;
        var allVerts = new Vector3[totalVerts];
        fillVerts.CopyTo(allVerts, 0);
        lineVerts.CopyTo(allVerts, fillVertCount);

        // Offset line tri indices
        for (int i = 0; i < lineTris.Length; i++)
            lineTris[i] += fillVertCount;

        var mesh = new Mesh();
        mesh.vertices = allVerts;
        mesh.subMeshCount = 2;
        mesh.SetTriangles(fillTris, 0);
        mesh.SetTriangles(lineTris, 1);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update()
    {
        _elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        // Scale: burst out then settle
        float s = Mathf.Lerp(0.6f, 1.2f, t);
        transform.localScale = Vector3.one * s * _scale;

        // Lines fade out, fill stays white longer then fades
        Color lineColor = _color;
        lineColor.a = Mathf.Lerp(1f, 0f, t);
        _mat.color = lineColor;

        Color fillColor = Color.white;
        fillColor.a = Mathf.Lerp(1f, 0f, Mathf.Pow(t, 2f)); // fades slower
        _fillMat.color = fillColor;

        if (_elapsed >= _duration)
            Destroy(gameObject);
    }
}
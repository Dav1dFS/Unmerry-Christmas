using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NPCVisionCone : MonoBehaviour
{
    [SerializeField] private NpcController _npc;
    [SerializeField] private float _range = 8f;
    [SerializeField] private float _angle = 60f;
    [SerializeField] private int _segments = 20;
    [SerializeField] private float _height = 0.05f; // above ground

    [SerializeField] private Color _busyColor = new Color(1f, 1f, 0f, 0.25f);
    [SerializeField] private Color _alertedColor = new Color(1f, 0.5f, 0f, 0.4f);
    [SerializeField] private Color _watchingColor = new Color(1f, 0f, 0f, 0.5f);

    private Mesh _mesh;
    private MeshRenderer _renderer;
    private Material _mat;

    void Awake()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        _renderer = GetComponent<MeshRenderer>();
        _mat = new Material(Shader.Find("Sprites/Default"));
        _mat.color = _busyColor;
        _renderer.material = _mat;
        _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _renderer.receiveShadows = false;

        if (_npc == null) _npc = GetComponent<NpcController>();
    }

    void Update()
    {
        UpdateColor();
        BuildMesh();
    }

    void UpdateColor()
    {
        switch (_npc.CurrentState)
        {
            case NpcStates.Watching:
                _mat.color = _watchingColor;
                break;
            case NpcStates.Alerted:
            case NpcStates.Distracted:
                _mat.color = _alertedColor;
                break;
            default:
                // changes between green and yellow based on detection progress
                _mat.color = Color.Lerp(_busyColor, _alertedColor, _npc.DetectionProgress);
                break;
        }
    }

    void BuildMesh()
    {
        int vertCount = _segments + 2;
        Vector3[] verts = new Vector3[vertCount];
        int[] tris = new int[_segments * 3];

        verts[0] = new Vector3(0, _height, 0);

        float halfAngle = _angle;
        float angleStep = (halfAngle * 2f) / _segments;

        for (int i = 0; i <= _segments; i++)
        {
            float a = -halfAngle + angleStep * i;
            float rad = a * Mathf.Deg2Rad;
            verts[i + 1] = new Vector3(Mathf.Sin(rad) * _range, _height, Mathf.Cos(rad) * _range);
        }

        for (int i = 0; i < _segments; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
    }
}
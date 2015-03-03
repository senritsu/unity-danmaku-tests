using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;
using UnityEngine.UI;

public class SingleMeshPool : MonoBehaviour
{
    public int BulletCount;

    public Material Material;

    private bool _scale;
    private bool _rotate;

    public class QuadBullet
    {
        public int Index { get; set; }
        public float Lifetime { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float Size { get; set; }
        public float Speed { get; set; }
        public Color32 Color { get; set; }

        public float Time { get; set; }
    }

    private const int MaxBullets = 16000;

    private Mesh _mesh;
    private readonly Vector3[] _vertices = new Vector3[MaxBullets*4];
    private readonly Vector3[] _normals = Enumerable.Repeat(Vector3.back, MaxBullets*4).ToArray();
    private readonly int[] _indices = new int[MaxBullets*6];

    private readonly Vector2[] _uvs =
        Enumerable.Repeat(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up }, MaxBullets)
            .SelectMany(x => x)
            .ToArray();

    private readonly Color32[] _colors = Enumerable.Repeat((Color32)Color.white, MaxBullets*4).ToArray();

    private readonly Queue<int> _available = new Queue<int>(Enumerable.Range(0, MaxBullets));
    private List<QuadBullet> _active = new List<QuadBullet>(); 
    private List<QuadBullet> _temp;

    private Transform _player;

    void Start ()
    {
        _mesh = new Mesh {vertices = _vertices, normals = _normals, uv = _uvs, triangles = _indices, colors32 = _colors};
        var text = GetComponentInChildren<Text>();
        _player = GameObject.Find("Player").transform;
        Observable.EveryUpdate().Subscribe(_ => text.text = string.Format("{0} Bullets active", _active.Count));
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Alpha1)).Subscribe(_ => _rotate = !_rotate);
        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Alpha2)).Subscribe(_ => _scale = !_scale);
    }

    public void AddBullet(QuadBullet bullet)
    {
        if (_available.Count == 0)
        {
            Debug.LogWarning("No available quads, failed to add bullet");
            return;
        }

        bullet.Index = _available.Dequeue();
        bullet.Time = 0;

        _indices[bullet.Index*6] = bullet.Index*4;
        _indices[bullet.Index*6+1] = bullet.Index*4+2;
        _indices[bullet.Index*6+2] = bullet.Index*4+1;
        _indices[bullet.Index*6+3] = bullet.Index*4+3;
        _indices[bullet.Index*6+4] = bullet.Index*4+2;
        _indices[bullet.Index*6+5] = bullet.Index*4;

        _active.Add(bullet);
    }

    public void RemoveBullet(QuadBullet bullet)
    {
        _indices[bullet.Index * 6] = 0;
        _indices[bullet.Index * 6 + 1] = 0;
        _indices[bullet.Index * 6 + 2] = 0;
        _indices[bullet.Index * 6 + 3] = 0;
        _indices[bullet.Index * 6 + 4] = 0;
        _indices[bullet.Index * 6 + 5] = 0;

        _available.Enqueue(bullet.Index);
    }

    private void UpdateBullet(QuadBullet bullet)
    {
        var offset = bullet.Index*4;

        // update bullet state
        bullet.Time += Time.deltaTime;
        bullet.Position += Time.deltaTime*bullet.Speed*bullet.Direction;

        if (_rotate || _scale)
        {
// rotate
            var angleOffset = _rotate ? bullet.Time*180 : 0;
            var pointer = -0.5f*Vector3.one;

            // pulse size
            var size = _scale ? bullet.Size*(1 + 0.5f*Mathf.Sin(bullet.Time*Mathf.PI)) : bullet.Size;

            // update quad
            _vertices[offset] = bullet.Position + Quaternion.AngleAxis(angleOffset, Vector3.back)*(size*pointer);
            _vertices[offset + 1] = bullet.Position +
                                    Quaternion.AngleAxis(angleOffset + 90, Vector3.back)*(size*pointer);
            _vertices[offset + 2] = bullet.Position +
                                    Quaternion.AngleAxis(angleOffset + 180, Vector3.back)*(size*pointer);
            _vertices[offset + 3] = bullet.Position +
                                    Quaternion.AngleAxis(angleOffset + 270, Vector3.back)*(size*pointer);
        }
        else
        {
            _vertices[offset] = new Vector3(bullet.Position.x - 0.5f*bullet.Size, bullet.Position.y - 0.5f*bullet.Size);
            _vertices[offset + 1] = new Vector3(bullet.Position.x + 0.5f * bullet.Size, bullet.Position.y - 0.5f * bullet.Size);
            _vertices[offset + 2] = new Vector3(bullet.Position.x + 0.5f * bullet.Size, bullet.Position.y + 0.5f * bullet.Size);
            _vertices[offset + 3] = new Vector3(bullet.Position.x - 0.5f * bullet.Size, bullet.Position.y + 0.5f * bullet.Size);
        }

        // check for collisions
    }

    private void Update()
    {
        _temp = new List<QuadBullet>(_active.Count);

        foreach (var bullet in _active)
        {
            if (bullet.Time >= bullet.Lifetime || (bullet.Position - _player.position).sqrMagnitude <= bullet.Size * bullet.Size)
            {
                RemoveBullet(bullet);
                continue;
            }

            UpdateBullet(bullet);

            _temp.Add(bullet);
        }

        _active = _temp;
        BulletCount = _active.Count;

        _mesh.vertices = _vertices;
        _mesh.triangles = _indices;

        _mesh.RecalculateBounds();

        Graphics.DrawMesh(_mesh, transform.position, transform.rotation, Material, 0);
    }

    //public void OnPostRender()
    //{
    //    Material.SetPass(1);
    //    Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
    //}
}

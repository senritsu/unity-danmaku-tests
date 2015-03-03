using System;
using UnityEngine;
using System.Collections;
using UniRx;

public class MeshBulletShooter : MonoBehaviour {

    public SingleMeshPool Pool;

    public float FiringRate;
    public float Size;
    public float Lifetime;
    public float Speed;

    // Use this for initialization
    void Start ()
    {
        Observable.Interval(TimeSpan.FromSeconds(1/FiringRate))
            .Subscribe(i => Pool.AddBullet(new SingleMeshPool.QuadBullet
            {
                Position = transform.position,
                Direction = transform.up,
                Lifetime = Lifetime,
                Speed = Speed,
                Size = Size
            }));
    }
}

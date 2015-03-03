using UnityEngine;
using System.Collections;
using UniRx;

public class Rotate : MonoBehaviour
{

    public float Speed;

    // Use this for initialization
    void Start ()
    {
        Observable.EveryUpdate().Subscribe(_ => transform.Rotate(Vector3.forward, Speed*Time.deltaTime));
    }
    
    // Update is called once per frame
    void Update () {
    
    }
}

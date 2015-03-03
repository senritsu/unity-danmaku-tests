using UnityEngine;
using System.Collections;
using UniRx;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;

    // Use this for initialization
    void Start ()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetKey(KeyCode.RightArrow))
            .Subscribe(_ => transform.position += MoveSpeed*Time.deltaTime*Vector3.right);
        Observable.EveryUpdate()
            .Where(_ => Input.GetKey(KeyCode.LeftArrow))
            .Subscribe(_ => transform.position += MoveSpeed * Time.deltaTime * Vector3.left);
        Observable.EveryUpdate()
            .Where(_ => Input.GetKey(KeyCode.UpArrow))
            .Subscribe(_ => transform.position += MoveSpeed * Time.deltaTime * Vector3.up);
        Observable.EveryUpdate()
            .Where(_ => Input.GetKey(KeyCode.DownArrow))
            .Subscribe(_ => transform.position += MoveSpeed * Time.deltaTime * Vector3.down);
    }
    
    // Update is called once per frame
    void Update () {
        
    }
}

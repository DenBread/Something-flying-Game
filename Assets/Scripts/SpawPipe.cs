using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawPipe : MonoBehaviour
{
    [SerializeField] private float _maxTime = 1;
    [SerializeField] private float _timer = 0;
    public GameObject pipe;
    public float height;

    private void Update()
    {
        if(_timer > _maxTime)
        {
            GameObject newPipe = Instantiate(pipe);
            newPipe.transform.position = transform.position + new Vector3(0, Random.Range(-height, height), 0);
            Destroy(newPipe, 15);
            _timer = 0;
        }

        _timer += Time.deltaTime;
    }
}

using UnityEngine;

public class RaiseDoor : MonoBehaviour
{
    public GameObject door;
    public float raiseSpeed = 1.0f;
    public float raiseHeight = 2.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (door != null)
        {
            if (door.transform.position.y < raiseHeight)
            {
                door.transform.Translate(Vector3.up * raiseSpeed * Time.deltaTime);
            }
        }
    }
}

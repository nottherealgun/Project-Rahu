using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewScript : MonoBehaviour
{
    float speed = 5.0f;
    public Rigidbody rb;
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Time.deltaTime * speed;
        }else if(Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * Time.deltaTime * speed;
        }else if(Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * Time.deltaTime * speed;
        }else if(Input.GetKey(KeyCode.D)){
            transform.position += transform.right * Time.deltaTime * speed;
        }if(Input.GetKey(KeyCode.Space)){
            // transform.position -= transform.up * Time.deltaTime * speed;
            rb.AddForce(Vector3.up, ForceMode.Impulse);
        }

        if(Input.GetMouseButtonDown(0)){
            GameObject b = Instantiate(bullet, transform.position+Vector3.forward, transform.rotation);
            b.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
        }
    }
}

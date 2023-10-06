using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetworkBehaviour))]
[RequireComponent(typeof(Rigidbody2D))]
public class TestNNDriver : MonoBehaviour
{
    public float speed;
    public float steer;
    [Min(1)]
    private int sensorResolution = 10;
    [Min(0)]
    public float maxSensorRange = 10;
    [Range(0, 180)]
    public float maxSensorAngle = 180;
    public float speedMultiplier = 1;
    public float steerMultiplier = 1;
    private NeuralNetworkBehaviour nb;
    private Rigidbody2D rb;
    private float[] sensors;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nb = GetComponent<NeuralNetworkBehaviour>();
        nb.OnNetworkUpdate += UpdateDriving;
        sensorResolution = nb.network.InputCount();
        sensors = new float[sensorResolution];
    }

    void UpdateDriving (float[] outputs) {
        speed = outputs[0];
        steer = outputs[1];
    }

    // Update is called once per frame
    void Update()
    {
        // Move
        rb.MovePosition(transform.position + (transform.right * speed * Time.deltaTime * speedMultiplier));
        rb.MoveRotation(transform.eulerAngles.z + (steer * Time.deltaTime * steerMultiplier));

        // Read into sensory

        float stepAngle = maxSensorAngle / (float)(sensorResolution - 1);
        float startOffsetAngle = -(maxSensorAngle / 2f);

        for (int i = 0; i < sensorResolution; i++) {
            float cornerAngle = Mathf.Deg2Rad * (startOffsetAngle + (stepAngle * i));
            float pX = maxSensorRange * Mathf.Cos(cornerAngle);
            float pY = maxSensorRange * Mathf.Sin(cornerAngle);
            Vector2 direction = new Vector2(pX, pY);
            direction = transform.TransformDirection(direction);
            RaycastHit2D hit2d = Physics2D.Raycast(transform.position, direction);
            if (hit2d.collider != null) {
                float normalized = 1f - (Vector2.Distance(hit2d.point, transform.position) / maxSensorRange);
                sensors[i] = (normalized * 2f) - 1f;
                Debug.DrawLine(transform.position, transform.position + (Vector3)direction, new Color(normalized, 1f - normalized, 0f));
            } else {
                sensors[i] = -1f;
                Debug.DrawLine(transform.position, transform.position + (Vector3)direction, Color.green);
            }
        }

        nb.UpdateNetwork(sensors);
    }
}

using Unity.VisualScripting;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("ī�޶� ��鸮�� ���� ����")]
    [SerializeField] public float cameraShakeAmount;

    float shakeTime; // ī�޶� ��鸮�� �ð�

    private void Update()
    { 

        if(shakeTime > 0)
        {
            transform.position = Random.insideUnitSphere * cameraShakeAmount + transform.position;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 0.0f;
            transform.position = new Vector3(transform.position.x,0,-20);
        }
    }

    public void CameraShakeForTime(float time)
    {
        shakeTime = time;
    }
}

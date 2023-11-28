using UnityEngine;

public class CameraShake : MonoBehaviour
{
    float cameraShakeAmount;
    float shakeTime; // ī�޶� ��鸮�� �ð�
    bool shakeEnabled;
    Vector3 cameraPosition;

    private void Update()
    {

        if (shakeEnabled)
        {
            if (shakeTime > 0)
            {
                transform.position = Random.insideUnitSphere * cameraShakeAmount + transform.position;
                transform.position = new Vector3(transform.position.x,transform.position.y,-20);
                shakeTime -= Time.deltaTime;
            }
            else
            {
                shakeTime = 0.0f;
                transform.position = cameraPosition;
                shakeEnabled = false;
            }
        }
    }

    public void CameraShakeForTime(float time, float Amount)
    {
        shakeTime = time;
        cameraShakeAmount = Amount;

        cameraPosition = transform.position;
        shakeEnabled = true;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class hpUIController : MonoBehaviour
{
    public Image[] heart;

    public float hp { get; private set; }

    private float hpMax;

    private void Awake()
    {
        hpMax = heart.Length;
        hp = hpMax;

        for (int i = 0; i < heart.Length; i++)
        {
            if (heart[i].transform.childCount > 0)
            {
                Transform childTransform = heart[i].transform.GetChild(0);

                Image heartImage = childTransform.GetComponent<Image>();

                if (heartImage != null)
                {
                    heart[i] = heartImage;
                }
                else
                {
                    LogPrintSystem.SystemLogPrint(transform, $"�ڽĿ� �̹��� ������Ʈ�� �����ϴ�.", ELogType.Player);
                }
            }      
            else
            {
                LogPrintSystem.SystemLogPrint(transform, $"�ڽ��� �����ϴ�.", ELogType.Player);
            }
        }
    }

    public void Sethp(float damage)
    {
        damage /= 20;
        hp -= damage;

        hp = Mathf.Clamp(hp, 0, hpMax); // �ִ���� ���� ���ϵ��� ó��    

        for (int i = 0; i < heart.Length; i++)
            heart[i].fillAmount = 0;

        for (int i = 0; i < heart.Length; i++)
        {
            heart[i].fillAmount = 0;

            if ((int)hp > i)
                heart[i].fillAmount = 1;

            if ((int)hp == i)
                heart[i].fillAmount = hp - (int)hp; // �Ҽ����� ���� ó��
        }
    }
}

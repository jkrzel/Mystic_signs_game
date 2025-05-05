using UnityEngine;
using UnityEngine.UI;

public class right_hand_script : MonoBehaviour
{
    public Image rightHandImage;
    public Sprite noneRightHand;
    public Sprite IRightHand;
    public Sprite IIRightHand;
    public Sprite IIIRightHand;
    public Sprite IVRightHand;

    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            rightHandImage.sprite = IRightHand;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            rightHandImage.sprite = IIRightHand;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            rightHandImage.sprite = IIIRightHand;
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            rightHandImage.sprite = IVRightHand;
        }
        else
        {
            rightHandImage.sprite = noneRightHand;
        }
    }
}

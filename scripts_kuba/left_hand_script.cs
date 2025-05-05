using UnityEngine;
using UnityEngine.UI;

public class left_hand_script : MonoBehaviour
{
    public Image rightHandImage;
    public Sprite noneRightHand;
    public Sprite IRightHand;
    public Sprite IIRightHand;
    public Sprite IIIRightHand;
    public Sprite IVRightHand;

    public Image fistImage;
    public Sprite fist;
    public Sprite magicFist;
    public Sprite poisonFist;
    public Sprite iceFist;
    public Sprite fireFist;

    public Image leftHandImage;
    public Sprite noneLeftHand;
    public Sprite fistLeftHand;
    public Sprite IILeftHand;

    void Start()
    {
        leftHandImage.enabled = true;
        rightHandImage.enabled = true;
        fistImage.enabled = false;
    }

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


        if (Input.GetKey(KeyCode.LeftArrow))
        {
            leftHandImage.sprite = fistLeftHand;
            if (Input.GetKey(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftArrow))
            {
                fistImage.sprite = magicFist;
            }
            else if (Input.GetKey(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftArrow))
            {
                fistImage.sprite = poisonFist;
            }
            else if (Input.GetKey(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftArrow))
            {
                fistImage.sprite = iceFist;
            }
            else if (Input.GetKey(KeyCode.Alpha4) && Input.GetKey(KeyCode.LeftArrow))
            {
                fistImage.sprite = fireFist;
            }
            else
            {
                fistImage.enabled = true;
                fistImage.sprite = fist;
            }
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            leftHandImage.sprite = IILeftHand;
        }
        else
        {
            leftHandImage.sprite = noneLeftHand;
            fistImage.enabled = false;
        }
    }
}

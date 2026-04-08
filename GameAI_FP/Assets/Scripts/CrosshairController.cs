using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    private Image crosshairImage;

    void Start()
    {
        crosshairImage = GetComponent<Image>();
    }

    void Update()
    {
        crosshairImage.enabled = !Input.GetMouseButton(1);
    }
}
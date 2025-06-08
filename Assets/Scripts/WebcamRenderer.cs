using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;
using Slider = UnityEngine.UIElements.Slider;
using DropdownField = UnityEngine.UIElements.DropdownField;
using ProgressBar = UnityEngine.UIElements.ProgressBar;

public class WebcamRenderer : MonoBehaviour
{
    public RawImage rawImage;
    private WebCamTexture webcamTexture;

    public void Start()
    {
        if (rawImage == null)
        {
            Debug.LogError("RawImage не назначен в WebcamRenderer!");
            return;
        }
        var device = WebCamTexture.devices.FirstOrDefault();
        if (!string.IsNullOrEmpty(device.name))
        {
            WebCamTexture tex = new WebCamTexture(device.name);
            // запускаем и привязываем
        }
        else
        {
            Debug.LogWarning("Камера не найдена");
        }
    }

    public void StartCamera(string deviceName)
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
            Destroy(webcamTexture);
        }

        webcamTexture = new WebCamTexture(deviceName, 1080, 720, 120);
        webcamTexture.Play();
        rawImage.texture = webcamTexture;
    }

    public void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}

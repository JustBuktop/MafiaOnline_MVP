using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SettingsManager
{
    private UIDocument settingsUIDocument;
    private WebcamRenderer webcamRenderer;
    private MonoBehaviour coroutineRunner;

    private AudioSource micSource;
    private string micDevice;
    private AudioClip micClip;

    public SettingsManager(UIDocument settingsUIDocument, WebcamRenderer webcamRenderer, MonoBehaviour coroutineRunner)
    {
        this.settingsUIDocument = settingsUIDocument;
        this.webcamRenderer = webcamRenderer;
        this.coroutineRunner = coroutineRunner;
    }

    public void InitSettingsUI()
    {
        var root = settingsUIDocument.rootVisualElement;

        micSource = GameObject.Find("MicPlayer").GetComponent<AudioSource>();

        SetupMicrophone(root);
        SetupResolution(root);
        SetupWindowMode(root);
        SetupCamera(root);
        SetupVolume(root);
    }

    private void SetupMicrophone(VisualElement root)
    {
        var micDropdown = root.Q<DropdownField>("microphone-dropdown");
        micDropdown.choices = new List<string>(Microphone.devices);
        micDropdown.value = micDropdown.choices.FirstOrDefault();

        micDevice = micDropdown.value;
        micClip = !string.IsNullOrEmpty(micDevice) ? Microphone.Start(micDevice, true, 1, 44100) : null;

        var micBar = root.Q<ProgressBar>("mic-level");
        if (micClip != null)
            coroutineRunner.StartCoroutine(UpdateMicLevel(micClip, micBar));

        var toggle = root.Q<Toggle>("mic-monitor-toggle");
        toggle.SetEnabled(micClip != null);
        toggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                micSource.clip = micClip;
                micSource.loop = true;
                while (Microphone.GetPosition(micDevice) <= 0) { }
                micSource.Play();
            }
            else
            {
                micSource.Stop();
            }
        });
    }

    private IEnumerator UpdateMicLevel(AudioClip clip, ProgressBar bar)
    {
        float[] data = new float[128];
        while (true)
        {
            int micPos = Microphone.GetPosition(micDevice);
            if (micPos >= data.Length)
            {
                clip.GetData(data, micPos - data.Length);
                bar.value = Mathf.Clamp01(data.Select(Mathf.Abs).Average() * 20f);
            }
            else bar.value = 0f;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetupResolution(VisualElement root)
    {
        var resDropdown = root.Q<DropdownField>("resolution-dropdown");
        var resolutions = Screen.resolutions
            .Select(r => $"{r.width}x{r.height} @ {r.refreshRate}Hz")
            .Distinct().ToList();

        resDropdown.choices = resolutions;
        resDropdown.value = resolutions.FirstOrDefault();

        resDropdown.RegisterValueChangedCallback(evt =>
        {
            string[] parts = evt.newValue.Split(new[] { "x", "@", "Hz" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 &&
                int.TryParse(parts[0].Trim(), out int w) &&
                int.TryParse(parts[1].Trim(), out int h))
                Screen.SetResolution(w, h, Screen.fullScreenMode);
        });
    }

    private void SetupWindowMode(VisualElement root)
    {
        var dropdown = root.Q<DropdownField>("window-mode-dropdown");
        dropdown.choices = new() {
            "Оконный (с рамкой)",
            "Без рамки (Borderless)",
            "Полноэкранный"
        };

        dropdown.value = Screen.fullScreenMode switch
        {
            FullScreenMode.Windowed => "Оконный (с рамкой)",
            FullScreenMode.FullScreenWindow => "Без рамки (Borderless)",
            FullScreenMode.ExclusiveFullScreen => "Полноэкранный",
            _ => "Оконный (с рамкой)"
        };

        dropdown.RegisterValueChangedCallback(evt =>
        {
            Screen.fullScreenMode = evt.newValue switch
            {
                "Оконный (с рамкой)" => FullScreenMode.Windowed,
                "Без рамки (Borderless)" => FullScreenMode.FullScreenWindow,
                "Полноэкранный" => FullScreenMode.ExclusiveFullScreen,
                _ => FullScreenMode.Windowed
            };
        });
    }

    private void SetupCamera(VisualElement root)
    {
        var camDropdown = root.Q<DropdownField>("camera-dropdown");
        camDropdown.choices = WebCamTexture.devices.Select(d => d.name).ToList();
        camDropdown.value = camDropdown.choices.FirstOrDefault();

        camDropdown.RegisterValueChangedCallback(evt =>
        {
            if (webcamRenderer.gameObject.activeSelf)
            {
                webcamRenderer.StopCamera();
                webcamRenderer.StartCamera(evt.newValue);
            }
        });

        webcamRenderer.gameObject.SetActive(false);

        var toggle = root.Q<Toggle>("camera-preview-toggle");
        toggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                webcamRenderer.gameObject.SetActive(true);
                webcamRenderer.StartCamera(camDropdown.value);
            }
            else
            {
                webcamRenderer.StopCamera();
                webcamRenderer.gameObject.SetActive(false);
            }
        });
    }

    private void SetupVolume(VisualElement root)
    {
        var volumeSlider = root.Q<Slider>("volume-slider");
        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"Громкость: {evt.newValue}");
        });
    }

    public void HideCameraPreview()
    {
        var toggle = settingsUIDocument.rootVisualElement.Q<Toggle>("camera-preview-toggle");
        toggle.value = false;
    }
}

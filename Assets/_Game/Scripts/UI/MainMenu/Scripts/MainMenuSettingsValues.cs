using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuSettingsValues : MonoBehaviour
{
    private const string MasterPrefKey = "Settings.MasterVolume";
    private const string MusicPrefKey = "Settings.MusicVolume";
    private const string SfxPrefKey = "Settings.SfxVolume";
    private const string FullScreenPrefKey = "Settings.FullScreen";
    private const string DisplayModePrefKey = "Settings.DisplayMode";
    private const string ResolutionWidthPrefKey = "Settings.ResolutionWidth";
    private const string ResolutionHeightPrefKey = "Settings.ResolutionHeight";

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterParameter = "MasterVolume";
    [SerializeField] private string musicParameter = "MusicVolume";
    [SerializeField] private string sfxParameter = "SFXVolume";

    [Header("Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private Button resetButton;

    [Header("Defaults")]
    [SerializeField, Range(0f, 1f)] private float defaultMasterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.8f;

    private bool savePending;
    private float saveAtUnscaledTime;
    private readonly List<Vector2Int> availableResolutions = new List<Vector2Int>();

    private void Start()
    {
        ConfigureSlider(masterVolumeSlider);
        ConfigureSlider(musicVolumeSlider);
        ConfigureSlider(sfxVolumeSlider);
        PopulateResolutionDropdown();
        PopulateDisplayModeDropdown();

        float master = PlayerPrefs.GetFloat(MasterPrefKey, defaultMasterVolume);
        float music = PlayerPrefs.GetFloat(MusicPrefKey, defaultMusicVolume);
        float sfx = PlayerPrefs.GetFloat(SfxPrefKey, defaultSfxVolume);
        int displayModeIndex = LoadDisplayModeIndex();
        int resolutionWidth = PlayerPrefs.GetInt(ResolutionWidthPrefKey, Screen.currentResolution.width);
        int resolutionHeight = PlayerPrefs.GetInt(ResolutionHeightPrefKey, Screen.currentResolution.height);
        int resolutionIndex = FindClosestResolutionIndex(resolutionWidth, resolutionHeight);

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(master);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(music);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfx);

        if (fullScreenToggle != null)
            fullScreenToggle.SetIsOnWithoutNotify(displayModeIndex != 0);

        if (resolutionDropdown != null && resolutionIndex >= 0)
            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);

        if (displayModeDropdown != null)
            displayModeDropdown.SetValueWithoutNotify(displayModeIndex);

        ApplyMasterVolume(master, false);
        ApplyMusicVolume(music, false);
        ApplySfxVolume(sfx, false);
        ApplyResolutionAndMode(resolutionIndex, displayModeIndex, false);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(OnFullScreenChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (displayModeDropdown != null)
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
    }

    private void Update()
    {
        if (!savePending || Time.unscaledTime < saveAtUnscaledTime)
            return;

        SaveNow();
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.RemoveListener(OnFullScreenChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (displayModeDropdown != null)
            displayModeDropdown.onValueChanged.RemoveListener(OnDisplayModeChanged);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetToDefaults);

        if (savePending)
            SaveNow();
    }

    private void OnApplicationQuit()
    {
        if (savePending)
            SaveNow();
    }

    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(defaultMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(defaultMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(defaultSfxVolume);

        if (fullScreenToggle != null)
            fullScreenToggle.SetIsOnWithoutNotify(true);

        int defaultResolutionIndex = FindClosestResolutionIndex(
            Screen.currentResolution.width,
            Screen.currentResolution.height);

        if (resolutionDropdown != null && defaultResolutionIndex >= 0)
            resolutionDropdown.SetValueWithoutNotify(defaultResolutionIndex);

        if (displayModeDropdown != null)
            displayModeDropdown.SetValueWithoutNotify(1);

        ApplyMasterVolume(defaultMasterVolume, true);
        ApplyMusicVolume(defaultMusicVolume, true);
        ApplySfxVolume(defaultSfxVolume, true);
        ApplyResolutionAndMode(defaultResolutionIndex, 1, true);
    }

    private void OnMasterVolumeChanged(float value)
    {
        ApplyMasterVolume(value, true);
    }

    private void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value, true);
    }

    private void OnSfxVolumeChanged(float value)
    {
        ApplySfxVolume(value, true);
    }

    private void OnFullScreenChanged(bool enabled)
    {
        int displayModeIndex = enabled ? 1 : 0;

        if (displayModeDropdown != null)
            displayModeDropdown.SetValueWithoutNotify(displayModeIndex);

        ApplyResolutionAndMode(GetSelectedResolutionIndex(), displayModeIndex, true);
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        ApplyResolutionAndMode(resolutionIndex, GetSelectedDisplayModeIndex(), true);
    }

    private void OnDisplayModeChanged(int displayModeIndex)
    {
        displayModeIndex = Mathf.Clamp(displayModeIndex, 0, 2);

        if (fullScreenToggle != null)
            fullScreenToggle.SetIsOnWithoutNotify(displayModeIndex != 0);

        ApplyResolutionAndMode(GetSelectedResolutionIndex(), displayModeIndex, true);
    }

    private void ApplyMasterVolume(float value, bool queueSave)
    {
        value = Mathf.Clamp01(value);
        SetMixerVolume(masterParameter, value);
        PlayerPrefs.SetFloat(MasterPrefKey, value);

        if (queueSave)
            QueueSave();
    }

    private void ApplyMusicVolume(float value, bool queueSave)
    {
        value = Mathf.Clamp01(value);
        SetMixerVolume(musicParameter, value);
        PlayerPrefs.SetFloat(MusicPrefKey, value);

        if (queueSave)
            QueueSave();
    }

    private void ApplySfxVolume(float value, bool queueSave)
    {
        value = Mathf.Clamp01(value);
        SetMixerVolume(sfxParameter, value);
        PlayerPrefs.SetFloat(SfxPrefKey, value);

        if (queueSave)
            QueueSave();
    }

    private void ApplyResolutionAndMode(int resolutionIndex, int displayModeIndex, bool queueSave)
    {
        if (availableResolutions.Count == 0)
            return;

        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, availableResolutions.Count - 1);
        displayModeIndex = Mathf.Clamp(displayModeIndex, 0, 2);

        Vector2Int resolution = availableResolutions[resolutionIndex];
        FullScreenMode mode = DisplayModeFromIndex(displayModeIndex);

        Screen.SetResolution(resolution.x, resolution.y, mode);

        PlayerPrefs.SetInt(ResolutionWidthPrefKey, resolution.x);
        PlayerPrefs.SetInt(ResolutionHeightPrefKey, resolution.y);
        PlayerPrefs.SetInt(DisplayModePrefKey, displayModeIndex);
        PlayerPrefs.SetInt(FullScreenPrefKey, displayModeIndex == 0 ? 0 : 1);

        if (queueSave)
            QueueSave();
    }

    private void SetMixerVolume(string parameter, float linearValue)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(parameter))
            return;

        audioMixer.SetFloat(parameter, LinearToDecibels(linearValue));
    }

    private void QueueSave()
    {
        savePending = true;
        saveAtUnscaledTime = Time.unscaledTime + 0.5f;
    }

    private void SaveNow()
    {
        PlayerPrefs.Save();
        savePending = false;
    }

    private static void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void PopulateResolutionDropdown()
    {
        availableResolutions.Clear();
        var seen = new HashSet<string>();

        foreach (Resolution resolution in Screen.resolutions)
        {
            string key = resolution.width + "x" + resolution.height;
            if (!seen.Add(key))
                continue;

            availableResolutions.Add(new Vector2Int(resolution.width, resolution.height));
        }

        if (availableResolutions.Count == 0)
        {
            availableResolutions.Add(new Vector2Int(
                Screen.currentResolution.width,
                Screen.currentResolution.height));
        }

        availableResolutions.Sort((a, b) =>
        {
            int widthComparison = a.x.CompareTo(b.x);
            return widthComparison != 0 ? widthComparison : a.y.CompareTo(b.y);
        });

        if (resolutionDropdown == null)
            return;

        var options = new List<string>(availableResolutions.Count);
        foreach (Vector2Int resolution in availableResolutions)
            options.Add(resolution.x + " × " + resolution.y);

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
    }

    private void PopulateDisplayModeDropdown()
    {
        if (displayModeDropdown == null)
            return;

        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string>
        {
            "Windowed",
            "Borderless Fullscreen",
            "Exclusive Fullscreen"
        });
        displayModeDropdown.RefreshShownValue();
    }

    private int LoadDisplayModeIndex()
    {
        if (PlayerPrefs.HasKey(DisplayModePrefKey))
            return Mathf.Clamp(PlayerPrefs.GetInt(DisplayModePrefKey), 0, 2);

        bool legacyFullScreen = PlayerPrefs.GetInt(
            FullScreenPrefKey,
            Screen.fullScreen ? 1 : 0) != 0;

        return legacyFullScreen ? 1 : 0;
    }

    private int GetSelectedResolutionIndex()
    {
        if (resolutionDropdown != null)
            return Mathf.Clamp(resolutionDropdown.value, 0, availableResolutions.Count - 1);

        return FindClosestResolutionIndex(Screen.width, Screen.height);
    }

    private int GetSelectedDisplayModeIndex()
    {
        if (displayModeDropdown != null)
            return Mathf.Clamp(displayModeDropdown.value, 0, 2);

        if (fullScreenToggle != null)
            return fullScreenToggle.isOn ? 1 : 0;

        return Screen.fullScreenMode == FullScreenMode.Windowed ? 0 : 1;
    }

    private int FindClosestResolutionIndex(int width, int height)
    {
        if (availableResolutions.Count == 0)
            return -1;

        int closestIndex = 0;
        long closestDistance = long.MaxValue;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            long deltaWidth = availableResolutions[i].x - width;
            long deltaHeight = availableResolutions[i].y - height;
            long distance = deltaWidth * deltaWidth + deltaHeight * deltaHeight;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestIndex = i;
        }

        return closestIndex;
    }

    private static FullScreenMode DisplayModeFromIndex(int displayModeIndex)
    {
        switch (displayModeIndex)
        {
            case 0:
                return FullScreenMode.Windowed;
            case 2:
                return FullScreenMode.ExclusiveFullScreen;
            default:
                return FullScreenMode.FullScreenWindow;
        }
    }

    private static float LinearToDecibels(float linearValue)
    {
        return linearValue <= 0.0001f
            ? -80f
            : Mathf.Log10(linearValue) * 20f;
    }
}

using _Project.Scripts.TimeSystem;
using UnityEngine;
using Zenject;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lighting References")] [SerializeField]
    private Light _sunLight;

    [SerializeField] private Light _moonLight;

    [SerializeField] private Color _duskColor = new Color(1f, 0.5f, 0.25f);
    [SerializeField] private Color _nightColor = new Color(0.2f, 0.2f, 0.5f);
    [SerializeField] private float _transitionDuration = 2.0f;
    [SerializeField] private float _rotationSpeed = 2.0f;

    private ITimeSettings _timeSettings;
    private ITimeProvider _timeProvider;


    [Inject]
    private void Constructor(ITimeSettings timeSettings, ITimeProvider timeProvider)
    {
        this._timeSettings = timeSettings;
        this._timeProvider = timeProvider;
    }

    private void Update()
    {
        UpdateSunAndMoonPositions();
        SmoothTransition();
    }

    private void UpdateSunAndMoonPositions()
    {
        float currentTime = _timeProvider.Hours + _timeProvider.Minute / 60.0f;

        UpdateLightPosition(_sunLight, currentTime, _timeSettings.SunriseHour, _timeSettings.SunsetHour);
        UpdateLightPosition(_moonLight, currentTime, _timeSettings.SunsetHour, _timeSettings.SunriseHour);
    }

    private void UpdateLightPosition(Light light, float time, float riseHour, float setHour)
    {
        float solarDeclination = CalculateSolarDeclination();
        float solarHourAngle = CalculateSolarHourAngle(time);
        Vector3 lightDirection = CalculateLightDirection(solarDeclination, solarHourAngle);

        Quaternion targetRotation = Quaternion.LookRotation(-lightDirection);

        light.transform.rotation =
            Quaternion.Lerp(light.transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    private float CalculateSolarDeclination()
    {
        float dayOfYear = System.DateTime.Now.DayOfYear;
        return 23.44f * Mathf.Sin(Mathf.Deg2Rad * ((360.0f / 365.0f) * (dayOfYear - 81)));
    }

    private float CalculateSolarHourAngle(float time)
    {
        return (time - 12.0f) * 15.0f;
    }

    private Vector3 CalculateLightDirection(float declination, float hourAngle)
    {
        float latRad = Mathf.Deg2Rad * _timeSettings.Latitude;
        float declRad = Mathf.Deg2Rad * declination;
        float hourAngleRad = Mathf.Deg2Rad * hourAngle;

        float solarAltitude = Mathf.Asin(
            Mathf.Sin(latRad) * Mathf.Sin(declRad) +
            Mathf.Cos(latRad) * Mathf.Cos(declRad) * Mathf.Cos(hourAngleRad)
        );

        float solarAzimuth = Mathf.Atan2(
            -Mathf.Sin(hourAngleRad),
            Mathf.Cos(hourAngleRad) * Mathf.Sin(latRad) - Mathf.Tan(declRad) * Mathf.Cos(latRad)
        );

        return new Vector3(
            Mathf.Cos(solarAzimuth) * Mathf.Cos(solarAltitude),
            Mathf.Sin(solarAltitude),
            Mathf.Sin(solarAzimuth) * Mathf.Cos(solarAltitude)
        );
    }

    private void SmoothTransition()
    {
        float currentTime = _timeProvider.Hours + _timeProvider.Minute / 60.0f;

        if (IsWithinTransitionPeriod(currentTime, _timeSettings.SunsetHour))
        {
            PerformTransition(currentTime, _timeSettings.SunsetHour, _duskColor, _nightColor, isSunSetting: true);
        }
        else if (IsWithinTransitionPeriod(currentTime, _timeSettings.SunriseHour))
        {
            PerformTransition(currentTime, _timeSettings.SunriseHour, _nightColor, _duskColor, isSunSetting: false);
        }
        else
        {
            if (currentTime >= _timeSettings.SunsetHour || currentTime < _timeSettings.SunriseHour)
            {
                _sunLight.intensity = 0f;
                _moonLight.intensity = Mathf.Clamp01(_moonLight.intensity);
            }
            else
            {
                _sunLight.intensity = Mathf.Clamp01(_sunLight.intensity);
                _moonLight.intensity = 0f;
            }
        }
    }

    private bool IsWithinTransitionPeriod(float time, float transitionHour)
    {
        return Mathf.Abs(time - transitionHour) <= _transitionDuration;
    }

    private void PerformTransition(float time, float transitionHour, Color startColor, Color endColor,
        bool isSunSetting)
    {
        float t = Mathf.InverseLerp(transitionHour - _transitionDuration, transitionHour + _transitionDuration, time);

        _sunLight.intensity = Mathf.Lerp(isSunSetting ? 1f : 0f, isSunSetting ? 0f : 1f, t);
        _moonLight.intensity = Mathf.Lerp(isSunSetting ? 0f : 1f, isSunSetting ? 1f : 0f, t);
        _moonLight.color = Color.Lerp(startColor, endColor, t);
    }
}
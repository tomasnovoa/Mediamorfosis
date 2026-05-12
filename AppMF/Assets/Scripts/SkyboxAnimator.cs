using UnityEngine;

public class SkyboxAnimator : MonoBehaviour
{
    [Header("Rotation Speed")]
    [SerializeField] private float rotationSpeed = 0.5f;

    [Header("Drift")]
    [SerializeField] private bool driftEnabled = true;
    [SerializeField] private float driftAmplitude = 0.3f;
    [SerializeField] private float driftFrequency = 0.2f;

    [Header("Reflection Sync")]
    [SerializeField] private ReflectionProbe reflectionProbe; // arrastra tu probe aquí
    [SerializeField] private int reflectionUpdateInterval = 2; // cada cuántos frames actualiza

    private float currentRotation = 0f;
    private int frameCounter = 0;

    void Start()
    {
        // Crear instancia propia del material para no modificar el asset original
        RenderSettings.skybox = new Material(RenderSettings.skybox);

        // Sincronización inicial
        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        // Rotación base + drift
        currentRotation += rotationSpeed * Time.deltaTime;
        float drift = driftEnabled
            ? Mathf.Sin(Time.time * driftFrequency) * driftAmplitude
            : 0f;

        RenderSettings.skybox.SetFloat("_Rotation", currentRotation + drift);

        if (currentRotation >= 360f)
            currentRotation -= 360f;

        // Actualizar reflexiones cada N frames para no saturar la GPU
        frameCounter++;
        if (frameCounter >= reflectionUpdateInterval)
        {
            frameCounter = 0;
            DynamicGI.UpdateEnvironment();

            // Forzar re-render de la Reflection Probe si está asignada
            if (reflectionProbe != null)
                reflectionProbe.RenderProbe();
        }
    }

    public void SetSpeed(float speed) => rotationSpeed = speed;
    public void PauseRotation() => rotationSpeed = 0f;
}

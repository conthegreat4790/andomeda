using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    private Material runtimeSkyboxMat;
    private float currentRotation = 0f;

    void Start()
    {
        // Delayed initialization prevents the GUIStyle editor error
        Invoke(nameof(InitializeSkybox), 0.05f);
    }

    void InitializeSkybox()
    {
        if (RenderSettings.skybox == null) return;

        // Create runtime instance to avoid modifying original project assets permanently
        runtimeSkyboxMat = new Material(RenderSettings.skybox);
        RenderSettings.skybox = runtimeSkyboxMat;

        // Cache initial material rotation
        currentRotation = runtimeSkyboxMat.GetFloat("_Rotation");
        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        if (runtimeSkyboxMat != null)
        {
            // Advance rotation over time, looping back to 0 after 360 degrees
            currentRotation = (currentRotation + (rotationSpeed * Time.deltaTime)) % 360f;
            runtimeSkyboxMat.SetFloat("_Rotation", currentRotation);

            // Keep environment lighting synced with the rotation
            DynamicGI.UpdateEnvironment();
        }
    }
}

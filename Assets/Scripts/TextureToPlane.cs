using Unity.VisualScripting;
using UnityEngine;

public class TextureToPlane : MonoBehaviour
{
    public Texture2D texture; // Input Texture

    private GameObject plane;
    private float planeWidth, planeHeight;
    public float scaleX = 138.5768509f, scaleY = 138.5768509f;
    public float moveFactorX = 8f, moveFactorY = 6f;
    [SerializeField] private bool destroyVisuals = true;

    void Start()
    {
        if (texture != null)
        {
            CreatePlaneFromTexture(texture.width, texture.height, texture);
        }
        else
        {
            Debug.LogError("No se ha asignado una textura");
        }
    }

    void CreatePlaneFromTexture(float width, float height, Texture2D tex)
    {
        plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.transform.SetParent(transform);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Guardar dimensiones del plano en unidades del mundo
        planeWidth = width / scaleX;
        planeHeight = height / scaleY;
        plane.transform.localScale = new Vector3(planeWidth, planeHeight, 1f);

        // Aplicar material y textura
        //Renderer renderer = plane.GetComponent<Renderer>();
        if (destroyVisuals)
        {
            Destroy(plane.GetComponent<MeshCollider>());
            Destroy(plane.GetComponent<MeshRenderer>());
        }
        //Material material = new Material(Shader.Find("Universal/RenderPipeline/Unlit"));
        //material.mainTexture = tex;
        //material.SetTexture("_BaseMap", tex);
        //renderer.material = material;
    }

    public Vector3 PixelToWorldPosition(int pixelX, int pixelY)
    {
        if (texture == null || plane == null) return Vector3.zero;

        // Convertir píxeles a coordenadas normalizadas (0 a 1)
        float xNormalized = (float)pixelX / (float)texture.width;
        float yNormalized = (float)pixelY / (float)texture.height;

        // Convertir a coordenadas del mundo (ajustando por la rotación y la posición del pivote)
        float worldX = (xNormalized - 0.5f) * planeWidth;
        float worldZ = (0.5f - yNormalized) * planeHeight;
        //float worldZ = (yNormalized - 0.5f) * planeHeight;

        Vector3 worldPosition = plane.transform.position;
        Vector3 newWorldPosition = new Vector3(worldPosition.x + worldX + (xNormalized / moveFactorX), worldPosition.y, worldPosition.z + worldZ + (yNormalized / moveFactorY));
        return newWorldPosition;
    }

}

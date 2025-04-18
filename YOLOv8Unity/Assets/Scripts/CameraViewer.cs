using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.TextureProviders;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Barracuda;

namespace Assets.Scripts
{
    public class CameraViewer : MonoBehaviour
    {
        [Tooltip("RawImage component donde se mostrará la cámara")]
        [SerializeField]
        private RawImage cameraImage;

        [SerializeField]
        private TextureProviderType.ProviderType textureProviderType;

        [SerializeReference]
        private TextureProvider textureProvider = null;

        [Tooltip("Ancho deseado para la textura de la cámara (0 para usar resolución original)")]
        [SerializeField] 
        private int textureWidth = 640;

        [Tooltip("Alto deseado para la textura de la cámara (0 para usar resolución original)")]
        [SerializeField] 
        private int textureHeight = 480;

        private bool scaleInitialized = false;

        private void OnEnable()
        {
            // Inicializar el proveedor de textura
            textureProvider = GetTextureProvider();
            textureProvider.Start();
            
            // Ajustar el tamaño del RawImage a la resolución deseada
            cameraImage.rectTransform.sizeDelta = new Vector2(textureWidth, textureHeight);
        }

        private void Update()
        {
            // Obtener la textura de la cámara y mostrarla
            Texture2D texture = textureProvider.GetTexture();
            cameraImage.texture = texture;
            
            // Ajustar la escala la primera vez
            if (!scaleInitialized)
            {
                RectTransform rt = cameraImage.GetComponent<RectTransform>();
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                float imageWidth = rt.rect.width;
                float imageHeight = rt.rect.height;
                float scaleX = screenWidth / imageWidth;
                float scaleY = screenHeight / imageHeight;
                float scale = Mathf.Min(scaleX, scaleY);
                rt.localScale = new Vector3(scale, scale, 1f);

                Debug.Log($"ImageUI position: {rt.anchoredPosition}, resolution: {imageWidth}x{imageHeight}, scale: {scale}");
                scaleInitialized = true;
            }
        }

        private TextureProvider GetTextureProvider()
        {
            TextureProvider provider;
            switch (textureProviderType)
            {
                case TextureProviderType.ProviderType.WebCam:
                    provider = new WebCamTextureProvider(textureProvider as WebCamTextureProvider, textureWidth, textureHeight);
                    break;

                case TextureProviderType.ProviderType.Video:
                    provider = new VideoTextureProvider(textureProvider as VideoTextureProvider, textureWidth, textureHeight);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            Debug.Log($"Proveedor de textura inicializado con resolución solicitada: {textureWidth}x{textureHeight}");
            return provider;
        }

        private void OnDisable()
        {
            textureProvider.Stop();
        }

        private void OnValidate()
        {
            var t = TextureProviderType.GetProviderType(textureProviderType);
            if (textureProvider == null || t != textureProvider.GetType())
            {
                textureProvider = RuntimeHelpers.GetUninitializedObject(t) as TextureProvider;
            }
        }
    }
}
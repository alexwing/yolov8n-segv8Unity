using Assets.Scripts;
using Assets.Scripts.TextureProviders;
using NN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public class Detector : MonoBehaviour
{
    [Tooltip("File of YOLO model.")]
    [SerializeField]
    protected NNModel ModelFile;

    [Tooltip("RawImage component which will be used to draw resuls.")]
    [SerializeField]
    protected RawImage ImageUI;

    [Tooltip("RawImage component to render camera texture.")]
    [SerializeField]
    protected RawImage CameraImageUI;
    
    [Range(0.0f, 1f)]
    [Tooltip("The minimum value of box confidence below which boxes won't be drawn.")]
    [SerializeField]
    protected float MinBoxConfidence = 0.3f;

    [Tooltip("Si es verdadero, solo muestra la silueta y el recuadro con fondo transparente.")]
    [SerializeField]
    protected bool UseTransparentBackground = false;

    [SerializeField]
    protected TextureProviderType.ProviderType textureProviderType;

    [SerializeReference]
    protected TextureProvider textureProvider = null;

    [Tooltip("Frame rate objetivo para CameraImageUI (0 = sin límite)")]
    [Range(0, 120)]
    [SerializeField]
    protected int cameraFrameRate = 30;

    [Tooltip("Frame rate objetivo para ImageUI con procesamiento YOLO (0 = sin límite)")]
    [Range(0, 120)]
    [SerializeField]
    protected int yoloFrameRate = 15;

    protected NNHandler nn;
    protected Color[] colorArray = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };

    [Tooltip("Nombres de las clases que puede detectar el modelo YOLO")]
    [SerializeField]
    protected string[] classNames = new string[] {
        "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
        "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
        "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
        "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle",
        "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange",
        "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch", "potted plant", "bed",
        "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven",
        "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush"
    };

    // Controladores de frame rate
    protected FrameRateController cameraFpsController;
    protected FrameRateController yoloFpsController;

    protected YOLOv8 yolo;

    private void OnEnable()
    {
        nn = new NNHandler(ModelFile);
        yolo = new YOLOv8Segmentation(nn);

        textureProvider = GetTextureProvider(nn.model);
        textureProvider.Start();
        
        // Inicializar controladores de frame rate
        cameraFpsController = new FrameRateController(cameraFrameRate);
        yoloFpsController = new FrameRateController(yoloFrameRate);
    }

    // Update is called once per frame
    private void Update()
    {
        // Actualizar los controladores de frame rate si se cambiaron los valores en el inspector
        if (cameraFpsController != null && cameraFrameRate != cameraFpsController.TargetFrameRate)
            cameraFpsController.SetFrameRate(cameraFrameRate);
            
        if (yoloFpsController != null && yoloFrameRate != yoloFpsController.TargetFrameRate)
            yoloFpsController.SetFrameRate(yoloFrameRate);
            
        // Actualizar CameraImageUI según el frame rate configurado
        if (CameraImageUI != null && cameraFpsController != null && cameraFpsController.ShouldUpdate())
        {
            CameraImageUI.texture = textureProvider.GetRawTexture();
        }

        // Actualizar ImageUI según el frame rate configurado
        if (yoloFpsController != null && yoloFpsController.ShouldUpdate())
        {
            // Procesamiento YOLO y visualización de resultados
            YOLOv8OutputReader.DiscardThreshold = MinBoxConfidence;
            Texture2D texture = GetNextTexture();

            var boxes = yolo.Run(texture);
            DrawResults(boxes, texture);
            ImageUI.texture = texture;
        }
    }

    protected TextureProvider GetTextureProvider(Model model)
    {
        var firstInput = model.inputs[0];
        int height = firstInput.shape[5];
        int width = firstInput.shape[6];

        TextureProvider provider;
        switch (textureProviderType)
        {
            case TextureProviderType.ProviderType.WebCam:
                provider = new WebCamTextureProvider(textureProvider as WebCamTextureProvider, width, height);
                break;

            case TextureProviderType.ProviderType.Video:
                provider = new VideoTextureProvider(textureProvider as VideoTextureProvider, width, height);
                break;
            default:
                throw new InvalidEnumArgumentException();
        }
        Debug.Log($"Texture resized to {width}x{height}");
        // Set YOLOv8 input dimensions dynamically
        YOLOv8OutputReader.InputWidth = width;
        YOLOv8OutputReader.InputHeight = height;
        return provider;
    }

    protected Texture2D GetNextTexture()
    {
        return textureProvider.GetTexture();
    }

    void OnDisable()
    {
        nn.Dispose();
        textureProvider.Stop();
    }

    protected void DrawResults(IEnumerable<ResultBox> results, Texture2D img)
    {
        results.ForEach(box => DrawBox(box, img));
    }

    protected virtual void DrawBox(ResultBox box, Texture2D img)
    {
        Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
        int boxWidth = (int)(box.score / MinBoxConfidence);
        TextureTools.DrawRectOutline(img, box.rect, boxColor, boxWidth, rectIsNormalized: false, revertY: true);
    }

    private void OnValidate()
    {
        Type t = TextureProviderType.GetProviderType(textureProviderType);
        if (textureProvider == null || t != textureProvider.GetType())
        {
            if (nn == null)
                textureProvider = RuntimeHelpers.GetUninitializedObject(t) as TextureProvider;
            else
            {
                textureProvider = GetTextureProvider(nn.model);
                textureProvider.Start();
            }

        }
    }
}

using NN;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class Segmentator : Detector
    {
        YOLOv8Segmentation yolo;
        private bool scaleInitialized = false;

        // Use this for initialization
        void OnEnable()
        {
            nn = new NNHandler(ModelFile);
            yolo = new YOLOv8Segmentation(nn);

            textureProvider = GetTextureProvider(nn.model);
            textureProvider.Start();
            // Redimensionar RawImage al tamaño del modelo
            ImageUI.rectTransform.sizeDelta = new Vector2(YOLOv8OutputReader.InputWidth, YOLOv8OutputReader.InputHeight);
        }

        // Update is called once per frame
        void Update()
        {
            // Mostrar la cámara en CameraImageUI a máxima velocidad
            if (CameraImageUI != null)
            {
                CameraImageUI.texture = textureProvider.GetRawTexture();
            }

            YOLOv8OutputReader.DiscardThreshold = MinBoxConfidence;
            Texture2D texture = GetNextTexture();

            var boxes = yolo.Run(texture);
            
            if (UseTransparentBackground)
            {
                // Crear una textura transparente con solo la silueta y el recuadro
                texture = CreateTransparentMaskTexture(boxes, texture);
            }
            else
            {
                // Modo normal, dibujar sobre la textura original
                DrawResults(boxes, texture);
            }
            
            ImageUI.texture = texture;

            if (!scaleInitialized)
            {
                RectTransform rt = ImageUI.GetComponent<RectTransform>();
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                float imageWidth = rt.rect.width;
                float imageHeight = rt.rect.height;
                float scaleX = screenWidth / imageWidth;
                float scaleY = screenHeight / imageHeight;
                float scale = Mathf.Min(scaleX, scaleY);
                rt.localScale = new Vector3(scale, scale, 1f);

                Debug.Log($"ImageUI position: {rt.anchoredPosition}, scale: {scale}");
                scaleInitialized = true;
            }
        }

        private Texture2D CreateTransparentMaskTexture(List<ResultBoxWithMask> results, Texture2D sourceTexture)
        {
            int w = sourceTexture.width;
            int h = sourceTexture.height;
            
            // Crear una nueva textura con formato RGBA32 para permitir transparencia
            Texture2D transparentTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            
            // Inicializar todos los píxeles como transparentes
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            // Aplicar los píxeles iniciales
            transparentTexture.SetPixels(pixels);
            
            // Dibujar las siluetas y los recuadros en la textura transparente
            foreach (ResultBoxWithMask box in results)
            {
                // Dibujar la silueta
                Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
                TextureTools.RenderMaskOnTransparentTexture(box.masks, transparentTexture, sourceTexture, boxColor);
                
                // Dibujar el recuadro
                int boxWidth = (int)(box.score / MinBoxConfidence);
                TextureTools.DrawRectOutline(transparentTexture, box.rect, boxColor, boxWidth, rectIsNormalized: false, revertY: true);
                
                // Liberar el tensor de la máscara
                box.masks.tensorOnDevice.Dispose();
            }
            
            transparentTexture.Apply();
            return transparentTexture;
        }

        void OnDisable()
        {
            nn.Dispose();
            textureProvider.Stop();
        }

        protected override void DrawBox(ResultBox box, Texture2D img)
        {
            base.DrawBox(box, img);

            ResultBoxWithMask boxWithMask = box as ResultBoxWithMask;
            Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
            TextureTools.RenderMaskOnTexture(boxWithMask.masks, img, boxColor);
            boxWithMask.masks.tensorOnDevice.Dispose();
        }
    }
}
﻿using NN;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class Segmentator : Detector
    {
        YOLOv8Segmentation yolo;
        private bool scaleInitialized = false;
        private FrameRateController cameraFpsController;
        private FrameRateController yoloFpsController;
        // Textura reutilizable para el modo transparente
        private Texture2D transparentTexture;

        // Use this for initialization
        void OnEnable()
        {
            nn = new NNHandler(ModelFile);
            yolo = new YOLOv8Segmentation(nn);

            textureProvider = GetTextureProvider(nn.model);
            textureProvider.Start();
            // Redimensionar RawImage al tamaño del modelo
            ImageUI.rectTransform.sizeDelta = new Vector2(YOLOv8OutputReader.InputWidth, YOLOv8OutputReader.InputHeight);
            
            // Inicializar controladores de frame rate
            cameraFpsController = new FrameRateController(cameraFrameRate);
            yoloFpsController = new FrameRateController(yoloFrameRate);
        }

        // Update is called once per frame
        void Update()
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
            }

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

                //inicializar CameraImageUI
                RectTransform rtCamera = CameraImageUI.GetComponent<RectTransform>();
                imageWidth = rtCamera.rect.width;
                imageHeight = rtCamera.rect.height;
                scaleX = screenWidth / imageWidth;
                scaleY = screenHeight / imageHeight;
                rtCamera.localScale = new Vector3(scaleX, scaleY, 1f);

            

                Debug.Log($"ImageUI position: {rt.anchoredPosition}, scale: {scale}");
                scaleInitialized = true;
            }
        }

        private Texture2D CreateTransparentMaskTexture(List<ResultBoxWithMask> results, Texture2D sourceTexture)
        {
            int w = sourceTexture.width;
            int h = sourceTexture.height;
            
            // Reutilizar la textura si ya existe y tiene el tamaño correcto
            if (transparentTexture == null || transparentTexture.width != w || transparentTexture.height != h)
            {
                // Liberar la textura anterior si existe
                if (transparentTexture != null)
                    Destroy(transparentTexture);
                    
                transparentTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            }
            
            // Limpiar la textura directamente sin crear un nuevo array
            transparentTexture.SetPixels(new Color[w * h]); // Más eficiente que iterar pixel por pixel
            
            // Dibujar las siluetas y los recuadros en la textura transparente
            foreach (ResultBoxWithMask box in results)
            {
                // Dibujar la silueta
                Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
                
                // Usar bloque using para garantizar la liberación del tensor
                using (box.masks.tensorOnDevice)
                {
                    TextureTools.RenderMaskOnTransparentTexture(box.masks, transparentTexture, sourceTexture, boxColor);
                    
                    // Dibujar el recuadro
                    int boxWidth = (int)(box.score / MinBoxConfidence);
                    TextureTools.DrawRectOutline(transparentTexture, box.rect, boxColor, boxWidth, rectIsNormalized: false, revertY: true);
                }
                // No es necesario llamar a Dispose manualmente al usar using

                // Obtener el nombre de la clase detectada
                string className = box.bestClassIndex < classNames.Length ? 
                    classNames[box.bestClassIndex] : 
                    $"Clase {box.bestClassIndex}";
                
                // Debug object type con el nombre de la clase
                Debug.Log($"Objeto detectado: {className}, Score: {box.score:F2}, Rect: {box.rect}");
            }
            
            transparentTexture.Apply();
            return transparentTexture;
        }

        void OnDisable()
        {
            nn.Dispose();
            textureProvider.Stop();
            
            // Liberar la textura transparente al deshabilitar
            if (transparentTexture != null)
            {
                Destroy(transparentTexture);
                transparentTexture = null;
            }
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
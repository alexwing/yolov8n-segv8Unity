using NN;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
            YOLOv8OutputReader.DiscardThreshold = MinBoxConfidence;
            Texture2D texture = GetNextTexture();

            // Debug.Log("Texture resolution: " + texture.width + "x" + texture.height);
            // Debug.Log("Yolo model resolution: " + nn.model.inputs[0].shape[5] + "x" + nn.model.inputs[0].shape[6]);

            var boxes = yolo.Run(texture);
            DrawResults(boxes, texture);
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
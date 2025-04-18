using NN;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Segmentator : Detector
    {
        YOLOv8Segmentation yolo;

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

            Debug.Log("Texture resolution: " + texture.width + "x" + texture.height);
            Debug.Log("Yolo model resolution: " + nn.model.inputs[0].shape[5] + "x" + nn.model.inputs[0].shape[6]);

            var boxes = yolo.Run(texture);
            DrawResults(boxes, texture);
            ImageUI.texture = texture;
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
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Assets.Scripts.TextureProviders
{
    [Serializable]
    public class WebCamTextureProvider : TextureProvider
    {
        [Tooltip("Leave empty for automatic selection.")]
        [SerializeField]
        private string cameraName;

        [SerializeField]
        internal string DeviceName = null;

        [SerializeField]
        internal int TargetFPS = 60;

        private WebCamTexture webCamTexture;

        public WebCamTextureProvider(int width, int height, TextureFormat format = TextureFormat.RGB24, string cameraName = null) : base(width, height, format)
        {
            cameraName = cameraName != null ? cameraName : SelectCameraDevice();
            webCamTexture = new WebCamTexture(cameraName);
            InputTexture = webCamTexture;
        }

        public WebCamTextureProvider(WebCamTextureProvider provider, int width, int height, TextureFormat format = TextureFormat.RGB24) : this(width, height, format, provider?.cameraName)
        {
            if (provider != null)
            {
                this.DeviceName = provider.DeviceName;
                this.TargetFPS = provider.TargetFPS;
            }
        }

        public override void Start()
        {
            string deviceName = DeviceName;
            if (string.IsNullOrEmpty(deviceName))
                deviceName = SelectCameraDevice();

            webCamTexture = new WebCamTexture(deviceName, requestedWidth: 1280, requestedHeight: 720, requestedFPS: TargetFPS);
            webCamTexture.Play();
            InputTexture = webCamTexture;
        }

        public override void Stop()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                GameObject.Destroy(webCamTexture);
                webCamTexture = null;
            }
        }

        public override TextureProviderType.ProviderType TypeEnum()
        {
            return TextureProviderType.ProviderType.WebCam;
        }

        public override Texture GetRawTexture()
        {
            // Retorna la textura de la webcam sin procesar
            return webCamTexture;
        }

        /// <summary>
        /// Return first backfaced camera name if avaible, otherwise first possible
        /// </summary>
        private string SelectCameraDevice()
        {
            if (WebCamTexture.devices.Length == 0)
                throw new Exception("Any camera isn't avaible!");

            foreach (var cam in WebCamTexture.devices)
            {
                if (!cam.isFrontFacing)
                    return cam.name;
            }
            return WebCamTexture.devices[0].name;
        }
    }
}
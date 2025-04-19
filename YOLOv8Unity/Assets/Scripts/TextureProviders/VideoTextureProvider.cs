using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Scripts.TextureProviders
{
    [Serializable]
    public class VideoTextureProvider : TextureProvider
    {
        [SerializeField]
        private VideoClip videoClip;

        [SerializeField]
        internal VideoClip Clip = null;

        private VideoPlayer videoPlayer;
        private RenderTexture videoRenderTexture;

        public VideoTextureProvider(int width, int height, TextureFormat format = TextureFormat.RGB24) : base(width, height, format)
        {
        }

        public VideoTextureProvider(VideoTextureProvider provider, int width, int height, TextureFormat format = TextureFormat.RGB24) : this(width, height, format)
        {
            if (provider != null)
                this.Clip = provider.Clip;
        }

        public override void Start()
        {
            if (Clip == null)
                throw new NullReferenceException("Video clip isn't set");

            videoPlayer = new GameObject("VideoPlayer").AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.clip = Clip;
            videoPlayer.isLooping = true;

            videoRenderTexture = new RenderTexture(1920, 1080, 0);
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRenderTexture;

            videoPlayer.Play();
            InputTexture = videoRenderTexture;
        }

        public override Texture GetRawTexture()
        {
            // Devolver la textura sin procesar del video
            return videoRenderTexture;
        }

        public override void Stop()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                GameObject.Destroy(videoPlayer.gameObject);
                videoPlayer = null;
            }

            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
                GameObject.Destroy(videoRenderTexture);
                videoRenderTexture = null;
            }
        }

        public override TextureProviderType.ProviderType TypeEnum()
        {
            return TextureProviderType.ProviderType.Video;
        }

        public override Texture2D GetTexture()
        {
            bool reachedEnd = (ulong)videoPlayer.frame == videoPlayer.frameCount - 1;
            if (reachedEnd && videoPlayer.isLooping)
                videoPlayer.frame = 0;

            videoPlayer.StepForward();
            InputTexture = videoPlayer.texture ? videoPlayer.texture : ResultTexture;
            return base.GetTexture();
        }
    }
}
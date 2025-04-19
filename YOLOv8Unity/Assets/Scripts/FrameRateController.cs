using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Clase para controlar y limitar el frame rate
    /// </summary>
    public class FrameRateController
    {
        private float lastUpdateTime;
        private int targetFrameRate;
        private float frameInterval;
        private bool unlimitedFrameRate;

        /// <summary>
        /// Frame rate objetivo actual
        /// </summary>
        public int TargetFrameRate => targetFrameRate;

        /// <summary>
        /// Inicializa un nuevo controlador de frame rate
        /// </summary>
        /// <param name="initialFrameRate">Frame rate objetivo (0 = sin límite)</param>
        public FrameRateController(int initialFrameRate = 0)
        {
            SetFrameRate(initialFrameRate);
            lastUpdateTime = Time.time;
        }

        /// <summary>
        /// Establece un nuevo frame rate objetivo
        /// </summary>
        /// <param name="frameRate">Frame rate objetivo (0 = sin límite)</param>
        public void SetFrameRate(int frameRate)
        {
            targetFrameRate = frameRate;
            unlimitedFrameRate = frameRate <= 0;
            frameInterval = unlimitedFrameRate ? 0 : 1f / frameRate;
        }

        /// <summary>
        /// Comprueba si es el momento de actualizar según el frame rate configurado
        /// </summary>
        /// <returns>True si debe actualizarse, False si no ha pasado suficiente tiempo</returns>
        public bool ShouldUpdate()
        {
            if (unlimitedFrameRate)
                return true;

            float currentTime = Time.time;
            if (currentTime - lastUpdateTime >= frameInterval)
            {
                lastUpdateTime = currentTime;
                return true;
            }

            return false;
        }
    }
}
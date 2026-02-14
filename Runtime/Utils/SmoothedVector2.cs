using UnityEngine;

namespace TouchScript.Utils
{
    public class SmoothedVector2
    {
        private Vector2 _value = Vector2.zero;
        private bool _initialized = false;
        private readonly float _baseWeight;
        private readonly int _maxSamples;
        private int _count;

        public Vector2 Value => _value;

        /// <param name="samples">
        /// Approximate number of samples to smooth over (e.g. 5–30)
        /// </param>
        public SmoothedVector2(int maxSamples)
        {
            _maxSamples = Mathf.Max(1, maxSamples);
            _baseWeight = 2f / (_maxSamples + 1f);
            _count = 0;
        }

        /// <summary>
        /// Updates the filter with a new angle (radians)
        /// and returns a smoothed normalized direction vector.
        /// </summary>
        public Vector2 Update(float angleRad)
        {
            Vector2 input = new Vector2(
                Mathf.Cos(angleRad),
                Mathf.Sin(angleRad)
            );
            return Update(input);
        }

        /// <summary>
        /// Updates the filter with a new vector
        /// and returns a smoothed normalized direction vector.
        /// </summary>
        public Vector2 Update(Vector2 input)
        {
            if (input.sqrMagnitude <= 1e-8f)
                return _value;

            input = input.normalized;

            if (!_initialized)
            {
                _value = input;
                _initialized = true;
                _count = 1;
                return _value;
            }

            // Exponential moving average
            _count++;
            float weight = ComputeWeight(_count);
            _value = weight * input + (1f - weight) * _value;

            // Normalize to stay on unit circle
            if (_value.SqrMagnitude() > 1e-8f)
                _value = _value.normalized;

            return _value;
        }

        /// <summary>
        /// returns the angle representation of the smoothed normalized direction vector.
        /// The range is from 0 to 2 PI
        /// </summary>
        public float ToAngle()
        {
            float angle = Mathf.Atan2(_value.y, _value.x);
            const float TWO_PI = 2 * Mathf.PI;
            if (angle < 0f) angle += TWO_PI;
            return angle;
        }

        public void Reset()
        {
            _initialized = false;
            _count = 0;
            _value = Vector2.zero;
        }

        float ComputeWeight(int sampleCount)
        {
            float scale = Mathf.Min(1f, sampleCount / (float) _maxSamples);
            return _baseWeight * scale;
        }
    }
}

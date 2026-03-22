// Copyright CodeGamified 2025-2026
// MIT License — Rubik
using UnityEngine;

namespace Rubik.Game
{
    /// <summary>
    /// Identity component on each cubie GameObject.
    /// Stickers are colored once at creation and never change.
    /// Tracks sticker face normals in grid space; normals rotate with face moves.
    /// </summary>
    public class RubikCubie : MonoBehaviour
    {
        public struct Sticker
        {
            public Vector3Int Normal;
            public int ColorIndex;
        }

        private Sticker[] _stickers;

        public int StickerCount => _stickers?.Length ?? 0;

        public void Init(Sticker[] stickers)
        {
            _stickers = stickers;
        }

        /// <summary>
        /// Returns the color index of the sticker facing the given direction, or -1 if none.
        /// </summary>
        public int GetColorAt(Vector3Int faceNormal)
        {
            if (_stickers == null) return -1;
            for (int i = 0; i < _stickers.Length; i++)
                if (_stickers[i].Normal == faceNormal)
                    return _stickers[i].ColorIndex;
            return -1;
        }

        /// <summary>
        /// Rotate all sticker normals by 90° around the given axis.
        /// direction: 1 = CW looking from +axis, -1 = CCW.
        /// </summary>
        public void RotateStickers(Vector3 axis, int direction)
        {
            if (_stickers == null) return;
            Quaternion q = Quaternion.AngleAxis(direction * 90f, axis);
            for (int i = 0; i < _stickers.Length; i++)
            {
                Vector3 n = new Vector3(_stickers[i].Normal.x, _stickers[i].Normal.y, _stickers[i].Normal.z);
                Vector3 r = q * n;
                _stickers[i].Normal = new Vector3Int(
                    Mathf.RoundToInt(r.x), Mathf.RoundToInt(r.y), Mathf.RoundToInt(r.z));
            }
        }
    }
}

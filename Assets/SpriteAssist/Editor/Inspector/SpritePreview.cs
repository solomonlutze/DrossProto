﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpriteAssist
{
    public class SpritePreview : IDisposable
    {
        public Rect rect;

        private List<SpritePreviewWireframe> _wireframes;
        private string _infoText;
        private TextureInfo _textureInfo;

        public SpritePreview(List<SpritePreviewWireframe> wireframes)
        {
            _wireframes = wireframes;
        }

        public void Update(Rect rect, Sprite baseSprite, Sprite dummySprite, TextureInfo textureInfo, SpriteConfigData configData)
        {
            this.rect = rect;
            _textureInfo = textureInfo;
            _infoText = "";

            foreach (var wireframe in _wireframes)
            {
                wireframe.UpdateAndResize(this.rect, baseSprite, dummySprite, _textureInfo, configData);
                _infoText += wireframe.GetInfo(textureInfo) + "\n";
            }
        }

        public void Show(bool hasMultipleTargets)
        {
            foreach (var wireframe in _wireframes)
            {
                wireframe.Draw(rect, _textureInfo);
            }

            if (!hasMultipleTargets)
            {
                var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, richText = true, fontStyle = FontStyle.Bold };
                EditorGUI.DropShadowLabel(rect, _infoText, style);
            }
        }

        public void SetWireframes(List<SpritePreviewWireframe> wireframes)
        {
            Dispose();

            _wireframes = wireframes;
        }

        public void Dispose()
        {
            foreach (var wireframe in _wireframes)
            {
                wireframe.Dispose();
            }

            _wireframes.Clear();
        }
    }
}

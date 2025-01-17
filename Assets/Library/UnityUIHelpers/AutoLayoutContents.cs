using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Library.UnityUIHelpers
{
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class AutoLayoutContents : MonoBehaviour
    {
        private enum Alignment
        {
            Left,
            Right,
            Center
        }
        
        [SerializeField] private float _paddingLeft;
        [SerializeField] private float _paddingRight;
        [SerializeField] private float _paddingTop;
        [SerializeField] private float _spacingHorizontal;
        [SerializeField] private float _spacingVertical;
        [SerializeField] private Alignment _alignment;

        private const float UPDATE_FREQUENCY = 0f;

        private RectTransform _rectTransform;
        private float _lastUpdateTime;
        private readonly List<List<RectTransform>> _table = new();

        private void Awake()
        {
            AssignRectTransform();
        }

        private void Update()
        {
            if (!Application.isPlaying && Time.time > _lastUpdateTime + UPDATE_FREQUENCY)
            {
                HandleEditor();
            }
        }

        private void HandleEditor()
        {
            AssignRectTransform();
            RefreshLayout();
            
            _lastUpdateTime = Time.time;
        }

        private void AssignRectTransform()
        {
            _rectTransform ??= GetComponent<RectTransform>();
        }

        private void RefreshLayout()
        {
            _table.Clear();

            LayoutChildrenObjects();
            ApplyAlignment();
        }

        private List<RectTransform> GetRectTransformChildren()
        {
            List<RectTransform> children = new List<RectTransform>();

            foreach (Transform child in gameObject.transform)
            {
                if (child.TryGetComponent(out RectTransform rectTransform))
                    children.Add(rectTransform);
            }

            return children;
        }
        
        private void LayoutChildrenObjects()
        {
            List<RectTransform> rectTransformChildren = GetRectTransformChildren();
            if (rectTransformChildren.Count == 0)
                return;

            RectTransform previousRectTransform = rectTransformChildren.First();
            Vector2 currentPosition = new Vector2(
                -_rectTransform.rect.width / 2f + previousRectTransform.rect.width / 2f + _paddingLeft,
                _rectTransform.rect.height / 2f - previousRectTransform.rect.height / 2f - _paddingTop
            );

            UIPositionHelper.SetAbsoluteAnchoredCenterPosition(previousRectTransform, currentPosition);
            List<RectTransform> row = new() {previousRectTransform};

            for (int i = 1; i < gameObject.transform.childCount; i++)
            {
                if (!gameObject.transform.GetChild(i).TryGetComponent(out RectTransform currentRectTransform))
                    continue;

                float offsetX = previousRectTransform.rect.width / 2f + currentRectTransform.rect.width / 2f + _spacingHorizontal;

                if (currentPosition.x + offsetX + currentRectTransform.rect.width / 2f + _paddingRight > _rectTransform.rect.width / 2f)
                {
                    var offsetY = -previousRectTransform.rect.height - _spacingVertical;
                    currentPosition = new Vector2(
                        -_rectTransform.rect.width / 2f + currentRectTransform.rect.width / 2f + _paddingLeft,
                        currentPosition.y + offsetY
                    );
                    
                    _table.Add(row);
                    row = new() {currentRectTransform};
                }
                else
                {
                    row.Add(currentRectTransform);
                    currentPosition.x += offsetX;
                }
                
                UIPositionHelper.SetAbsoluteAnchoredCenterPosition(currentRectTransform, currentPosition);
                
                previousRectTransform = currentRectTransform;
            }
            
            _table.Add(row);
        }

        private void ApplyAlignment()
        {
            foreach (var row in _table)
            {
                foreach (RectTransform rectTransform in row)
                {
                    float borderX = GetRectHorizontalBorder(row.LastOrDefault());
                    float offsetX = GetAlignmentOffset(borderX);
                    
                    Vector2 targetPosition = rectTransform.anchoredPosition + Vector2.right * offsetX;
                    UIPositionHelper.SetAnchoredCenterPosition(rectTransform, targetPosition);
                }
            }
        }

        private float GetAlignmentOffset(float borderX)
        {
            switch (_alignment)
            {
                case Alignment.Right:
                    return _rectTransform.rect.width - borderX + _paddingRight;
                case Alignment.Center:
                    return (_rectTransform.rect.width - borderX) / 2;
                default:
                    return 0;
            }
        }

        private float GetRectHorizontalBorder(RectTransform rectTransform)
        {
            return rectTransform.anchoredPosition.x + rectTransform.rect.width / 2;
        }
    }
}

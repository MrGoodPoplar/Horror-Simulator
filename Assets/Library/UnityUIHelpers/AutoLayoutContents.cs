using System.Collections.Generic;
using System.Linq;
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

            if (Application.isPlaying)
                ClearChildren();
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

        public void RefreshLayout()
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
                if (child.gameObject.activeSelf && child.TryGetComponent(out RectTransform rectTransform))
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
                -GetRectTransformWidth(_rectTransform)  / 2f + GetRectTransformWidth(previousRectTransform) / 2f + _paddingLeft,
                GetRectTransformHeight(_rectTransform) / 2f - GetRectTransformHeight(previousRectTransform) / 2f - _paddingTop
            );

            UIPositionHelper.SetAbsoluteAnchoredCenterPosition(previousRectTransform, currentPosition);
            List<RectTransform> row = new() {previousRectTransform};

            for (int i = 1; i < gameObject.transform.childCount; i++)
            {
                if (!gameObject.transform.GetChild(i).TryGetComponent(out RectTransform currentRectTransform))
                    continue;

                float offsetX = GetRectTransformWidth(previousRectTransform) / 2f + GetRectTransformWidth(currentRectTransform) / 2f + _spacingHorizontal;

                if (currentPosition.x + offsetX + GetRectTransformWidth(currentRectTransform) / 2f + _paddingRight > GetRectTransformWidth(_rectTransform) / 2f)
                {
                    var offsetY = -GetRectTransformHeight(previousRectTransform) - _spacingVertical;
                    currentPosition = new Vector2(
                        -GetRectTransformWidth(_rectTransform) / 2f + GetRectTransformWidth(currentRectTransform) / 2f + _paddingLeft,
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

        private float GetRectTransformWidth(RectTransform rectTransform)
        {
            return rectTransform.rect.width * rectTransform.localScale.x;
        }
        
        private float GetRectTransformHeight(RectTransform rectTransform)
        {
            return rectTransform.rect.height * rectTransform.localScale.y;
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
                    return GetRectTransformWidth(_rectTransform) - borderX - _paddingRight;
                case Alignment.Center:
                    return (GetRectTransformWidth(_rectTransform) - borderX) / 2;
                default:
                    return 0;
            }
        }

        private float GetRectHorizontalBorder(RectTransform rectTransform)
        {
            return rectTransform.anchoredPosition.x + GetRectTransformWidth(rectTransform) / 2;
        }

        public void AttachChild(Transform newChild)
        {
            newChild.SetParent(transform);
        }
        
        public void ClearChildren()
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
        }
    }
}

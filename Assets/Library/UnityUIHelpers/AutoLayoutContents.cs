using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Library.UnityUIHelpers
{
    [ExecuteAlways]
    public class AutoLayoutContents : MonoBehaviour
    {
        [SerializeField] private float _paddingLeft;
        [SerializeField] private float _paddingRight;
        [SerializeField] private float _paddingTop;
        [SerializeField] private float _spacingHorizontal;
        [SerializeField] private float _spacingVertical;

        private const float UPDATE_FREQUENCY = 0.5f;
        
        private float _lastUpdateTime;

        private  void Awake()
        { 
            LayoutChildrenObjects(gameObject);
        }

        private void Update()
        {
            if (Application.isEditor && Time.time > _lastUpdateTime + UPDATE_FREQUENCY)
            {
                LayoutChildrenObjects(gameObject);
                _lastUpdateTime = Time.time;
            }
        }

        private void LayoutChildrenObjects(GameObject parent = null)
        {
            // Default to the current GameObject if no parent is specified.
            parent ??= gameObject;

            // Ensure the parent has a RectTransform.
            RectTransform parentRectTransform = parent.GetComponent<RectTransform>();
            if (!parentRectTransform)
            {
                Debug.LogWarning("Parent does not have a RectTransform. Layout aborted.");
                return;
            }

            // Initialize row count and check for children.
            if (parent.transform.childCount < 1)
            {
                return;
            }

            // Process the first child.
            Transform firstChild = parent.transform.GetChild(0);
            RectTransform firstChildRect = firstChild.GetComponent<RectTransform>();
            if (!firstChildRect)
            {
                Debug.LogWarning("First child does not have a RectTransform. Layout aborted.");
                return;
            }

            Vector2 currentPosition = new Vector2(
                -parentRectTransform.rect.width / 2f + firstChildRect.rect.width / 2f + _paddingLeft,
                parentRectTransform.rect.height / 2f - firstChildRect.rect.height / 2f - _paddingTop
            );

            UIPositionHelper.SetAbsoluteAnchoredCenterPosition(firstChildRect, currentPosition);

            // Iterate through the rest of the children.
            RectTransform previousRectTransform = firstChildRect;
            for (int i = 1; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                RectTransform currentRectTransform = child.GetComponent<RectTransform>();

                if (!currentRectTransform)
                {
                    Debug.LogWarning($"Child at index {i} does not have a RectTransform. Skipping.");
                    continue;
                }

                // Calculate offsets and positions.
                float offsetX = previousRectTransform.rect.width / 2f + currentRectTransform.rect.width / 2f + _spacingHorizontal;

                // Check for horizontal overflow.
                if (currentPosition.x + offsetX + currentRectTransform.rect.width / 2f + _paddingRight > parentRectTransform.rect.width / 2f)
                {
                    var offsetY = -previousRectTransform.rect.height - _spacingVertical;
                    currentPosition = new Vector2(
                        -parentRectTransform.rect.width / 2f + currentRectTransform.rect.width / 2f + _paddingLeft,
                        currentPosition.y + offsetY
                    );
                }
                else
                {
                    currentPosition.x += offsetX;
                }

                UIPositionHelper.SetAbsoluteAnchoredCenterPosition(currentRectTransform, currentPosition);
                previousRectTransform = currentRectTransform;
            }
            
            return;
        }
    }
}

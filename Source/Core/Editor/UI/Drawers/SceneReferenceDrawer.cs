using System;
using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(UniqueNameReference))]
    public class SceneReferenceDrawer : AbstractDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            UniqueNameReference nameReference = (UniqueNameReference)currentValue;

            Rect nextPosition = new Rect(rect.x, rect.y, rect.width, rect.height);

            nextPosition = DrawerLocator.GetDrawerForValue(nameReference.SceneReferenceType, typeof(SceneReferenceType)).Draw(nextPosition, nameReference.SceneReferenceType, (value) => nameReference.SceneReferenceType = (SceneReferenceType)value, label);

            float height = EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;

            nextPosition.y = rect.y + height;


            switch (nameReference.SceneReferenceType)
            {
                case SceneReferenceType.Object:
                    nextPosition = new UniqueNameReferenceDrawer().Draw(nextPosition, currentValue, changeValueCallback, " ");
                    height += EditorDrawingHelper.SingleLineHeight;
                    nextPosition.y = rect.y + height;
                    break;
                case SceneReferenceType.Category:
                    height += EditorDrawingHelper.SingleLineHeight;
                    nextPosition.y = rect.y + height;
                    break;
                case SceneReferenceType.Dynamic:
                    break;
            }

            rect.height = height;
            return rect;
        }
    }
}

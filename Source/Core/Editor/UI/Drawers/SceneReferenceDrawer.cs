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

            rect = DrawerLocator.GetDrawerForValue(nameReference.SceneReferenceType, typeof(SceneReferenceType)).Draw(rect, nameReference.SceneReferenceType, (value) => nameReference.SceneReferenceType = (SceneReferenceType)value, label);

            rect.y += EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;

            switch (nameReference.SceneReferenceType)
            {
                case SceneReferenceType.Object:
                    rect = new UniqueNameReferenceDrawer().Draw(rect, currentValue, changeValueCallback, label).height;
                    rect.y += EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;
                    break;
                case SceneReferenceType.Category:
                    break;
                case SceneReferenceType.Dynamic:
                    break;
            }

            return rect;
        }
    }
}

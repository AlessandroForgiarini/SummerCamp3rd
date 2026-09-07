using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    [CreateAssetMenu]
    public class ElementsListSO : ScriptableObject
    {
        [Serializable]
        public enum ElementType
        {
            INVALID,
            FIRE,
            WATER,
            GRASS
        }

        [Serializable]
        public struct ElementData
        {
            public ElementType type;
            public Color color;
        }

        public List<ElementData> elements;

        public Color GetColorFromElement(ElementType elementType)
        {
            Color color = Color.magenta;
            foreach (ElementData element in elements)
            {
                if (elementType == element.type)
                {
                    color = element.color;
                    break;
                }
            }
            return color;
        }
    }
}
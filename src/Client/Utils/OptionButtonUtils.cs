using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Utils
{
    public static class OptionButtonUtils
    {
        /// <summary>
        /// Generic method to populate an OptionButton with the values from an enum
        /// </summary>
        /// <param name="optionButton"></param>
        /// <typeparam name="T"></typeparam>
        public static void PopulateOptionButton<T>(this OptionButton optionButton) where T : Enum
        {
            optionButton.Clear();
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                // This will return the enum value as a string, or it will
                // call a custom override in EnumUtils for this type if we
                // have defined one
                optionButton.AddItem(EnumUtils.GetLabel(value));
            }

        }
    }
}
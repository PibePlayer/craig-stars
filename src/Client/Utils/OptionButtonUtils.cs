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
        public static void PopulateOptionButton<T>(this OptionButton optionButton, Func<T, string> GetLabelAction = null) where T : Enum
        {
            optionButton.Clear();
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                // This will return the enum value as a string, or it will
                // call a custom override in EnumUtils for this type if we
                // have defined one
                if (GetLabelAction != null)
                {
                    optionButton.AddItem(GetLabelAction(value));

                }
                else
                {
                    optionButton.AddItem(EnumUtils.GetLabel<T>((T)value));
                }
            }

        }
    }
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CraigStars
{
    /// <summary>
    /// A stack of a single ShipDesign type in a fleet.  A fleet is made up of many Ship Stacks
    /// </summary>
    public class ShipToken
    {
        [JsonIgnore]
        public ShipDesign Design { get; set; } = new ShipDesign();
        public int Quantity { get; set; }

        #region Serializer Helpers

        public string DesignName
        {
            get
            {
                if (designName == null)
                {
                    designName = Design.Name;
                }
                return designName;
            }
            set
            {
                designName = value;
            }
        }
        string designName;

        /// <summary>
        /// Prepare this object for serialization
        /// </summary>
        public void PreSerialize()
        {
            designName = null;
        }

        /// <summary>
        /// After serialization, wire up values we stored by guid
        /// </summary>
        /// <param name="planetsByGuid"></param>
        public void PostSerialize(Dictionary<string, ShipDesign> designsByName)
        {
            designsByName.TryGetValue(DesignName, out var design);
            Design = design;
        }

        #endregion

    }
}
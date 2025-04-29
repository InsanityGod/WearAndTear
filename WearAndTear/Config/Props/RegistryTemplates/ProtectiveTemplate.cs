using Newtonsoft.Json.Linq;

namespace WearAndTear.Config.Props.RegistryTemplates
{
    public class ProtectiveTemplate
    {
        /// <summary>
        /// The general part properties
        /// </summary>
        public PartProps PartProps { get; set; } = new();

        /// <summary>
        /// The properties specific to protective parts
        /// </summary>
        public ProtectivePartProps ProtectiveProps { get; set; } = new();

        internal JContainer AsMergedJContainer()
        {
            var container = (JContainer)JToken.FromObject(PartProps);
            container.Merge(JToken.FromObject(ProtectiveProps));
            return container;
        }
    }
}
using Newtonsoft.Json.Linq;

namespace WearAndTear.Config.Props.RegistryTemplates
{
    public class WearAndTearProtectiveTemplate
    {
        /// <summary>
        /// The general part properties
        /// </summary>
        public WearAndTearPartProps PartProps { get; set; } = new();

        /// <summary>
        /// The properties specific to protective parts
        /// </summary>
        public WearAndTearProtectivePartProps ProtectiveProps { get; set; } = new();

        internal JContainer AsMergedJContainer()
        {
            var container = (JContainer)JToken.FromObject(PartProps);
            container.Merge(JToken.FromObject(ProtectiveProps));
            return container;
        }
    }
}
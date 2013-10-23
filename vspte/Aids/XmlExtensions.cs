using System.Xml.Linq;

namespace vspte.Aids
{
    internal static class XmlExtensions
    {
        public static XElement FixNamespace(this XElement element, string @namespace)
        {
            element.Name = XName.Get(element.Name.ToString(), @namespace);

            foreach (var child in element.Elements())
            {
                child.FixNamespace(@namespace);
            }

            return element;
        }
    }
}
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Serialization;

namespace Api.Serialization {
    public class CustomXmlSerializerOutputFormatter : XmlSerializerOutputFormatter {
        public CustomXmlSerializerOutputFormatter()
            : base(new XmlWriterSettings {
                OmitXmlDeclaration =false,
#if DEBUG
                Indent = true,
                NewLineHandling = NewLineHandling.Entitize,
#endif
            }) {
        }

        public CustomXmlSerializerOutputFormatter(ILoggerFactory loggerFactory)
            : base(loggerFactory) {
        }

        public CustomXmlSerializerOutputFormatter(XmlWriterSettings writerSettings)
            : base(writerSettings) {
        }

        public CustomXmlSerializerOutputFormatter(XmlWriterSettings writerSettings, ILoggerFactory loggerFactory)
            : base(writerSettings, loggerFactory) {
        }

        protected override void Serialize(XmlSerializer xmlSerializer, XmlWriter xmlWriter, object value) {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "http://subsonic.org/restapi");

            xmlSerializer.Serialize(xmlWriter, value, namespaces);
        }
    }
}

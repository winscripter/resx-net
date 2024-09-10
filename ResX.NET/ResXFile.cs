using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ResX.NET
{
    /// <summary>
    /// Represents a single &lt;data&gt; tag in a RESX files.
    /// </summary>
    /// <param name="Name">Name of the data.</param>
    /// <param name="Value">Value of the data.</param>
    /// <param name="Comment">Comment of the data (if qualified).</param>
    public record ResXData(string Name, string Value, string? Comment);

    /// <summary>
    /// Parses and builds a RESX file. This class cannot be inherited.
    /// </summary>
    public sealed partial class ResXFile
    {
        /// <summary>
        /// A list of descendant ResX data tags.
        /// </summary>
        public List<ResXData> DataList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResXFile"/> class.
        /// </summary>
        /// <param name="dataList">List of <see cref="ResXData"/> to start from.</param>
#if NET8_0_OR_GREATER // Only applicable if the project uses C# 12
        [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
#endif
        public ResXFile(List<ResXData> dataList)
        {
            DataList = dataList;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResXFile"/> class.
        /// </summary>
        public ResXFile() : this(new List<ResXData>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResXFile"/> class.
        /// </summary>
        /// <param name="singleData">A single instance of <see cref="ResXData"/> to start from.</param>
        public ResXFile(ResXData singleData) : this(new List<ResXData>() { singleData })
        {
        }

        /// <summary>
        /// Converts this instance of <see cref="ResXFile"/> to a ResX plain text string.
        /// </summary>
        /// <returns>A plain string.</returns>
        public string Build()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            builder.AppendLine("<root>");

            builder.AppendLine(string.Join("\r\n", RootHeader.Split(new[] { "\r\n" }, StringSplitOptions.None).Select(line =>
            {
                var nestedBuilder = new StringBuilder();
                nestedBuilder.Append("  ");
                nestedBuilder.Append(line);
                return nestedBuilder.ToString();
            })));

            foreach (var data in DataList)
            {
                var nestedBuilder = new StringBuilder();
                nestedBuilder.Append("  ");
                nestedBuilder.Append("<data name=\"");
                nestedBuilder.Append(data.Name);
                nestedBuilder.AppendLine("\">");
                nestedBuilder.Append("    ");
                nestedBuilder.Append("<value>");
                nestedBuilder.Append(data.Value);
                nestedBuilder.AppendLine("</value>");
                if (data.Comment != null)
                {
                    nestedBuilder.Append("    ");
                    nestedBuilder.Append("<comment>");
                    nestedBuilder.Append(data.Comment);
                    nestedBuilder.AppendLine("</comment>");
                }
                nestedBuilder.AppendLine("  </data>");
                builder.Append(nestedBuilder);
            }

            builder.AppendLine("</root>");
            return builder.ToString();
        }

        /// <summary>
        /// Parses the raw plain text of the ResX file into an enumerable of
        /// <see cref="ResXData"/>.
        /// </summary>
        /// <param name="rawText">Input plain text of the ResX file.</param>
        /// <returns>An enumerable of <see cref="ResXData"/>.</returns>
        /// <exception cref="XmlException">In this method, thrown when the root tag is missing.</exception>
        /// <exception cref="ResXBadRootTagException">In this method, thrown when the root tag name is not "root".</exception>
        /// <exception cref="ResXNoAttributeException">In this method, thrown when the data tag does not have the required attribute.</exception>
        /// <exception cref="ResXNoValueException">In this method, thrown when the data tag does not have a child &lt;value&gt; tag.</exception>
        public static IEnumerable<ResXData> Parse(string rawText)
        {
            var rootTag = XDocument.Parse(rawText).Root ?? throw new XmlException("Root tag is missing");
            if (!rootTag.Name.LocalName.Equals("root", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ResXBadRootTagException($"Root tag of the ResX file is \"{rootTag.Name.LocalName}\", but expected \"root\"");
            }

            var resxData = new List<ResXData>();
            foreach (var descendant in rootTag.Elements())
            {
                if (!descendant.Name.LocalName.Equals("data", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                string nameAttribute = descendant.Attribute("name")?.Value ?? throw new ResXNoValueException("No attribute named \"name\"");
                string value = descendant.Descendants()
                    .Where(tag => tag.Name.LocalName.Equals("value", StringComparison.CurrentCultureIgnoreCase))
                    .FirstOrDefault()?.Value ?? throw new ResXNoAttributeException("Data does not have descendant element of type \"value\"");
                string? comment = descendant.Descendants()
                    .Where(tag => tag.Name.LocalName.Equals("comment", StringComparison.CurrentCultureIgnoreCase))
                    .FirstOrDefault()?.Value;

                resxData.Add(new ResXData(nameAttribute, value, comment));
            }

            return resxData;
        }
    }
}

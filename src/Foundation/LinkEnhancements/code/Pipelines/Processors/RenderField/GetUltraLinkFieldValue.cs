using Foundation.LinkEnhancements.Xml;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.RenderField;
using Sitecore.Xml.Xsl;

namespace Foundation.LinkEnhancements.Pipelines.Processors.RenderField
{
    public class GetUltraLinkFieldValue
    {
        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Process(RenderFieldArgs args)
        {
            if (!SkipProcessor(args))
            {
                SetWebEditParameters(args, "class", "text", "target", "haschildren");
                if (!string.IsNullOrEmpty(args.Parameters["text"]))
                {
                    args.WebEditParameters["text"] = args.Parameters["text"];
                }

                UltraLinkRenderer linkRenderer = CreateRenderer(args.Item);
                linkRenderer.FieldName = args.FieldName;
                linkRenderer.FieldValue = args.FieldValue;
                linkRenderer.Parameters = args.Parameters;
                linkRenderer.RawParameters = args.RawParameters;
                args.DisableWebEditFieldWrapping = true;
                args.DisableWebEditContentEditing = true;
                RenderFieldResult renderFieldResult = linkRenderer.Render();
                args.Result.FirstPart = renderFieldResult.FirstPart;
                args.Result.LastPart = renderFieldResult.LastPart;
            }
        }

        /// <summary>
        /// Sets the webedit parameters.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="parameterNames">The parameter names.</param>
        /// <contract>
        ///   <requires name="args" condition="not null" />
        ///   <requires name="parameterNames" condition="not null" />
        /// </contract>
        private static void SetWebEditParameters(RenderFieldArgs args, params string[] parameterNames)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(parameterNames, "parameterNames");
            foreach (string key in parameterNames)
            {
                if (!string.IsNullOrEmpty(args.Parameters[key]))
                {
                    args.WebEditParameters[key] = args.Parameters[key];
                }
            }
        }

        /// <summary>
        /// Creates the renderer.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The renderer.</returns>
        protected virtual UltraLinkRenderer CreateRenderer(Item item)
        {
            return new UltraLinkRenderer(item);
        }

        /// <summary>
        /// Checks if the field should not be handled by the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>true if the field should not be handled by the processor; otherwise false</returns>
        protected virtual bool SkipProcessor(RenderFieldArgs args)
        {
            if (args == null)
            {
                return true;
            }
            return args.FieldTypeKey != "ultra link";
        }
    }
}

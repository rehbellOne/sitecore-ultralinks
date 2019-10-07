//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Globalization;
//using System.IO;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Mvc.Html;
//using Foundation.LinkEnhancements.Mvc.Html;
//using Sitecore.Data.Items;
//using Sitecore.Diagnostics;
//using Sitecore.Mvc.Extensions;
//using Sitecore.Mvc.Helpers;
//using Sitecore.Mvc.Pipelines;
//using Sitecore.Mvc.Pipelines.Response.RenderRendering;
//using Sitecore.Pipelines;
//using Sitecore.Pipelines.RenderField;
//using Sitecore.Xml.Xsl;
//using Sitecore.Mvc.Presentation;

//namespace Foundation.LinkEnhancements.Mvc.Helpers
//{
//    public class UltraLinkHelper //: SitecoreHelper
//    {
//        //internal Action<Rendering, StringWriter> RenderRenderingPipeline = (Action<Rendering, StringWriter>)((rendering, writer) => UltraLinkHelper.RunRenderRenderingPipeline(rendering, (TextWriter)writer));
//        protected virtual Stack<string> EndFieldStack => this._endFieldStack ?? (this._endFieldStack = new Stack<string>());
//        public ViewContext ViewContext => this.HtmlHelper.ViewContext;
//        private Stack<string> _endFieldStack;
//        public virtual Item CurrentItem
//        {
//            get
//            {
//                Rendering currentRendering = this.CurrentRendering;
//                if (currentRendering == null)
//                    return PageContext.Current.Item;
//                return currentRendering.Item;
//            }
//        }

//        public virtual Rendering CurrentRendering
//        {
//            get
//            {
//                return RenderingContext.CurrentOrNull.ValueOrDefault<RenderingContext, Rendering>((Func<RenderingContext, Rendering>)(context => context.Rendering));
//            }
//        }
        

//        public string FormatValue(object value, string format)
//        {
//            return FormatValueInternal(value, format);
//        }
//        internal static string FormatValueInternal(object value, string format)
//        {
//            if (value == null)
//                return string.Empty;
//            if (string.IsNullOrEmpty(format))
//                return System.Convert.ToString(value, (IFormatProvider)CultureInfo.CurrentCulture);
//            return string.Format((IFormatProvider)CultureInfo.CurrentCulture, format, value);
//        }
//        //public string FormatValue()
//        internal HtmlHelper HtmlHelper { get; set; }

//        public UltraLinkHelper(HtmlHelper htmlHelper)// : base(htmlHelper)
//        {

//        }

//        #region Normal Render Field

//        internal virtual RenderFieldArgs GetRenderFieldArgs(Item item, string fieldName)
//        {
//            return new RenderFieldArgs
//            {
//                Item = item,
//                FieldName = fieldName
//            };
//        }


//        public object GetModelStateValue(string key, Type destinationType)
//        {
//            ModelState modelState;
//            if (this.HtmlHelper.ViewData.ModelState.TryGetValue(key, out modelState) && modelState.Value != null)
//                return modelState.Value.ConvertTo(destinationType, (CultureInfo)null);
//            return (object)null;
//        }
//        private static void RunRenderRenderingPipeline(Rendering rendering, TextWriter writer)
//        {
//            PipelineService.Get().RunPipeline<RenderRenderingArgs>("mvc.renderRendering", new RenderRenderingArgs(rendering, writer));
//        }
//        public static UltraLinkHtml BeginRenderLink(UltraLinkHelper htmlHelper, string fieldName, Item dataSourceItem, NameValueCollection htmlAttributes = null)
//        {
//            //call LinkBuilder
//            return UltraLinkExtensions.BeginRenderLink(htmlHelper, fieldName, dataSourceItem, htmlAttributes);
//        }

//        public virtual HtmlString RenderUltraLink(string fieldName, Item item)
//        {
//            Assert.ArgumentNotNull(fieldName, "fieldName");
//            RenderFieldArgs renderFieldArgs = GetRenderFieldArgs(item, fieldName);


//            renderFieldArgs.Item = (renderFieldArgs.Item ?? CurrentItem);

//            if (renderFieldArgs.Item == null)
//            {
//                EndFieldStack.Push(string.Empty);
//                return new HtmlString(string.Empty);
//            }

//            SetFieldRenderParameters(renderFieldArgs, fieldName);

//            RunPipeline("renderField", renderFieldArgs);
            
//            RenderFieldResult result2 = renderFieldArgs.Result;

//            string value = result2.ValueOrDefault((RenderFieldResult result) => result.FirstPart).OrEmpty();
//            string item2 = result2.ValueOrDefault((RenderFieldResult result) => result.LastPart).OrEmpty();

//            EndFieldStack.Push(item2);

//            return new HtmlString(value);
//        }
//        internal virtual void RunPipeline(string pipelineName, PipelineArgs args)
//        {
//            CorePipeline.Run("renderField", args);
//        }

//        private static void SetFieldRenderParameters(RenderFieldArgs args, string fieldName)
//        {
//            if (args.Item?.Fields[fieldName] != null && args.Item.Fields[fieldName].TypeKey == "ultra link")
//            {
//                args.RenderParameters["linebreaks"] = "<br/>";
//            }
//        }
//        #endregion



//        public virtual HtmlString EndRenderUltraLink()
//        {
//            Stack<string> endFieldStack = EndFieldStack;
//            if (endFieldStack.Count == 0)
//            {
//                throw new InvalidOperationException("There was a call to EndField with no corresponding call to BeginField");
//            }
//            string value = endFieldStack.Pop();
//            return new HtmlString(value);
//        }
//    }
//}
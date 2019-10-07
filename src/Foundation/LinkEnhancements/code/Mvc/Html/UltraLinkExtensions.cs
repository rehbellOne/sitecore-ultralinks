

//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq.Expressions;
//using System.Text;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Mvc.Html;
//using System.Web.Mvc.Properties;
//using System.Web.Routing;
//using Foundation.LinkEnhancements.CustomFields;
//using Foundation.LinkEnhancements.Mvc.Helpers;
//using Sitecore.Collections;
//using Sitecore.Data.Items;
//using Sitecore.Diagnostics;
//using Sitecore.Mvc.Extensions;
//using Sitecore.Mvc.Helpers;

//namespace Foundation.LinkEnhancements.Mvc.Html
//{
//    public static class UltraLinkExtensions
//    {
//        private static readonly string UltraLinkHelperKey = "UltraLinkHelperKey";

//        public static UltraLinkHelper UltraLinks(this HtmlHelper htmlHelper)
//        {
//            Assert.ArgumentNotNull(htmlHelper, "htmlHelper");

//            var helper = htmlHelper.ViewData[UltraLinkHelperKey] as UltraLinkHelper;

//            if (helper != null)
//            {
//                return helper;
//            }

//            helper = new UltraLinkHelper(htmlHelper);

//            htmlHelper.ViewData[UltraLinkHelperKey] = helper;

//            return helper;
//        }

//        public static UltraLinkHtml BeginRenderLink(this UltraLinkHelper htmlHelper, UltraLinkField field, SafeDictionary<string> htmlAttributes = null)
//        {
//            //call LinkBuilder
//            return UltraLinkExtensions.TagHelper(htmlHelper, field, htmlAttributes == null ? null : htmlAttributes.ToDictionary());
//        }

//        public static UltraLinkHtml BeginRenderLink(this UltraLinkHelper htmlHelper, string fieldName, Item dataSourceItem, SafeDictionary<string> htmlAttributes = null)
//        {
//            if (dataSourceItem == null) return null;

//            var ultraLinkField = ((UltraLinkField)dataSourceItem.Fields[fieldName]) as UltraLinkField;
            
//            return UltraLinkExtensions.TagHelper(htmlHelper, ultraLinkField, htmlAttributes == null ? null : htmlAttributes.ToDictionary());
//        }

//        private static UltraLinkHtml TagHelper(UltraLinkHelper htmlHelper, UltraLinkField field, IDictionary<string, string> htmlAttributes)
//        {
//            TagBuilder tagBuilder = new TagBuilder("a");

//            //tagBuilder.MergeAttributes<string, string>(htmlAttributes);
           
//            htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
            
//            UltraLinkHtml mvcForm = new UltraLinkHtml(htmlHelper.ViewContext);

//            return mvcForm;
//        }

//        internal static void EndUltraLink(ViewContext viewContext)
//        {
//            viewContext.Writer.Write("</a>");
//        }

//    }
//}
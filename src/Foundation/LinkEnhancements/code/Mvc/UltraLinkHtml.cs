//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Mvc.Html;
//using System.Web.Mvc.Properties;
//using Foundation.LinkEnhancements.Mvc.Helpers;
//using Foundation.LinkEnhancements.Mvc.Html;
//using Sitecore.Data.Items;
//using Sitecore.Diagnostics;
//using Sitecore.Mvc.Extensions;
//using Sitecore.Pipelines;
//using Sitecore.Pipelines.RenderField;
//using Sitecore.Xml.Xsl;

//namespace Foundation.LinkEnhancements.Mvc
//{
//    public class UltraLinkHtml : IDisposable
//    {
//        public readonly ViewContext ViewContext;
//        private bool _disposed;

//        public UltraLinkHtml(ViewContext viewContext)
//        {
//            if (viewContext == null)
//                throw new ArgumentNullException(nameof(viewContext));

//            this.ViewContext = viewContext;
//        }

//        public void Dispose()
//        {
//            this.Dispose(true);
//            GC.SuppressFinalize((object) this);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (this._disposed)
//                return;
//            this._disposed = true;
//            UltraLinkExtensions.EndUltraLink(this.ViewContext);
//        }
//        public void EndUltraLink()
//        {
//            this.Dispose(true);
//        }

//    }
//}
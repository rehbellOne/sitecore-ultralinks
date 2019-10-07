using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Text;
using Sitecore.Xml.Xsl;

namespace Foundation.LinkEnhancements.Xml
{
    public class UltraLinkUrl
    {
        public string GetUrl(Item item, string fieldName)
        {
            Database database = item.Database;
            Field field = item.Fields[fieldName];
            if (field == null)
                return string.Empty;

            return this.GetUrl(new XmlField(field, string.Empty), database);
        }
        public string GetUrl(XmlField field, Database database)
        {
            Assert.ArgumentNotNull((object)field, nameof(field));
            Assert.ArgumentNotNull((object)database, nameof(database));
            //string attribute1 = field.GetAttribute("linktype");
            //string attribute2 = field.GetAttribute("url");
            //string attribute3 = field.GetAttribute("id");
            //string anchor = field.GetAttribute("anchor");
            //string attribute4 = field.GetAttribute("querystring");
            //if (!string.IsNullOrEmpty(anchor))
            //    anchor = "#" + anchor;
            //if (attribute1 == "anchor")
            //    return anchor;
            //if (attribute1 == "external")
            //    return this.GetExternalUrl(attribute2);
            //if (attribute1 == "internal")
            //    return this.GetInternalUrl(database, attribute2, attribute3, anchor, attribute4);
            //if (attribute1 == "javascript")
            //    return this.GetJavaScriptUrl(attribute2);
            //if (attribute1 == "mailto")
            //    return this.GetMailToLink(attribute2);
            //if (attribute1 == "media")
            //    return this.GetMediaUrl(database, attribute3);
            return string.Empty;
        }

        protected virtual string GetExternalUrl(string url)
        {
            Assert.ArgumentNotNull((object)url, nameof(url));
            if (!url.StartsWith("/", StringComparison.InvariantCulture) && url.IndexOf("://", StringComparison.InvariantCulture) < 0)
                url = "http://" + url;
            return url;
        }

        protected virtual UrlString GetUrlString(Item item)
        {
            return new UrlString(LinkManager.GetItemUrl(item, UrlOptions.DefaultOptions));
        }
        protected virtual string GetPhoneLink(string url)
        {
            Assert.ArgumentNotNull((object)url, nameof(url));
            if (!url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                url = "mailto:" + url;
            return url;
        }
        protected virtual string GetMailToLink(string url)
        {
            Assert.ArgumentNotNull((object)url, nameof(url));
            if (!url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                url = "mailto:" + url;
            return url;
        }
        protected virtual string GetQueryString(string queryString)
        {
            Assert.ArgumentNotNull((object)queryString, nameof(queryString));
            if (!string.IsNullOrEmpty(queryString))
                return "?" + queryString;
            return queryString;
        }
    }
}
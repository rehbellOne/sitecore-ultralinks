using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Foundation.LinkEnhancements.CustomFields;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Xml.Xsl;

namespace Foundation.LinkEnhancements.Xml
{
    public class UltraLinkRenderer : FieldRendererBase
    {
        private readonly char[] _delimiter = new char[2]
        {
            '=',
            '&'
        };

        private string _fieldName;
        private string _fieldValue;
        private UltraLinkField _linkField;
        private SafeDictionary<string> _parameters;
        private string _rawParameters;

        public UltraLinkRenderer(Item item)
        {
            Assert.ArgumentNotNull((object) item, nameof(item));
            this.Item = item;
        }

        public string FieldName
        {
            get { return this._fieldName; }
            set
            {
                Assert.ArgumentNotNull((object) value, nameof(value));
                this._fieldName = value;
            }
        }

        public string FieldValue
        {
            get { return this._fieldValue; }
            set
            {
                Assert.ArgumentNotNull((object) value, nameof(value));
                this._fieldValue = value;
            }
        }

        public Item Item { get; }

        public SafeDictionary<string> Parameters
        {
            get { return this._parameters; }
            set
            {
                Assert.ArgumentNotNull((object) value, nameof(value));
                this._parameters = value;
            }
        }

        public string RawParameters
        {
            get { return this._rawParameters; }
            set
            {
                Assert.ArgumentNotNull((object) value, nameof(value));
                this._rawParameters = value;
            }
        }

        protected UltraLinkField LinkField
        {
            get
            {
                if (string.IsNullOrEmpty(this.FieldName))
                    return (UltraLinkField) null;
                if (this._linkField == null)
                    this._linkField = new UltraLinkField(this.Item.Fields[this.FieldName], this.FieldValue);
                return this._linkField;
            }
        }

        protected string LinkType
        {
            get
            {
                UltraLinkField linkField = this.LinkField;
                if (linkField != null)
                    return linkField.LinkType;
                return "internal";
            }
        }

        protected Item TargetItem
        {
            get
            {
                UltraLinkField linkField = this.LinkField;
                if (linkField != null)
                    return linkField.TargetItem;
                return this.Item;
            }
        }
        
        private static readonly Set<string> ReservedSet = Set<string>.Create(new string[11]
        {
            "field",
            "select",
            "text",
            "haschildren",
            "before",
            "after",
            "enclosingtag",
            "fieldname",
            "disable-web-editing",
            "query",
            "anchor"
        });

        protected void SetParameters(SafeDictionary<string> attributes, SafeDictionary<string> parameters, string key, string defaultValue)
        {
            string keyResult;

            if (parameters.TryGetValue(key, out keyResult))
            {
                attributes[key] = keyResult;
                return;
            }
            
            if(!string.IsNullOrEmpty(defaultValue))
            {
                attributes[key] = defaultValue;
            }

        }
        protected void SetParameters(SafeDictionary<string> attributes, string key, string defaultValue)
        {
            string keyResult;
            if (attributes.TryGetValue(key, out keyResult))
            {
                return;
            }

            if (!string.IsNullOrEmpty(defaultValue))
            {
                attributes[key] = defaultValue;
            }
        }

        protected string BuildInternalUrl(UltraLinkField linkField) {

            if (linkField.TargetItem != null)
            {
                if (string.IsNullOrEmpty(linkField.Url))
                {
                    return LinkManager.GetItemUrl(linkField.TargetItem, UrlOptions.DefaultOptions);
                }
                else
                {
                    if (linkField.Url.StartsWith("/"))
                    {
                        var path = linkField.Url;
                        var foundItem = linkField.TargetItem.Database.GetItem(path);
                        return LinkManager.GetItemUrl(foundItem, UrlOptions.DefaultOptions);
                    }
                }
            }
            return string.Empty;
        }

        protected string BuildExternalUrl(UltraLinkField linkField)
        {
            var urlValue = !string.IsNullOrEmpty(linkField.Url) ? linkField.Url : linkField.CustomLink;
            
            if (string.IsNullOrEmpty(urlValue)) return string.Empty;

            if (linkField.ForceHttps)
            {
                if (!urlValue.StartsWith("https://"))
                {
                    if (urlValue.StartsWith("http://"))
                    {
                        return urlValue.Replace("http://", "https://");
                    }
                    else
                    {
                        return "https://" + urlValue;
                    }
                }
                return urlValue;
            }
            else
            {
                if (!urlValue.StartsWith("http://"))
                {
                    if (urlValue.StartsWith("https://"))
                    {
                        return urlValue.Replace("https://", "http://");
                    }
                    else
                    {
                        return "http://" + urlValue;
                    }
                }
                return urlValue;
            }
        }

        protected string BuildPhone(UltraLinkField linkField)
        {
            var urlValue = !string.IsNullOrEmpty(linkField.Url) ? linkField.Url : linkField.CustomLink;
            if (string.IsNullOrEmpty(urlValue)) return string.Empty;

            if (!urlValue.StartsWith("tel:+"))
            {
                return "tel:+" + urlValue;
            }
            return urlValue;
        }

        protected string BuildEmail(UltraLinkField linkField)
        {
            var urlValue = !string.IsNullOrEmpty(linkField.Url) ? linkField.Url : linkField.CustomLink;
            if (string.IsNullOrEmpty(urlValue)) return string.Empty;

            if (!urlValue.StartsWith("mailTo:"))
            {
                return "mailTo:" + urlValue;
            }
            return urlValue;
        }

        protected string AppendAnchorTarget(SafeDictionary<string> attributes, UltraLinkField linkField, string url)
        {
            if (!linkField.IsExternal && !linkField.IsInternal) return url;

            var anchor = attributes.ContainsKey("anchor") ? attributes["anchor"] : linkField.Anchor;

            if (!string.IsNullOrEmpty(anchor))
            {
                if (!url.EndsWith("#"))
                {
                    url = url + "#" + anchor;
                }
                else
                {
                    url = url + anchor;
                }
            }
            return url;
        }

        protected string AppendQueryString(SafeDictionary<string> attributes, UltraLinkField linkField, string url)
        {
            if (string.IsNullOrEmpty(url) || !(linkField.IsExternal || linkField.IsInternal)) return url;
            
            var developerQueryParams = new NameValueCollection();

            if (attributes.ContainsKey("query"))
            {
                var pairs = attributes["query"].Split('&');
                foreach (var pair in pairs)
                {
                    var split = pair.Split('=');
                    var key = split[0];

                    if (split.Length > 1)
                    {
                        developerQueryParams.Add(key, split[1]);
                    }
                    else
                    {
                        developerQueryParams.Add(key, string.Empty);
                    }
                }
            }

            var builder = new StringBuilder();

            foreach (string key in linkField.QueryStringParameters)
            {
                var overrideValue = developerQueryParams[key];

                var value = linkField.QueryStringParameters[key];

                if (!string.IsNullOrEmpty(overrideValue))
                {
                    builder.AppendFormat("{0}={1}&", key, overrideValue);
                }
                else
                {
                    builder.AppendFormat("{0}={1}&", key, value);
                }
            }

            string query = null;
            if (builder.Length > 0)
            {
                query = builder.Remove(builder.Length - 1, 1).ToString();
                builder.Clear();
            }

            builder.Append(url);

            if (query != null)
            {

                if (!url.EndsWith("?"))
                {
                    builder.Append("?");
                }
                builder.Append(query);
            }


            return builder.ToString();
        }

        protected string BuildUrl(SafeDictionary<string> attributes, UltraLinkField linkField)
        {
            string output = null;

            switch (linkField.LinkType)
            {
                case "internal":
                    output = BuildInternalUrl(linkField);
                    break;
                case "external":
                    output = BuildExternalUrl(linkField);
                    break;
                case "phone":
                    output = BuildPhone(linkField);
                    break;
                case "email":
                    output = BuildEmail(linkField);
                    break;
            }

            output = AppendQueryString(attributes, linkField, output);
            output = AppendAnchorTarget(attributes, linkField, output);

            return output;
        }

        protected void SetCustomAttributes(SafeDictionary<string> attributes, UltraLinkField linkField)
        {
            foreach (string key in linkField.CustomAttributes)
            {
                if (!ReservedSet.Contains(key.ToLowerInvariant()))
                {
                    var value = linkField.CustomAttributes[key];

                    if (!attributes.ContainsKey(key))
                    {
                        attributes[key] = value;
                    }
                }
            }
        }

        public virtual RenderFieldResult Render()
        {
            UltraLinkField linkField = this.LinkField;

            if (linkField == null)
            {
                return new RenderFieldResult()
                {
                    FirstPart = string.Empty,
                    LastPart = string.Empty
                };
            }


            SafeDictionary<string> attributes = new SafeDictionary<string>();
            
            if (this.Parameters.Any())
            {
                attributes.AddRange((SafeDictionary<string, string>)this.Parameters);
            }

            this.SetParameters(attributes,"class", linkField.Class);
            this.SetParameters(attributes,"id", linkField.ElementId);
            this.SetRelAttribute(attributes, linkField);
            this.SetExternalTargetAttribute(attributes, linkField);
            this.SetCustomAttributes(attributes, linkField);
            this.SetParameters(attributes, "href", this.BuildUrl(attributes, linkField));

            StringBuilder tag = new StringBuilder(linkField.TextBeforeLink);

            if (!string.IsNullOrEmpty(linkField.TextBeforeLink))
            {
                tag.Append(" ");
            }

            tag.Append("<a");

            foreach (KeyValuePair<string, string> keyValuePair in (SafeDictionary<string, string>)attributes)
            {
                string key = keyValuePair.Key;
                string value = keyValuePair.Value;

                if (!ReservedSet.Contains(key.ToLowerInvariant()))
                {
                    FieldRendererBase.AddAttribute(tag, key, value);
                }
            }
            tag.Append('>');

            tag.Append(GetInnerText(attributes, linkField));

            tag.Append("</a>");

            if (!string.IsNullOrEmpty(linkField.TextAfterLink))
            {
                tag.Append(" ");
            }

            return new RenderFieldResult()
            {
                FirstPart = tag.ToString(),
                LastPart = linkField.TextAfterLink
            };
        }

        protected string GetInnerText(SafeDictionary<string> attributes, UltraLinkField linkField)
        {
            if (attributes.ContainsKey("text"))
            {
                return attributes["text"];
            }

            if (attributes.ContainsKey("description"))
            {
                return attributes["description"];
            }

            if (linkField.IsInternal)
            {
                if (linkField.UseItemDisplayName && linkField.TargetItem != null)
                {
                    return string.IsNullOrEmpty(linkField.TargetItem.DisplayName) ? linkField.TargetItem.Name : linkField.TargetItem.DisplayName;
                }
                return linkField.LinkDisplayText;
            }

            if (!string.IsNullOrEmpty(linkField.LinkDisplayText))
            {
                return linkField.LinkDisplayText;
            }

            if (linkField.LinkType == "phone")
            {
                if (linkField.PhoneMask != null && !string.IsNullOrEmpty(linkField.CustomLink))
                {
                    var maskValue = linkField.PhoneMask["Mask"];

                    if (string.IsNullOrEmpty(maskValue)) return linkField.CustomLink;

                    var mask = maskValue?.Replace(".", "\\.").Replace("_", "-");

                    return FormatPhoneNumber(linkField.CustomLink, mask);
                }
            }

            if (string.IsNullOrEmpty(linkField.LinkDisplayText))
            {
                return linkField.CustomLink;
            }

            return string.Empty;
        }
        protected string FormatPhoneNumber(string phoneNumber, string mask)
        {
            var cleanPhone = Regex.Replace(phoneNumber, "[^0-9]", "");

            if (string.IsNullOrEmpty(cleanPhone)) return phoneNumber;


            return System.Convert.ToInt64(cleanPhone).ToString(mask);
        }
        protected virtual string GetUrl(XmlField field)
        {
            return field != null ? new UltraLinkUrl().GetUrl(field, this.Item.Database) : LinkManager.GetItemUrl(this.Item, UrlOptions.DefaultOptions);
        }

        protected virtual void SetExternalTargetAttribute(SafeDictionary<string> attributes, UltraLinkField linkField)
        {
            if ((linkField.IsExternal || linkField.IsInternal) && linkField.OpenNewWindow)
            {
                attributes["target"] = "_blank";
            }
        }
        protected virtual void SetRelAttribute(SafeDictionary<string> attributes, UltraLinkField linkField)
        {
            var relBuilder = new StringBuilder();

            if ((linkField.IsExternal || linkField.IsInternal) && (linkField.OpenNewWindow || Settings.ProtectExternalLinksWithBlankTarget))
            {
                relBuilder.Append("noopener ");
            }

            if ((linkField.IsExternal || linkField.IsInternal) && linkField.NoReferrer)
            {
                relBuilder.Append("noreferrer ");
            }
            if (linkField.NoFollow)
            {
                relBuilder.Append("nofollow ");
            }
            if (linkField.NoIndex)
            {
                relBuilder.Append("noindex ");
            }

            var relValue = relBuilder.ToString().TrimEnd();

            attributes["rel"] = relValue;
        }
    }
}
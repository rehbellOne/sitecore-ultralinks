using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Foundation.LinkEnhancements.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.IO;
using Sitecore.Links;
using Sitecore.Xml.Xsl;

namespace Foundation.LinkEnhancements.CustomFields
{
    public class UltraLinkField : XmlField
    {
        private NameValueCollection _queryStringParameters;
        private NameValueCollection _customAttributes;
        
        private void GetCustomAttributes(NameValueCollection customAttributes, string attributeStr)
        {
            if (string.IsNullOrWhiteSpace(attributeStr))
            {
                return;
            }

            var groups = attributeStr.Split(',');

            foreach (var group in groups)
            {
                var pair = group.Split('|');

                if (pair.Length < 2)
                {
                    continue;
                }

                customAttributes.Add($"{pair[0]}", pair[1]);
            }
        }

        private string SetCustomAttributes(NameValueCollection attributes)
        {
            var builder = new StringBuilder();

            foreach (KeyValuePair<string, string> pair in attributes)
            {
                builder.Append($"{pair.Key}|{pair.Value},");
            }
            builder.Remove(builder.Length, 1);

            return builder.ToString();
        }

        public NameValueCollection CustomAttributes
        {
            get
            {
                if (this._customAttributes == null)
                {
                    this._customAttributes = new NameValueCollection();

                    GetCustomAttributes(_customAttributes, this.GetAttribute("customAttributes"));

                }
                return this._customAttributes;
            }
            set
            {
                _customAttributes = value;

                this.SetAttribute("customAttributes", SetCustomAttributes(this._customAttributes));
            }
        }



        public NameValueCollection QueryStringParameters
        {
            get
            {
                if (this._queryStringParameters == null)
                {
                    this._queryStringParameters = new NameValueCollection();

                    GetCustomAttributes(_queryStringParameters, this.GetAttribute("queryStringParameters"));
                }
                return this._queryStringParameters;
            }
            set
            {
                _queryStringParameters = value;

                this.SetAttribute("queryStringParameters", SetCustomAttributes(this._queryStringParameters));
            }
        }

        public string TextBeforeLink
        {
            get
            {
                return this.GetAttribute("textBeforeLink");
            }
            set
            {
                this.SetAttribute("textBeforeLink", value);
            }
        }

        public string LinkDisplayText
        {
            get
            {
                return this.GetAttribute("linkDisplayText");
            }
            set
            {
                this.SetAttribute("linkDisplayText", value);
            }
        }

        public string TextAfterLink
        {
            get
            {
                return this.GetAttribute("textAfterLink");
            }
            set
            {
                this.SetAttribute("textAfterLink", value);
            }
        }

        public string Class
        {
            get
            {
                return this.GetAttribute("cssClass");
            }
            set
            {
                this.SetAttribute("cssClass", value);
            }
        }

        public string Anchor
        {
            get
            {
                return this.GetAttribute("anchorTarget");
            }
            set
            {
                this.SetAttribute("anchorTarget", value);
            }
        }

        public string InternalPath
        {
            get
            {
                if (this.IsInternal)
                {
                    string part2 = string.IsNullOrEmpty(this.Url) ? this.GetFriendlyUrl() : this.Url;
                    if (part2.Length > 0)
                    {
                        if (!part2.StartsWith("/sitecore", StringComparison.OrdinalIgnoreCase))
                            part2 = FileUtil.MakePath("/sitecore/content", part2);
                        return part2;
                    }
                }
                return string.Empty;
            }
        }

     
        //TODO:
        public string Url
        {
            get
            {
                return this.GetAttribute("url");
            }
            set
            {
                this.SetAttribute("url", value);
            }
        }

        private string _linkType;
        private bool _linkTypeSet;

        public string LinkType
        {

            get
            {
                if (!_linkTypeSet)
                {
                    _linkType = this.GetAttribute("linkType");
                    _linkTypeSet = true;
                }
                return _linkType;
            }
            set
            {
                this.SetAttribute("linkType", value);
                _linkType = value;
                _linkTypeSet = true;
            }
        }
        public ID TargetID
        {
            get
            {
                string attribute = this.GetAttribute("id");
                if (ID.IsID(attribute))
                    return ID.Parse(attribute);
                return ID.Null;
            }
            set
            {
                this.SetAttribute("id", value.ToString());
            }
        }

        public Item TargetItem
        {
            get
            {
                ID targetId = this.TargetID;
                if (!targetId.IsNull)
                    return this.InnerField.Database.Items[targetId];
                string internalPath = this.InternalPath;
                if (internalPath.Length > 0)
                    return this.InnerField.Database.Items[internalPath];
                return (Item)null;
            }
        }

        public bool NoIndex { get { return this.GetAttribute("noIndexCheckBox") == "true"; } }
        public bool NoFollow { get { return this.GetAttribute("noFollowCheckBox") == "true"; } }
        public bool NoReferrer { get { return this.GetAttribute("noReferrerCheckBox") == "true"; } }
        public bool ForceHttps { get { return this.GetAttribute("forceScureLinkCheckBox") == "true"; } }
        public bool IsInternal { get { return this.LinkType == "internal"; } }
        public bool IsExternal { get { return this.LinkType == "external"; } }
        
        public bool OpenNewWindow { get { return this.GetAttribute("openNewWindowCheckBox") == "true"; } }
        public bool UseItemDisplayName { get { return this.GetAttribute("useDisplayNameCheckBox") == "true"; } }

        public string LinkTypesLoadedItem
        {
            get
            {

                var itemTarget = this.GetAttribute("linkTypesLoadedItem");

                var item = this.GetDatabase().GetItem(ID.Parse(itemTarget));

                return this.GetAttribute("linkTypesLoadedItem");
            }
            set
            {
                this.SetAttribute("linkTypesLoadedItem", value);
            }
        }

        private Item _phoneMask;
        private bool _phoneMaskSet;

        public Item PhoneMask
        {
            get
            {
                if (!_phoneMaskSet)
                {
                    var targetId = this.GetAttribute("phoneMasksLoadedItem");

                    if (!string.IsNullOrEmpty(targetId))
                    {
                        _phoneMask = this.GetDatabase().GetItem(ID.Parse(targetId));
                    }
                    _phoneMaskSet = true;
                }
                return _phoneMask;
            }
        }

        public string CustomLink
        {
            get
            {
                return this.GetAttribute("customLink");
            }
            set
            {
                this.SetAttribute("customLink", value);
            }
        }     

        public string ElementId
        {
            get
            {
                return this.GetAttribute("elementId");
            }
            set
            {
                this.SetAttribute("elementId", value);
            }
        }

        public static implicit operator UltraLinkField(Field field)
        {
            if (field != null)
            {
                return new UltraLinkField(field);
            }
            return (UltraLinkField)null;
        }

        public UltraLinkField(Field innerField) :base(innerField, "ultralink")
        {

        }
        public UltraLinkField(Field innerField, string runtimeValue) : base(innerField, "ultralink", runtimeValue)
        {

        }
       
        public override void Relink(ItemLink itemLink, Item newLink)
        {
            if (!(this.TargetID == itemLink.TargetItemID))
                return;
            if (this.IsInternal)
                this.Url = newLink.Paths.ContentPath;
          
            this.TargetID = newLink.ID;
        }

        public override void RemoveLink(ItemLink itemLink)
        {
            if (!(this.TargetID == itemLink.TargetItemID))
                return;
            this.Clear();
        }

        public override void UpdateLink(ItemLink itemLink)
        {
            Item targetItem = itemLink.GetTargetItem();
            if (targetItem == null || !(this.TargetID == targetItem.ID))
                return;
            if (!this.IsInternal) return;
            
            this.Url = targetItem.Paths.ContentPath;
        }

        public override void ValidateLinks(LinksValidationResult result)
        {
            if (!this.IsInternal) return;

            if (this.TargetID.IsNull && string.IsNullOrEmpty(this.InternalPath))
                return;
                
            Item targetItem = this.TargetItem;

            if (targetItem != null)
                result.AddValidLink(targetItem, this.InternalPath);
            else
                result.AddBrokenLink(this.InternalPath);
        }
        public void Clear()
        {
            this.Value = string.Empty;
        }

        public string GetFriendlyUrl()
        {
            return new UltraLinkUrl().GetUrl(new XmlField(this.InnerField, string.Empty), this.InnerField.Database);
        }

    }
}

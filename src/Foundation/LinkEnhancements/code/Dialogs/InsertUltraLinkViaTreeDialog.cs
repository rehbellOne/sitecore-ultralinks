using System.Net;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Shell;
using Sitecore.Speak.Applications;
using Sitecore.Web;
using Sitecore.Web.PageCodes;

namespace Foundation.LinkEnhancements.Dialogs
{
    public class InsertUltraLinkViaTreeDialog : PageCodeBase
    {

        //Legacy Fields
        
        public Sitecore.Mvc.Presentation.Rendering TreeView { get; set; }

        public Sitecore.Mvc.Presentation.Rendering ListViewToggleButton { get; set; }

        public Sitecore.Mvc.Presentation.Rendering TreeViewToggleButton { get; set; }

        public Sitecore.Mvc.Presentation.Rendering InsertEmailButton { get; set; }

        public Sitecore.Mvc.Presentation.Rendering InsertAnchorButton { get; set; }


        //Ultra Link Fields

        //Behavior

        public Sitecore.Mvc.Presentation.Rendering ElementId { get; set; }
        public Sitecore.Mvc.Presentation.Rendering CustomLink { get; set; }
        public Sitecore.Mvc.Presentation.Rendering AnchorTarget { get; set; }
        public Sitecore.Mvc.Presentation.Rendering LinkTypes { get; set; }
        public Sitecore.Mvc.Presentation.Rendering LinkTypesLoadedValue { get; set; }
        public Sitecore.Mvc.Presentation.Rendering LinkTypesLoadedItem { get; set; }
        public Sitecore.Mvc.Presentation.Rendering RootItem { get; set; }


        //Custom
        public Sitecore.Mvc.Presentation.Rendering CustomAttributes { get; set; }
        public Sitecore.Mvc.Presentation.Rendering QueryStringParameters { get; set; }

        //Presentation

        public Sitecore.Mvc.Presentation.Rendering TextBeforeLink { get; set; }
        public Sitecore.Mvc.Presentation.Rendering LinkDisplayText { get; set; }
        public Sitecore.Mvc.Presentation.Rendering TextAfterLink { get; set; }
        public Sitecore.Mvc.Presentation.Rendering CssClass { get; set; }
        public Sitecore.Mvc.Presentation.Rendering UseDisplayNameCheckBox { get; set; }

        //SEO

        public Sitecore.Mvc.Presentation.Rendering NoFollowCheckBox { get; set; }
        public Sitecore.Mvc.Presentation.Rendering NoIndexCheckBox { get; set; }
        public Sitecore.Mvc.Presentation.Rendering NoReferrerCheckBox { get; set; }
        


        //Phone
        public Sitecore.Mvc.Presentation.Rendering PhoneMasks { get; set; }
        public Sitecore.Mvc.Presentation.Rendering PhoneMasksLoadedValue { get; set; }
        public Sitecore.Mvc.Presentation.Rendering PhoneMasksLoadedItem { get; set; }


        //Security
        public Sitecore.Mvc.Presentation.Rendering OpenNewWindowCheckBox { get; set; }
        public Sitecore.Mvc.Presentation.Rendering ForceSecureLinkCheckBox { get; set; }    



        public override void Initialize()
        {
            var setting = Settings.GetSetting("BucketConfiguration.ItemBucketsEnabled");
            this.ListViewToggleButton.Parameters["IsVisible"] = setting;
            this.TreeViewToggleButton.Parameters["IsVisible"] = setting;
            this.TreeView.Parameters["ShowHiddenItems"] = UserOptions.View.ShowHiddenItems.ToString();
            this.TreeView.Parameters["ContentLanguage"] = WebUtil.GetQueryString("la");
            this.ReadQueryParamsAndUpdatePlaceholders();
        }

        private string GetXmlAttributeValue(XElement element, string attrName)
        {
            return element.Attribute((XName)attrName) != null ? element.Attribute((XName)attrName).Value : string.Empty;
        }

        private void ReadQueryParamsAndUpdatePlaceholders()
        {
            var source = WebUtil.GetQueryString("ro");

            var queryString2 = WebUtil.GetQueryString("hdl");

            if (!string.IsNullOrEmpty(source) && source != "{0}")
            {
                this.TreeView.Parameters["RootItem"] = source;
            }

            Item contextItem = ((Database)ClientHost.Databases.ContentDatabase).GetItem(source ?? string.Empty) ?? ((Database)ClientHost.Databases.ContentDatabase).GetRootItem();

            if (contextItem != null)
            {
                this.RootItem.Parameters["Text"] = contextItem.Paths.LongID;
            }

            this.InsertAnchorButton.Parameters["Click"] = string.Format(this.InsertAnchorButton.Parameters["Click"], (object)WebUtility.UrlEncode(source), (object)WebUtility.UrlEncode(queryString2));
            this.InsertEmailButton.Parameters["Click"] = string.Format(this.InsertEmailButton.Parameters["Click"], (object)WebUtility.UrlEncode(source), (object)WebUtility.UrlEncode(queryString2));
            this.ListViewToggleButton.Parameters["Click"] = string.Format(this.ListViewToggleButton.Parameters["Click"], (object)WebUtility.UrlEncode(source), (object)WebUtility.UrlEncode(queryString2));

            var flag = queryString2 != string.Empty;

            var text = string.Empty;

            if (flag)
            {
                text = UrlHandle.Get()["va"];
            }

            if (text == string.Empty && contextItem != null) {
                this.TreeView.Parameters["PreLoadPath"] = contextItem.Paths.LongID;
            }

            if (text == string.Empty)
            {
                return;
            }

            XElement element = XElement.Parse(text);

            

            if (!string.IsNullOrEmpty(this.GetXmlAttributeValue(element, "id")))
            {
                Item linkedItem = (Item)SelectMediaDialog.GetMediaItemFromQueryString(this.GetXmlAttributeValue(element, "id"));

                if (contextItem != null && linkedItem != null && linkedItem.Paths.LongID.StartsWith(contextItem.Paths.LongID))
                {
                    var preLoadPath = contextItem.ID.ToString() + linkedItem.Paths.LongID.Substring(contextItem.Paths.LongID.Length);

                    this.TreeView.Parameters["PreLoadPath"] = preLoadPath;
                }
            }

           
          
            //All Links
            this.LinkTypes.Parameters["Text"] = this.GetXmlAttributeValue(element, "linkTypes");
            this.LinkTypesLoadedValue.Parameters["Text"] = this.GetXmlAttributeValue(element, "linkTypesLoadedValue");
            this.LinkTypesLoadedItem.Parameters["Text"] = this.GetXmlAttributeValue(element, "linkTypesLoadedItem");            

            //External/Phone/Email
            this.CustomLink.Parameters["Text"] = this.GetXmlAttributeValue(element, "customLink");

            //Internal Link            
            this.UseDisplayNameCheckBox.Parameters["Text"] = this.GetXmlAttributeValue(element, "useDisplayNameCheckBox");

            //External Link            
            this.ForceSecureLinkCheckBox.Parameters["Text"] = this.GetXmlAttributeValue(element, "forceSecureLinkCheckBox");
            this.OpenNewWindowCheckBox.Parameters["Text"] = this.GetXmlAttributeValue(element, "forceSecureLinkCheckBox");

            //Behavior
            this.ElementId.Parameters["Text"] = this.GetXmlAttributeValue(element, "elementId");
            this.TextBeforeLink.Parameters["Text"] = this.GetXmlAttributeValue(element, "textBeforeLink");
            this.LinkDisplayText.Parameters["Text"] = this.GetXmlAttributeValue(element, "linkDisplayText");
            this.TextAfterLink.Parameters["Text"] = this.GetXmlAttributeValue(element, "textAfterLink");
            this.AnchorTarget.Parameters["Text"] = this.GetXmlAttributeValue(element, "anchorTarget");

            //Custom
            this.CustomAttributes.Parameters["Text"] = this.GetXmlAttributeValue(element, "customAttributes");
            this.QueryStringParameters.Parameters["Text"] = this.GetXmlAttributeValue(element, "queryStringParameters");

            //Presentation
            this.CssClass.Parameters["Text"] = this.GetXmlAttributeValue(element, "cssClass");
            
            //SEO
            this.NoFollowCheckBox.Parameters["Text"] = this.GetXmlAttributeValue(element, "noFollowCheckBox");
            this.NoIndexCheckBox.Parameters["Text"] = this.GetXmlAttributeValue(element, "noIndexCheckBox");
            this.NoReferrerCheckBox.Parameters["Text"] = this.GetXmlAttributeValue(element, "noReferrerCheckBox");           
            
            this.PhoneMasks.Parameters["Text"] = this.GetXmlAttributeValue(element, "phoneMasks");
            this.PhoneMasksLoadedValue.Parameters["Text"] = this.GetXmlAttributeValue(element, "phoneMasksLoadedValue");
            this.PhoneMasksLoadedItem.Parameters["Text"] = this.GetXmlAttributeValue(element, "phoneMasksLoadedItem");

            

        }
    }
}

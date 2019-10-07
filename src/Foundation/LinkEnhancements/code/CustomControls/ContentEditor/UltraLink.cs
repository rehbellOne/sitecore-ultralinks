using System;
using System.Collections.Specialized;
using Foundation.LinkEnhancements.Constants;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.Sheer;

namespace Foundation.LinkEnhancements.CustomControls.ContentEditor
{
    public class UltraLink : Link
    {
        private static readonly string UltraLinkVirtualPagePath = "/sitecore/shell/Applications/Dialogs/Ultra Link.aspx";
        private string _itemid;

        public string ItemId
        {
            get
            {
                return StringUtil.GetString(new string[1] { this._itemid });
            }
            set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this._itemid = value;
            }
        }


        public override string Source
        {
            get
            {
                return this.GetViewStateString(SourceConstants.SourceFieldName);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");

                var newValue = value;

                if (value.StartsWith(SourceConstants.QueryPrefix, StringComparison.InvariantCulture))
                {
                    var item = Client.ContentDatabase.GetItem(this.ItemId);

                    Item sourceItem = item?.Axes.SelectSingleItem(value.Substring(SourceConstants.QueryPrefixLength));

                    if (sourceItem != null)
                    {
                        base.SetViewStateString(SourceConstants.SourceFieldName, sourceItem.Paths.FullPath);
                    }
                }
                else
                {
                    var str = MainUtil.UnmapPath(newValue);

                    if (str.EndsWith("/", StringComparison.InvariantCulture))
                    {
                        str = str.Substring(0, str.Length - 1);
                    }
                    base.SetViewStateString(SourceConstants.SourceFieldName, str);
                }
            }
        }



        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));

            base.HandleMessage(message);

            if (message["id"] != this.ID)
            {
                return;
            }

            if (message.Name == "contentlink:ultralink")
            {
                this.Insert(UltraLinkVirtualPagePath, new NameValueCollection()
                {
                    { "height", "418"}

                });
                return;
            }


            base.HandleMessage(message);
        }
    }
}

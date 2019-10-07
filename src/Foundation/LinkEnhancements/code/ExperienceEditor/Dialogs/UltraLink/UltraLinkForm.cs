using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Xml;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Framework;
using Sitecore.StringExtensions;
using Sitecore.Utils;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Xml;

namespace Foundation.LinkEnhancements.ExperienceEditor.Dialogs.UltraLink
{
    public class UltraLinkForm : DialogForm
    {
        /// <summary>
        /// The class.
        /// </summary>
        protected Edit Class;

        /// <summary>
        /// The custom
        /// </summary>
        protected Literal Custom;

        /// <summary>
        /// The custom target.
        /// </summary>
        protected Edit CustomTarget;

        /// <summary>
        /// The internal link data context.
        /// </summary>
        protected DataContext InternalLinkDataContext;

        /// <summary>
        /// Internal link Treeview
        /// </summary>
        protected TreeviewEx InternalLinkTreeview;

        /// <summary>
        /// The Treeview Container
        /// </summary>
        protected Border InternalLinkTreeviewContainer;

        /// <summary>
        /// The JavaScriptCode;
        /// </summary>
        protected Memo JavascriptCode;

        /// <summary>
        /// The anchor.
        /// </summary>
        protected Edit LinkAnchor;

        /// <summary>
        /// Mailt to Container
        /// </summary>
        protected Border MailToContainer;

        /// <summary>
        /// The mail to
        /// </summary>
        protected Edit MailToLink;

        /// <summary>
        /// The media link data context.
        /// </summary>
        protected DataContext MediaLinkDataContext;

        /// <summary>
        /// Media link Treeview
        /// </summary>
        protected TreeviewEx MediaLinkTreeview;

        /// <summary>
        /// The Treeview Container
        /// </summary>
        protected Border MediaLinkTreeviewContainer;

        /// <summary>
        /// The preview.
        /// </summary>
        protected Border MediaPreview;

        /// <summary>
        /// The modes
        /// </summary>
        protected Border Modes;

        /// <summary>
        /// The querystring.
        /// </summary>
        protected Edit Querystring;

        /// <summary>
        /// The section header
        /// </summary>
        protected Literal SectionHeader;

        /// <summary>
        /// The target.
        /// </summary>
        protected Combobox Target;

        /// <summary>
        /// The text.
        /// </summary>
        protected Edit Text;

        /// <summary>
        /// The title.
        /// </summary>
        protected Edit Title;

        /// <summary>
        /// The Treeview Container
        /// </summary>
        protected Scrollbox TreeviewContainer;

        /// <summary>
        /// The upload media
        /// </summary>
        protected Button UploadMedia;

        /// <summary>
        /// The url
        /// </summary>
        protected Edit Url;

        /// <summary>
        /// The url container
        /// </summary>
        protected Border UrlContainer;

        /// <summary>
        /// Gets or sets CurrentMode.
        /// </summary>
        /// <value>The current mode.</value>
        private string CurrentMode
        {
            get
            {
                string text = ServerProperties["current_mode"] as string;
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
                return "internal";
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                ServerProperties["current_mode"] = value;
            }
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (CurrentMode != "media")
            {
                base.HandleMessage(message);
                return;
            }
            Item item = null;
            if (message.Arguments.Count > 0 && ID.IsID(message.Arguments["id"]))
            {
                IDataView dataView = MediaLinkTreeview.GetDataView();
                if (dataView != null)
                {
                    item = dataView.GetItem(message.Arguments["id"]);
                }
            }
            if (item == null)
            {
                item = MediaLinkTreeview.GetSelectionItem();
            }
            Dispatcher.Dispatch(message, item);
        }

        /// <summary>
        /// Called when the listbox has changed.
        /// </summary>
        protected void OnListboxChanged()
        {
            if (Target.Value == "Custom")
            {
                CustomTarget.Disabled = false;
                Custom.Class = string.Empty;
            }
            else
            {
                CustomTarget.Value = string.Empty;
                CustomTarget.Disabled = true;
                Custom.Class = "disabled";
            }
        }

        /// <summary>
        /// The on load.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <remarks>
        /// This method notifies the server control that it should perform actions common to each HTTP
        /// request for the page it is associated with, such as setting up a database query. At this
        /// stage in the page lifecycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client postback,
        /// or if it is being loaded and accessed for the first time.
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                CurrentMode = (this.LinkType ?? string.Empty);
                InitControls();
                SetModeSpecificControls();
                RegisterScripts();
            }
        }

        /// <summary>
        /// Called when the media has open.
        /// </summary>
        protected void OnMediaOpen()
        {
            Item selectionItem = MediaLinkTreeview.GetSelectionItem();
            if (selectionItem != null && selectionItem.HasChildren)
            {
                MediaLinkDataContext.SetFolder(selectionItem.Uri);
            }
        }

        /// <summary>
        /// Called when the mode has change.
        /// </summary>
        /// <param name="mode">The mode.</param>
        protected void OnModeChange(string mode)
        {
            Assert.ArgumentNotNull(mode, "mode");
            CurrentMode = mode;
            SetModeSpecificControls();

            if (!UIUtil.IsIE())
            {
                SheerResponse.Eval("scForm.browser.initializeFixsizeElements();");
            }
        }

        /// <summary>
        /// Handles a click on the OK button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <remarks>
        /// When the user clicks OK, the dialog is closed by calling
        /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.
        /// </remarks>
        /// <exception cref="T:System.ArgumentException"><c>ArgumentException</c>.</exception>
        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Packet packet = new Packet("link");
            SetCommonAttributes(packet);
            bool flag;
            switch (CurrentMode)
            {
                case "internal":
                    flag = SetInternalLinkAttributes(packet);
                    break;
                case "media":
                    flag = SetMediaLinkAttributes(packet);
                    break;
                case "external":
                    flag = SetExternalLinkAttributes(packet);
                    break;
                case "mailto":
                    flag = SetMailToLinkAttributes(packet);
                    break;
                case "anchor":
                    flag = SetAnchorLinkAttributes(packet);
                    break;
                case "javascript":
                    flag = SetJavascriptLinkAttributes(packet);
                    break;
                default:
                    throw new ArgumentException("Unsupported mode: " + CurrentMode);
            }
            if (flag)
            {
                SheerResponse.SetDialogValue(packet.OuterXml);
                base.OnOK(sender, args);
            }
        }

        /// <summary>
        /// Selects the tree node.
        /// </summary>
        protected void SelectMediaTreeNode()
        {
            Item selectionItem = MediaLinkTreeview.GetSelectionItem();
            if (selectionItem != null)
            {
                UpdateMediaPreview(selectionItem);
            }
        }

        /// <summary>
        /// Uploads the image.
        /// </summary>
        protected void UploadImage()
        {
            Item selectionItem = MediaLinkTreeview.GetSelectionItem();
            if (selectionItem != null)
            {
                if (!selectionItem.Access.CanCreate())
                {
                    SheerResponse.Alert("You do not have permission to create a new item here.");
                }
                else
                {
                    Context.ClientPage.SendMessage(this, "media:upload(edit=1,load=1)");
                }
            }
        }

        /// <summary>
        /// The hide containing row.
        /// </summary>
        /// <param name="control">The control.</param>
        private static void HideContainingRow(Control control)
        {
            Assert.ArgumentNotNull(control, "control");
            if (!Context.ClientPage.IsEvent)
            {
                (control.Parent as GridPanel)?.SetExtensibleProperty(control, "row.style", "display:none");
            }
            else
            {
                SheerResponse.SetStyle(control.ID + "Row", "display", "none");
            }
        }

        /// <summary>
        /// The show containing row.
        /// </summary>
        /// <param name="control">The control.</param>
        private static void ShowContainingRow(Control control)
        {
            Assert.ArgumentNotNull(control, "control");
            if (!Context.ClientPage.IsEvent)
            {
                (control.Parent as GridPanel)?.SetExtensibleProperty(control, "row.style", string.Empty);
            }
            else
            {
                SheerResponse.SetStyle(control.ID + "Row", "display", string.Empty);
            }
        }

        /// <summary>
        /// The init controls.
        /// </summary>
        private void InitControls()
        {
            string value = string.Empty;
            string text = this.LinkAttributes["target"];
            string linkTargetValue = UltraLinkForm.GetLinkTargetValue(text);
            if (linkTargetValue == "Custom")
            {
                value = text;
                CustomTarget.Disabled = false;
                Custom.Class = string.Empty;
            }
            else
            {
                CustomTarget.Disabled = true;
                Custom.Class = "disabled";
            }
            Text.Value = this.LinkAttributes["text"];
            Target.Value = linkTargetValue;
            CustomTarget.Value = value;
            Class.Value = this.LinkAttributes["class"];
          
            InitMediaLinkDataContext();
            InitInternalLinkDataContext();
        }

        /// <summary>
        /// The init internal link data context.
        /// </summary>
        private void InitInternalLinkDataContext()
        {
            InternalLinkDataContext.GetFromQueryString();
            string queryString = WebUtil.GetQueryString("ro");
            string text = this.LinkAttributes["id"];
            if (!string.IsNullOrEmpty(text) && ID.IsID(text))
            {
                ID itemID = new ID(text);
                ItemUri folder = new ItemUri(itemID, Client.ContentDatabase);
                InternalLinkDataContext.SetFolder(folder);
            }
            if (queryString.Length > 0)
            {
                InternalLinkDataContext.Root = queryString;
            }
        }

        /// <summary>
        /// The init media link data context.
        /// </summary>
        private void InitMediaLinkDataContext()
        {
            MediaLinkDataContext.GetFromQueryString();
            string text = this.LinkAttributes["url"].IsNullOrEmpty() ? this.LinkAttributes["id"] : this.LinkAttributes["url"];
            if (CurrentMode != "media")
            {
                text = string.Empty;
            }
            if (text.Length == 0)
            {
                text = "/sitecore/media library";
            }
            else
            {
                if (!ID.IsID(text) && !text.StartsWith("/sitecore", StringComparison.InvariantCulture) && !text.StartsWith("/{11111111-1111-1111-1111-111111111111}", StringComparison.InvariantCulture))
                {
                    text = "/sitecore/media library" + text;
                }
                IDataView dataView = MediaLinkTreeview.GetDataView();
                if (dataView == null)
                {
                    return;
                }
                Item item = dataView.GetItem(text);
                if (item != null && item.Parent != null)
                {
                    MediaLinkDataContext.SetFolder(item.Uri);
                }
            }
            MediaLinkDataContext.AddSelected(new DataUri(text));
            MediaLinkDataContext.Root = "/sitecore/media library";
        }

        /// <summary>
        /// The register scripts.
        /// </summary>
        private static void RegisterScripts()
        {
            string script = "window.Texts = {{ ErrorOcurred: \"{0}\"}};".FormatWith(Translate.Text("An error occured:"));
            Context.ClientPage.ClientScript.RegisterClientScriptBlock(Context.ClientPage.GetType(), "translationsScript", script, addScriptTags: true);
        }

        /// <summary>
        /// The set anchor link attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The set anchor link attributes.</returns>
        private bool SetAnchorLinkAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            string text = LinkAnchor.Value;
            if (text.Length > 0 && text.StartsWith("#", StringComparison.InvariantCulture))
            {
                text = text.Substring(1);
            }
            UltraLinkForm.SetAttribute(packet, "url", text);
            UltraLinkForm.SetAttribute(packet, "anchor", text);
            return true;
        }

        /// <summary>
        /// The set anchor link controls.
        /// </summary>
        private void SetAnchorLinkControls()
        {
            ShowContainingRow(LinkAnchor);
            string text = this.LinkAttributes["anchor"];
            if (this.LinkType != "anchor" && string.IsNullOrEmpty(LinkAnchor.Value))
            {
                text = string.Empty;
            }
            if (!string.IsNullOrEmpty(text) && !text.StartsWith("#", StringComparison.InvariantCulture))
            {
                text = "#" + text;
            }
            LinkAnchor.Value = (text ?? string.Empty);
            SectionHeader.Text = Translate.Text("Specify the name of the anchor, e.g. #header1, and any additional properties");
        }

        /// <summary>
        /// The set common attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void SetCommonAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            UltraLinkForm.SetAttribute(packet, "linktype", CurrentMode);
            UltraLinkForm.SetAttribute(packet, "text", Text);
            UltraLinkForm.SetAttribute(packet, "title", Title);
            UltraLinkForm.SetAttribute(packet, "class", Class);
        }

        /// <summary>
        /// The set external link attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The set external link attributes.</returns>
        private bool SetExternalLinkAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            string text = Url.Value;
            if (text.Length > 0 && text.IndexOf("://", StringComparison.InvariantCulture) < 0 && !text.StartsWith("/", StringComparison.InvariantCulture))
            {
                text = "http://" + text;
            }
            string linkTargetAttributeFromValue = UltraLinkForm.GetLinkTargetAttributeFromValue(Target.Value, CustomTarget.Value);
            UltraLinkForm.SetAttribute(packet, "url", text);
            UltraLinkForm.SetAttribute(packet, "anchor", string.Empty);
            UltraLinkForm.SetAttribute(packet, "target", linkTargetAttributeFromValue);
            return true;
        }

        /// <summary>
        /// The set external link controls.
        /// </summary>
        private void SetExternalLinkControls()
        {
            if (this.LinkType == "external" && string.IsNullOrEmpty(Url.Value))
            {
                string value = this.LinkAttributes["url"];
                Url.Value = value;
            }
            ShowContainingRow(UrlContainer);
            ShowContainingRow(Target);
            ShowContainingRow(CustomTarget);
            SectionHeader.Text = Translate.Text("Specify the URL, e.g. http://www.sitecore.net and any additional properties.");
        }

        /// <summary>
        /// The set internal link attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The set internal link attributes.</returns>
        private bool SetInternalLinkAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            Item selectionItem = InternalLinkTreeview.GetSelectionItem();
            if (selectionItem == null)
            {
                Context.ClientPage.ClientResponse.Alert("Select an item.");
                return false;
            }
            string linkTargetAttributeFromValue = UltraLinkForm.GetLinkTargetAttributeFromValue(Target.Value, CustomTarget.Value);
            string text = Querystring.Value;
            if (text.StartsWith("?", StringComparison.InvariantCulture))
            {
                text = text.Substring(1);
            }
            UltraLinkForm.SetAttribute(packet, "anchor", LinkAnchor);
            UltraLinkForm.SetAttribute(packet, "querystring", text);
            UltraLinkForm.SetAttribute(packet, "target", linkTargetAttributeFromValue);
            UltraLinkForm.SetAttribute(packet, "id", selectionItem.ID.ToString());
            return true;
        }

        /// <summary>
        /// The set internal link contols.
        /// </summary>
        private void SetInternalLinkContols()
        {
            LinkAnchor.Value = this.LinkAttributes["anchor"];
            InternalLinkTreeviewContainer.Visible = true;
            MediaLinkTreeviewContainer.Visible = false;
            ShowContainingRow(TreeviewContainer);
            ShowContainingRow(Querystring);
            ShowContainingRow(LinkAnchor);
            ShowContainingRow(Target);
            ShowContainingRow(CustomTarget);
            SectionHeader.Text = Translate.Text("Select the item that you want to create a link to and specify the appropriate properties.");
        }

        /// <summary>
        /// The set java script link controls.
        /// </summary>
        private void SetJavaScriptLinkControls()
        {
            ShowContainingRow(JavascriptCode);
            string value = this.LinkAttributes["url"];
            if (this.LinkType != "javascript" && string.IsNullOrEmpty(JavascriptCode.Value))
            {
                value = string.Empty;
            }
            JavascriptCode.Value = value;
            SectionHeader.Text = Translate.Text("Specify the JavaScript and any additional properties.");
        }

        /// <summary>
        /// The set javascript link attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The set javascript link attributes.</returns>
        private bool SetJavascriptLinkAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            string text = JavascriptCode.Value;
            if (text.Length > 0 && text.IndexOf("javascript:", StringComparison.InvariantCulture) < 0)
            {
                text = "javascript:" + text;
            }
            UltraLinkForm.SetAttribute(packet, "url", text);
            UltraLinkForm.SetAttribute(packet, "anchor", string.Empty);
            return true;
        }

        /// <summary>
        /// The set mail link controls.
        /// </summary>
        private void SetMailLinkControls()
        {
            if (this.LinkType == "mailto" && string.IsNullOrEmpty(Url.Value))
            {
                string value = this.LinkAttributes["url"];
                MailToLink.Value = value;
            }
            ShowContainingRow(MailToContainer);
            SectionHeader.Text = Translate.Text("Specify the email address and any additional properties. To send a test mail use the 'Send a test mail' button.");
        }

        /// <summary>
        /// The set mail to link attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The set mail to link attributes.</returns>
        private bool SetMailToLinkAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            string value = MailToLink.Value;
            value = StringUtil.GetLastPart(value, ':', value);
            if (!EmailUtility.IsValidEmailAddress(value))
            {
                SheerResponse.Alert("The e-mail address is invalid.");
                return false;
            }
            if (!string.IsNullOrEmpty(value))
            {
                value = "mailto:" + value;
            }
            UltraLinkForm.SetAttribute(packet, "url", value ?? string.Empty);
            UltraLinkForm.SetAttribute(packet, "anchor", string.Empty);
            return true;
        }

        /// <summary>
        /// The set media link attributes.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>The set media link attributes.</returns>
        private bool SetMediaLinkAttributes(Packet packet)
        {
            Assert.ArgumentNotNull(packet, "packet");
            Item selectionItem = MediaLinkTreeview.GetSelectionItem();
            if (selectionItem == null)
            {
                Context.ClientPage.ClientResponse.Alert("Select a media item.");
                return false;
            }
            string linkTargetAttributeFromValue = UltraLinkForm.GetLinkTargetAttributeFromValue(Target.Value, CustomTarget.Value);
            UltraLinkForm.SetAttribute(packet, "target", linkTargetAttributeFromValue);
            UltraLinkForm.SetAttribute(packet, "id", selectionItem.ID.ToString());
            return true;
        }

        protected virtual string GetLink()
        {
            return StringUtil.GetString(UrlHandle.Get()["va"], "<ultralink/>");
        }

        private NameValueCollection linkAttributes;
        protected new NameValueCollection LinkAttributes
        {
            get
            {
                if (this.linkAttributes != null)
                    return this.linkAttributes;
                this.ParseLink(this.GetLink());
                return this.linkAttributes;
            }
        }
        protected new string LinkType
        {
            get
            {
                return this.LinkAttributes["linkType"];
            }
        }

        protected virtual void ParseLink(string link)
        {
            Assert.ArgumentNotNull((object)link, nameof(link));
            this.linkAttributes = new NameValueCollection();
            XmlDocument xmlDocument = XmlUtil.LoadXml(link);
            if (xmlDocument == null)
                return;
            XmlNode node = xmlDocument.SelectSingleNode("/ultralink");
            if (node == null)
                return;
            this.linkAttributes["text"] = XmlUtil.GetAttribute("linkDisplayText", node);
            this.linkAttributes["class"] = XmlUtil.GetAttribute("cssClass", node);
            this.linkAttributes["title"] = XmlUtil.GetAttribute("title", node);
            this.linkAttributes["anchor"] = XmlUtil.GetAttribute("anchorTarget", node);
            this.linkAttributes["linktype"] = XmlUtil.GetAttribute("linkType", node);
            this.linkAttributes["url"] = XmlUtil.GetAttribute("url", node);
            this.linkAttributes["target"] = XmlUtil.GetAttribute("target", node);
            this.linkAttributes["id"] = XmlUtil.GetAttribute("id", node);
        }
        /// <summary>
        /// The set media link controls.
        /// </summary>
        private void SetMediaLinkControls()
        {
            InternalLinkTreeviewContainer.Visible = false;
            MediaLinkTreeviewContainer.Visible = true;
            MediaPreview.Visible = true;
            UploadMedia.Visible = true;
            Item folder = MediaLinkDataContext.GetFolder();
            if (folder != null)
            {
                UpdateMediaPreview(folder);
            }
            ShowContainingRow(TreeviewContainer);
            ShowContainingRow(Target);
            ShowContainingRow(CustomTarget);
            SectionHeader.Text = Translate.Text("Select an item from the media library and specify any additional properties.");
        }

        /// <summary>
        /// The set mode specific controls.
        /// </summary>
        /// <exception cref="T:System.ArgumentException">
        /// </exception>
        private void SetModeSpecificControls()
        {
            HideContainingRow(TreeviewContainer);
            MediaPreview.Visible = false;
            UploadMedia.Visible = false;
            HideContainingRow(UrlContainer);
            HideContainingRow(Querystring);
            HideContainingRow(MailToContainer);
            HideContainingRow(LinkAnchor);
            HideContainingRow(JavascriptCode);
            HideContainingRow(Target);
            HideContainingRow(CustomTarget);
            switch (CurrentMode)
            {
                case "internal":
                    SetInternalLinkContols();
                    break;
                case "phone":
                    SetMediaLinkControls();
                    break;
                case "external":
                    SetExternalLinkControls();
                    break;
                case "email":
                    SetMailLinkControls();
                    break;
                case "anchor":
                    SetAnchorLinkControls();
                    break;
                default:
                    throw new ArgumentException("Unsupported mode: " + CurrentMode);
            }
            foreach (Border control in Modes.Controls)
            {
                if (control != null)
                {
                    control.Class = ((control.ID.ToLowerInvariant() == CurrentMode) ? "selected" : string.Empty);
                }
            }
        }

        /// <summary>
        /// Updates the preview.
        /// </summary>
        /// <param name="item">The item.</param>
        private void UpdateMediaPreview(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            MediaUrlOptions thumbnailOptions = MediaUrlOptions.GetThumbnailOptions(item);
            thumbnailOptions.UseDefaultIcon = true;
            thumbnailOptions.Width = 96;
            thumbnailOptions.Height = 96;
            thumbnailOptions.Language = item.Language;
            thumbnailOptions.AllowStretch = false;
            string mediaUrl = MediaManager.GetMediaUrl(item, thumbnailOptions);
            MediaPreview.InnerHtml = "<img src=\"" + mediaUrl + "\" width=\"96px\" height=\"96px\" border=\"0\" alt=\"\" />";
        }

        protected static void SetAttribute(Packet packet, string name, Control control)
        {
            Assert.ArgumentNotNull((object)packet, nameof(packet));
            Assert.ArgumentNotNull((object)name, nameof(name));
            Assert.ArgumentNotNull((object)control, nameof(control));
            if (control.Value.Length <= 0)
                return;
            UltraLinkForm.SetAttribute(packet, name, control.Value);
        }

        protected static void SetAttribute(Packet packet, string name, string value)
        {
            Assert.ArgumentNotNull((object)packet, nameof(packet));
            Assert.ArgumentNotNull((object)name, nameof(name));
            Assert.ArgumentNotNull((object)value, nameof(value));
            packet.SetAttribute(name, value);
        }

        protected static string GetLinkTargetValue(string target)
        {
            if (target != null && target.Length == 0 || target == "_self")
                return "Self";
            return target == "_blank" ? "New" : "Custom";
        }

        protected static string GetLinkTargetAttributeFromValue(string value, string defaultValue)
        {
            Assert.ArgumentNotNull((object)value, nameof(value));
            Assert.ArgumentNotNull((object)defaultValue, nameof(defaultValue));
            if (value == "Self")
                return string.Empty;
            if (value == "New")
                return "_blank";
            return defaultValue;
        }

        protected static class LinkAttributeNames
        {
            public const string Anchor = "anchor";
            public const string Class = "class";
            public const string Id = "id";
            public const string LinkType = "linktype";
            public const string QueryString = "querystring";
            public const string Target = "target";
            public const string Text = "text";
            public const string Title = "title";
            public const string Url = "url";
        }
    }
}
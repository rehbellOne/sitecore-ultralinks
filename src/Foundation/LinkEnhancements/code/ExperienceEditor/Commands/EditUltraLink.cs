using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Xml.Xsl;

namespace Foundation.LinkEnhancements.ExperienceEditor.Commands
{
    [Serializable]
    public class EditUltraLink : WebEditLinkCommand
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            string formValue = WebUtil.GetFormValue("scPlainValue");
            context.Parameters.Add("fieldValue", formValue);
            Context.ClientPage.Start(this, "Run", context.Parameters);
        }

        protected static void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = Context.ContentDatabase.GetItem(args.Parameters["itemid"]);
            Assert.IsNotNull(item, typeof(Item));
            Field field = item.Fields[args.Parameters["fieldid"]];
            Assert.IsNotNull(field, typeof(Field));
            string value = args.Parameters["controlid"];
            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    RenderFieldResult renderFieldResult = RenderLink(args);
                    string text = renderFieldResult.ToString();
                    SheerResponse.SetAttribute("scHtmlValue", "value",
                        string.IsNullOrEmpty(text) ? WebEditLinkCommand.GetDefaultText() : text);
                    SheerResponse.SetAttribute("scPlainValue", "value", args.Result);
                    ScriptInvokationBuilder scriptInvokationBuilder = new ScriptInvokationBuilder("scSetHtmlValue");
                    scriptInvokationBuilder.AddString(value);
                    if (!string.IsNullOrEmpty(text) && string.IsNullOrEmpty(StringUtil.RemoveTags(text)))
                    {
                        scriptInvokationBuilder.Add("true");
                    }

                    SheerResponse.Eval(scriptInvokationBuilder.ToString());
                }
            }
            else
            {
                UrlString urlString = new UrlString(Context.Site.XmlControlPage);
                urlString["xmlcontrol"] = "UltraLink";
                UrlHandle urlHandle = new UrlHandle();
                urlHandle["va"] = new XmlValue(args.Parameters["fieldValue"], "ultralink").ToString();
                urlHandle.Add(urlString);
                urlString.Append("ro", field.Source);
                Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), "550", "650", string.Empty,
                    response: true);
                args.WaitForPostBack();
            }
        }

        private static RenderFieldResult RenderLink(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            string result = args.Result;
            string value = args.Parameters["itemid"];
            string name = args.Parameters["language"];
            string value2 = args.Parameters["version"];
            string value3 = args.Parameters["fieldid"];
            Item item = Context.ContentDatabase.GetItem(ID.Parse(value), Language.Parse(name),
                Sitecore.Data.Version.Parse(value2));
            if (item == null)
            {
                SheerResponse.Alert("The item was not found.\n\nIt may have been deleted by another user.");
                new RenderFieldResult();
            }

            Field field = item.Fields[ID.Parse(value3)];
            using (FieldRenderer fieldRenderer = new FieldRenderer())
            {
                string text = args.Parameters["webeditparams"];
                SafeDictionary<string> parameters = new SafeDictionary<string>();
                if (!string.IsNullOrEmpty(text))
                {
                    parameters = WebUtil.ParseQueryString(text);
                }

                fieldRenderer.Item = item;
                fieldRenderer.FieldName = field.Name;
                fieldRenderer.Parameters = WebUtil.BuildQueryString(parameters, xhtml: false);
                fieldRenderer.OverrideFieldValue(result);
                fieldRenderer.DisableWebEditing = true;
                string formValue = WebUtil.GetFormValue("scSite");
                if (!string.IsNullOrEmpty(formValue))
                {
                    SiteContext siteContext = SiteContextFactory.GetSiteContext(formValue);
                    Assert.IsNotNull(siteContext, "siteContext");
                    using (new SiteContextSwitcher(siteContext))
                    {
                        return fieldRenderer.RenderField();
                    }
                }

                return fieldRenderer.RenderField();
            }
        }
    }
}
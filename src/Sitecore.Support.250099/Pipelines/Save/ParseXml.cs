namespace Sitecore.Support.Pipelines.Save
{
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Pipelines.Save;
  using Sitecore.Xml;
  using System;
  using System.Xml;

  public class ParseXml
  {
    public void Process(SaveArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      XmlDocument xml = args.Xml;
      Assert.IsNotNull(xml, "Missing XML for saving item");
      XmlDocument document2 = new XmlDocument();
      XmlNode node = XmlUtil.AddElement("sitecore", document2);
      XmlNodeList list = xml.SelectNodes("/sitecore/field");
      XmlNodeList listx = list;


      Assert.IsNotNull(list, "/sitecore/field");
      foreach (XmlNode node2 in list)
      {
        // This for loop has been updated to fix Bug#250099 
        string outerxml = node2.OuterXml;
        string securityFieldId = "DEC8D2D5-E3CF-48B6-A653-8E69E2716641"; // fixed id of security field
        int ind = outerxml.IndexOf(securityFieldId);
        if (ind < 0) // Update only if not a security field. Security fields should not be saved using the Save command as security fields are already updated at runtime through a background call.
        {

          string attribute = XmlUtil.GetAttribute("itemid", node2);
          string str2 = XmlUtil.GetAttribute("language", node2);
          string str3 = XmlUtil.GetAttribute("version", node2);
          string str4 = XmlUtil.GetAttribute("itemrevision", node2);
          string str5 = XmlUtil.GetAttribute("fieldid", node2);
          string childValue = XmlUtil.GetChildValue("value", node2);
          XmlNode node3 = document2.SelectSingleNode("/sitecore/item[@itemid='" + attribute + "' and @language='" + str2 + "' and @version='" + str3 + "']");
          if (node3 == null)
          {
            node3 = XmlUtil.AddElement("item", node);
            XmlUtil.SetAttribute("itemid", attribute, node3);
            XmlUtil.SetAttribute("language", str2, node3);
            XmlUtil.SetAttribute("version", str3, node3);
            XmlUtil.SetAttribute("itemrevision", str4, node3);
          }
          XmlNode node4 = XmlUtil.AddElement("field", node3);
          XmlUtil.SetAttribute("fieldid", str5, node4);
          XmlUtil.SetValue(childValue, node4);
        }
      }
      XmlNodeList list2 = document2.SelectNodes("/sitecore/item");
      Assert.IsNotNull(list2, "/sitecore/item");
      SaveArgs.SaveItem[] itemArray = new SaveArgs.SaveItem[list2.Count];
      for (int i = 0; i < list2.Count; i++)
      {
        XmlNode node5 = list2[i];
        XmlNodeList list3 = node5.SelectNodes("field");
        Assert.IsNotNull(list3, "field");
        SaveArgs.SaveItem item = new SaveArgs.SaveItem
        {
          ID = ID.Parse(XmlUtil.GetAttribute("itemid", node5)),
          Version = Sitecore.Data.Version.Parse(XmlUtil.GetAttribute("version", node5)),
          Language = Language.Parse(XmlUtil.GetAttribute("language", node5)),
          Revision = XmlUtil.GetAttribute("itemrevision", node5),
          Fields = new SaveArgs.SaveField[list3.Count]
        };
        for (int j = 0; j < list3.Count; j++)
        {
          XmlNode node6 = list3[j];
          SaveArgs.SaveField field = new SaveArgs.SaveField
          {
            ID = new ID(XmlUtil.GetAttribute("fieldid", node6)),
            Value = XmlUtil.GetValue(node6)
          };
          item.Fields[j] = field;
        }
        itemArray[i] = item;
      }
      args.Items = itemArray;
    }
  }
}

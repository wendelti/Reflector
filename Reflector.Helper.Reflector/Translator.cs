using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Reflector.Helper.Reflector.RequestAttribute;

namespace Reflector.Helper.Reflector
{
    public class Translator : Reflector
    {

        public static Dictionary<ValueProvider, ValuesTranslateProvider> Providers = new Dictionary<ValueProvider, ValuesTranslateProvider>();

        public static string baseText = "{0}: De [{1}] Para [{2}]";
        public static string baseiInnerPhrasesText = "{0} - {1}";

        public static string GetFirstField(string field)
        {
            if (field.Contains("["))
            {
                return field.Substring(0, field.IndexOf("["));
            }
            return field;
        }

        public static List<string> Translate(object item, Change change)
        {
            return Translate(item, new List<Change>() { change });
        }

        public static List<string> Translate(object item, List<Change> changes)
        {
            var phrases = new List<string>();

            var itemType = item.GetType();
            var itemProps = itemType.GetProperties();

            foreach (var change in changes)
            {
                var prop = itemProps
                                    .Where(p => Attribute.IsDefined(p, typeof(RequestAttribute)))
                                    .Where(p => ((RequestAttribute)p.GetCustomAttributes(false).First()).name == GetFirstField(change.Field))
                                    .First();

                var attribute = prop.GetCustomAttributes(false);
                if (attribute.Length == 0) continue;
                RequestAttribute request = (RequestAttribute)attribute[0];

                if (IsList(prop))
                {
                    var listValues = ((IList)prop.GetValue(item));

                    if (listValues != null)
                    {
                        for (var i = 0; i < listValues.Count; i++)
                        {
                            var listItem = listValues[i];
                            var listProps = listItem.GetType().GetProperties();
                            var keyProp = listItem.GetType().GetProperties().Where(p => p.Name.Equals(request.indexProp)).FirstOrDefault();
                            var keyValue = keyProp.GetValue(listItem);

                            foreach (var listProp in listProps)
                            {
                                var listAttribute = listProp.GetCustomAttributes(false);
                                RequestAttribute listRequest = (RequestAttribute)listAttribute[0];

                                if (change.Field.Equals(string.Format("{0}[{1}][{2}]", request.name, i, listRequest.name)))
                                {
                                    var fieldLabel = string.Format("{0}[{1}] {2}", request.alias, keyValue, listRequest.alias);
                                    phrases.Add(string.Format(baseText, fieldLabel, listRequest.GetValue(change.OldValue), listRequest.GetValue(change.NewValue)));
                                }
                            }
                        }
                    }
                }
                else if (!IsClrType(prop.PropertyType))
                {
                    var value = prop.GetValue(item);

                    var innerChange = new Change()
                    {
                        Field = change.Field.Replace(string.Format("{0}[", prop.Name), string.Empty).Replace("]", string.Empty),
                        OldValue = change.OldValue,
                        NewValue = change.NewValue
                    };

                    var innerPhrases = Translate(value, innerChange);
                    innerPhrases.ForEach(p => phrases.Add(string.Format(baseiInnerPhrasesText, request.alias, p)));
                }
                else
                {
                    phrases.Add(string.Format(baseText, request.alias, request.GetValue(change.OldValue), request.GetValue(change.NewValue)));
                }
            }

            return phrases;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Helper.Reflector
{
    public class Reflector
    {

        public static void SetValues(object item, List<Change> changes)
        {
            var itemType = item.GetType();

            foreach (var prop in itemType.GetProperties())
            {

                var attribute = prop.GetCustomAttributes(false);

                if (attribute.Length == 0) continue;

                RequestAttribute request = (RequestAttribute)attribute[0];
                var propName = request.name;

                if (attribute.Length > 0)
                {

                    if (IsList(prop))
                    {
                        var values = ((IList)prop.GetValue(item));

                        if (values != null)
                        {
                            for (var i = 0; i < values.Count; i++)
                            {
                                var listItem = values[i];
                                var listProps = listItem.GetType().GetProperties();

                                foreach (var listProp in listProps)
                                {
                                    var listAttribute = listProp.GetCustomAttributes(false);
                                    RequestAttribute listRequest = (RequestAttribute)listAttribute[0];

                                    var change = changes.Where(x => x.Field == string.Format("{0}[{1}][{2}]", propName, i, listRequest.name)).FirstOrDefault();
                                    if (change != null)
                                    {
                                        var type = listProp.PropertyType;
                                        if (Nullable.GetUnderlyingType(listProp.PropertyType) != null)
                                        {
                                            type = Nullable.GetUnderlyingType(listProp.PropertyType);
                                        }

                                        listProp.SetValue(listItem, Convert.ChangeType(change.NewValue, type));
                                    }

                                }
                            }
                        }
                    }

                    else if (!IsClrType(prop.PropertyType))
                    {

                        var value = prop.GetValue(item);

                        var innerChanges = changes.Where(change => change.Field.Contains(string.Format("{0}[", propName)) && !change.Field.Contains(string.Format("{0}[]", propName))).ToList();

                        innerChanges.ForEach(change => change.Field = change.Field.Replace(string.Format("{0}[", propName), string.Empty).Replace("]", string.Empty));

                        if (value == null)
                        {
                            value = Activator.CreateInstance(prop.PropertyType);
                            prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType));
                        }

                        SetValues(value, innerChanges);


                    }

                    else
                    {

                        var change = changes.Where(x => x.Field == propName).FirstOrDefault();
                        if (change != null)
                        {
                            var innerNullableType = Nullable.GetUnderlyingType(prop.PropertyType);
                            var nullableHasValue = false;

                            if (innerNullableType != null)
                            {
                                nullableHasValue = change.NewValue != null && !string.IsNullOrEmpty(change.NewValue.ToString());
                                if (nullableHasValue)
                                {
                                    prop.SetValue(item, Convert.ChangeType(change.NewValue, innerNullableType, CultureInfo.InvariantCulture));
                                }
                                else
                                {
                                    change.NewValue = null;
                                }
                            }

                            if (!nullableHasValue)
                            {
                                if(change.NewValue == null)
                                {
                                    prop.SetValue(item, null);
                                }
                                else
                                {
                                    prop.SetValue(item, Convert.ChangeType(change.NewValue, prop.PropertyType));
                                }
                            }


                        }

                    }

                }
            }

            var changesToAdd = changes.Where(change => change.Field.Contains("[]"));

            foreach (var add in changesToAdd)
            {
                var propName = add.Field.Replace("[]", string.Empty);
                var listProp = itemType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(RequestAttribute))).Where(p => ((RequestAttribute)p.GetCustomAttributes(false).First()).name == propName).First();
                if (IsList(listProp))
                {

                    var listType = listProp.PropertyType.GetGenericArguments()[0];
                    var values = ((IList)listProp.GetValue(item));
                    values.GetType().GetMethod("Add").Invoke(values, new[] { JsonConvert.DeserializeObject(add.NewValue.ToString(), listType) });

                }
                else
                {
                    listProp.SetValue(item, JsonConvert.DeserializeObject(add.NewValue.ToString(), listProp.PropertyType));
                }
            }

        }


        public static List<Change> GetChanges(object itemOld, object itemNew)
        {
            return GetChanges(itemOld, itemNew, true);
        }

        public static List<Change> GetChanges(object itemOld, object itemNew, bool ignoreNulls)
        {
            List<Change> changes = new List<Change>();

            foreach (var prop in itemOld.GetType().GetProperties())
            {

                var attribute = prop.GetCustomAttributes(false);
                if (attribute.Length > 0)
                {
                    RequestAttribute request = (RequestAttribute)attribute[0];

                    if (IsList(prop))
                    {

                        var valuesOld = ((IList)prop.GetValue(itemOld));
                        var valuesNew = ((IList)prop.GetValue(itemNew));

                        var valuesOldCount = 0;
                        if (valuesOld != null)
                        {
                            valuesOldCount = valuesOld.Count;

                            for (var i = 0; i < valuesOld.Count; i++)
                            {
                                var listItemOld = valuesOld[i];
                                var listItemNew = valuesNew[i];
                                var listProps = listItemOld.GetType().GetProperties();

                                foreach (var listProp in listProps)
                                {
                                    var listAttribute = listProp.GetCustomAttributes(false);
                                    RequestAttribute listRequest = (RequestAttribute)listAttribute[0];

                                    if (!Object.Equals(listProp.GetValue(listItemNew), listProp.GetValue(listItemOld)))
                                    {
                                        changes.Add(new Change() { Field = string.Format("{0}[{1}][{2}]", request.name, i, listRequest.name), OldValue = listProp.GetValue(listItemOld), NewValue = listProp.GetValue(listItemNew) });
                                    }

                                }
                            }

                        }

                        if (valuesNew != null)
                        {
                            if (valuesNew.Count > valuesOldCount)
                            {
                                for (var i = valuesOldCount; i < valuesNew.Count; i++)
                                {
                                    changes.Add(new Change() { Field = string.Format("{0}[]", request.name), OldValue = string.Empty, NewValue = JsonConvert.SerializeObject(valuesNew[i]) });
                                }
                            }
                        }

                    }

                    else if (!IsClrType(prop.PropertyType))
                    {

                        var newValue = prop.GetValue(itemNew);
                        var oldValue = prop.GetValue(itemOld);

                        if (newValue == null) newValue = Activator.CreateInstance(prop.PropertyType);
                        if (oldValue == null) oldValue = Activator.CreateInstance(prop.PropertyType);

                        var innerChanges = GetChanges(oldValue, newValue, ignoreNulls);

                        innerChanges.ForEach(change => changes.Add(new Change() { Field = string.Format("{0}[{1}]", request.name, change.Field), OldValue = change.OldValue, NewValue = change.NewValue }));

                    }

                    else
                    {

                        var newValue = prop.GetValue(itemNew);
                        var oldValue = prop.GetValue(itemOld);

                        if (ignoreNulls)
                        {
                            if (newValue == null) newValue = GetDefault(prop.PropertyType);
                            if (oldValue == null) oldValue = GetDefault(prop.PropertyType);
                        }

                        if (!Object.Equals(oldValue, newValue))
                        {
                            changes.Add(new Change() { Field = request.name, OldValue = oldValue, NewValue = newValue });
                        }

                    }

                }


            }

            return changes;
        }

        public static object GetDefault(Type propertyType)
        {

            if (Nullable.GetUnderlyingType(propertyType) != null)
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }

            if (propertyType == typeof(string))
                return string.Empty;
            else
                return Activator.CreateInstance(propertyType);
        }

        public static bool IsList(object o)
        {
            return o.ToString().Contains("Generic.List");
        }

        public static bool IsClrType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return IsClrType(Nullable.GetUnderlyingType(type));
            }
            return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
        }



    }
}

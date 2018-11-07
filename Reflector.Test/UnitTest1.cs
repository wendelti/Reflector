using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Reflector.Helper.Reflector;
using Newtonsoft.Json;
using System.Globalization;

namespace Reflector.Test
{


    public class StatusProvider : ValuesTranslateProvider
    {
        public override string GetValue(string id)
        {
            switch (id)
            {
                case "1":
                    return "EM ANDAMENTO";
                case "2":
                    return "NEGADO";
                case "3":
                    return "APROVADO";
                default:
                    return "NOT_FOUND";
            }


        }

    }

    public class Item
    {

        [Request("Volumn")]
        public int Volumn { get; set; }

        [Request("Status", "Estado do Item", RequestAttribute.ValueProvider.Status)]
        public int Status { get; set; }

        [Request("Location", "Pais de Origem")]
        public string Location { get; set; }

        [Request("Supplier", "Fabricante", "Name")]
        public List<Supplier> Suppliers { get; set; }


        [Request("Origin")]
        public Origin Origin { get; set; }

        [Request("Origin2", "Origem de Compra")]
        public Origin Origin2 { get; set; }

        [Request("NewId")]
        public int? NewId { get; set; }


        [Request("Value")]
        public double? Value { get; set; }
    }

    public class Supplier
    {

        [Request("Name", "Nome")]
        public string Name { get; set; }

        [Request("YearsOfWork")]
        public int? YearsOfWork { get; set; }

    }

    public class Origin
    {

        [Request("Country", "Pais")]
        public string Country { get; set; }

        [Request("State", "Estado")]
        public string State { get; set; }
    }





    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        //public void TestSetValues()
        //{

        //    var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

        //    var changes = new List<Change>();
        //    changes.Add(new Change() { Field = "Location", NewValue = "CL" });
        //    changes.Add(new Change() { Field = "Supplier[0][Name]", NewValue = "AA" });
        //    changes.Add(new Change() { Field = "Supplier[]", NewValue = "{Name:'WW'}" });
        //    changes.Add(new Change() { Field = "Origin[]", NewValue = "{Country:'BR'}" });
        //    changes.Add(new Change() { Field = "Origin2[Country]", NewValue = "USA" });

        //    Reflector.SetValues(item, changes);

        //    Assert.AreEqual(item.Location, "CL");
        //    Assert.AreEqual(item.Suppliers.First().Name, "AA");
        //    Assert.AreEqual(item.Suppliers.Count, 2);
        //    Assert.AreEqual(item.Origin.Country, "BR");
        //    Assert.AreEqual(item.Origin2.Country, "USA");
        //}


        [TestMethod]
        public void TestEnumDescrition()
        {
            var enumValue = RequestAttribute.ValueProvider.Status;
            Assert.AreEqual(enumValue.ToString(), "Status");
        }

        [TestMethod]
        public void TestSetValues_Nullable()
        {

            var item = new Item();

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "NewId", NewValue = 1 });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.NewId.Value, 1);
        }

        [TestMethod]
        public void TestSetValues_NullableByEmptyString()
        {

            var item = new Item();

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "NewId", NewValue = string.Empty });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.NewId.HasValue, false);
        }



        [TestMethod]
        public void TestSetValues_DecimalPoint()
        {

            var item = new Item();

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Value", NewValue = "0.0151" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.Value, 0.0151);
        }


        [TestMethod]
        public void TestSetValues_EditPrimitiveProp()
        {

            var item = new Item()
            {
                Location = "BR",
                Volumn = 1,
                Origin2 = new Origin() { Country = "AR" },
                Suppliers = new List<Supplier>() {
                    new Supplier() {
                        Name = "WW"
                    }
                }
            };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Location", NewValue = "CL" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.Location, "CL");
        }


        [TestMethod]
        public void TestSetValues_EditListIndexedProp()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Supplier[0][Name]", NewValue = "AA" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.Suppliers.First().Name, "AA");
        }


        [TestMethod]
        public void TestSetValues_EditListIndexedNullableProp()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW", YearsOfWork = null } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Supplier[0][YearsOfWork]", NewValue = "5" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.Suppliers.First().YearsOfWork, 5);
        }

        [TestMethod]
        public void TestSetValues_AddToList()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Supplier[]", NewValue = "{Name:'WW'}" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.Suppliers.Count, 2);
        }

        [TestMethod]
        public void TestSetValues_EditObject()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Origin[]", NewValue = "{Country:'BR'}" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);

            Assert.AreEqual(item.Origin.Country, "BR");
        }

        [TestMethod]
        public void TestSetValues_EditObjectProp()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Origin2[Country]", NewValue = "USA" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);
            Assert.AreEqual(item.Origin2.Country, "USA");
        }

        [TestMethod]
        public void TestSetValues_EditObjectPropOfNull()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Origin2[Country]", NewValue = "USA" });

            Reflector.Helper.Reflector.Reflector.SetValues(item, changes);
            Assert.AreEqual(item.Origin2.Country, "USA");
        }





        [TestMethod]
        public void TestGetValues()
        {

            var itemOld = new Item() { Location = "BR", Volumn = 1 };
            var itemNew = new Item() { Location = "BR", Volumn = 2, Suppliers = new List<Supplier>() { new Supplier() { Name = "AA" } } };


            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew);

            Assert.AreEqual(changes.Count, 2);


        }


        [TestMethod]
        public void TestGetValues2()
        {

            var itemOld = new Item() { Origin = new Origin() { Country = "BR" }, Volumn = 1 };
            var itemNew = new Item() { Origin = new Origin() { Country = "CL" }, Volumn = 2 };


            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew);

            Assert.AreEqual(changes.Count, 2);


        }



        [TestMethod]
        public void TestGetValues_DecimalPoint()
        {

            var itemOld = new Item() { Value = 1.0005 };
            var itemNew = new Item() { Value = 1.5050 };


            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew);

            Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "{0}", changes.First().NewValue), "1.505");


        }


        [TestMethod]
        public void TestGetValues_NullToEmpty_NotIgnoringNulls()
        {

            var itemOld = new Item() { Location = string.Empty };
            var itemNew = new Item() { Location = null };


            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew, false);

            Assert.AreEqual(changes.Count, 1);


        }

        [TestMethod]
        public void TestGetValues_Nullable_NotIgnoringNulls()
        {

            var itemOld = new Item() { NewId = null };
            var itemNew = new Item() { NewId = 0 };

            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew, false);

            Assert.AreEqual(changes.Count, 1);


        }


        [TestMethod]
        public void TestGetValues_NullToEmpty_IgnoringNulls()
        {

            var itemOld = new Item() { Location = string.Empty };
            var itemNew = new Item() { Location = null };


            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew, true);

            Assert.AreEqual(changes.Count, 0);


        }

        [TestMethod]
        public void TestGetValues_Nullable_IgnoringNulls()
        {

            var itemOld = new Item() { NewId = null };
            var itemNew = new Item() { NewId = 0 };

            var changes = new List<Change>();

            changes = Reflector.Helper.Reflector.Reflector.GetChanges(itemOld, itemNew, true);

            Assert.AreEqual(changes.Count, 0);


        }



        [TestMethod]
        public void TestTranslate_SimpleChange()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Location", OldValue = "BR", NewValue = "CL" });

            var phrases = Translator.Translate(item, changes);

            Assert.AreEqual(phrases.Count, 1);
            Assert.AreEqual(phrases.First(), "Pais de Origem: De [BR] Para [CL]");


        }

        [TestMethod]
        public void TestTranslate_IndexedProp()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Supplier[0][Name]", OldValue = "WW", NewValue = "AA" });

            var phrases = Translator.Translate(item, changes);

            Assert.AreEqual(phrases.Count, 1);
            Assert.AreEqual(phrases.First(), "Fabricante[WW] Nome: De [WW] Para [AA]");

        }


        [TestMethod]
        public void TestTranslate_ObjectProp()
        {

            var item = new Item() { Location = "BR", Volumn = 1, Origin2 = new Origin() { Country = "AR", State = "PASSOS" }, Suppliers = new List<Supplier>() { new Supplier() { Name = "WW" } } };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Origin2[State]", OldValue = "PASSOS", NewValue = "Buenos" });

            var phrases = Translator.Translate(item, changes);

            Assert.AreEqual(phrases.Count, 1);
            Assert.AreEqual(phrases.First(), "Origem de Compra - Estado: De [PASSOS] Para [Buenos]");

        }


        [TestMethod]
        public void TestTranslate_ValueProvider()
        {
            Translator.Providers.Add(RequestAttribute.ValueProvider.Status, new StatusProvider());

            var item = new Item() { Status = 1 };

            var changes = new List<Change>();
            changes.Add(new Change() { Field = "Status", OldValue = "1", NewValue = "2" });

            var phrases = Translator.Translate(item, changes);

            Assert.AreEqual(phrases.Count, 1);
            Assert.AreEqual(phrases.First(), "Estado do Item: De [EM ANDAMENTO] Para [NEGADO]");

        }




    }
}

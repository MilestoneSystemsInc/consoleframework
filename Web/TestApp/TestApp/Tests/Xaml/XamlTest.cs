﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Binding.Observables;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestApp;
using Xaml;

namespace TestProject1.Xaml
{
//    [TestClass]
    public class XamlTest
    {
        public class XamlObject
        {
            private readonly int x;
            public int X { get{return x;} }

            public XamlObject( int x ) {
                this.x = x;
            }

            public String StrProp { get; set; }

            public XamlObject Content { get; set; }

            private List<String>  items = new List< string >();
            public List< String > Items {
                get {return items;}
            }
        }

        public class ItemsDonor
        {
            private IObservableList items = new ObservableList( new List< string >() );
            public IObservableList Items
            {
                get { return items; }
            }
        }

        /// <summary>
        /// Just a loading object without default ctor using standard ObjectFactory.
        /// </summary>
//        [TestMethod]
        public void TestXamlObject1()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "TestApp.Tests.Xaml.XamlObject1.xml";
            XamlObject createdFromXaml;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                createdFromXaml = XamlParser.CreateFromXaml<XamlObject>(result, null, new List<string>());
            }
            Assert.IsTrue( createdFromXaml.X == 5 );
            Assert.IsTrue(createdFromXaml.StrProp == "str");
            Assert.IsTrue(createdFromXaml.Content.X == 10);
        }
    }
}

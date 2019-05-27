﻿using System;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Binding;
using Binding.Observables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.Binding
{
    [TestClass]
    public class CollectionsTest
    {
        class TargetClass
        {
            public TargetClass( ) {
                Items = new List< string >();
            }

            public List<String> Items
            {
                get; set; 
            }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public SourceClass( ) {
                SourceItems = new ObservableList<String>( new List<String>() );
            }

            public ObservableList<String> SourceItems { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged( string propertyName ) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        [TestMethod]
        public void TestListBinding()
        {
            SourceClass source = new SourceClass(  );
            TargetClass target = new TargetClass(  );
            BindingBase binding = new BindingBase( target, "Items", source, "SourceItems", BindingMode.OneWay );
            binding.Bind(  );
            source.SourceItems.Add( "1" );
            Assert.IsTrue(target.Items[0] == "1");
            source.SourceItems.Add( "2" );
            Assert.IsTrue( target.Items[0] == "1" );
            Assert.IsTrue(target.Items[1] == "2");
            source.SourceItems.Remove( "1" );
            Assert.IsTrue(target.Items.Count == 1);
            Assert.IsTrue(target.Items[0] == "2");
        }

        [TestMethod]
        public void TestListBinding2() {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "Items", source, "SourceItems", BindingMode.OneWay);
            source.SourceItems.Add("1");
            binding.Bind();
            Assert.IsTrue(target.Items[0] == "1");
            source.SourceItems.Remove("1");
            Assert.IsTrue(target.Items.Count == 0);
        }
    }
}

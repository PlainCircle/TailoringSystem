//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StitchingShop
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    
    public partial class DesignType : BusinessModel
    {
        public static readonly DependencyProperty DesignTypeIDProperty = DependencyProperty.Register("DesignTypeID", typeof(int), typeof(DesignType), new PropertyMetadata());
        public static readonly DependencyProperty DesignTypeNameProperty = DependencyProperty.Register("DesignTypeName", typeof(string), typeof(DesignType), new PropertyMetadata());
    
        public static readonly DependencyProperty DesignsProperty = DependencyProperty.Register("Designs", typeof(ObservableCollection<Design>), typeof(DesignType), new PropertyMetadata());
        public static readonly DependencyProperty SizeFieldsProperty = DependencyProperty.Register("SizeFields", typeof(ObservableCollection<SizeField>), typeof(DesignType), new PropertyMetadata());
        public DesignType()
        {
            this.Designs = new ObservableCollection<Design>();
            this.SizeFields = new ObservableCollection<SizeField>();
        }
    
        public int DesignTypeID
    	{
    		get
    		{
    			return (int)this.GetValue(DesignTypeIDProperty);
    		}
    		set
    		{
    			this.SetValue(DesignTypeIDProperty, value);
    		}
    	}
        public string DesignTypeName
    	{
    		get
    		{
    			return (string)this.GetValue(DesignTypeNameProperty);
    		}
    		set
    		{
    			this.SetValue(DesignTypeNameProperty, value);
    		}
    	}
    
        public virtual ObservableCollection<Design> Designs
    	{
    		get
    		{
    			return (ObservableCollection<Design>)this.GetValue(DesignsProperty);
    		}
    		set
    		{
    			this.SetValue(DesignsProperty, value);
    		}
    	}
        public virtual ObservableCollection<SizeField> SizeFields
    	{
    		get
    		{
    			return (ObservableCollection<SizeField>)this.GetValue(SizeFieldsProperty);
    		}
    		set
    		{
    			this.SetValue(SizeFieldsProperty, value);
    		}
    	}
    }
}
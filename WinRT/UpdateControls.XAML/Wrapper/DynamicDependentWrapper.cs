﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;

namespace UpdateControls.XAML.Wrapper
{
    class DynamicDependentWrapper : DynamicObject, INotifyPropertyChanged, IDataErrorInfo, IEditableObject
    {
        private readonly object _wrappedObject;

        private Dictionary<string, DependentProperty> _propertyByName = new Dictionary<string, DependentProperty>();

        public DynamicDependentWrapper(object wrappedObject)
        {
            _wrappedObject = wrappedObject;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            DependentProperty dependentProperty = GetDependentProperty(binder.Name);
            if (dependentProperty == null)
            {
                result = null;
                return false;
            }

            result = dependentProperty.GetValue();
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            DependentProperty dependentProperty = GetDependentProperty(binder.Name);
            if (dependentProperty == null)
                return false;

            dependentProperty.SetValue(value);
            return true;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;
            DynamicDependentWrapper that = obj as DynamicDependentWrapper;
            if (that == null)
                return false;
            return Object.Equals(this._wrappedObject, that._wrappedObject);
        }

        public override int GetHashCode()
        {
            return _wrappedObject.GetHashCode();
        }

        public override string ToString()
        {
            return _wrappedObject.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal object GetWrappedObject()
        {
            return _wrappedObject;
        }

        private DependentProperty GetDependentProperty(string propertyName)
        {
            DependentProperty dependentProperty;
            if (!_propertyByName.TryGetValue(propertyName, out dependentProperty))
            {
                PropertyInfo propertyInfo = _wrappedObject.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                    return null;

                dependentProperty = new DependentProperty(this, _wrappedObject, propertyInfo);
                _propertyByName.Add(propertyName, dependentProperty);
            }
            return dependentProperty;
        }

        public string Error
        {
            get
            {
                IDataErrorInfo wrappedObject = _wrappedObject as IDataErrorInfo;
                return wrappedObject != null ? wrappedObject.Error : null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                IDataErrorInfo wrappedObject = _wrappedObject as IDataErrorInfo;
                return wrappedObject != null ? wrappedObject[columnName] : null;
            }
        }

        public void BeginEdit()
        {
            IEditableObject wrappedObject = _wrappedObject as IEditableObject;
            if (wrappedObject != null)
                wrappedObject.BeginEdit();
        }

        public void CancelEdit()
        {
            IEditableObject wrappedObject = _wrappedObject as IEditableObject;
            if (wrappedObject != null)
                wrappedObject.CancelEdit();
        }

        public void EndEdit()
        {
            IEditableObject wrappedObject = _wrappedObject as IEditableObject;
            if (wrappedObject != null)
                wrappedObject.EndEdit();
        }
    }
}
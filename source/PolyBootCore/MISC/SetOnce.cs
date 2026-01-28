using System;

namespace PolyBootCore.MISC
{
  class SetOnce<T>
  {
    private string VariableName = null;
    private bool _isSet;
    private T _value;

    public bool IsSet => _isSet;

    public T Value
    {
      get
      {
        if (!_isSet)
        {
          if (VariableName == null)
            throw new InvalidOperationException("Value not set");
          else
            throw new InvalidOperationException($"Variable '{VariableName}' is not set");
        }
        return _value;
      }
      set
      {
        if (_isSet)
        {
          if (VariableName == null)
            throw new InvalidOperationException("Value already set");
          else
            throw new InvalidOperationException($"Variable '{VariableName}' is already set");
        }
        _value = value;
        _isSet = true;
      }
    }

    public SetOnce()
    {

    }

    public SetOnce(string variableName)
    {
      VariableName = variableName;
    }

    public bool TryGetValue(out T value)
    {
      if (!_isSet) { value = default(T); return false; }
      value = _value;
      return true;
    }

    public bool TrySetValue(T value)
    {
      if (_isSet)
        return false;
      _value = value;
      _isSet = true;
      return true;
    }
  }

}

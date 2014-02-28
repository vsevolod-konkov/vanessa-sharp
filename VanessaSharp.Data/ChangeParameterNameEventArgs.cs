using System;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Аргументы события изменения имени параметра.
    /// </summary>
    internal sealed class ChangeParameterNameEventArgs : EventArgs
    {
        private readonly string _oldName;
        private readonly string _newName;

        public ChangeParameterNameEventArgs(string oldName, string newName)
        {
            _oldName = oldName;
            _newName = newName;
        }

        public string OldName
        {
            get { return _oldName; }
        }

        public string NewName
        {
            get { return _newName; }
        }
    }
}

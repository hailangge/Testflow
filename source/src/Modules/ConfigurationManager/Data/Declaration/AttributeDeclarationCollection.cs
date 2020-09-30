using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Testflow.Data.Attributes;

namespace Testflow.ConfigurationManager.Data.Declaration
{
    public class AttributeDeclarationCollection : IAttributeDeclarationCollection
    {
        private readonly IDictionary<string, IAttributeDeclaration> _innerCollection;
        private readonly IDictionary<string, IList<string>> _declaredTypes;

        internal AttributeDeclarationCollection(int capacity)
        {
            _innerCollection = new Dictionary<string, IAttributeDeclaration>(capacity*4/3);
        }

        public IEnumerator<KeyValuePair<string, IAttributeDeclaration>> GetEnumerator()
        {
            return _innerCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, IAttributeDeclaration> item)
        {
            throw new System.NotImplementedException();
        }

        internal void Add(IAttributeDeclaration declaration)
        {
            if (!_declaredTypes.ContainsKey(declaration.Target))
            {
                _declaredTypes.Add(declaration.Target, new List<string>(10));
            }
            _declaredTypes[declaration.Target].Add(declaration.Type);
            _innerCollection.Add(declaration.FullName, declaration);
        }

        public void Clear()
        {
            _innerCollection.Clear();
        }

        public bool Contains(KeyValuePair<string, IAttributeDeclaration> item)
        {
            return _innerCollection.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IAttributeDeclaration>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, IAttributeDeclaration> item)
        {
            throw new System.NotImplementedException();
        }

        public int Count => _innerCollection.Count;
        public bool IsReadOnly => true;
        public bool ContainsKey(string key)
        {
            return _innerCollection.ContainsKey(key);
        }

        public void Add(string key, IAttributeDeclaration value)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(string key, out IAttributeDeclaration value)
        {
            return _innerCollection.TryGetValue(key, out value);
        }

        public IAttributeDeclaration this[string key]
        {
            get { return _innerCollection[key]; }
            set { throw new System.NotImplementedException(); }
        }

        public ICollection<string> Keys => _innerCollection.Keys;
        public ICollection<IAttributeDeclaration> Values => _innerCollection.Values;
        public IList<string> GetDeclaredTargets()
        {
            return new List<string>(_innerCollection.Keys);
        }

        public IList<string> GetDeclaredTypes(string target)
        {
            return _declaredTypes[target];
        }

        public IAttributeDeclaration GetAttributeDeclaration(string target, string type)
        {
            string typeFullName = AttributeDeclaration.GetTypeFullName(target, type);
            return _innerCollection[typeFullName];
        }
    }
}
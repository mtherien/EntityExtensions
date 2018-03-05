using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EntityExtensions.Common
{
    /// <summary>
    /// Stores information about an entity's column
    /// </summary>
    public class EntityColumnInformation
    {
        private object _discriminatorValue;

        public string Name { get; set; }

        public Type Type { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public bool HasDiscriminator { get; private set; }

        public object DiscriminatorValue
        {
            get => _discriminatorValue;
            set
            {
                HasDiscriminator = true;
                _discriminatorValue = value;
            }
        }
    }
}

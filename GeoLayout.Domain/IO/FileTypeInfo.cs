
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace GeoLayout.Domain.IO {
    public class FileTypeInfo : IFileTypeInfo {
        private readonly ReadOnlyCollection<string> _extensions;
        private readonly string _description;

        public FileTypeInfo(string description, string extension) : this(description, new[] { extension }) { }
        
        public FileTypeInfo(string description, IEnumerable<string> extensions) {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException();
            if (extensions == null)
                throw new ArgumentNullException();

            _extensions = extensions.ToList().AsReadOnly();

            if (_extensions.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException();
            if (_extensions.Count == 0)
                throw new ArgumentException();
            if (_extensions.Any(e => e[0] != '.'))
                throw new ArgumentException();

            _description = description;
            Groups = new string[0];
        }

        public string[] Groups { private get; set; }

        public ReadOnlyCollection<string> GetExtensions() {
            return _extensions;
        }
        public string GetDescription(CultureInfo ci) {
            return _description;
        }
        public string[] GetGroups() {
            return Groups;
        }

    }
}
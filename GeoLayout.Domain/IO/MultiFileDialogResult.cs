using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoLayout.Domain.IO {
    public class MultiFileDialogResult {
        private readonly IFileTypeInfo _fileTypeInfo;
        private readonly string[] _fileNames;

        public MultiFileDialogResult(IFileTypeInfo fileTypeInfo, string[] fileNames) {
            _fileTypeInfo = fileTypeInfo;
            _fileNames = fileNames;
        }

        public IFileTypeInfo FileTypeInfo {
            get { return _fileTypeInfo; }
        }
        public string[] FileNames {
            get { return _fileNames; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications.Services;

namespace GeoLayout.Domain.IO {
    public class SaveFileDialogResult {
        
        public SaveFileDialogResult(FileType fileType, string fileName) {
            FileType = fileType;
            FileName = fileName;
        }

        public FileType FileType { get; }
        public string FileName { get; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ProjectType
{
    internal static class FileAndContentTypes
    {
        [Export]
        [Name("mcc")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition hidingContentTypeDefinition;

        [Export]
        [FileExtension(".mcc")]
        [ContentType("mcc")]
        internal static FileExtensionToContentTypeDefinition hiddenFileExtensionDefinition;
    }
}

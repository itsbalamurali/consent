using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;

namespace CHC.Consent.DataTool.Features.ImportData
{
    [Command("import")]
    class ImportCommand
    {
        private readonly XmlImporter xmlImporter;

        [Required, LegalFilePath, Argument(0, "file", "file to import")]
        public string File { get; set; }

        
        
        /// <inheritdoc />
        public ImportCommand(Serilog.ILogger logger, ApiClientProvider apiClientProvider)
        {
            xmlImporter = new XmlImporter(logger, apiClientProvider);
        }

        [UsedImplicitly]
        private async Task<int> OnExecuteAsync()
        {
            await Import(File);

            return 0;
        }
        
        private async Task Import(string filePath)
        {
            await xmlImporter.Import(filePath);

        }
    }
}
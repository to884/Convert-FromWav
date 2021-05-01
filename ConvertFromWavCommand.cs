using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Convert_FromWav
{
    [Cmdlet(System.Management.Automation.VerbsData.Convert, "FromWav")]
    public class ConvertFromWavCommand : PSCmdlet
    {
        static void Main(string[] args)
        {
           // Nothing to do. 
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var wavs = new Kaitai.Wav[paths.Length];
            var index = 0;
            
            foreach (var p in paths)
            {
                var data = Kaitai.Wav.FromFile(p);
                wavs[index] = data;
            }
            
            WriteObject(wavs, true);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

        [Parameter(Mandatory = true)]
        public string[] Path
        {
            get { return paths; }
            set { paths = value;  }
        }

        private string[] paths;
    }
}
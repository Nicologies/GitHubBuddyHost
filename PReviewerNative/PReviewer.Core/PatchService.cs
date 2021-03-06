﻿using Nicologies;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PReviewer.Core
{
    public class PatchService: IPatchService
    {
        public async Task RevertViaPatch(string content, string patch, string patchTo)
        {
            var name = Path.GetFileName(patchTo);
            using (var stream = File.OpenWrite(patchTo))
            {
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(content);
                }
            }

            var dir = Path.GetDirectoryName(patchTo);
            var tmpPatchFile = Path.Combine(dir, Path.GetRandomFileName() + ".patch");
            using (var stream = File.OpenWrite(tmpPatchFile))
            {
                var headOfPatch = $"--- a/{name}\r\n+++ b/{name}\r\n";
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteLineAsync(headOfPatch);
                    await sw.WriteAsync(patch);
                    if (!patch.EndsWith("\n"))
                    {
                        await sw.WriteAsync("\n");
                    }
                }
            }

            var patchExe = Path.Combine(PathHelper.ProcessDir, "PatchBin", "patch.exe");

            var p = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = patchExe,
                    WorkingDirectory = dir,
                    CreateNoWindow = true,
                    Arguments = "-p1 -R --binary -i " + Path.GetFileName(tmpPatchFile)
                }
            };
            p.OutputDataReceived += p_OutputDataReceived;
            p.ErrorDataReceived += POnErrorDataReceived;
            p.Start();
            p.WaitForExit();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            try
            {
                File.Delete(tmpPatchFile);
            }
            catch
            {
                // ignored
            }
        }

        private void POnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var d = e.Data;
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var d = e.Data;
        }
    }
}

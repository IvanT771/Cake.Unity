﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Unity
{
    internal class SeekerOfEditors
    {
        private readonly ICakeEnvironment environment;
        private readonly IGlobber globber;
        private readonly ICakeLog log;

        public SeekerOfEditors(ICakeEnvironment environment, IGlobber globber, ICakeLog log)
        {
            this.environment = environment;
            this.globber = globber;
            this.log = log;
        }

        private string ProgramFiles => environment.GetSpecialPath(SpecialPath.ProgramFiles).FullPath;

        public IEnumerable<UnityEditorDescriptor> Seek()
        {
            string searchPattern = $"{ProgramFiles}/*Unity*/**/Editor/Unity.exe";

            log.Debug("Searching for available Unity Editors...");
            log.Debug("Search pattern: {0}", searchPattern);
            IEnumerable<FilePath> candidates = globber.GetFiles(searchPattern);

            return
                from candidatePath in candidates
                let version = DetermineVersion(candidatePath)
                where version != null
                select new UnityEditorDescriptor(version, candidatePath);
        }

        private string DetermineVersion(FilePath editorPath)
        {
            log.Debug("Determining version of Unity Editor at path {0}...", editorPath);

            var fileVersion = FileVersionInfo.GetVersionInfo(editorPath.FullPath);

            var (year, stream, update) = (fileVersion.FileMajorPart, fileVersion.FileMinorPart, fileVersion.FileBuildPart);

            if (year <= 0 || stream <= 0 || update < 0)
            {
                log.Debug("Failed: file version {0} is incorrect. Expected first two parts to be positive numbers and third one to be non negative.", fileVersion);
                return null;
            }

            string version = $"{year}.{stream}.{update}";

            log.Debug("Success: {0}", version);

            return version;
        }
    }
}

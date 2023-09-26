using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using VsFileMaker;

namespace VsFileMaker
{
    public class SLN : SerializableData<SLN>
    {
        private string VSversion = "17.7.34031.279", VSminVersion = "10.0.40219.1";
        private project project;
        private Global global;

        public SLN(FileInfo slnFile)
        {
            var sln = Deserialize(slnFile.FullName);
            project = sln.project;
            global = sln.global;
        }

        public SLN(string projectName, List<CSProj> csprojects = null) 
        {
            project = new project(projectName, csprojects);
            global = new Global();
            //SolutionConfigurationPlatforms Global Section
            Dictionary<string, string> keyValuePairs_forSolutionConfigurationPlatforms = new Dictionary<string, string>();
            keyValuePairs_forSolutionConfigurationPlatforms["Debug|Any CPU"] = "Debug|Any CPU";
            keyValuePairs_forSolutionConfigurationPlatforms["ExportDebug|Any CPU"] = "ExportDebug|Any CPU";
            keyValuePairs_forSolutionConfigurationPlatforms["ExportRelease|Any CPU"] = "ExportRelease|Any CPU";
            global.sections.Add(new GlobalSection("SolutionConfigurationPlatforms", "preSolution", keyValuePairs_forSolutionConfigurationPlatforms));

            //ProjectConfigurationPlatforms Global Section
            Dictionary<string, string> keyValuePairs_forProjectConfigurationPlatforms = new Dictionary<string, string>();
            var upper_project_id = project.projectId.ToString().ToUpper();
            keyValuePairs_forProjectConfigurationPlatforms[$"{{{upper_project_id}}}.Debug|Any CPU.ActiveCfg"] = "Debug|Any CPU";
            keyValuePairs_forProjectConfigurationPlatforms[$"{{{upper_project_id}}}.Debug|Any CPU.Build.0"] = "Debug|Any CPU";
            keyValuePairs_forProjectConfigurationPlatforms[$"{{{upper_project_id}}}.ExportDebug|Any CPU.ActiveCfg"] = "ExportDebug|Any CPU";
            keyValuePairs_forProjectConfigurationPlatforms[$"{{{upper_project_id}}}.ExportDebug|Any CPU.Build.0"] = "ExportDebug|Any CPU";
            keyValuePairs_forProjectConfigurationPlatforms[$"{{{upper_project_id}}}.ExportRelease|Any CPU.ActiveCfg"] = "ExportRelease|Any CPU";
            keyValuePairs_forProjectConfigurationPlatforms[$"{{{upper_project_id}}}.ExportRelease|Any CPU.Build.0"] = "ExportRelease|Any CPU";
            global.sections.Add(new GlobalSection("ProjectConfigurationPlatforms", "postSolution", keyValuePairs_forProjectConfigurationPlatforms));
        }


        public SLN(string projectName, Dictionary<string, string> SolutionConfigurationPlatforms, Dictionary<string, string> ProjectConfigurationPlatforms, List<CSProj> csprojects = null)
        {
            project = new project(projectName, csprojects);
            global = new Global();
            //SolutionConfigurationPlatforms Global Section
            global.sections.Add(new GlobalSection("SolutionConfigurationPlatforms", "preSolution", SolutionConfigurationPlatforms));

            //ProjectConfigurationPlatforms Global Section
            global.sections.Add(new GlobalSection("ProjectConfigurationPlatforms", "postSolution", ProjectConfigurationPlatforms));
        }

        private SLN(project project, Global global)
        {
            this.project = project;
            this.global = global;
        }

        public SLN Deserialize(string filelocation)
        {
            FileInfo fi = new FileInfo(filelocation);
            project = new project(fi);
            global = new Global(fi);
            SLN sln = new SLN(project, global);
            return sln;
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine($"# Visual Studio 2012");
            sb.AppendLine($"VisualStudioVersion = {VSversion}");
            sb.AppendLine($"MinimumVisualStudioVersion = {VSminVersion}");
            sb.AppendLine(project.Serialize());
            sb.AppendLine(global.Serialize());
            return sb.ToString();
        }
    }

    internal class project : SerializableData<project>
    {
        public Guid configId { get; private set; }
        public Guid projectId { get; private set; }
        public string projectName { get; private set; }
        public List<CSProj> csprojList { get; private set; }

        public project(Guid configId, Guid projectId, string projectName, List<CSProj> csprojList)
        {
            this.configId = configId;
            this.projectId = projectId;
            this.projectName = projectName;
            this.csprojList = csprojList;
        }

        public project(FileInfo slnFile)
        {
            string file_data = File.ReadAllText(slnFile.FullName);
            project deseialized = Deserialize(file_data);
            configId = deseialized.configId;
            projectId = deseialized.projectId;
            projectName = deseialized.projectName;
            csprojList = deseialized.csprojList;
        }

        public project(string projectName, List<CSProj> csprojList = null) 
        { 
            configId = Guid.NewGuid();
            projectId = Guid.NewGuid();
            this.projectName = projectName;
            this.csprojList = csprojList;
        }

        public project Deserialize(string filedata)
        {
            string pattern = @"Project\(([^)]*)\) = ""([^""]+)""(?:,\s*""([^""]+)"")*[^""]*EndProject";
            RegexOptions options = RegexOptions.Singleline;

            MatchCollection matches = Regex.Matches(filedata, pattern, options);

            if (matches.Count > 0)
            {
                string configId = matches[0].Groups[1].Value.Replace("\"", "").Replace("{", "").Replace("}", "");
                string project_name = matches[0].Groups[2].Value.Replace("\"", "");
                var projectID = matches[0].Groups[3].Captures[matches[0].Groups[3].Captures.Count-1].Value.Replace("\"", "").Replace("{", "").Replace("}", "");

                List<CSProj> CsprojList = null;
                if(matches[0].Groups[3].Captures.Count > 1)
                {
                    CsprojList = new List<CSProj>();
                    for (int i = 0; i < matches[0].Groups[3].Captures.Count; i++)
                    {
                        var match = matches[0].Groups[3].Captures[i];
                        if (match.Value.Contains("csproj"))
                        {
                            CsprojList.Add(new CSProj(match.Value));
                        }
                    }
                }
                project p = new project(Guid.Parse(configId), Guid.Parse(projectID), project_name, CsprojList);

                return p;
            }
            return null;
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Project(\"{{{configId.ToString().ToUpper()}}}\") = \"{projectName}\"");
            if(csprojList != null)
            {
                foreach(CSProj proj in csprojList)
                {
                    sb.Append($", \"{proj.pathFromRoot}\"");
                }
            }
            sb.AppendLine($", \"{{{projectId.ToString().ToUpper()}}}\"");
            sb.AppendLine("EndProject");
            return sb.ToString();
        }
    }

    internal class Global : SerializableData<Global>
    {
        const string pattern = @"Global([\s\S]*?)EndGlobal";
        public List<GlobalSection> sections { get; private set; } = new List<GlobalSection>();

        public Global(string filedata)
        {
            var g =  Deserialize(filedata);
            sections = g.sections;
        }

        public Global(FileInfo file)
        {
            var g = Deserialize(File.ReadAllText(file.FullName));
            sections = g.sections;
        }

        public Global()
        {
        }

        public Global Deserialize(string filedata)
        {
            RegexOptions options = RegexOptions.Singleline;
            Global global = new Global();
            MatchCollection matches = Regex.Matches(filedata, pattern, options);
            foreach(Match match in matches)
            {
                global.sections.Add(new GlobalSection(match.Value));
            }

            return global;
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Global");
            foreach(GlobalSection section in sections)
            {
                sb.AppendLine(section.Serialize());
            }
            sb.AppendLine("EndGlobal");

            return sb.ToString();
        }
    }

    internal class GlobalSection : SerializableData<GlobalSection>
    {
        const string pattern = @"GlobalSection\(([^)]*)\)\s*=\s*([^\n]*)(?:\s*([^=]*\s*=\s*[^\n]*))+(?=\s*EndGlobalSection|$)";
        private string sectionName, sectionEvent;
        public Dictionary<string, string> section_data { get; private set; } = new Dictionary<string, string>();

        public GlobalSection(string sectionName, string sectionEvent, Dictionary<string, string> section_data)
        {
            this.sectionEvent = sectionEvent;
            this.sectionName = sectionName;
            this.section_data = section_data;
        }

        public GlobalSection(string filedata)
        {
           var us = Deserialize(filedata);
            sectionEvent  = us.sectionEvent; sectionName = us.sectionName;
            section_data = us.section_data;
        }

        public GlobalSection Deserialize(string filedata)
        {
            RegexOptions opt = RegexOptions.Multiline;
            MatchCollection matches = Regex.Matches(filedata, pattern, opt);
            Match match = matches[0];

            Dictionary<string, string> section_dict = new Dictionary<string, string>();

            for (int i = 0; i < match.Groups[3].Captures.Count; i++)
            {
                string[] values = match.Groups[3].Captures[i].Value.Split('=');
                section_dict[values[0]] = values[1];
            }

            GlobalSection g = new GlobalSection(match.Groups[1].Value, match.Groups[2].Value, section_dict);

            return g;
        }

        public string Serialize()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"GlobalSection({sectionName}) = {sectionEvent}");
            foreach (var item in section_data)
            {
                stringBuilder.AppendLine($"{item.Key} = {item.Value}");
            }
            stringBuilder.AppendLine("EndGlobalSection");
            return stringBuilder.ToString();
        }
    }
}

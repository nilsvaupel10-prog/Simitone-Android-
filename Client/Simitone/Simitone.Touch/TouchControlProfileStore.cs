using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Simitone.Touch
{
    /// <summary>
    /// Reads and writes the on-screen control profiles.
    /// </summary>
    /// <remarks>
    /// Profiles are plain JSON files so that they can be backed up, shared or
    /// edited by hand. The store keeps one file per profile inside a single
    /// folder.
    /// </remarks>
    public class TouchControlProfileStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IncludeFields = false
        };

        private readonly string directory;

        /// <summary>Folder that holds the profile files.</summary>
        public string Directory
        {
            get { return directory; }
        }

        public TouchControlProfileStore(string directory)
        {
            this.directory = directory;
        }

        private string PathFor(string name)
        {
            return System.IO.Path.Combine(directory, SanitizeFileName(name) + ".json");
        }

        /// <summary>Makes a profile name safe to use as a file name.</summary>
        public static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "profile";
            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' ') builder.Append(c);
                else builder.Append('_');
            }
            var result = builder.ToString().Trim();
            return result.Length == 0 ? "profile" : result;
        }

        /// <summary>Lists the names of every stored profile.</summary>
        public List<string> ListProfiles()
        {
            var names = new List<string>();
            try
            {
                if (!System.IO.Directory.Exists(directory)) return names;
                foreach (var file in System.IO.Directory.GetFiles(directory, "*.json"))
                {
                    names.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }
            }
            catch (Exception)
            {
                // A broken profile directory must not stop the game.
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        /// <summary>True when a profile with that name exists.</summary>
        public bool Exists(string name)
        {
            try
            {
                return File.Exists(PathFor(name));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Saves a profile to disk.</summary>
        public void Save(TouchControlProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            try
            {
                System.IO.Directory.CreateDirectory(directory);
                var json = JsonSerializer.Serialize(profile, SerializerOptions);
                File.WriteAllText(PathFor(profile.Name), json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                throw new IOException("Could not save the control profile '" + profile.Name + "'.", e);
            }
        }

        /// <summary>Loads a profile from disk (returns null when it does not exist).</summary>
        public TouchControlProfile Load(string name)
        {
            try
            {
                var path = PathFor(name);
                if (!File.Exists(path)) return null;
                var json = File.ReadAllText(path, Encoding.UTF8);
                var profile = JsonSerializer.Deserialize<TouchControlProfile>(json, SerializerOptions);
                if (profile != null)
                {
                    if (string.IsNullOrEmpty(profile.Name)) profile.Name = name;
                    if (profile.Settings == null) profile.Settings = new TouchControlSettings();
                    if (profile.Widgets == null) profile.Widgets = new Dictionary<string, TouchWidgetLayout>();
                    if (profile.Order == null) profile.Order = new List<string>();
                }
                return profile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Deletes a stored profile.</summary>
        public bool Delete(string name)
        {
            try
            {
                var path = PathFor(name);
                if (!File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Renames a stored profile.</summary>
        public bool Rename(string oldName, string newName)
        {
            var profile = Load(oldName);
            if (profile == null) return false;
            profile.Name = newName;
            Save(profile);
            Delete(oldName);
            return true;
        }
    }
}

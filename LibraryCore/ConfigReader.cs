using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Library
{
    public static class ConfigReader
    {
        private static readonly Regex HeaderRegex = new Regex(@"^\[(?<Header>.+)\]$", RegexOptions.Compiled);
        private static readonly Regex EntryRegex = new Regex(@"^(?<Key>.*?)=(?<Value>.*)$", RegexOptions.Compiled);
        private static readonly Regex ColorRegex = new Regex(@"\[A:\s?(?<A>[0-9]{1,3}),\s?R:\s?(?<R>[0-9]{1,3}),\s?G:\s?(?<G>[0-9]{1,3}),\s?B:\s?(?<B>[0-9]{1,3})\]", RegexOptions.Compiled);

        public static readonly Dictionary<Type, object> ConfigObjects = new Dictionary<Type, object>();

        private static readonly Dictionary<Type, Dictionary<string, Dictionary<string, string>>> ConfigContents = new Dictionary<Type, Dictionary<string, Dictionary<string, string>>>();
        
        public static void Load(Assembly assembly)
        {
            try
            {
                Type[] types = assembly.GetTypes();
            
                foreach (Type type in types)
                {
                    ConfigPath config = type.GetCustomAttribute<ConfigPath>();

                    if (config == null) continue;

                    object ob = null;
                    if (!type.IsAbstract || !type.IsSealed)
                        ConfigObjects[type] = ob = Activator.CreateInstance(type);

                    ReadConfig(type, config.Path, ob);
                }

            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static void Save(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                ConfigPath config = type.GetCustomAttribute<ConfigPath>();

                if (config == null) continue;

                object ob = null;

                if (!type.IsAbstract || !type.IsSealed)
                    ob = ConfigObjects[type];

                SaveConfig(type, config.Path, ob);
            }
        }

        private static void ReadConfig(Type type, string path, object ob)
        {
            if (!File.Exists(path)) return;

            PropertyInfo[] properties = type.GetProperties();

            Dictionary<string, Dictionary<string, string>> contents = ConfigContents[type] = new Dictionary<string, Dictionary<string, string>>();
            
            string[] lines = File.ReadAllLines(path);

            Dictionary<string, string> section = null;

            foreach (string line in lines)
            {
                Match match = HeaderRegex.Match(line);
                if (match.Success)
                {
                    section = new Dictionary<string, string>();
                    contents[match.Groups["Header"].Value] = section;
                    continue;
                }

                if (section == null) continue;

                match = EntryRegex.Match(line);

                if (!match.Success) continue;

                section[match.Groups["Key"].Value] = match.Groups["Value"].Value;
            }

            string lastSection = null;

            foreach (PropertyInfo property in properties)
            {
                ConfigSection config = property.GetCustomAttribute<ConfigSection>();

                if (config != null) lastSection = config.Section;

                if (lastSection == null) continue;

                ConfigPropertyIgnore ignore = property.GetCustomAttribute<ConfigPropertyIgnore>();

                if (ignore != null) continue;

                MethodInfo method = typeof(ConfigReader).GetMethod("Read", new[] { typeof(Type), typeof(string), typeof(string), property.PropertyType });

                property.SetValue(ob, method.Invoke(null, new[] { type, lastSection, property.Name, property.GetValue(ob) }));
            }
        }
        private static void SaveConfig(Type type, string path, object ob)
        {
            PropertyInfo[] properties = type.GetProperties();
            Dictionary<string, Dictionary<string, string>> contents = ConfigContents[type] = new Dictionary<string, Dictionary<string, string>>();

            string lastSection = null;

            foreach (PropertyInfo property in properties)
            {
                ConfigSection config = property.GetCustomAttribute<ConfigSection>();

                if (config != null) lastSection = config.Section;

                if (lastSection == null) continue;

                ConfigPropertyIgnore ignore = property.GetCustomAttribute<ConfigPropertyIgnore>();

                if (ignore != null) continue;

                MethodInfo method = typeof(ConfigReader).GetMethod("Write", new[] { typeof(Type), typeof(string), typeof(string), property.PropertyType });

                method.Invoke(ob, new[] { type, lastSection, property.Name, property.GetValue(ob) });
            }

            List<string> lines = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, string>> header in contents)
            {
                lines.Add($"[{header.Key}]");

                foreach (KeyValuePair<string, string> entries in header.Value)
                    lines.Add($"{entries.Key}={entries.Value}");

                lines.Add(string.Empty);
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllLines(path, lines, Encoding.Unicode);
        }

        private static bool TryGetEntry(Type type, string section, string key, out string value)
        {
            value = null;
            Dictionary<string, Dictionary<string, string>> contents;
            Dictionary<string, string> entries;

            if (!ConfigContents.TryGetValue(type, out contents))
                ConfigContents[type] = contents = new Dictionary<string, Dictionary<string, string>>();
                
            if (contents.TryGetValue(section, out entries))
                return entries.TryGetValue(key, out value);

            entries = new Dictionary<string, string>();
            contents[section] = entries;

            return false;
        }

        #region Reads
        public static Boolean Read(Type type,string section, string key, Boolean value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Boolean result;

                if (Boolean.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }

        public static Byte Read(Type type, string section, string key, Byte value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Byte result;

                if (Byte.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static Int16 Read(Type type, string section, string key, Int16 value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Int16 result;

                if (Int16.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static Int32 Read(Type type, string section, string key, Int32 value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Int32 result;

                if (Int32.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static Int64 Read(Type type, string section, string key, Int64 value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Int64 result;

                if (Int64.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }

        public static SByte Read(Type type, string section, string key, SByte value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                SByte result;

                if (SByte.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static UInt16 Read(Type type, string section, string key, UInt16 value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                UInt16 result;

                if (UInt16.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static UInt32 Read(Type type, string section, string key, UInt32 value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                UInt32 result;

                if (UInt32.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static UInt64 Read(Type type, string section, string key, UInt64 value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                UInt64 result;

                if (UInt64.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }

        public static Single Read(Type type, string section, string key, Single value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Single result;

                if (Single.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);

            return value;
        }
        public static Double Read(Type type, string section, string key, Double value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Double result;

                if (Double.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);

            return value;
        }
        public static Decimal Read(Type type, string section, string key, Decimal value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Decimal result;

                if (Decimal.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);

            return value;
        }

        public static Char Read(Type type, string section, string key, Char value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                Char result;

                if (Char.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static String Read(Type type, string section, string key, String value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
                return entry;

            ConfigContents[type][section][key] = value;

            return value;
        }
        
        public static Point Read(Type type, string section, string key, Point value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                string[] data = entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int x, y;

                if (data.Length == 2 && int.TryParse(data[0], out x) && int.TryParse(data[1], out y))
                    return new Point(x, y);
            }

            ConfigContents[type][section][key] = $"{value.X}, {value.Y}";

            return value;
        }
        public static Size Read(Type type, string section, string key, Size value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                string[] data = entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int width, height;

                if (data.Length == 2 && int.TryParse(data[0], out width) && int.TryParse(data[1], out height))
                    return new Size(width, height);
            }

            ConfigContents[type][section][key] = $"{value.Width}, {value.Height}";

            return value;
        }
        public static SizeF Read(Type type, string section, string key, SizeF value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                string[] data = entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                float width, height;

                if (data.Length == 2 && float.TryParse(data[0], out width) && float.TryParse(data[1], out height))
                    return new SizeF(width, height);
            }

            ConfigContents[type][section][key] = $"{value.Width}, {value.Height}";

            return value;
        }

        public static TimeSpan Read(Type type, string section, string key, TimeSpan value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                TimeSpan result;

                if (TimeSpan.TryParse(entry, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString();

            return value;
        }
        public static DateTime Read(Type type, string section, string key, DateTime value)
        {
            string entry;

            if (TryGetEntry(type, section, key, out entry))
            {
                DateTime result;

                if (DateTime.TryParse(entry, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return result;
            }

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);

            return value;
        }

        public static Color Read(Type type, string section, string key, Color value)
        {
            string entry;


            if (TryGetEntry(type, section, key, out entry))
            {
                Match match = ColorRegex.Match(entry);
                if (match.Success)
                {
                    int a = int.Parse(match.Groups["A"].Value);
                    int r = int.Parse(match.Groups["R"].Value);
                    int g = int.Parse(match.Groups["G"].Value);
                    int b = int.Parse(match.Groups["B"].Value);
                    
                    return Color.FromArgb(
                        Math.Min(Byte.MaxValue, Math.Max(Byte.MinValue, a)), 
                        Math.Min(Byte.MaxValue, Math.Max(Byte.MinValue, r)),
                        Math.Min(Byte.MaxValue, Math.Max(Byte.MinValue, g)),
                        Math.Min(Byte.MaxValue, Math.Max(Byte.MinValue, b)));
                }
            }

            ConfigContents[type][section][key] = $"[A:{value.A}, R:{value.R}, G:{value.G}, B:{value.B}]";

            return value;
        }
        #endregion

        #region Writes
        public static void Write(Type type, string section, string key, Boolean value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }

        public static void Write(Type type, string section, string key, Byte value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, Int16 value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, Int32 value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, Int64 value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }

        public static void Write(Type type, string section, string key, SByte value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, UInt16 value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, UInt32 value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, UInt64 value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }

        public static void Write(Type type, string section, string key, Single value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);
        }
        public static void Write(Type type, string section, string key, Double value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);
        }
        public static void Write(Type type, string section, string key, Decimal value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);
        }

        public static void Write(Type type, string section, string key, Char value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, String value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value;
        }

        public static void Write(Type type, string section, string key, Point value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = $"{value.X}, {value.Y}";
        }
        public static void Write(Type type, string section, string key, Size value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = $"{value.Width}, {value.Height}";
        }

        public static void Write(Type type, string section, string key, TimeSpan value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString();
        }
        public static void Write(Type type, string section, string key, DateTime value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = value.ToString(CultureInfo.InvariantCulture);
        }
        public static void Write(Type type, string section, string key, Color value)
        {
            if (!ConfigContents[type].ContainsKey(section)) ConfigContents[type][section] = new Dictionary<string, string>();

            ConfigContents[type][section][key] = $"[A:{value.A}, R:{value.R}, G:{value.G}, B:{value.B}]";
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigPath : Attribute
    {
        public string Path { get; set; }

        public ConfigPath(string path)
        {
            Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigSection : Attribute
    {
        public string Section { get; set; }

        public ConfigSection(string section)
        {
            Section = section;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigPropertyIgnore : Attribute
    {
        public ConfigPropertyIgnore() { }
    }

}

#nullable disable
//
// Options.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>, <Jonathan.Pryor@microsoft.com>
//  Federico Di Gregorio <fog@initd.org>
//  Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
// Copyright (C) 2009 Federico Di Gregorio.
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
// Copyright (C) 2017 Microsoft Corporation (http://www.microsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

// Compile With:
//   mcs -debug+ -r:System.Core Options.cs -o:Mono.Options.dll -t:library
//   mcs -debug+ -d:LINQ -r:System.Core Options.cs -o:Mono.Options.dll -t:library
//
// The LINQ version just changes the implementation of
// OptionSet.Parse(IEnumerable<string>), and confers no semantic changes.

//
// A Getopt::Long-inspired option parsing library for C#.
//
// Mono.Options.OptionSet is built upon a key/value table, where the
// key is a option format string and the value is a delegate that is 
// invoked when the format string is matched.
//
// Option format strings:
//  Regex-like BNF Grammar: 
//    name: .+
//    type: [=:]
//    sep: ( [^{}]+ | '{' .+ '}' )?
//    aliases: ( name type sep ) ( '|' name type sep )*
// 
// Each '|'-delimited name is an alias for the associated action.  If the
// format string ends in a '=', it has a required value.  If the format
// string ends in a ':', it has an optional value.  If neither '=' or ':'
// is present, no value is supported.  `=' or `:' need only be defined on one
// alias, but if they are provided on more than one they must be consistent.
//
// Each alias portion may also end with a "key/value separator", which is used
// to split option values if the option accepts > 1 value.  If not specified,
// it defaults to '=' and ':'.  If specified, it can be any character except
// '{' and '}' OR the *string* between '{' and '}'.  If no separator should be
// used (i.e. the separate values should be distinct arguments), then "{}"
// should be used as the separator.
//
// Options are extracted either from the current option by looking for
// the option name followed by an '=' or ':', or is taken from the
// following option IFF:
//  - The current option does not contain a '=' or a ':'
//  - The current option requires a value (i.e. not a Option type of ':')
//
// The `name' used in the option format string does NOT include any leading
// option indicator, such as '-', '--', or '/'.  All three of these are
// permitted/required on any named option.
//
// Option bundling is permitted so long as:
//   - '-' is used to start the option group
//   - all of the bundled options are a single character
//   - at most one of the bundled options accepts a value, and the value
//     provided starts from the next character to the end of the string.
//
// This allows specifying '-a -b -c' as '-abc', and specifying '-D name=value'
// as '-Dname=value'.
//
// Option processing is disabled by specifying "--".  All options after "--"
// are returned by OptionSet.Parse() unchanged and unprocessed.
//
// Unprocessed options are returned from OptionSet.Parse().
//
// Examples:
//  int verbose = 0;
//  OptionSet p = new OptionSet ()
//    .Add ("v", v => ++verbose)
//    .Add ("name=|value=", v => Console.WriteLine (v));
//  p.Parse (new string[]{"-v", "--v", "/v", "-name=A", "/name", "B", "extra"});
//
// The above would parse the argument string array, and would invoke the
// lambda expression three times, setting `verbose' to 3 when complete.  
// It would also print out "A" and "B" to standard output.
// The returned array would contain the string "extra".
//
// C# 3.0 collection initializers are supported and encouraged:
//  var p = new OptionSet () {
//    { "h|?|help", v => ShowHelp () },
//  };
//
// System.ComponentModel.TypeConverter is also supported, allowing the use of
// custom data types in the callback type; TypeConverter.ConvertFromString()
// is used to convert the value option to an instance of the specified
// type:
//
//  var p = new OptionSet () {
//    { "foo=", (Foo f) => Console.WriteLine (f.ToString ()) },
//  };
//
// Random other tidbits:
//  - Boolean options (those w/o '=' or ':' in the option format string)
//    are explicitly enabled if they are followed with '+', and explicitly
//    disabled if they are followed with '-':
//      string a = null;
//      var p = new OptionSet () {
//        { "a", s => a = s },
//      };
//      p.Parse (new string[]{"-a"});   // sets v != null
//      p.Parse (new string[]{"-a+"});  // sets v != null
//      p.Parse (new string[]{"-a-"});  // sets v == null
//

//
// Mono.Options.CommandSet allows easily having separate commands and
// associated command options, allowing creation of a *suite* along the
// lines of **git**(1), **svn**(1), etc.
//
// CommandSet allows intermixing plain text strings for `--help` output,
// Option values -- as supported by OptionSet -- and Command instances,
// which have a name, optional help text, and an optional OptionSet.
//
//  var suite = new CommandSet ("suite-name") {
//    // Use strings and option values, as with OptionSet
//    "usage: suite-name COMMAND [OPTIONS]+",
//    { "v:", "verbosity", (int? v) => Verbosity = v.HasValue ? v.Value : Verbosity+1 },
//    // Commands may also be specified
//    new Command ("command-name", "command help") {
//      Options = new OptionSet {/*...*/},
//      Run     = args => { /*...*/},
//    },
//    new MyCommandSubclass (),
//  };
//  return suite.Run (new string[]{...});
//
// CommandSet provides a `help` command, and forwards `help COMMAND`
// to the registered Command instance by invoking Command.Invoke()
// with `--help` as an option.
//

#if PCL
using System.Reflection;
#else
using System.Runtime.Serialization;
using System.Security.Permissions;
#endif

#if LINQ
using System.Linq;
#endif

#if TEST
using NDesk.Options;
#endif

#if PCL
using MessageLocalizerConverter = System.Func<string, string>;
#else
using MessageLocalizerConverter = System.Converter<string, string>;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#if NDESK_OPTIONS
namespace NDesk.Options
#else
namespace Mono
#endif
{
    static class StringCoda
    {
        public static IEnumerable<string> WrappedLines(string self, params int[] widths)
        {
            IEnumerable<int> w = widths;
            return WrappedLines(self, w);
        }

        public static IEnumerable<string> WrappedLines(string self, IEnumerable<int> widths)
        {
            if (widths == null)
                throw new ArgumentNullException("widths");
            return CreateWrappedLinesIterator(self, widths);
        }

        private static IEnumerable<string> CreateWrappedLinesIterator(string self, IEnumerable<int> widths)
        {
            if (string.IsNullOrEmpty(self))
            {
                yield return string.Empty;
                yield break;
            }

            using (IEnumerator<int> ewidths = widths.GetEnumerator())
            {
                bool? hw = null;
                int width = GetNextWidth(ewidths, int.MaxValue, ref hw);
                int start = 0, end;
                do
                {
                    end = GetLineEnd(start, width, self);
                    char c = self[end - 1];
                    if (char.IsWhiteSpace(c))
                        --end;
                    bool needContinuation = end != self.Length && !IsEolChar(c);
                    string continuation = "";
                    if (needContinuation)
                    {
                        --end;
                        continuation = "-";
                    }

                    string line = self.Substring(start, end - start) + continuation;
                    yield return line;
                    start = end;
                    if (char.IsWhiteSpace(c))
                        ++start;
                    width = GetNextWidth(ewidths, width, ref hw);
                } while (start < self.Length);
            }
        }

        private static int GetNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
        {
            if (!eValid.HasValue || ( eValid.HasValue && eValid.Value ))
            {
                curWidth = ( eValid = ewidths.MoveNext() ).Value ? ewidths.Current : curWidth;
                // '.' is any character, - is for a continuation
                const string minWidth = ".-";
                if (curWidth < minWidth.Length)
                    throw new ArgumentOutOfRangeException("widths",
                        string.Format("Element must be >= {0}, was {1}.", minWidth.Length, curWidth));
                return curWidth;
            }

            // no more elements, use the last element.
            return curWidth;
        }

        private static bool IsEolChar(char c)
        {
            return !char.IsLetterOrDigit(c);
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int end = System.Math.Min(start + length, description.Length);
            int sep = -1;
            for (int i = start; i < end; ++i)
            {
                if (description[i] == '\n')
                    return i + 1;
                if (IsEolChar(description[i]))
                    sep = i + 1;
            }

            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }
    }

    public class MonoOptionValueCollection : IList, IList<string>
    {
        List<string> values = new();
        MonoOptionContext c;

        internal MonoOptionValueCollection(MonoOptionContext c)
        {
            this.c = c;
        }

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            ( this.values as ICollection ).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { return ( this.values as ICollection ).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ( this.values as ICollection ).SyncRoot; }
        }

        #endregion

        #region ICollection<T>

        public void Add(string item)
        {
            this.values.Add(item);
        }

        public void Clear()
        {
            this.values.Clear();
        }

        public bool Contains(string item)
        {
            return this.values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            this.values.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return this.values.Remove(item);
        }

        public int Count
        {
            get { return this.values.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.values.GetEnumerator();
        }

        #endregion

        #region IEnumerable<T>

        public IEnumerator<string> GetEnumerator()
        {
            return this.values.GetEnumerator();
        }

        #endregion

        #region IList

        int IList.Add(object value)
        {
            return ( this.values as IList ).Add(value);
        }

        bool IList.Contains(object value)
        {
            return ( this.values as IList ).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return ( this.values as IList ).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ( this.values as IList ).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            ( this.values as IList ).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            ( this.values as IList ).RemoveAt(index);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { ( this.values as IList )[index] = value; }
        }

        #endregion

        #region IList<T>

        public int IndexOf(string item)
        {
            return this.values.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            this.values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.values.RemoveAt(index);
        }

        private void AssertValid(int index)
        {
            if (this.c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= this.c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException("index");
            if (this.c.Option.OptionValueType == OptionValueType.Required &&
                index >= this.values.Count)
                throw new MonoOptionException(string.Format(
                        this.c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), this.c.OptionName),
                    this.c.OptionName);
        }

        public string this[int index]
        {
            get
            {
                this.AssertValid(index);
                return index >= this.values.Count ? null : this.values[index];
            }
            set { this.values[index] = value; }
        }

        #endregion

        public List<string> ToList()
        {
            return new List<string>(this.values);
        }

        public string[] ToArray()
        {
            return this.values.ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", this.values.ToArray());
        }
    }

    public class MonoOptionContext
    {
        private MonoOption option;
        private string name;
        private int index;
        private MonoOptionSet set;
        private MonoOptionValueCollection c;

        public MonoOptionContext(MonoOptionSet set)
        {
            this.set = set;
            this.c = new MonoOptionValueCollection(this);
        }

        public MonoOption Option
        {
            get { return this.option; }
            set { this.option = value; }
        }

        public string OptionName
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public int OptionIndex
        {
            get { return this.index; }
            set { this.index = value; }
        }

        public MonoOptionSet OptionSet
        {
            get { return this.set; }
        }

        public MonoOptionValueCollection OptionValues
        {
            get { return this.c; }
        }
    }

    public enum OptionValueType
    {
        None,
        Optional,
        Required,
    }

    public abstract class MonoOption
    {
        string prototype, description;
        string[] names;
        OptionValueType type;
        int count;
        string[] separators;
        bool hidden;

        protected MonoOption(string prototype, string description)
            : this(prototype, description, 1, false)
        {
        }

        protected MonoOption(string prototype, string description, int maxValueCount)
            : this(prototype, description, maxValueCount, false)
        {
        }

        protected MonoOption(string prototype, string description, int maxValueCount, bool hidden)
        {
            if (prototype == null)
                throw new ArgumentNullException("prototype");
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", "prototype");
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException("maxValueCount");

            this.prototype = prototype;
            this.description = description;
            this.count = maxValueCount;
            this.names = ( this is MonoOptionSet.Category )
                // append GetHashCode() so that "duplicate" categories have distinct
                // names, e.g. adding multiple "" categories should be valid.
                ? new[] { prototype + this.GetHashCode() }
                : prototype.Split('|');

            if (this is MonoOptionSet.Category || this is CommandOption)
                return;

            this.type = this.ParsePrototype();
            this.hidden = hidden;

            if (this.count == 0 && this.type != OptionValueType.None)
                throw new ArgumentException(
                    "Cannot provide maxValueCount of 0 for OptionValueType.Required or " +
                    "OptionValueType.Optional.",
                    "maxValueCount");
            if (this.type == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(
                    string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount),
                    "maxValueCount");
            if (Array.IndexOf(this.names, "<>") >= 0 &&
                ( ( this.names.Length == 1 && this.type != OptionValueType.None ) ||
                  ( this.names.Length > 1 && this.MaxValueCount > 1 ) ))
                throw new ArgumentException(
                    "The default option handler '<>' cannot require values.",
                    "prototype");
        }

        public string Prototype
        {
            get { return this.prototype; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public OptionValueType OptionValueType
        {
            get { return this.type; }
        }

        public int MaxValueCount
        {
            get { return this.count; }
        }

        public bool Hidden
        {
            get { return this.hidden; }
        }

        public string[] GetNames()
        {
            return (string[])this.names.Clone();
        }

        public string[] GetValueSeparators()
        {
            if (this.separators == null)
                return new string [0];
            return (string[])this.separators.Clone();
        }

        protected static T Parse<T>(string value, MonoOptionContext c)
        {
            Type tt = typeof(T);
#if PCL
			TypeInfo ti = tt.GetTypeInfo ();
#else
            Type ti = tt;
#endif
            bool nullable =
                ti.IsValueType &&
                ti.IsGenericType &&
                !ti.IsGenericTypeDefinition &&
                ti.GetGenericTypeDefinition() == typeof(Nullable<>);
#if PCL
			Type targetType = nullable ? tt.GenericTypeArguments [0] : tt;
#else
            Type targetType = nullable ? tt.GetGenericArguments()[0] : tt;
#endif
            T t = default(T);
            try
            {
                if (value != null)
                {
#if PCL
					if (targetType.GetTypeInfo ().IsEnum)
						t = (T) Enum.Parse (targetType, value, true);
					else
						t = (T) Convert.ChangeType (value, targetType);
#else
                    TypeConverter conv = TypeDescriptor.GetConverter(targetType);
                    t = (T)conv.ConvertFromString(value);
#endif
                }
            }
            catch (Exception e)
            {
                throw new MonoOptionException(
                    string.Format(
                        c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
                        value, targetType.Name, c.OptionName),
                    c.OptionName, e);
            }

            return t;
        }

        internal string[] Names
        {
            get { return this.names; }
        }

        internal string[] ValueSeparators
        {
            get { return this.separators; }
        }

        static readonly char[] NameTerminator = new char[] { '=', ':' };

        private OptionValueType ParsePrototype()
        {
            char type = '\0';
            List<string> seps = new();
            for (int i = 0; i < this.names.Length; ++i)
            {
                string name = this.names[i];
                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", "prototype");

                int end = name.IndexOfAny(NameTerminator);
                if (end == -1)
                    continue;
                this.names[i] = name.Substring(0, end);
                if (type == '\0' || type == name[end])
                    type = name[end];
                else
                    throw new ArgumentException(
                        string.Format("Conflicting option types: '{0}' vs. '{1}'.", type, name[end]),
                        "prototype");
                AddSeparators(name, end, seps);
            }

            if (type == '\0')
                return OptionValueType.None;

            if (this.count <= 1 && seps.Count != 0)
                throw new ArgumentException(
                    string.Format("Cannot provide key/value separators for Options taking {0} value(s).", this.count),
                    "prototype");
            if (this.count > 1)
            {
                if (seps.Count == 0)
                    this.separators = new string[] { ":", "=" };
                else if (seps.Count == 1 && seps[0].Length == 0)
                    this.separators = null;
                else
                    this.separators = seps.ToArray();
            }

            return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int start = -1;
            for (int i = end + 1; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case '{':
                        if (start != -1)
                            throw new ArgumentException(
                                string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                                "prototype");
                        start = i + 1;
                        break;
                    case '}':
                        if (start == -1)
                            throw new ArgumentException(
                                string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                                "prototype");
                        seps.Add(name.Substring(start, i - start));
                        start = -1;
                        break;
                    default:
                        if (start == -1)
                            seps.Add(name[i].ToString());
                        break;
                }
            }

            if (start != -1)
                throw new ArgumentException(
                    string.Format("Ill-formed name/value separator found in \"{0}\".", name),
                    "prototype");
        }

        public void Invoke(MonoOptionContext c)
        {
            this.OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(MonoOptionContext c);

        internal void InvokeOnParseComplete(MonoOptionContext c)
        {
            this.OnParseComplete(c);
        }

        public override string ToString()
        {
            return this.Prototype;
        }
    }

    public abstract class ArgumentSource
    {
        protected ArgumentSource()
        {
        }

        public abstract string[] GetNames();
        public abstract string Description { get; }
        public abstract bool GetArguments(string value, out IEnumerable<string> replacement);

#if !PCL || NETSTANDARD1_3
        public static IEnumerable<string> GetArgumentsFromFile(string file)
        {
            return GetArguments(File.OpenText(file), true);
        }
#endif

        public static IEnumerable<string> GetArguments(TextReader reader)
        {
            return GetArguments(reader, false);
        }

        // Cribbed from mcs/driver.cs:LoadArgs(string)
        static IEnumerable<string> GetArguments(TextReader reader, bool close)
        {
            try
            {
                StringBuilder arg = new();

                string line;
                while (( line = reader.ReadLine() ) != null)
                {
                    int t = line.Length;

                    for (int i = 0; i < t; i++)
                    {
                        char c = line[i];

                        if (c == '"' || c == '\'')
                        {
                            char end = c;

                            for (i++; i < t; i++)
                            {
                                c = line[i];

                                if (c == end)
                                    break;
                                arg.Append(c);
                            }
                        }
                        else if (c == ' ')
                        {
                            if (arg.Length > 0)
                            {
                                yield return arg.ToString();
                                arg.Length = 0;
                            }
                        }
                        else
                            arg.Append(c);
                    }

                    if (arg.Length > 0)
                    {
                        yield return arg.ToString();
                        arg.Length = 0;
                    }
                }
            }
            finally
            {
                if (close)
                    reader.Dispose();
            }
        }
    }

#if !PCL || NETSTANDARD1_3
    public class ResponseFileSource : ArgumentSource
    {
        public override string[] GetNames()
        {
            return new string[] { "@file" };
        }

        public override string Description
        {
            get { return "Read response file for more options."; }
        }

        public override bool GetArguments(string value, out IEnumerable<string> replacement)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("@"))
            {
                replacement = null;
                return false;
            }

            replacement = ArgumentSource.GetArgumentsFromFile(value.Substring(1));
            return true;
        }
    }
#endif

#if !PCL
    [Serializable]
#endif
    public class MonoOptionException : Exception
    {
        private string option;

        public MonoOptionException()
        {
        }

        public MonoOptionException(string message, string optionName)
            : base(message)
        {
            this.option = optionName;
        }

        public MonoOptionException(string message, string optionName, Exception innerException)
            : base(message, innerException)
        {
            this.option = optionName;
        }

#if !PCL
        protected MonoOptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.option = info.GetString("OptionName");
        }
#endif

        public string OptionName
        {
            get { return this.option; }
        }

#if !PCL
#pragma warning disable 618 // SecurityPermissionAttribute is obsolete
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
#pragma warning restore 618
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OptionName", this.option);
        }
#endif
    }

    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);

    public class MonoOptionSet : KeyedCollection<string, MonoOption>
    {
        public MonoOptionSet()
            : this(null)
        {
        }

        public MonoOptionSet(MessageLocalizerConverter localizer)
        {
            this.roSources = new ReadOnlyCollection<ArgumentSource>(this.sources);
            this.localizer = localizer;
            if (this.localizer == null)
            {
                this.localizer = delegate(string f) { return f; };
            }
        }

        MessageLocalizerConverter localizer;

        public MessageLocalizerConverter MessageLocalizer
        {
            get { return this.localizer; }
            internal set { this.localizer = value; }
        }

        List<ArgumentSource> sources = new();
        ReadOnlyCollection<ArgumentSource> roSources;

        public ReadOnlyCollection<ArgumentSource> ArgumentSources
        {
            get { return this.roSources; }
        }


        protected override string GetKeyForItem(MonoOption item)
        {
            if (item == null)
                throw new ArgumentNullException("option");
            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];
            // This should never happen, as it's invalid for Option to be
            // constructed w/o any names.
            throw new InvalidOperationException("Option has no names!");
        }

        [Obsolete("Use KeyedCollection.this[string]")]
        protected MonoOption GetOptionForName(string option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            try
            {
                return base[option];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        protected override void InsertItem(int index, MonoOption item)
        {
            base.InsertItem(index, item);
            this.AddImpl(item);
        }

        protected override void RemoveItem(int index)
        {
            MonoOption p = this.Items[index];
            base.RemoveItem(index);
            // KeyedCollection.RemoveItem() handles the 0th item
            for (int i = 1; i < p.Names.Length; ++i)
            {
                this.Dictionary.Remove(p.Names[i]);
            }
        }

        protected override void SetItem(int index, MonoOption item)
        {
            base.SetItem(index, item);
            this.AddImpl(item);
        }

        private void AddImpl(MonoOption option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            List<string> added = new(option.Names.Length);
            try
            {
                // KeyedCollection.InsertItem/SetItem handle the 0th name.
                for (int i = 1; i < option.Names.Length; ++i)
                {
                    this.Dictionary.Add(option.Names[i], option);
                    added.Add(option.Names[i]);
                }
            }
            catch (Exception)
            {
                foreach (string name in added)
                    this.Dictionary.Remove(name);
                throw;
            }
        }

        public MonoOptionSet Add(string header)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            this.Add(new Category(header));
            return this;
        }

        internal sealed class Category : MonoOption
        {
            // Prototype starts with '=' because this is an invalid prototype
            // (see Option.ParsePrototype(), and thus it'll prevent Category
            // instances from being accidentally used as normal options.
            public Category(string description)
                : base("=:Category:= " + description, description)
            {
            }

            protected override void OnParseComplete(MonoOptionContext c)
            {
                throw new NotSupportedException("Category.OnParseComplete should not be invoked.");
            }
        }


        public new MonoOptionSet Add(MonoOption option)
        {
            base.Add(option);
            return this;
        }

        sealed class ActionOption : MonoOption
        {
            Action<MonoOptionValueCollection> action;

            public ActionOption(string prototype, string description, int count, Action<MonoOptionValueCollection> action)
                : this(prototype, description, count, action, false)
            {
            }

            public ActionOption(string prototype, string description, int count, Action<MonoOptionValueCollection> action, bool hidden)
                : base(prototype, description, count, hidden)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                this.action = action;
            }

            protected override void OnParseComplete(MonoOptionContext c)
            {
                this.action(c.OptionValues);
            }
        }

        public MonoOptionSet Add(string prototype, Action<string> action)
        {
            return this.Add(prototype, null, action);
        }

        public MonoOptionSet Add(string prototype, string description, Action<string> action)
        {
            return this.Add(prototype, description, action, false);
        }

        public MonoOptionSet Add(string prototype, string description, Action<string> action, bool hidden)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            MonoOption p = new ActionOption(prototype, description, 1,
                delegate(MonoOptionValueCollection v) { action(v[0]); }, hidden);
            base.Add(p);
            return this;
        }

        public MonoOptionSet Add(string prototype, OptionAction<string, string> action)
        {
            return this.Add(prototype, null, action);
        }

        public MonoOptionSet Add(string prototype, string description, OptionAction<string, string> action)
        {
            return this.Add(prototype, description, action, false);
        }

        public MonoOptionSet Add(string prototype, string description, OptionAction<string, string> action, bool hidden)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            MonoOption p = new ActionOption(prototype, description, 2,
                delegate(MonoOptionValueCollection v) { action(v[0], v[1]); }, hidden);
            base.Add(p);
            return this;
        }

        sealed class ActionOption<T> : MonoOption
        {
            Action<T> action;

            public ActionOption(string prototype, string description, Action<T> action)
                : base(prototype, description, 1)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                this.action = action;
            }

            protected override void OnParseComplete(MonoOptionContext c)
            {
                this.action(Parse<T>(c.OptionValues[0], c));
            }
        }

        sealed class ActionOption<TKey, TValue> : MonoOption
        {
            OptionAction<TKey, TValue> action;

            public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
                : base(prototype, description, 2)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                this.action = action;
            }

            protected override void OnParseComplete(MonoOptionContext c)
            {
                this.action(
                    Parse<TKey>(c.OptionValues[0], c),
                    Parse<TValue>(c.OptionValues[1], c));
            }
        }

        public MonoOptionSet Add<T>(string prototype, Action<T> action)
        {
            return this.Add(prototype, null, action);
        }

        public MonoOptionSet Add<T>(string prototype, string description, Action<T> action)
        {
            return this.Add(new ActionOption<T>(prototype, description, action));
        }

        public MonoOptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
        {
            return this.Add(prototype, null, action);
        }

        public MonoOptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
        {
            return this.Add(new ActionOption<TKey, TValue>(prototype, description, action));
        }

        public MonoOptionSet Add(ArgumentSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            this.sources.Add(source);
            return this;
        }

        protected virtual MonoOptionContext CreateOptionContext()
        {
            return new MonoOptionContext(this);
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            MonoOptionContext c = this.CreateOptionContext();
            c.OptionIndex = -1;
            bool process = true;
            List<string> unprocessed = new();
            MonoOption def = this.Contains("<>") ? this["<>"] : null;
            ArgumentEnumerator ae = new(arguments);
            foreach (string argument in ae)
            {
                ++c.OptionIndex;
                if (argument == "--")
                {
                    process = false;
                    continue;
                }

                if (!process)
                {
                    Unprocessed(unprocessed, def, c, argument);
                    continue;
                }

                if (this.AddSource(ae, argument))
                    continue;
                if (!this.Parse(argument, c))
                    Unprocessed(unprocessed, def, c, argument);
            }

            if (c.Option != null)
                c.Option.Invoke(c);
            return unprocessed;
        }

        class ArgumentEnumerator : IEnumerable<string>
        {
            List<IEnumerator<string>> sources = new();

            public ArgumentEnumerator(IEnumerable<string> arguments)
            {
                this.sources.Add(arguments.GetEnumerator());
            }

            public void Add(IEnumerable<string> arguments)
            {
                this.sources.Add(arguments.GetEnumerator());
            }

            public IEnumerator<string> GetEnumerator()
            {
                do
                {
                    IEnumerator<string> c = this.sources[this.sources.Count - 1];
                    if (c.MoveNext())
                        yield return c.Current;
                    else
                    {
                        c.Dispose();
                        this.sources.RemoveAt(this.sources.Count - 1);
                    }
                } while (this.sources.Count > 0);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        bool AddSource(ArgumentEnumerator ae, string argument)
        {
            foreach (ArgumentSource source in this.sources)
            {
                IEnumerable<string> replacement;
                if (!source.GetArguments(argument, out replacement))
                    continue;
                ae.Add(replacement);
                return true;
            }

            return false;
        }

        private static bool Unprocessed(ICollection<string> extra, MonoOption def, MonoOptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
                return false;
            }

            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);
            return false;
        }

        private readonly Regex ValueOption = new(
            @"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
        {
            if (argument == null)
                throw new ArgumentNullException("argument");

            flag = name = sep = value = null;
            Match m = this.ValueOption.Match(argument);
            if (!m.Success)
            {
                return false;
            }

            flag = m.Groups["flag"].Value;
            name = m.Groups["name"].Value;
            if (m.Groups["sep"].Success && m.Groups["value"].Success)
            {
                sep = m.Groups["sep"].Value;
                value = m.Groups["value"].Value;
            }

            return true;
        }

        protected virtual bool Parse(string argument, MonoOptionContext c)
        {
            if (c.Option != null)
            {
                this.ParseValue(argument, c);
                return true;
            }

            string f, n, s, v;
            if (!this.GetOptionParts(argument, out f, out n, out s, out v))
                return false;

            MonoOption p;
            if (this.Contains(n))
            {
                p = this[n];
                c.OptionName = f + n;
                c.Option = p;
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        c.OptionValues.Add(n);
                        c.Option.Invoke(c);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        this.ParseValue(v, c);
                        break;
                }

                return true;
            }

            // no match; is it a bool option?
            if (this.ParseBool(argument, n, c))
                return true;
            // is it a bundled option?
            if (this.ParseBundledValue(f, string.Concat(n + s + v), c))
                return true;

            return false;
        }

        private void ParseValue(string option, MonoOptionContext c)
        {
            if (option != null)
                foreach (string o in c.Option.ValueSeparators != null
                             ? option.Split(c.Option.ValueSeparators, c.Option.MaxValueCount - c.OptionValues.Count, StringSplitOptions.None)
                             : new string[] { option })
                {
                    c.OptionValues.Add(o);
                }

            if (c.OptionValues.Count == c.Option.MaxValueCount ||
                c.Option.OptionValueType == OptionValueType.Optional)
                c.Option.Invoke(c);
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
            {
                throw new MonoOptionException(this.localizer(string.Format(
                        "Error: Found {0} option values when expecting {1}.",
                        c.OptionValues.Count, c.Option.MaxValueCount)),
                    c.OptionName);
            }
        }

        private bool ParseBool(string option, string n, MonoOptionContext c)
        {
            MonoOption p;
            string rn;
            if (n.Length >= 1 && ( n[n.Length - 1] == '+' || n[n.Length - 1] == '-' ) &&
                this.Contains(( rn = n.Substring(0, n.Length - 1) )))
            {
                p = this[rn];
                string v = n[n.Length - 1] == '+' ? option : null;
                c.OptionName = option;
                c.Option = p;
                c.OptionValues.Add(v);
                p.Invoke(c);
                return true;
            }

            return false;
        }

        private bool ParseBundledValue(string f, string n, MonoOptionContext c)
        {
            if (f != "-")
                return false;
            for (int i = 0; i < n.Length; ++i)
            {
                MonoOption p;
                string opt = f + n[i].ToString();
                string rn = n[i].ToString();
                if (!this.Contains(rn))
                {
                    if (i == 0)
                        return false;
                    throw new MonoOptionException(string.Format(this.localizer(
                        "Cannot use unregistered option '{0}' in bundle '{1}'."), rn, f + n), null);
                }

                p = this[rn];
                switch (p.OptionValueType)
                {
                    case OptionValueType.None:
                        Invoke(c, opt, n, p);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                    {
                        string v = n.Substring(i + 1);
                        c.Option = p;
                        c.OptionName = opt;
                        this.ParseValue(v.Length != 0 ? v : null, c);
                        return true;
                    }
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
                }
            }

            return true;
        }

        private static void Invoke(MonoOptionContext c, string name, string value, MonoOption option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        private const int OptionWidth = 29;
        private const int Description_FirstWidth = 80 - OptionWidth;
        private const int Description_RemWidth = 80 - OptionWidth - 2;

        static readonly string CommandHelpIndentStart = new(' ', OptionWidth);
        static readonly string CommandHelpIndentRemaining = new(' ', OptionWidth + 2);

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (MonoOption p in this)
            {
                int written = 0;

                if (p.Hidden)
                    continue;

                Category c = p as Category;
                if (c != null)
                {
                    this.WriteDescription(o, p.Description, "", 80, 80);
                    continue;
                }

                CommandOption co = p as CommandOption;
                if (co != null)
                {
                    this.WriteCommandDescription(o, co.Command, co.CommandName);
                    continue;
                }

                if (!this.WriteOptionPrototype(o, p, ref written))
                    continue;

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                this.WriteDescription(o, p.Description, new string(' ', OptionWidth + 2),
                    Description_FirstWidth, Description_RemWidth);
            }

            foreach (ArgumentSource s in this.sources)
            {
                string[] names = s.GetNames();
                if (names == null || names.Length == 0)
                    continue;

                int written = 0;

                Write(o, ref written, "  ");
                Write(o, ref written, names[0]);
                for (int i = 1; i < names.Length; ++i)
                {
                    Write(o, ref written, ", ");
                    Write(o, ref written, names[i]);
                }

                if (written < OptionWidth)
                    o.Write(new string(' ', OptionWidth - written));
                else
                {
                    o.WriteLine();
                    o.Write(new string(' ', OptionWidth));
                }

                this.WriteDescription(o, s.Description, new string(' ', OptionWidth + 2),
                    Description_FirstWidth, Description_RemWidth);
            }
        }

        internal void WriteCommandDescription(TextWriter o, MonoCommand c, string commandName)
        {
            string name = new string(' ', 8) + ( commandName ?? c.Name );
            if (name.Length < OptionWidth - 1)
            {
                this.WriteDescription(o, name + new string(' ', OptionWidth - name.Length) + c.Help, CommandHelpIndentRemaining, 80, Description_RemWidth);
            }
            else
            {
                this.WriteDescription(o, name, "", 80, 80);
                this.WriteDescription(o, CommandHelpIndentStart + c.Help, CommandHelpIndentRemaining, 80, Description_RemWidth);
            }
        }

        void WriteDescription(TextWriter o, string value, string prefix, int firstWidth, int remWidth)
        {
            bool indent = false;
            foreach (string line in GetLines(this.localizer(GetDescription(value)), firstWidth, remWidth))
            {
                if (indent)
                    o.Write(prefix);
                o.WriteLine(line);
                indent = true;
            }
        }

        bool WriteOptionPrototype(TextWriter o, MonoOption p, ref int written)
        {
            string[] names = p.Names;

            int i = GetNextOptionIndex(names, 0);
            if (i == names.Length)
                return false;

            if (names[i].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }

            for (i = GetNextOptionIndex(names, i + 1);
                 i < names.Length;
                 i = GetNextOptionIndex(names, i + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, names[i].Length == 1 ? "-" : "--");
                Write(o, ref written, names[i]);
            }

            if (p.OptionValueType == OptionValueType.Optional ||
                p.OptionValueType == OptionValueType.Required)
            {
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, this.localizer("["));
                }

                Write(o, ref written, this.localizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
                string sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
                    ? p.ValueSeparators[0]
                    : " ";
                for (int c = 1; c < p.MaxValueCount; ++c)
                {
                    Write(o, ref written, this.localizer(sep + GetArgumentName(c, p.MaxValueCount, p.Description)));
                }

                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, this.localizer("]"));
                }
            }

            return true;
        }

        static int GetNextOptionIndex(string[] names, int i)
        {
            while (i < names.Length && names[i] == "<>")
            {
                ++i;
            }

            return i;
        }

        static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        static string GetArgumentName(int index, int maxIndex, string description)
        {
            MatchCollection matches = Regex.Matches(description ?? "", @"(?<=(?<!\{)\{)[^{}]*(?=\}(?!\}))"); // ignore double braces 
            string argName = "";
            foreach (Match match in matches)
            {
                string[] parts = match.Value.Split(':');
                // for maxIndex=1 it can be {foo} or {0:foo}
                if (maxIndex == 1)
                {
                    argName = parts[parts.Length - 1];
                }

                // look for {i:foo} if maxIndex > 1
                if (maxIndex > 1 && parts.Length == 2 &&
                    parts[0] == index.ToString(CultureInfo.InvariantCulture))
                {
                    argName = parts[1];
                }
            }

            if (string.IsNullOrEmpty(argName))
            {
                argName = maxIndex == 1 ? "VALUE" : "VALUE" + ( index + 1 );
            }

            return argName;
        }

        private static string GetDescription(string description)
        {
            if (description == null)
                return string.Empty;
            StringBuilder sb = new(description.Length);
            int start = -1;
            for (int i = 0; i < description.Length; ++i)
            {
                switch (description[i])
                {
                    case '{':
                        if (i == start)
                        {
                            sb.Append('{');
                            start = -1;
                        }
                        else if (start < 0)
                            start = i + 1;

                        break;
                    case '}':
                        if (start < 0)
                        {
                            if (( i + 1 ) == description.Length || description[i + 1] != '}')
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++i;
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(description.Substring(start, i - start));
                            start = -1;
                        }

                        break;
                    case ':':
                        if (start < 0)
                            goto default;
                        start = i + 1;
                        break;
                    default:
                        if (start < 0)
                            sb.Append(description[i]);
                        break;
                }
            }

            return sb.ToString();
        }

        private static IEnumerable<string> GetLines(string description, int firstWidth, int remWidth)
        {
            return StringCoda.WrappedLines(description, firstWidth, remWidth);
        }
    }

    public class MonoCommand
    {
        public string Name { get; }
        public string Help { get; }

        public MonoOptionSet Options { get; set; }
        public Action<IEnumerable<string>> Run { get; set; }

        public MonoCommandSet CommandSet { get; internal set; }

        public MonoCommand(string name, string help = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            this.Name = NormalizeCommandName(name);
            this.Help = help;
        }

        static string NormalizeCommandName(string name)
        {
            StringBuilder value = new(name.Length);
            bool space = false;
            for (int i = 0; i < name.Length; ++i)
            {
                if (!char.IsWhiteSpace(name, i))
                {
                    space = false;
                    value.Append(name[i]);
                }
                else if (!space)
                {
                    space = true;
                    value.Append(' ');
                }
            }

            return value.ToString();
        }

        public virtual int Invoke(IEnumerable<string> arguments)
        {
            IEnumerable<string> rest = this.Options?.Parse(arguments) ?? arguments;
            this.Run?.Invoke(rest);
            return 0;
        }
    }

    class CommandOption : MonoOption
    {
        public MonoCommand Command { get; }
        public string CommandName { get; }

        // Prototype starts with '=' because this is an invalid prototype
        // (see Option.ParsePrototype(), and thus it'll prevent Category
        // instances from being accidentally used as normal options.
        public CommandOption(MonoCommand command, string commandName = null, bool hidden = false)
            : base("=:Command:= " + ( commandName ?? command?.Name ), ( commandName ?? command?.Name ), maxValueCount: 0, hidden: hidden)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            this.Command = command;
            this.CommandName = commandName ?? command.Name;
        }

        protected override void OnParseComplete(MonoOptionContext c)
        {
            throw new NotSupportedException("CommandOption.OnParseComplete should not be invoked.");
        }
    }

    class HelpOption : MonoOption
    {
        MonoOption option;
        MonoCommandSet commands;

        public HelpOption(MonoCommandSet commands, MonoOption d)
            : base(d.Prototype, d.Description, d.MaxValueCount, d.Hidden)
        {
            this.commands = commands;
            this.option = d;
        }

        protected override void OnParseComplete(MonoOptionContext c)
        {
            this.commands.showHelp = true;

            this.option?.InvokeOnParseComplete(c);
        }
    }

    class CommandOptionSet : MonoOptionSet
    {
        MonoCommandSet commands;

        public CommandOptionSet(MonoCommandSet commands, MessageLocalizerConverter localizer)
            : base(localizer)
        {
            this.commands = commands;
        }

        protected override void SetItem(int index, MonoOption item)
        {
            if (this.ShouldWrapOption(item))
            {
                base.SetItem(index, new HelpOption(this.commands, item));
                return;
            }

            base.SetItem(index, item);
        }

        bool ShouldWrapOption(MonoOption item)
        {
            if (item == null)
                return false;
            HelpOption help = item as HelpOption;
            if (help != null)
                return false;
            foreach (string n in item.Names)
            {
                if (n == "help")
                    return true;
            }

            return false;
        }

        protected override void InsertItem(int index, MonoOption item)
        {
            if (this.ShouldWrapOption(item))
            {
                base.InsertItem(index, new HelpOption(this.commands, item));
                return;
            }

            base.InsertItem(index, item);
        }
    }

    public class MonoCommandSet : KeyedCollection<string, MonoCommand>
    {
        readonly string suite;

        MonoOptionSet options;
        TextWriter outWriter;
        TextWriter errorWriter;

        internal List<MonoCommandSet> NestedCommandSets;

        internal HelpCommand help;

        internal bool showHelp;

        internal MonoOptionSet Options => this.options;

#if !PCL || NETSTANDARD1_3
        public MonoCommandSet(string suite, MessageLocalizerConverter localizer = null)
            : this(suite, Console.Out, Console.Error, localizer)
        {
        }
#endif

        public MonoCommandSet(string suite, TextWriter output, TextWriter error, MessageLocalizerConverter localizer = null)
        {
            if (suite == null)
                throw new ArgumentNullException(nameof(suite));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            this.suite = suite;
            this.options = new CommandOptionSet(this, localizer);
            this.outWriter = output;
            this.errorWriter = error;
        }

        public string Suite => this.suite;
        public TextWriter Out => this.outWriter;
        public TextWriter Error => this.errorWriter;
        public MessageLocalizerConverter MessageLocalizer => this.options.MessageLocalizer;

        protected override string GetKeyForItem(MonoCommand item)
        {
            return item?.Name;
        }

        public new MonoCommandSet Add(MonoCommand value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.AddCommand(value);
            this.options.Add(new CommandOption(value));
            return this;
        }

        void AddCommand(MonoCommand value)
        {
            if (value.CommandSet != null && value.CommandSet != this)
            {
                throw new ArgumentException("Command instances can only be added to a single CommandSet.", nameof(value));
            }

            value.CommandSet = this;
            if (value.Options != null)
            {
                value.Options.MessageLocalizer = this.options.MessageLocalizer;
            }

            base.Add(value);

            this.help = this.help ?? value as HelpCommand;
        }

        public MonoCommandSet Add(string header)
        {
            this.options.Add(header);
            return this;
        }

        public MonoCommandSet Add(MonoOption option)
        {
            this.options.Add(option);
            return this;
        }

        public MonoCommandSet Add(string prototype, Action<string> action)
        {
            this.options.Add(prototype, action);
            return this;
        }

        public MonoCommandSet Add(string prototype, string description, Action<string> action)
        {
            this.options.Add(prototype, description, action);
            return this;
        }

        public MonoCommandSet Add(string prototype, string description, Action<string> action, bool hidden)
        {
            this.options.Add(prototype, description, action, hidden);
            return this;
        }

        public MonoCommandSet Add(string prototype, OptionAction<string, string> action)
        {
            this.options.Add(prototype, action);
            return this;
        }

        public MonoCommandSet Add(string prototype, string description, OptionAction<string, string> action)
        {
            this.options.Add(prototype, description, action);
            return this;
        }

        public MonoCommandSet Add(string prototype, string description, OptionAction<string, string> action, bool hidden)
        {
            this.options.Add(prototype, description, action, hidden);
            return this;
        }

        public MonoCommandSet Add<T>(string prototype, Action<T> action)
        {
            this.options.Add(prototype, null, action);
            return this;
        }

        public MonoCommandSet Add<T>(string prototype, string description, Action<T> action)
        {
            this.options.Add(prototype, description, action);
            return this;
        }

        public MonoCommandSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
        {
            this.options.Add(prototype, action);
            return this;
        }

        public MonoCommandSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
        {
            this.options.Add(prototype, description, action);
            return this;
        }

        public MonoCommandSet Add(ArgumentSource source)
        {
            this.options.Add(source);
            return this;
        }

        public MonoCommandSet Add(MonoCommandSet nestedCommands)
        {
            if (nestedCommands == null)
                throw new ArgumentNullException(nameof(nestedCommands));

            if (this.NestedCommandSets == null)
            {
                this.NestedCommandSets = new List<MonoCommandSet>();
            }

            if (!this.AlreadyAdded(nestedCommands))
            {
                this.NestedCommandSets.Add(nestedCommands);
                foreach (MonoOption o in nestedCommands.options)
                {
                    if (o is CommandOption c)
                    {
                        this.options.Add(new CommandOption(c.Command, $"{nestedCommands.Suite} {c.CommandName}"));
                    }
                    else
                    {
                        this.options.Add(o);
                    }
                }
            }

            nestedCommands.options = this.options;
            nestedCommands.outWriter = this.outWriter;
            nestedCommands.errorWriter = this.errorWriter;

            return this;
        }

        bool AlreadyAdded(MonoCommandSet value)
        {
            if (value == this)
                return true;
            if (this.NestedCommandSets == null)
                return false;
            foreach (MonoCommandSet nc in this.NestedCommandSets)
            {
                if (nc.AlreadyAdded(value))
                    return true;
            }

            return false;
        }

        public IEnumerable<string> GetCompletions(string prefix = null)
        {
            string rest;
            ExtractToken(ref prefix, out rest);

            foreach (MonoCommand command in this)
            {
                if (command.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    yield return command.Name;
                }
            }

            if (this.NestedCommandSets == null)
                yield break;

            foreach (MonoCommandSet subset in this.NestedCommandSets)
            {
                if (subset.Suite.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string c in subset.GetCompletions(rest))
                    {
                        yield return $"{subset.Suite} {c}";
                    }
                }
            }
        }

        static void ExtractToken(ref string input, out string rest)
        {
            rest = "";
            input = input ?? "";

            int top = input.Length;
            for (int i = 0; i < top; i++)
            {
                if (char.IsWhiteSpace(input[i]))
                    continue;

                for (int j = i; j < top; j++)
                {
                    if (char.IsWhiteSpace(input[j]))
                    {
                        rest = input.Substring(j).Trim();
                        input = input.Substring(i, j).Trim();
                        return;
                    }
                }

                rest = "";
                if (i != 0)
                    input = input.Substring(i).Trim();
                return;
            }
        }

        public int Run(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            this.showHelp = false;
            if (this.help == null)
            {
                this.help = new HelpCommand();
                this.AddCommand(this.help);
            }

            Action<string> setHelp = v => this.showHelp = v != null;
            if (!this.options.Contains("help"))
            {
                this.options.Add("help", "", setHelp, hidden: true);
            }

            if (!this.options.Contains("?"))
            {
                this.options.Add("?", "", setHelp, hidden: true);
            }

            List<string> extra = this.options.Parse(arguments);
            if (extra.Count == 0)
            {
                if (this.showHelp)
                {
                    return this.help.Invoke(extra);
                }

                this.Out.WriteLine(this.options.MessageLocalizer($"Use `{this.Suite} help` for usage."));
                return 1;
            }

            MonoCommand command = this.GetCommand(extra);
            if (command == null)
            {
                this.help.WriteUnknownCommand(extra[0]);
                return 1;
            }

            if (this.showHelp)
            {
                if (command.Options?.Contains("help") ?? true)
                {
                    extra.Add("--help");
                    return command.Invoke(extra);
                }

                command.Options.WriteOptionDescriptions(this.Out);
                return 0;
            }

            return command.Invoke(extra);
        }

        internal MonoCommand GetCommand(List<string> extra)
        {
            return this.TryGetLocalCommand(extra) ?? this.TryGetNestedCommand(extra);
        }

        MonoCommand TryGetLocalCommand(List<string> extra)
        {
            string name = extra[0];
            if (this.Contains(name))
            {
                extra.RemoveAt(0);
                return this[name];
            }

            for (int i = 1; i < extra.Count; ++i)
            {
                name = name + " " + extra[i];
                if (!this.Contains(name))
                    continue;
                extra.RemoveRange(0, i + 1);
                return this[name];
            }

            return null;
        }

        MonoCommand TryGetNestedCommand(List<string> extra)
        {
            if (this.NestedCommandSets == null)
                return null;

            MonoCommandSet nestedCommands = this.NestedCommandSets.Find(c => c.Suite == extra[0]);
            if (nestedCommands == null)
                return null;

            List<string> extraCopy = new(extra);
            extraCopy.RemoveAt(0);
            if (extraCopy.Count == 0)
                return null;

            MonoCommand command = nestedCommands.GetCommand(extraCopy);
            if (command != null)
            {
                extra.Clear();
                extra.AddRange(extraCopy);
                return command;
            }

            return null;
        }
    }

    public class HelpCommand : MonoCommand
    {
        public HelpCommand()
            : base("help", help: "Show this message and exit")
        {
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = new(arguments ?? new string [0]);
            MessageLocalizerConverter _ = this.CommandSet.Options.MessageLocalizer;
            if (extra.Count == 0)
            {
                this.CommandSet.Options.WriteOptionDescriptions(this.CommandSet.Out);
                return 0;
            }

            MonoCommand command = this.CommandSet.GetCommand(extra);
            if (command == this || extra.Contains("--help"))
            {
                this.CommandSet.Out.WriteLine(_($"Usage: {this.CommandSet.Suite} COMMAND [OPTIONS]"));
                this.CommandSet.Out.WriteLine(_($"Use `{this.CommandSet.Suite} help COMMAND` for help on a specific command."));
                this.CommandSet.Out.WriteLine();
                this.CommandSet.Out.WriteLine(_($"Available commands:"));
                this.CommandSet.Out.WriteLine();
                List<KeyValuePair<string, MonoCommand>> commands = this.GetCommands();
                commands.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));
                foreach (KeyValuePair<string, MonoCommand> c in commands)
                {
                    if (c.Key == "help")
                    {
                        continue;
                    }

                    this.CommandSet.Options.WriteCommandDescription(this.CommandSet.Out, c.Value, c.Key);
                }

                this.CommandSet.Options.WriteCommandDescription(this.CommandSet.Out, this.CommandSet.help, "help");
                return 0;
            }

            if (command == null)
            {
                this.WriteUnknownCommand(extra[0]);
                return 1;
            }

            if (command.Options != null)
            {
                command.Options.WriteOptionDescriptions(this.CommandSet.Out);
                return 0;
            }

            return command.Invoke(new[] { "--help" });
        }

        List<KeyValuePair<string, MonoCommand>> GetCommands()
        {
            List<KeyValuePair<string, MonoCommand>> commands = new();

            foreach (MonoCommand c in this.CommandSet)
            {
                commands.Add(new KeyValuePair<string, MonoCommand>(c.Name, c));
            }

            if (this.CommandSet.NestedCommandSets == null)
                return commands;

            foreach (MonoCommandSet nc in this.CommandSet.NestedCommandSets)
            {
                this.AddNestedCommands(commands, "", nc);
            }

            return commands;
        }

        void AddNestedCommands(List<KeyValuePair<string, MonoCommand>> commands, string outer, MonoCommandSet value)
        {
            foreach (MonoCommand v in value)
            {
                commands.Add(new KeyValuePair<string, MonoCommand>($"{outer}{value.Suite} {v.Name}", v));
            }

            if (value.NestedCommandSets == null)
                return;
            foreach (MonoCommandSet nc in value.NestedCommandSets)
            {
                this.AddNestedCommands(commands, $"{outer}{value.Suite} ", nc);
            }
        }

        internal void WriteUnknownCommand(string unknownCommand)
        {
            this.CommandSet.Error.WriteLine(this.CommandSet.Options.MessageLocalizer($"{this.CommandSet.Suite}: Unknown command: {unknownCommand}"));
            this.CommandSet.Error.WriteLine(this.CommandSet.Options.MessageLocalizer($"{this.CommandSet.Suite}: Use `{this.CommandSet.Suite} help` for usage."));
        }
    }
}

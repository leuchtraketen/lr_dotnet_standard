//
// getline.cs: A command line editor
//
// Authors:
//   Miguel de Icaza (miguel@microsoft.com)
//
// Copyright 2008 Novell, Inc.
// Copyright 2016 Xamarin Inc
// Copyright 2017 Microsoft
//
// Completion wanted:
//
//   * Enable bash-like completion window the window as an option for non-GUI people?
//
//   * Continue completing when Backspace is used?
//
//   * Should we keep the auto-complete on "."?
//
//   * Completion produces an error if the value is not resolvable, we should hide those errors
//
// Dual-licensed under the terms of the MIT X11 license or the
// Apache License 2.0
//
// USE -define:DEMO to build this as a standalone file and test it
//
// TODO:
//    Enter an error (a = 1);  Notice how the prompt is in the wrong line
//		This is caused by Stderr not being tracked by System.Console.
//    Completion support
//    Why is Thread.Interrupt not working?   Currently I resort to Abort which is too much.
//
// Limitations in System.Console:
//    Console needs SIGWINCH support of some sort
//    Console needs a way of updating its position after things have been written
//    behind its back (P/Invoke puts for example).
//    System.Console needs to get the DELETE character, and report accordingly.
//
// Bug:
//   About 8 lines missing, type "Con<TAB>" and not enough lines are inserted at the bottom.
//
//

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Mono;

/// <summary>
/// Interactive line editor.
/// </summary>
/// <remarks>
///   <para>
///     LineEditor is an interative line editor for .NET applications that provides
///     editing capabilities for an input line with common editing capabilities and
///     navigation expected in modern application as well as history, incremental
///     search over the history, completion (both textual or visual) and various
///     Emacs-like commands.
///   </para>
///   <para>
///     When you create your line editor, you can pass the name of your application,
///     which will be used to load and save the history of commands entered by the user
///     for this particular application.
///   </para>
///   <para>
///
///   </para>
///   <example>
///     The following example shows how you can instantiate a line editor that
///     can provide code completion for some words when the user presses TAB
///     and how the user can edit them.
///     <code>
/// LineEditor le = new LineEditor ("myshell") { HeuristicsMode = "csharp" };
/// le.AutoCompleteEvent += delegate (string line, int point){
///     string prefix = "";
///     var completions = new string [] {
///         "One", "Two", "Three", "Four", "Five",
///          "Six", "Seven", "Eight", "Nine", "Ten"
///     };
///     return new Mono.Terminal.LineEditor.Completion(prefix, completions);
/// };
///
/// string s;
///
/// while ((s = le.Edit("shell> ", "")) != null)
///    Console.WriteLine("You typed: [{0}]", s);			}
///     </code>
///   </example>
///   <para>
///      Users can use the cursor keys to navigate both the text on the current
///      line, or move back and forward through the history of commands that have
///      been entered.
///   </para>
///   <para>
///     The interactive commands and keybindings are inspired by the GNU bash and
///     GNU readline capabilities and follow the same concepts found there.
///   </para>
///   <para>
///      Copy and pasting works like bash, deleted words or regions are added to
///      the kill buffer.   Repeated invocations of the same deleting operation will
///      append to the kill buffer (for example, repeatedly deleting words) and to
///      paste the results you would use the Control-y command (yank).
///   </para>
///   <para>
///      The history search capability is triggered when you press
///      Control-r to start a reverse interactive-search
///      and start typing the text you are looking for, the edited line will
///      be updated with matches.  Typing control-r again will go to the next
///      match in history and so on.
///   </para>
///   <list type="table">
///     <listheader>
///       <term>Shortcut</term>
///       <description>Action performed</description>
///     </listheader>
///     <item>
///        <term>Left cursor, Control-b</term>
///        <description>
///          Moves the editing point left.
///        </description>
///     </item>
///     <item>
///        <term>Right cursor, Control-f</term>
///        <description>
///          Moves the editing point right.
///        </description>
///     </item>
///     <item>
///        <term>Alt-b</term>
///        <description>
///          Moves one word back.
///        </description>
///     </item>
///     <item>
///        <term>Alt-f</term>
///        <description>
///          Moves one word forward.
///        </description>
///     </item>
///     <item>
///        <term>Up cursor, Control-p</term>
///        <description>
///          Selects the previous item in the editing history.
///        </description>
///     </item>
///     <item>
///        <term>Down cursor, Control-n</term>
///        <description>
///          Selects the next item in the editing history.
///        </description>
///     </item>
///     <item>
///        <term>Home key, Control-a</term>
///        <description>
///          Moves the cursor to the beginning of the line.
///        </description>
///     </item>
///     <item>
///        <term>End key, Control-e</term>
///        <description>
///          Moves the cursor to the end of the line.
///        </description>
///     </item>
///     <item>
///        <term>Delete, Control-d</term>
///        <description>
///          Deletes the character in front of the cursor.
///        </description>
///     </item>
///     <item>
///        <term>Backspace</term>
///        <description>
///          Deletes the character behind the cursor.
///        </description>
///     </item>
///     <item>
///        <term>Tab</term>
///        <description>
///           Triggers the completion and invokes the AutoCompleteEvent which gets
///           both the line contents and the position where the cursor is.
///        </description>
///     </item>
///     <item>
///        <term>Control-k</term>
///        <description>
///          Deletes the text until the end of the line and replaces the kill buffer
///          with the deleted text.   You can paste this text in a different place by
///          using Control-y.
///        </description>
///     </item>
///     <item>
///        <term>Control-l refresh</term>
///        <description>
///           Clears the screen and forces a refresh of the line editor, useful when
///           a background process writes to the console and garbles the contents of
///           the screen.
///        </description>
///     </item>
///     <item>
///        <term>Control-r</term>
///        <description>
///          Initiates the reverse search in history.
///        </description>
///     </item>
///     <item>
///        <term>Alt-backspace</term>
///        <description>
///           Deletes the word behind the cursor and adds it to the kill ring.  You
///           can paste the contents of the kill ring with Control-y.
///        </description>
///     </item>
///     <item>
///        <term>Alt-d</term>
///        <description>
///           Deletes the word above the cursor and adds it to the kill ring.  You
///           can paste the contents of the kill ring with Control-y.
///        </description>
///     </item>
///     <item>
///        <term>Control-y</term>
///        <description>
///           Pastes the content of the kill ring into the current position.
///        </description>
///     </item>
///     <item>
///        <term>Control-q</term>
///        <description>
///          Quotes the next input character, to prevent the normal processing of
///          key handling to take place.
///        </description>
///     </item>
///   </list>
/// </remarks>
public class LineEditor
{
    /// <summary>
    /// Completion results returned by the completion handler.
    /// </summary>
    /// <remarks>
    /// You create an instance of this class to return the completion
    /// results for the text at the specific position.   The prefix parameter
    /// indicates the common prefix in the results, and the results contain the
    /// results without the prefix.   For example, when completing "ToString" and "ToDate"
    /// prefix would be "To" and the completions would be "String" and "Date".
    /// </remarks>
    public class Completion
    {
        /// <summary>
        /// Array of results, with the stem removed.
        /// </summary>
        public string[] Result;

        /// <summary>
        /// Shared prefix for the completion results.
        /// </summary>
        public string Prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mono.Terminal.LineEditor.Completion"/> class.
        /// </summary>
        /// <param name="prefix">Common prefix for all results, an be null.</param>
        /// <param name="result">Array of possible completions.</param>
        public Completion(string prefix, string[] result)
        {
            this.Prefix = prefix;
            this.Result = result;
        }
    }

    /// <summary>
    /// Method signature for auto completion handlers.
    /// </summary>
    /// <remarks>
    /// The completion handler receives the text as it is being edited as
    /// well as the position of the cursor in that line.   The method
    /// must return an instance of Completion with the possible completions.
    /// </remarks>
    public delegate Completion AutoCompleteHandler(string text, int pos);

    /// <summary>
    /// The heuristics mode used by code completion.
    /// </summary>
    /// <remarks>
    ///    <para>
    ///      This controls the heuristics style used to show the code
    ///      completion popup as well as when to accept an entry.
    ///    </para>
    ///    <para>
    ///      The default value is null which requires the user to explicitly
    ///      use the TAB key to trigger a completion.
    ///    </para>
    ///    <para>
    ///      Another possible value is "csharp" which will trigger auto-completion when a
    ///      "." is entered.
    ///    </para>
    /// </remarks>
    public string HeuristicsMode;

    //static StreamWriter log;

    // The text being edited.
    StringBuilder text;

    // The text as it is rendered (replaces (char)1 with ^A on display for example).
    StringBuilder rendered_text;

    // The prompt specified, and the prompt shown to the user.
    string prompt;
    string shown_prompt;

    // The current cursor position, indexes into "text", for an index
    // into rendered_text, use TextToRenderPos
    int cursor;

    // The row where we started displaying data.
    int home_row;

    // The maximum length that has been displayed on the screen
    int max_rendered;

    // If we are done editing, this breaks the interactive loop
    bool done = false;

    // The thread where the Editing started taking place
    Thread edit_thread;

    // Our object that tracks history
    History history;

    // The contents of the kill buffer (cut/paste in Emacs parlance)
    string kill_buffer = "";

    // The string being searched for
    string search;
    string last_search;

    // whether we are searching (-1= reverse; 0 = no; 1 = forward)
    int searching;

    // The position where we found the match.
    int match_at;

    // Used to implement the Kill semantics (multiple Alt-Ds accumulate)
    KeyHandler last_handler;

    // If we have a popup completion, this is not null and holds the state.
    CompletionState current_completion;

    // If this is set, it contains an escape sequence to reset the Unix colors to the ones that were used on startup
    static byte[] unix_reset_colors;

    // This contains a raw stream pointing to stdout, used to bypass the TermInfoDriver
    static Stream unix_raw_output;

    delegate void KeyHandler();

    struct Handler
    {
        public ConsoleKeyInfo CKI;
        public KeyHandler KeyHandler;
        public bool ResetCompletion;

        public Handler(ConsoleKey key, KeyHandler h, bool resetCompletion = true)
        {
            this.CKI = new ConsoleKeyInfo((char)0, key, false, false, false);
            this.KeyHandler = h;
            this.ResetCompletion = resetCompletion;
        }

        public Handler(char c, KeyHandler h, bool resetCompletion = true)
        {
            this.KeyHandler = h;
            // Use the "Zoom" as a flag that we only have a character.
            this.CKI = new ConsoleKeyInfo(c, ConsoleKey.Zoom, false, false, false);
            this.ResetCompletion = resetCompletion;
        }

        public Handler(ConsoleKeyInfo cki, KeyHandler h, bool resetCompletion = true)
        {
            this.CKI = cki;
            this.KeyHandler = h;
            this.ResetCompletion = resetCompletion;
        }

        public static Handler Control(char c, KeyHandler h, bool resetCompletion = true)
        {
            return new Handler((char)( c - 'A' + 1 ), h, resetCompletion);
        }

        public static Handler Alt(char c, ConsoleKey k, KeyHandler h)
        {
            ConsoleKeyInfo cki = new((char)c, k, false, true, false);
            return new Handler(cki, h);
        }
    }

    /// <summary>
    ///   Invoked when the user requests auto-completion using the tab character
    /// </summary>
    /// <remarks>
    ///    The result is null for no values found, an array with a single
    ///    string, in that case the string should be the text to be inserted
    ///    for example if the word at pos is "T", the result for a completion
    ///    of "ToString" should be "oString", not "ToString".
    ///
    ///    When there are multiple results, the result should be the full
    ///    text
    /// </remarks>
    public AutoCompleteHandler AutoCompleteEvent;

    static Handler[] handlers;

    private readonly bool isWindows;

    /// <summary>
    /// Initializes a new instance of the LineEditor, using the specified name for
    /// retrieving and storing the history.   The history will default to 10 entries.
    /// </summary>
    /// <param name="name">Prefix for storing the editing history.</param>
    public LineEditor(string name) : this(name, 10)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LineEditor, using the specified name for
    /// retrieving and storing the history.
    /// </summary>
    /// <param name="name">Prefix for storing the editing history.</param>
    /// <param name="histsize">Number of entries to store in the history file.</param>
    public LineEditor(string name, int histsize)
    {
        handlers = new Handler[]
        {
            new Handler(ConsoleKey.Home, this.CmdHome),
            new Handler(ConsoleKey.End, this.CmdEnd),
            new Handler(ConsoleKey.LeftArrow, this.CmdLeft),
            new Handler(ConsoleKey.RightArrow, this.CmdRight),
            new Handler(ConsoleKey.UpArrow, this.CmdUp, resetCompletion: false),
            new Handler(ConsoleKey.DownArrow, this.CmdDown, resetCompletion: false),
            new Handler(ConsoleKey.Enter, this.CmdDone, resetCompletion: false),
            new Handler(ConsoleKey.Backspace, this.CmdBackspace, resetCompletion: false),
            new Handler(ConsoleKey.Delete, this.CmdDeleteChar),
            new Handler(ConsoleKey.Tab, this.CmdTabOrComplete, resetCompletion: false),

            // Emacs keys
            Handler.Control('A', this.CmdHome),
            Handler.Control('E', this.CmdEnd),
            Handler.Control('B', this.CmdLeft),
            Handler.Control('F', this.CmdRight),
            Handler.Control('P', this.CmdUp, resetCompletion: false),
            Handler.Control('N', this.CmdDown, resetCompletion: false),
            Handler.Control('K', this.CmdKillToEOF),
            Handler.Control('Y', this.CmdYank),
            Handler.Control('D', this.CmdDeleteChar),
            Handler.Control('L', this.CmdRefresh),
            Handler.Control('R', this.CmdReverseSearch),
            Handler.Control('G', delegate { }),
            Handler.Alt('B', ConsoleKey.B, this.CmdBackwardWord),
            Handler.Alt('F', ConsoleKey.F, this.CmdForwardWord),

            Handler.Alt('D', ConsoleKey.D, this.CmdDeleteWord),
            Handler.Alt((char)8, ConsoleKey.Backspace, this.CmdDeleteBackword),

            // DEBUG
            //Handler.Control ('T', CmdDebug),

            // quote
            Handler.Control('Q', delegate { this.HandleChar(Console.ReadKey(true).KeyChar); })
        };

        this.rendered_text = new StringBuilder();
        this.text = new StringBuilder();

        this.history = new History(name, histsize);

        this.isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        this.GetUnixConsoleReset();
        //if (File.Exists ("log"))File.Delete ("log");
        //log = File.CreateText ("log");
    }

    // On Unix, there is a "default" color which is not represented by any colors in
    // ConsoleColor and it is not possible to set is by setting the ForegroundColor or
    // BackgroundColor properties, so we have to use the terminfo driver in Mono to
    // fetch these values

    void GetUnixConsoleReset()
    {
        //
        // On Unix, we want to be able to reset the color for the pop-up completion
        //
        if (this.isWindows)
            return;

        // Sole purpose of this call is to initialize the Terminfo driver
        int x = Console.CursorLeft;

        try
        {
            object? terminfo_driver = Type.GetType("System.ConsoleDriver")?.GetField("driver", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
            if (terminfo_driver == null)
                return;

            string? unix_reset_colors_str =
                ( terminfo_driver?.GetType()?.GetField("origPair", BindingFlags.Instance | BindingFlags.NonPublic) )?.GetValue(terminfo_driver) as string;

            if (unix_reset_colors_str != null)
                unix_reset_colors = Encoding.UTF8.GetBytes((string)unix_reset_colors_str);
            unix_raw_output = Console.OpenStandardOutput();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e);
        }
    }


    void CmdDebug()
    {
        this.history.Dump();
        Console.WriteLine();
        this.Render();
    }

    void Render()
    {
        Console.Write(this.shown_prompt);
        Console.Write(this.rendered_text);

        int max = System.Math.Max(this.rendered_text.Length + this.shown_prompt.Length, this.max_rendered);

        for (int i = this.rendered_text.Length + this.shown_prompt.Length; i < this.max_rendered; i++)
            Console.Write(' ');
        this.max_rendered = this.shown_prompt.Length + this.rendered_text.Length;

        // Write one more to ensure that we always wrap around properly if we are at the
        // end of a line.
        Console.Write(' ');

        this.UpdateHomeRow(max);
    }

    void UpdateHomeRow(int screenpos)
    {
        int lines = 1 + ( screenpos / Console.WindowWidth );

        this.home_row = Console.CursorTop - ( lines - 1 );
        if (this.home_row < 0)
            this.home_row = 0;
    }


    void RenderFrom(int pos)
    {
        int rpos = this.TextToRenderPos(pos);
        int i;

        for (i = rpos; i < this.rendered_text.Length; i++)
            Console.Write(this.rendered_text[i]);

        if (( this.shown_prompt.Length + this.rendered_text.Length ) > this.max_rendered)
            this.max_rendered = this.shown_prompt.Length + this.rendered_text.Length;
        else
        {
            int max_extra = this.max_rendered - this.shown_prompt.Length;
            for (; i < max_extra; i++)
                Console.Write(' ');
        }
    }

    void ComputeRendered()
    {
        this.rendered_text.Length = 0;

        for (int i = 0; i < this.text.Length; i++)
        {
            int c = (int)this.text[i];
            if (c < 26)
            {
                if (c == '\t')
                    this.rendered_text.Append("    ");
                else
                {
                    this.rendered_text.Append('^');
                    this.rendered_text.Append((char)( c + (int)'A' - 1 ));
                }
            }
            else
                this.rendered_text.Append((char)c);
        }
    }

    int TextToRenderPos(int pos)
    {
        int p = 0;

        for (int i = 0; i < pos; i++)
        {
            int c;

            c = (int)this.text[i];

            if (c < 26)
            {
                if (c == 9)
                    p += 4;
                else
                    p += 2;
            }
            else
                p++;
        }

        return p;
    }

    int TextToScreenPos(int pos)
    {
        return this.shown_prompt.Length + this.TextToRenderPos(pos);
    }

    string Prompt
    {
        get { return this.prompt; }
        set { this.prompt = value; }
    }

    int LineCount
    {
        get { return ( this.shown_prompt.Length + this.rendered_text.Length ) / Console.WindowWidth; }
    }

    void ForceCursor(int newpos)
    {
        this.cursor = newpos;

        int actual_pos = this.shown_prompt.Length + this.TextToRenderPos(this.cursor);
        int row = this.home_row + ( actual_pos / Console.WindowWidth );
        int col = actual_pos % Console.WindowWidth;

        if (row >= Console.BufferHeight)
            row = Console.BufferHeight - 1;
        Console.SetCursorPosition(col, row);

        //log.WriteLine ("Going to cursor={0} row={1} col={2} actual={3} prompt={4} ttr={5} old={6}", newpos, row, col, actual_pos, prompt.Length, TextToRenderPos (cursor), cursor);
        //log.Flush ();
    }

    void UpdateCursor(int newpos)
    {
        if (this.cursor == newpos)
            return;

        this.ForceCursor(newpos);
    }

    void InsertChar(char c)
    {
        int prev_lines = this.LineCount;
        this.text = this.text.Insert(this.cursor, c);
        this.ComputeRendered();
        if (prev_lines != this.LineCount)
        {
            Console.SetCursorPosition(0, this.home_row);
            this.Render();
            this.ForceCursor(++this.cursor);
        }
        else
        {
            this.RenderFrom(this.cursor);
            this.ForceCursor(++this.cursor);
            this.UpdateHomeRow(this.TextToScreenPos(this.cursor));
        }
    }

    static void SaveExcursion(Action code)
    {
        int saved_col = Console.CursorLeft;
        int saved_row = Console.CursorTop;
        ConsoleColor saved_fore = Console.ForegroundColor;
        ConsoleColor saved_back = Console.BackgroundColor;

        code();

        Console.CursorLeft = saved_col;
        Console.CursorTop = saved_row;
        if (unix_reset_colors != null)
        {
            unix_raw_output.Write(unix_reset_colors, 0, unix_reset_colors.Length);
        }
        else
        {
            Console.ForegroundColor = saved_fore;
            Console.BackgroundColor = saved_back;
        }
    }

    class CompletionState
    {
        public string Prefix;
        public string[] Completions;
        public int Col, Row, Width, Height;
        int selected_item, top_item;

        public CompletionState(int col, int row, int width, int height)
        {
            this.Col = col;
            this.Row = row;
            this.Width = width;
            this.Height = height;

            if (this.Col < 0)
                throw new ArgumentException("Cannot be less than zero" + this.Col, "Col");
            if (this.Row < 0)
                throw new ArgumentException("Cannot be less than zero", "Row");
            if (this.Width < 1)
                throw new ArgumentException("Cannot be less than one", "Width");
            if (this.Height < 1)
                throw new ArgumentException("Cannot be less than one", "Height");
        }

        void DrawSelection()
        {
            for (int r = 0; r < this.Height; r++)
            {
                int item_idx = this.top_item + r;
                bool selected = ( item_idx == this.selected_item );

                Console.ForegroundColor = selected ? ConsoleColor.Black : ConsoleColor.Gray;
                Console.BackgroundColor = selected ? ConsoleColor.Cyan : ConsoleColor.Blue;

                string item = this.Prefix + this.Completions[item_idx];
                if (item.Length > this.Width)
                    item = item.Substring(0, this.Width);

                Console.CursorLeft = this.Col;
                Console.CursorTop = this.Row + r;
                Console.Write(item);
                for (int space = item.Length; space <= this.Width; space++)
                    Console.Write(" ");
            }
        }

        public string Current
        {
            get { return this.Completions[this.selected_item]; }
        }

        public void Show()
        {
            SaveExcursion(this.DrawSelection);
        }

        public void SelectNext()
        {
            if (this.selected_item + 1 < this.Completions.Length)
            {
                this.selected_item++;
                if (this.selected_item - this.top_item >= this.Height)
                    this.top_item++;
                SaveExcursion(this.DrawSelection);
            }
        }

        public void SelectPrevious()
        {
            if (this.selected_item > 0)
            {
                this.selected_item--;
                if (this.selected_item < this.top_item)
                    this.top_item = this.selected_item;
                SaveExcursion(this.DrawSelection);
            }
        }

        void Clear()
        {
            for (int r = 0; r < this.Height; r++)
            {
                Console.CursorLeft = this.Col;
                Console.CursorTop = this.Row + r;
                for (int space = 0; space <= this.Width; space++)
                    Console.Write(" ");
            }
        }

        public void Remove()
        {
            SaveExcursion(this.Clear);
        }
    }

    void ShowCompletions(string prefix, string[] completions)
    {
        // Ensure we have space, determine window size
        int window_height = System.Math.Min(completions.Length, Console.WindowHeight / 5);
        int target_line = Console.WindowHeight - window_height - 1;
        if (!this.isWindows && Console.CursorTop > target_line)
        {
            int delta = Console.CursorTop - target_line;
            Console.CursorLeft = 0;
            Console.CursorTop = Console.WindowHeight - 1;
            for (int i = 0; i < delta + 1; i++)
            {
                for (int c = Console.WindowWidth; c > 0; c--)
                    Console.Write(" "); // To debug use ("{0}", i%10);
            }

            Console.CursorTop = target_line;
            Console.CursorLeft = 0;
            this.Render();
        }

        const int MaxWidth = 50;
        int window_width = 12;
        int plen = prefix.Length;
        foreach (string s in completions)
            window_width = System.Math.Max(plen + s.Length, window_width);
        window_width = System.Math.Min(window_width, MaxWidth);

        if (this.current_completion == null)
        {
            int left = Console.CursorLeft - prefix.Length;

            if (left + window_width + 1 >= Console.WindowWidth)
                left = Console.WindowWidth - window_width - 1;

            this.current_completion = new CompletionState(left, Console.CursorTop + 1, window_width, window_height)
            {
                Prefix = prefix,
                Completions = completions,
            };
        }
        else
        {
            this.current_completion.Prefix = prefix;
            this.current_completion.Completions = completions;
        }

        this.current_completion.Show();
        Console.CursorLeft = 0;
    }

    void HideCompletions()
    {
        if (this.current_completion == null)
            return;
        this.current_completion.Remove();
        this.current_completion = null;
    }

    //
    // Triggers the completion engine, if insertBestMatch is true, then this will
    // insert the best match found, this behaves like the shell "tab" which will
    // complete as much as possible given the options.
    //
    void Complete()
    {
        Completion completion = this.AutoCompleteEvent(this.text.ToString(), this.cursor);
        string[] completions = completion.Result;
        if (completions == null)
        {
            this.HideCompletions();
            return;
        }

        int ncompletions = completions.Length;
        if (ncompletions == 0)
        {
            this.HideCompletions();
            return;
        }

        if (completions.Length == 1)
        {
            this.InsertTextAtCursor(completions[0]);
            this.HideCompletions();
        }
        else
        {
            int last = -1;

            for (int p = 0; p < completions[0].Length; p++)
            {
                char c = completions[0][p];


                for (int i = 1; i < ncompletions; i++)
                {
                    if (completions[i].Length < p)
                        goto mismatch;

                    if (completions[i][p] != c)
                    {
                        goto mismatch;
                    }
                }

                last = p;
            }

            mismatch:
            string prefix = completion.Prefix;
            if (last != -1)
            {
                this.InsertTextAtCursor(completions[0].Substring(0, last + 1));

                // Adjust the completions to skip the common prefix
                prefix += completions[0].Substring(0, last + 1);
                for (int i = 0; i < completions.Length; i++)
                    completions[i] = completions[i].Substring(last + 1);
            }

            this.ShowCompletions(prefix, completions);
            this.Render();
            this.ForceCursor(this.cursor);
        }
    }

    //
    // When the user has triggered a completion window, this will try to update
    // the contents of it.   The completion window is assumed to be hidden at this
    // point
    //
    void UpdateCompletionWindow()
    {
        if (this.current_completion != null)
            throw new Exception("This method should only be called if the window has been hidden");

        Completion completion = this.AutoCompleteEvent(this.text.ToString(), this.cursor);
        string[] completions = completion.Result;
        if (completions == null)
            return;

        int ncompletions = completions.Length;
        if (ncompletions == 0)
            return;

        this.ShowCompletions(completion.Prefix, completion.Result);
        this.Render();
        this.ForceCursor(this.cursor);
    }


    //
    // Commands
    //
    void CmdDone()
    {
        if (this.current_completion != null)
        {
            this.InsertTextAtCursor(this.current_completion.Current);
            this.HideCompletions();
            return;
        }

        this.done = true;
    }

    void CmdTabOrComplete()
    {
        bool complete = false;

        if (this.AutoCompleteEvent != null)
        {
            if (this.TabAtStartCompletes)
                complete = true;
            else
            {
                for (int i = 0; i < this.cursor; i++)
                {
                    if (!char.IsWhiteSpace(this.text[i]))
                    {
                        complete = true;
                        break;
                    }
                }
            }

            if (complete)
                this.Complete();
            else
                this.HandleChar('\t');
        }
        else
            this.HandleChar('t');
    }

    void CmdHome()
    {
        this.UpdateCursor(0);
    }

    void CmdEnd()
    {
        this.UpdateCursor(this.text.Length);
    }

    void CmdLeft()
    {
        if (this.cursor == 0)
            return;

        this.UpdateCursor(this.cursor - 1);
    }

    void CmdBackwardWord()
    {
        int p = this.WordBackward(this.cursor);
        if (p == -1)
            return;
        this.UpdateCursor(p);
    }

    void CmdForwardWord()
    {
        int p = this.WordForward(this.cursor);
        if (p == -1)
            return;
        this.UpdateCursor(p);
    }

    void CmdRight()
    {
        if (this.cursor == this.text.Length)
            return;

        this.UpdateCursor(this.cursor + 1);
    }

    void RenderAfter(int p)
    {
        this.ForceCursor(p);
        this.RenderFrom(p);
        this.ForceCursor(this.cursor);
    }

    void CmdBackspace()
    {
        if (this.cursor == 0)
            return;

        bool completing = this.current_completion != null;
        this.HideCompletions();

        this.text.Remove(--this.cursor, 1);
        this.ComputeRendered();
        this.RenderAfter(this.cursor);
        if (completing)
            this.UpdateCompletionWindow();
    }

    void CmdDeleteChar()
    {
        // If there is no input, this behaves like EOF
        if (this.text.Length == 0)
        {
            this.done = true;
            this.text = null;
            Console.WriteLine();
            return;
        }

        if (this.cursor == this.text.Length)
            return;
        this.text.Remove(this.cursor, 1);
        this.ComputeRendered();
        this.RenderAfter(this.cursor);
    }

    int WordForward(int p)
    {
        if (p >= this.text.Length)
            return -1;

        int i = p;
        if (char.IsPunctuation(this.text[p]) || char.IsSymbol(this.text[p]) || char.IsWhiteSpace(this.text[p]))
        {
            for (; i < this.text.Length; i++)
            {
                if (char.IsLetterOrDigit(this.text[i]))
                    break;
            }

            for (; i < this.text.Length; i++)
            {
                if (!char.IsLetterOrDigit(this.text[i]))
                    break;
            }
        }
        else
        {
            for (; i < this.text.Length; i++)
            {
                if (!char.IsLetterOrDigit(this.text[i]))
                    break;
            }
        }

        if (i != p)
            return i;
        return -1;
    }

    int WordBackward(int p)
    {
        if (p == 0)
            return -1;

        int i = p - 1;
        if (i == 0)
            return 0;

        if (char.IsPunctuation(this.text[i]) || char.IsSymbol(this.text[i]) || char.IsWhiteSpace(this.text[i]))
        {
            for (; i >= 0; i--)
            {
                if (char.IsLetterOrDigit(this.text[i]))
                    break;
            }

            for (; i >= 0; i--)
            {
                if (!char.IsLetterOrDigit(this.text[i]))
                    break;
            }
        }
        else
        {
            for (; i >= 0; i--)
            {
                if (!char.IsLetterOrDigit(this.text[i]))
                    break;
            }
        }

        i++;

        if (i != p)
            return i;

        return -1;
    }

    void CmdDeleteWord()
    {
        int pos = this.WordForward(this.cursor);

        if (pos == -1)
            return;

        string k = this.text.ToString(this.cursor, pos - this.cursor);

        if (this.last_handler == this.CmdDeleteWord)
            this.kill_buffer = this.kill_buffer + k;
        else
            this.kill_buffer = k;

        this.text.Remove(this.cursor, pos - this.cursor);
        this.ComputeRendered();
        this.RenderAfter(this.cursor);
    }

    void CmdDeleteBackword()
    {
        int pos = this.WordBackward(this.cursor);
        if (pos == -1)
            return;

        string k = this.text.ToString(pos, this.cursor - pos);

        if (this.last_handler == this.CmdDeleteBackword)
            this.kill_buffer = k + this.kill_buffer;
        else
            this.kill_buffer = k;

        this.text.Remove(pos, this.cursor - pos);
        this.ComputeRendered();
        this.RenderAfter(pos);
    }

    //
    // Adds the current line to the history if needed
    //
    void HistoryUpdateLine()
    {
        this.history.Update(this.text.ToString());
    }

    void CmdHistoryPrev()
    {
        if (!this.history.PreviousAvailable())
            return;

        this.HistoryUpdateLine();

        this.SetText(this.history.Previous());
    }

    void CmdHistoryNext()
    {
        if (!this.history.NextAvailable())
            return;

        this.history.Update(this.text.ToString());
        this.SetText(this.history.Next());
    }

    void CmdUp()
    {
        if (this.current_completion == null)
            this.CmdHistoryPrev();
        else
            this.current_completion.SelectPrevious();
    }

    void CmdDown()
    {
        if (this.current_completion == null)
            this.CmdHistoryNext();
        else
            this.current_completion.SelectNext();
    }

    void CmdKillToEOF()
    {
        this.kill_buffer = this.text.ToString(this.cursor, this.text.Length - this.cursor);
        this.text.Length = this.cursor;
        this.ComputeRendered();
        this.RenderAfter(this.cursor);
    }

    void CmdYank()
    {
        this.InsertTextAtCursor(this.kill_buffer);
    }

    void InsertTextAtCursor(string str)
    {
        int prev_lines = this.LineCount;
        this.text.Insert(this.cursor, str);
        this.ComputeRendered();
        if (prev_lines != this.LineCount)
        {
            Console.SetCursorPosition(0, this.home_row);
            this.Render();
            this.cursor += str.Length;
            this.ForceCursor(this.cursor);
        }
        else
        {
            this.RenderFrom(this.cursor);
            this.cursor += str.Length;
            this.ForceCursor(this.cursor);
            this.UpdateHomeRow(this.TextToScreenPos(this.cursor));
        }
    }

    void SetSearchPrompt(string s)
    {
        this.SetPrompt("(reverse-i-search)`" + s + "': ");
    }

    void ReverseSearch()
    {
        int p;

        if (this.cursor == this.text.Length)
        {
            // The cursor is at the end of the string

            p = this.text.ToString().LastIndexOf(this.search);
            if (p != -1)
            {
                this.match_at = p;
                this.cursor = p;
                this.ForceCursor(this.cursor);
                return;
            }
        }
        else
        {
            // The cursor is somewhere in the middle of the string
            int start = ( this.cursor == this.match_at ) ? this.cursor - 1 : this.cursor;
            if (start != -1)
            {
                p = this.text.ToString().LastIndexOf(this.search, start);
                if (p != -1)
                {
                    this.match_at = p;
                    this.cursor = p;
                    this.ForceCursor(this.cursor);
                    return;
                }
            }
        }

        // Need to search backwards in history
        this.HistoryUpdateLine();
        string s = this.history.SearchBackward(this.search);
        if (s != null)
        {
            this.match_at = -1;
            this.SetText(s);
            this.ReverseSearch();
        }
    }

    void CmdReverseSearch()
    {
        if (this.searching == 0)
        {
            this.match_at = -1;
            this.last_search = this.search;
            this.searching = -1;
            this.search = "";
            this.SetSearchPrompt("");
        }
        else
        {
            if (this.search == "")
            {
                if (this.last_search != "" && this.last_search != null)
                {
                    this.search = this.last_search;
                    this.SetSearchPrompt(this.search);

                    this.ReverseSearch();
                }

                return;
            }

            this.ReverseSearch();
        }
    }

    void SearchAppend(char c)
    {
        this.search = this.search + c;
        this.SetSearchPrompt(this.search);

        //
        // If the new typed data still matches the current text, stay here
        //
        if (this.cursor < this.text.Length)
        {
            string r = this.text.ToString(this.cursor, this.text.Length - this.cursor);
            if (r.StartsWith(this.search))
                return;
        }

        this.ReverseSearch();
    }

    void CmdRefresh()
    {
        Console.Clear();
        this.max_rendered = 0;
        this.Render();
        this.ForceCursor(this.cursor);
    }

    void InterruptEdit(object sender, ConsoleCancelEventArgs a)
    {
        // Do not abort our program:
        a.Cancel = true;

        // Interrupt the editor
        this.edit_thread.Abort();
    }

    //
    // Implements heuristics to show the completion window based on the mode
    //
    bool HeuristicAutoComplete(bool wasCompleting, char insertedChar)
    {
        if (this.HeuristicsMode == "csharp")
        {
            // csharp heuristics
            if (wasCompleting)
            {
                if (insertedChar == ' ')
                {
                    return false;
                }

                return true;
            }

            // If we were not completing, determine if we want to now
            if (insertedChar == '.')
            {
                // Avoid completing for numbers "1.2" for example
                if (this.cursor > 1 && char.IsDigit(this.text[this.cursor - 2]))
                {
                    for (int p = this.cursor - 3; p >= 0; p--)
                    {
                        char c = this.text[p];
                        if (char.IsDigit(c))
                            continue;
                        if (c == '_')
                            return true;
                        if (char.IsLetter(c) || char.IsPunctuation(c) || char.IsSymbol(c) || char.IsControl(c))
                            return true;
                    }

                    return false;
                }

                return true;
            }
        }

        return false;
    }

    void HandleChar(char c)
    {
        if (this.searching != 0)
            this.SearchAppend(c);
        else
        {
            bool completing = this.current_completion != null;
            this.HideCompletions();

            this.InsertChar(c);
            if (this.HeuristicAutoComplete(completing, c))
                this.UpdateCompletionWindow();
        }
    }

    void EditLoop()
    {
        ConsoleKeyInfo cki;

        while (!this.done)
        {
            ConsoleModifiers mod;

            cki = Console.ReadKey(true);
            if (cki.Key == ConsoleKey.Escape)
            {
                if (this.current_completion != null)
                {
                    this.HideCompletions();
                    continue;
                }
                else
                {
                    cki = Console.ReadKey(true);

                    mod = ConsoleModifiers.Alt;
                }
            }
            else
                mod = cki.Modifiers;

            bool handled = false;

            foreach (Handler handler in handlers)
            {
                ConsoleKeyInfo t = handler.CKI;

                if (t.Key == cki.Key && t.Modifiers == mod)
                {
                    handled = true;
                    if (handler.ResetCompletion)
                        this.HideCompletions();
                    handler.KeyHandler();
                    this.last_handler = handler.KeyHandler;
                    break;
                }
                else if (t.KeyChar == cki.KeyChar && t.Key == ConsoleKey.Zoom)
                {
                    handled = true;
                    if (handler.ResetCompletion)
                        this.HideCompletions();

                    handler.KeyHandler();
                    this.last_handler = handler.KeyHandler;
                    break;
                }
            }

            if (handled)
            {
                if (this.searching != 0)
                {
                    if (this.last_handler != this.CmdReverseSearch)
                    {
                        this.searching = 0;
                        this.SetPrompt(this.prompt);
                    }
                }

                continue;
            }

            if (cki.KeyChar != (char)0)
            {
                this.HandleChar(cki.KeyChar);
            }
        }
    }

    void InitText(string initial)
    {
        this.text = new StringBuilder(initial);
        this.ComputeRendered();
        this.cursor = this.text.Length;
        this.Render();
        this.ForceCursor(this.cursor);
    }

    void SetText(string newtext)
    {
        Console.SetCursorPosition(0, this.home_row);
        this.InitText(newtext);
    }

    void SetPrompt(string newprompt)
    {
        this.shown_prompt = newprompt;
        Console.SetCursorPosition(0, this.home_row);
        this.Render();
        this.ForceCursor(this.cursor);
    }

    /// <summary>
    /// Edit a line, and provides both a prompt and the initial contents to edit
    /// </summary>
    /// <returns>The edit.</returns>
    /// <param name="prompt">Prompt shown to edit the line.</param>
    /// <param name="initial">Initial contents, can be null.</param>
    public string Edit(string prompt, string initial)
    {
        this.edit_thread = Thread.CurrentThread;
        this.searching = 0;
        Console.CancelKeyPress += this.InterruptEdit;

        this.done = false;
        this.history.CursorToEnd();
        this.max_rendered = 0;

        this.Prompt = prompt;
        this.shown_prompt = prompt;
        this.InitText(initial);
        this.history.Append(initial);

        do
        {
            try
            {
                this.EditLoop();
            }
            catch (ThreadAbortException)
            {
                this.searching = 0;
                Thread.ResetAbort();
                Console.WriteLine();
                this.SetPrompt(prompt);
                this.SetText("");
            }
        } while (!this.done);

        Console.WriteLine();

        Console.CancelKeyPress -= this.InterruptEdit;

        if (this.text == null)
        {
            this.history.Close();
            return null;
        }

        string result = this.text.ToString();
        if (result != "")
            this.history.Accept(result);
        else
            this.history.RemoveLast();

        return result;
    }

    /// <summary>
    /// Triggers the history to be written at this point, usually not necessary, history is saved on each line edited.
    /// </summary>
    public void SaveHistory()
    {
        if (this.history != null)
        {
            this.history.Close();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether hitting the TAB key before any text exists triggers completion or inserts a "tab" character into the buffer.  This is useful to allow users to copy/paste code that might contain whitespace at the start and you want to preserve it.
    /// </summary>
    /// <value><c>true</c> if tab at start completes; otherwise, <c>false</c>.</value>
    public bool TabAtStartCompletes { get; set; }

    //
    // Emulates the bash-like behavior, where edits done to the
    // history are recorded
    //
    class History
    {
        string[] history;
        int head, tail;
        int cursor, count;
        string histfile;

        public History(string app, int size)
        {
            if (size < 1)
                throw new ArgumentException("size");

            if (app != null)
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                //Console.WriteLine (dir);
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch
                    {
                        app = null;
                    }
                }

                if (app != null)
                    this.histfile = Path.Combine(dir, app) + ".history";
            }

            this.history = new string [size];
            this.head = this.tail = this.cursor = 0;

            if (File.Exists(this.histfile))
            {
                using (StreamReader sr = File.OpenText(this.histfile))
                {
                    string line;

                    while (( line = sr.ReadLine() ) != null)
                    {
                        if (line != "")
                            this.Append(line);
                    }
                }
            }
        }

        public void Close()
        {
            if (this.histfile == null)
                return;

            try
            {
                using (StreamWriter sw = File.CreateText(this.histfile))
                {
                    int start = ( this.count == this.history.Length ) ? this.head : this.tail;
                    for (int i = start; i < start + this.count; i++)
                    {
                        int p = i % this.history.Length;
                        sw.WriteLine(this.history[p]);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        //
        // Appends a value to the history
        //
        public void Append(string s)
        {
            //Console.WriteLine ("APPENDING {0} head={1} tail={2}", s, head, tail);
            this.history[this.head] = s;
            this.head = ( this.head + 1 ) % this.history.Length;
            if (this.head == this.tail)
                this.tail = ( this.tail + 1 % this.history.Length );
            if (this.count != this.history.Length)
                this.count++;
            //Console.WriteLine ("DONE: head={1} tail={2}", s, head, tail);
        }

        //
        // Updates the current cursor location with the string,
        // to support editing of history items.   For the current
        // line to participate, an Append must be done before.
        //
        public void Update(string s)
        {
            this.history[this.cursor] = s;
        }

        public void RemoveLast()
        {
            this.head = this.head - 1;
            if (this.head < 0)
                this.head = this.history.Length - 1;
        }

        public void Accept(string s)
        {
            int t = this.head - 1;
            if (t < 0)
                t = this.history.Length - 1;

            this.history[t] = s;
        }

        public bool PreviousAvailable()
        {
            //Console.WriteLine ("h={0} t={1} cursor={2}", head, tail, cursor);
            if (this.count == 0)
                return false;
            int next = this.cursor - 1;
            if (next < 0)
                next = this.count - 1;

            if (next == this.head)
                return false;

            return true;
        }

        public bool NextAvailable()
        {
            if (this.count == 0)
                return false;
            int next = ( this.cursor + 1 ) % this.history.Length;
            if (next == this.head)
                return false;
            return true;
        }


        //
        // Returns: a string with the previous line contents, or
        // nul if there is no data in the history to move to.
        //
        public string Previous()
        {
            if (!this.PreviousAvailable())
                return null;

            this.cursor--;
            if (this.cursor < 0)
                this.cursor = this.history.Length - 1;

            return this.history[this.cursor];
        }

        public string Next()
        {
            if (!this.NextAvailable())
                return null;

            this.cursor = ( this.cursor + 1 ) % this.history.Length;
            return this.history[this.cursor];
        }

        public void CursorToEnd()
        {
            if (this.head == this.tail)
                return;

            this.cursor = this.head;
        }

        public void Dump()
        {
            Console.WriteLine("Head={0} Tail={1} Cursor={2} count={3}", this.head, this.tail, this.cursor, this.count);
            for (int i = 0; i < this.history.Length; i++)
            {
                Console.WriteLine(" {0} {1}: {2}", i == this.cursor ? "==>" : "   ", i, this.history[i]);
            }
            //log.Flush ();
        }

        public string SearchBackward(string term)
        {
            for (int i = 0; i < this.count; i++)
            {
                int slot = this.cursor - i - 1;
                if (slot < 0)
                    slot = this.history.Length + slot;
                if (slot >= this.history.Length)
                    slot = 0;
                if (this.history[slot] != null && this.history[slot].IndexOf(term) != -1)
                {
                    this.cursor = slot;
                    return this.history[slot];
                }
            }

            return null;
        }
    }
}

#if DEMO
	class Demo {
		static void Main ()
		{
			LineEditor le = new LineEditor ("foo") {
				HeuristicsMode = "csharp"
			};
			le.AutoCompleteEvent += delegate (string a, int pos){
				string prefix = "";
				var completions = new string [] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
				return new Mono.Terminal.LineEditor.Completion (prefix, completions);
			};

			string s;

			while ((s = le.Edit ("shell> ", "")) != null){
				Console.WriteLine ("----> [{0}]", s);
			}
		}
	}
#endif

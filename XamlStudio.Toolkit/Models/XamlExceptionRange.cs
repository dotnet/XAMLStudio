using System;
using System.Text.RegularExpressions;

namespace XamlStudio.Toolkit.Models
{
    /// <summary>
    /// Xaml Parsing Error Message and Location.
    /// </summary>
    public sealed class XamlExceptionRange : Exception
    {
        public uint StartLine { get; set; }
        public uint StartColumn { get; set; }
        public uint EndLine { get; set; }
        public uint EndColumn { get; set; }
        public bool IsWholeLine { get; set; } = false;

        public XamlExceptionRange(string message, Exception error, uint startline, uint startcol, uint endline, uint endcol) : base(message, error)
        {
            StartLine = startline;
            StartColumn = startcol;
            EndLine = endline;
            EndColumn = endcol;
        }

        /// <summary>
        /// Pass in the line content to automatically have the endCol calculated. This assumes exception is on one line.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <param name="startline"></param>
        /// <param name="startcol"></param>
        /// <param name="lineContent"></param>
        public XamlExceptionRange(string message, Exception error, uint startline, uint startcol, string lineContent) : base(message, error)
        {
            StartLine = startline;
            EndLine = startline;

            var match = Regex.Match(lineContent?.Substring((int)startcol - 1), @"\W");

            if (match.Success)
            {
                StartColumn = startcol;
                EndColumn = (uint)match.Index + startcol;
            }
            else
            {
                StartColumn = 0;
                EndColumn = (uint)(lineContent?.Length ?? 0);
                IsWholeLine = true;
            }
        }
    }
}

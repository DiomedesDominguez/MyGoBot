//  Copyright 2004-2016 MOFT Dominicana - http://www.moft.com.do/
// 
//  Licensed under the Microsoft Public License (Ms-PL), (the "License");
//  you may not use this file except in compliance with the License.
// 
//  You may obtain a copy of the License at
// 
//      https://msdn.microsoft.com/en-us/library/ff649456.aspx
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
namespace PGB.WPF.Internals
{
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;

    /// <summary>
    /// Class ListBoxWriter.
    /// </summary>
    internal class ListBoxWriter : TextWriter
    {
        /// <summary>
        /// The _list box
        /// </summary>
        private readonly ListBox _listBox;
        /// <summary>
        /// The _content
        /// </summary>
        private StringBuilder _content = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxWriter"/> class.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        public ListBoxWriter(ListBox listBox)
        {
            _listBox = listBox;
        }

        /// <summary>
        /// The encoding
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            base.Write(value);
            _content.Append(value);
            if (value == '\n')
            {
                if (!_listBox.Dispatcher.CheckAccess())
                {
                    _listBox.Dispatcher.Invoke(WriteLogMessage);
                }
                else
                {
                    WriteLogMessage();
                }
                _content = new StringBuilder();
            }
        }

        private void WriteLogMessage()
        {
            _listBox.Items.Add(Regex.Replace(_content.ToString(), "\\r\\n?|\\n", ""));
            var num = 0;
            while (_listBox.Items.Count > 500)
            {
                _listBox.Items.RemoveAt(num);
                num--;
            }

            _listBox.ScrollToBottom();
        }
    }
}
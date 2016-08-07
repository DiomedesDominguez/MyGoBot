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

namespace PGB.Logic.Logging
{
    using System;

    /// <summary>
    ///     Class Logger.
    /// </summary>
    public static class Logger
    {
        #region Fields, properties, indexers and constants

        /// <summary>
        ///     The _logger
        /// </summary>
        private static ILogger _logger;

        #endregion

        #region Methods and other members

        /// <summary>
        ///     Sets the logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="level">The level.</param>
        /// <param name="color">The color.</param>
        public static void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            var logger = _logger;
            if (logger == null)
            {
                return;
            }

            var message1 = message;
            var num1 = (int) level;
            var num2 = (int) color;
            logger.Write(message1, (LogLevel) num1, (ConsoleColor) num2);
        }

        #endregion
    }
}